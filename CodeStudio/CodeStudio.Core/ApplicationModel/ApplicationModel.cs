//using LiteDB;
using Metrino.Development.UI.ViewModels;
//using Microsoft.AspNetCore.Mvc.ApplicationModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Metrino.Development.UI.Core;

public class ApplicationModel
{
    private Configuration _configuration = new Configuration();
    public Configuration Configuration { get { return _configuration; } }

    private FileSystem _fileSystem = new FileSystem();
    public FileSystem FileSystem { get { return _fileSystem; } }

    //private ILiteDatabase _database;
    //public ILiteDatabase Database { get { return _database; } }

    IBackendServiceProvider _serviceProvider;
    public IBackendServiceProvider ServiceProvider { get { return _serviceProvider; } }

    //IUnitTestDataFileCollection _unitTestDataFileCollection;
    //public IUnitTestDataFileCollection UnitTestDataFileCollection { get { return _unitTestDataFileCollection; } }

    RepositoryManager _gitBranches;
    public RepositoryManager GitBranches { get => _gitBranches; }

    public void Initialize(string configurationPath, string dataPath)
    {
        if (!File.Exists(configurationPath))
        {
            var defaultConfig = new Configuration
            {
                OtdrRepositoryPath = @"D:\Work\Git\OTDR",
                OtdrBuildPath = @"D:\Work\Build\Lib",
                OtdrBuildConfiguration = @"Debug",
                OtdrTargetFramework = @"net48",
                LocalTestDataBank = @"D:\Work\Data\AutomatedTests",
                RemoteTestDataBank = @"\\exfo.com\OpticMeasurement\OTDR\iOLM\AutomatedTests",
                UnitTestXmlRoot = @"D:\Work\Git\OTDR\UnitTestOlm\TestData",
                WorkspaceFolders = new List<string>()
            };
            defaultConfig.WriteTo(configurationPath);
        }

        _configuration.ReadFrom(configurationPath);

        foreach (var folder in _configuration.WorkspaceFolders)
            _fileSystem.AddRootFolder(folder);

        //_serviceProvider = new BackendServiceProvider(_configuration.OtdrBuildPath, _configuration.OtdrBuildConfiguration, _configuration.OtdrTargetFramework);
        //_database = new LiteDatabase(Path.Combine(dataPath, "database.db"));
        //_unitTestDataFileCollection = new UnitTestDataFileCollection(this, _database);

        _gitBranches = new RepositoryManager(Configuration.OtdrRepositoryPath);
    }

    public void Shutdown(string configurationPath)
    {
        _gitBranches.Dispose();

        _configuration.WorkspaceFolders = _fileSystem.RootFolders
            .Select(x => (x.Data as FileSystemNodeData)).Where(x => x != null)
            .Select(x => x.FullPath).ToList();

        _configuration.WriteTo(configurationPath);
    }

}
