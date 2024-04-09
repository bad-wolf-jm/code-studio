using System.IO;
using System.Threading.Tasks;

namespace Metrino.Development.Builder;

public class ToolBuilder
{
    public string RepositoryPath { get; set; } = string.Empty;
    public string ToolSourcePath { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;

    public async Task Build(string branch, string[]? consoleLoggerParameters = null)
    {
        consoleLoggerParameters = consoleLoggerParameters ?? new string[] { "Summary", "ErrorsOnly" };

        var exeOutputPath = Path.Combine(OutputPath, branch.Replace("/", "-"));
        var toolNeedsRebuilding = await MetrinoBuilder.BuildMetrino(RepositoryPath, branch, OutputPath, consoleLoggerParameters: consoleLoggerParameters);

        var metrinoBinaryPath = MetrinoBuilder.BuildOutputPath(OutputPath, branch, "Debug");

        await ProjectBase.BuildScriptingEngine(metrinoBinaryPath, ToolSourcePath, exeOutputPath, "Debug", toolNeedsRebuilding);
        await ProjectBase.BuildService(metrinoBinaryPath, ToolSourcePath, exeOutputPath, "Debug", toolNeedsRebuilding);
    }
}
