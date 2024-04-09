using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Metrino.Development.Builder;

public class NugetPackageReference
{
    public string Include { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;

    public string ToString()
    {
        return $"<PackageReference Include=\"{Include}\" Version=\"{Version}\" />";
    }
}

public class DllReference
{
    public string Include { get; set; } = string.Empty;
    public string DllPath { get; set; } = string.Empty;
    public bool CopyLocalSatelliteAssemblies { get; set; } = true;
    public bool ReferenceOutputAssembly { get; set; } = true;
    public bool? IsPrivate { get; set; } = true;

    public string ToString()
    {
        var root = $"<Reference Include=\"{Include.Replace("/", "\\")}\">\n";

        root += $"<CopyLocalSatelliteAssemblies>{CopyLocalSatelliteAssemblies}</CopyLocalSatelliteAssemblies>\n";
        root += $"<ReferenceOutputAssembly>{ReferenceOutputAssembly}</ReferenceOutputAssembly>\n";

        if (IsPrivate != null)
            root += $"<Private>{IsPrivate}</Private>\n";

        if (!string.IsNullOrEmpty(DllPath))
            root += $"<HintPath>{DllPath.Replace("/", "\\")}</HintPath>\n";

        root += "</Reference>";

        return root;
    }
}

public class SourceFile
{
    public string Include { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;

    public string ToString()
    {
        return $"<Compile Include = \"{Include.Replace("/", "\\")}\">\n<Link>{Link.Replace("/", "\\")}</Link></Compile>";
    }
}

public class ProjectBase
{
    public string Name { get; set; } = string.Empty;
    public string TargetFramework { get; set; } = "net48";
    public string OutputType { get; set; } = "Library";

    public IEnumerable<DllReference> References { get; set; } = new List<DllReference>();
    public IEnumerable<SourceFile> Sources { get; set; } = new List<SourceFile>();
    public IEnumerable<NugetPackageReference> Packages { get; set; } = new List<NugetPackageReference>();

    public string ToString()
    {
        var sources = string.Join("\n", Sources.Select(x => x.ToString()));
        var references = string.Join("\n", References.Select(x => x.ToString()));
        var packages = string.Join("\n", Packages.Select(x => x.ToString()));

        var lines = new string[] {
            "<Project Sdk=\"Microsoft.NET.Sdk\">",
            "  <PropertyGroup Label=\"Globals\">",
            "    <Keyword>Win32Proj</Keyword>",
            "    <AutoGenerateBindingRedirects>true</AutoGenerateBindingRedirects>",
            "    <NoWarn>168,3021,3014,3001</NoWarn>",
            "    <EnableDefaultItems>false</EnableDefaultItems>",
            "    <VCProjectUpgraderObjectName>NoUpgrade</VCProjectUpgraderObjectName>",
            "    <ManagedAssembly>true</ManagedAssembly>",
            "    <AppendTargetFrameworkToOutputPath>false</AppendTargetFrameworkToOutputPath>",
            $"    <TargetFramework>{TargetFramework}</TargetFramework>",
            $"    <OutputType>{OutputType}</OutputType>",
            "  </PropertyGroup>",
            "  <PropertyGroup Condition=\"'$(Configuration)' == 'Debug'\">",
            "    <DebugType>full</DebugType>",
            "    <DefineConstants>DEBUG</DefineConstants>",
            "    <ErrorReport>prompt</ErrorReport>",
            "    <LangVersion>latest</LangVersion>",
            "    <Optimize>false</Optimize>",
            "    <WarningLevel>3</WarningLevel>",
            "  </PropertyGroup>",
            "  <PropertyGroup Condition=\"'$(Configuration)' == 'Release'\">",
            "    <DebugType>none</DebugType>",
            "    <DefineConstants></DefineConstants>",
            "    <ErrorReport>queue</ErrorReport>",
            "    <LangVersion>latest</LangVersion>",
            "    <Optimize>true</Optimize>",
            "    <WarningLevel>1</WarningLevel>",
            "  </PropertyGroup>",
            "  <ItemGroup>",
            $"    {sources}",
            "  </ItemGroup>",
            "  <ItemGroup>",
            $"    {references}  ",
            "  </ItemGroup>",
            "  <ItemGroup>",
            $"    {packages}",
            "  </ItemGroup>",
            "</Project>"
        };

        return string.Join("\n", lines);
    }

