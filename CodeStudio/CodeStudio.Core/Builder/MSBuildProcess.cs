using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Metrino.Development.Builder;

public class MSBuildProcess
{
    public static Task<int> RunProcessAsync(string projectFile, string outputPath, string buildConfiguration = "Debug", string buildTarget = "x64", string? dotnetTargetVersion = null, bool restoreNugetPackages = false, string[]? consoleLoggerParameters = null)
    {
        var tcs = new TaskCompletionSource<int>();

        var programFiles = "C:\\Program Files (x86)";
        var msbuildPath = Path.Combine(programFiles, "Microsoft Visual Studio", "2019", "Professional", "MSBuild", "Current", "Bin");
        var msbuildExe = Path.Combine(msbuildPath, "MSBuild.exe");

        var startInfo = new ProcessStartInfo
        {
            UseShellExecute = false,
            FileName = msbuildExe,
            CreateNoWindow = false,
            RedirectStandardInput = false,
            RedirectStandardOutput = false
        };

        var output_path = $"/p:OutputPath={outputPath}";
        var configuration = $"/p:Configuration={buildConfiguration}";
        var platform = $"/p:Platform={buildTarget}";
        var processArgument = new List<string> {
            $"-m:{Environment.ProcessorCount}",
            "-nologo", 
            $"-clp:{string.Join(";", consoleLoggerParameters ?? new string[] { "Summary", "ErrorsOnly" })}",
            "-verbosity:Quiet" };

        if (restoreNugetPackages)
            processArgument.Add("/t:Restore;Build");

        if (dotnetTargetVersion != null)
            processArgument.Add($"/p:TargetFrameworkProfile=;TargetFramework={dotnetTargetVersion ?? "net48"}");

        processArgument.AddRange(new string[] { projectFile, configuration, platform, output_path });

        foreach (var argument in processArgument)
            startInfo.ArgumentList.Add(argument);

        var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

        process.Exited += (sender, args) =>
        {
            tcs.SetResult(process.ExitCode);
            process.Dispose();
        };

        process.Start();

        return tcs.Task;
    }
}