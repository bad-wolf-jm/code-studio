using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Metrino.Development.Builder;

public class MetrinoBuilder
{
    private struct BuildTag
    {
        public string Name;
        public string ProjectPath;
        public string SourcePath;
        public string BinaryPath;

        public string DllName => Path.Combine(BinaryPath, $"{Name}.dll");

        public bool NeedsRebuild(DateTime time, string repositoryPath, string buildConfiguration, string path)
        {
            if (!File.Exists(Path.Combine(path, buildConfiguration, DllName)))
                return true;

            var dllBuildTime = new FileInfo(Path.Combine(path, buildConfiguration, DllName)).LastWriteTime;
            return (time > dllBuildTime);
            //return false;
        }


    }

    private static List<BuildTag> metrinoModules = new List<BuildTag>
    {
        new BuildTag
        {
            Name = "Metrino.Otdr",
            ProjectPath = Path.Combine("OtdrBase", "FF4", "Metrino.Otdr", "Metrino.Otdr.csproj"),
            SourcePath = Path.Combine("OtdrBase", "Metrino.Otdr"),
            BinaryPath = Path.Combine("Library", "Metrino.Otdr")
        },
        new BuildTag
        {
            Name = "Metrino.Otdr.SignalProcessing",
            ProjectPath = Path.Combine("OtdrBase", "FF4", "Metrino.Otdr.SignalProcessing", "Metrino.Otdr.SignalProcessing.csproj"),
            SourcePath = Path.Combine("OtdrBase", "Metrino.Otdr.SignalProcessing"),
            BinaryPath = Path.Combine("Library", "Metrino.Otdr.SignalProcessing")
        },
        new BuildTag
        {
            Name = "Metrino.Otdr.Simulation",
            ProjectPath = Path.Combine("OtdrBase", "FF4", "Metrino.Otdr.Simulation", "Metrino.Otdr.Simulation.csproj"),
            SourcePath = Path.Combine("OtdrBase", "Metrino.Otdr.Simulation"),
            BinaryPath = Path.Combine("Library", "Metrino.Otdr.Simulation")
        },
        new BuildTag
        {
            Name = "Metrino.Otdr.FileConverter",
            ProjectPath = Path.Combine("OtdrBase", "FF4", "Metrino.Otdr.FileConverter", "Metrino.Otdr.FileConverter.csproj"),
            SourcePath = Path.Combine("OtdrBase", "Metrino.Otdr.FileConverter"),
            BinaryPath = Path.Combine("Library", "Metrino.Otdr.FileConverter")
        },
        new BuildTag
        {
            Name = "Metrino.Otdr.Instrument",
            ProjectPath = Path.Combine("OtdrInstrument", "FF4", "Metrino.Otdr.Instrument", "Metrino.Otdr.Instrument.csproj"),
            SourcePath = Path.Combine("OtdrInstrument", "Metrino.Otdr.Instrument"),
            BinaryPath = Path.Combine("Library", "Metrino.Otdr.Instrument")
        },
        new BuildTag
        {
            Name = "Metrino.Olm",
            ProjectPath = Path.Combine("OlmBase", "FF4", "Metrino.Olm", "Metrino.Olm.csproj"),
            SourcePath = Path.Combine("OlmBase", "Metrino.Olm"),
            BinaryPath = Path.Combine("Library", "Metrino.Olm")
        },
        new BuildTag
        {
            Name = "Metrino.Olm.SignalProcessing",
            ProjectPath = Path.Combine("OlmBase", "FF4", "Metrino.Olm.SignalProcessing", "Metrino.Olm.SignalProcessing.csproj"),
            SourcePath = Path.Combine("OlmBase", "Metrino.Olm.SignalProcessing"),
            BinaryPath = Path.Combine("Library", "Metrino.Olm.SignalProcessing")
        },
        new BuildTag
        {
            Name = "Metrino.Olm.Instrument",
            ProjectPath = Path.Combine("OlmInstrument", "FF4", "Metrino.Olm.Instrument", "Metrino.Olm.Instrument.csproj"),
            SourcePath = Path.Combine("OlmInstrument", "Metrino.Olm.Instrument"),
            BinaryPath = Path.Combine("Library", "Metrino.Olm.Instrument")
        },
        new BuildTag
        {
            Name = "UnitTestOlm",
            ProjectPath = Path.Combine("UnitTestOlm", "UnitTestOlm.csproj"),
            SourcePath = Path.Combine("UnitTestOlm"),
            BinaryPath = Path.Combine("Library", "UnitTestOlm")
        }
    };

    public static string BuildOutputPath(string outputPath, string branch, string buildConfiguration)
        => Path.Combine(outputPath, branch.Replace("/", "-"), buildConfiguration, "Library");

    public static async Task<bool> BuildMetrino(string repositoryPath, string branch, string outputPath, string buildConfiguration = "Debug", string buildTarget = "x64", string[]? consoleLoggerParameters = null)
    {
        var repo = new Repository(repositoryPath);
        var currentBranch = repo.Head;

        var branchToBuild = repo.Branches[branch];

        try
        {
            if (branchToBuild != currentBranch)
                _ = Commands.Checkout(repo, branchToBuild);

            var outputTaggedPath = Path.Combine(outputPath, branch.Replace("/", "-"));

            try
            {
                foreach (var module in metrinoModules)
                {
                    Console.WriteLine(module.Name + " ");
                    var lastCommit = branchToBuild.Tip.Author.When.DateTime;
                    var dllBuildTime = new FileInfo(Path.Combine(outputTaggedPath, buildConfiguration, module.DllName)).LastWriteTime;

                    if (lastCommit > dllBuildTime)
                    {
                        await MSBuildProcess.RunProcessAsync(
                            projectFile: Path.Combine(repositoryPath, module.ProjectPath),
                            outputPath: Path.Combine(outputTaggedPath, buildConfiguration, module.BinaryPath),
                            buildConfiguration: buildConfiguration,
                            buildTarget: buildTarget,
                            dotnetTargetVersion: "net48",
                            consoleLoggerParameters: consoleLoggerParameters
                        );

                        return true;
                    }
                    else if (branchToBuild == currentBranch)
                    {
                        var needsReduild = false;
                        foreach (var file in Directory.EnumerateFiles(Path.Combine(repositoryPath, module.SourcePath), "*.cs", SearchOption.AllDirectories))
                        {
                            var fileAccessTime = new FileInfo(file).LastWriteTime;
                            if (dllBuildTime < fileAccessTime)
                            {
                                needsReduild = true;
                                break;
                            }
                        }

                        if (needsReduild)
                        {
                            await MSBuildProcess.RunProcessAsync(
                                projectFile: Path.Combine(repositoryPath, module.ProjectPath),
                                outputPath: Path.Combine(outputTaggedPath, buildConfiguration, module.BinaryPath),
                                buildConfiguration: buildConfiguration,
                                buildTarget: buildTarget,
                                dotnetTargetVersion: "net48",
                                consoleLoggerParameters: consoleLoggerParameters
                            );

                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }

                return false;
            }
            finally
            {
                Commands.Checkout(repo, currentBranch);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }

    }
}