    public void Save(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        var outputFilePath = Path.Combine(path, $"{Name}.csproj");
        using (StreamWriter writer = new StreamWriter(outputFilePath))
            writer.Write(ToString());
    }

    public async Task Build(string outputPath, string buildConfiguration = "Debug", string buildTarget = "x64", string[]? consoleLoggerParameters = null)
    {
        string projectPath = Path.Combine(outputPath, ".csproj");
        if (!Directory.Exists(projectPath))
            Directory.CreateDirectory(projectPath);

        Save(projectPath);

        await MSBuildProcess.RunProcessAsync(
            projectFile: Path.Combine(projectPath, $"{Name}.csproj"),
            outputPath: Path.Combine(outputPath, buildConfiguration, Name),
            buildConfiguration: buildConfiguration,
            buildTarget: buildTarget,
            dotnetTargetVersion: "net48",
            restoreNugetPackages: true,
            consoleLoggerParameters: consoleLoggerParameters
        );
    }

    public static IEnumerable<SourceFile> GetSources(string path, string moduleName)
    {
        var length = path.Length;
        var files = Directory.GetFiles(Path.Combine(path, moduleName, "Source"), "*.cs", SearchOption.AllDirectories);

        return files.Select(p => new SourceFile { Include = p, Link = p.Substring(length + 1) });
    }

    public static async Task BuildService(string metrinoBuildOutputPath, string sourcePath, string outputPath, string buildConfiguration = "Debug", bool force = true)
    {
        var development_files = GetSources(sourcePath, "Metrino.Development");
        var data_model_files = GetSources(sourcePath, "Metrino.Development.DataModel");
        var service_files = GetSources(sourcePath, "Metrino.Development.Service");

        var service_sources = (new IEnumerable<SourceFile>[] { development_files, data_model_files, service_files }).SelectMany(s => s);
        var exeOutputPath = Path.Combine(outputPath, buildConfiguration, "Metrino.Development.Service", "Metrino.Development.Service.exe");
        var exeBuildTime = new FileInfo(exeOutputPath).LastWriteTime;

        var needsReduild = false;
        foreach (var file in service_sources)
        {
            var fileAccessTime = new FileInfo(file.Include).LastWriteTime;
            if (exeBuildTime < fileAccessTime)
            {
                needsReduild = true;
                break;
            }
        }

        if (!needsReduild && !force) return;

        var serviceProject = new ProjectBase
        {
            Name = "Metrino.Development.Service",
            Packages = PackageReferences.NugetPackageReferences,
            References = PackageReferences.DirectDllReferences(metrinoBuildOutputPath),
            Sources = service_sources,
            OutputType = "Exe"
        };

        await serviceProject.Build(outputPath, buildConfiguration: buildConfiguration);
    }

    public static async Task BuildScriptingEngine(string metrinoBuildOutputPath, string sourcePath, string outputPath, string buildConfiguration = "Debug", bool force = true)
    {
        var development_files = GetSources(sourcePath, "Metrino.Development");
        var data_model_files = GetSources(sourcePath, "Metrino.Development.DataModel");
        var script_files = GetSources(sourcePath, "Metrino.Development.Script");

        var script_sources = (new IEnumerable<SourceFile>[] { development_files, data_model_files, script_files }).SelectMany(s => s);
        var exeOutputPath = Path.Combine(outputPath, buildConfiguration, "Metrino.Development.Service", "Metrino.Development.Service.exe");
        var exeBuildTime = new FileInfo(exeOutputPath).LastWriteTime;

        var needsReduild = false;
        foreach (var file in script_sources)
        {
            var fileAccessTime = new FileInfo(file.Include).LastWriteTime;
            if (exeBuildTime < fileAccessTime)
            {
                needsReduild = true;
                break;
            }
        }

        if (!needsReduild && !force) return;

        var scrptingProject = new ProjectBase
        {
            Name = "Metrino.Development.Script",
            Packages = PackageReferences.NugetPackageReferences,
            References = PackageReferences.DirectDllReferences(metrinoBuildOutputPath),
            Sources = script_sources,
            OutputType = "Exe"
        };

        await scrptingProject.Build(outputPath, buildConfiguration: buildConfiguration);
    }
}
