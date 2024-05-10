using Metrino.Development.Studio.Library.ViewModels;
using Metrino.Development.UI.Core;
using Metrino.Development.UI.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using Metrino.Development.Studio.Library.Controls;

namespace Metrino.Development.Studio.Test.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject, IDisposable
    {
        [ObservableProperty]
        CodeDocumentViewModel _testCode;// => new CodeDocumentViewModel(@"D:\Work\Projects\Relax\UnitTesting\consolidation_unit_tests.lua");
        private IBackendServiceProvider _serviceProvider;
        private ApplicationModel applicationModel;

        public WorkspaceViewModel Workspace { get; set; }

        //public OlmDocumentViewModel OlmDocument {  get; set; }
        //public OtdrDocumentViewModel OtdrDocument { get; set; }

        //public UnitTestDocumentViewModel UnitTests { get; set; }
        //public DataFilesViewModel UnitTestsDataFiles { get; set; }

        public CodeDocument Code { get; private set; }
        public FileSystem _fileSystem { get; set; }
        //public Library.ViewModels.FileTreeViewModel TestTreeView { get; set; }

        public ObservableCollection<ObservableObject> WorkspaceItems { get; set; }
        public ObservableCollection<DocumentViewModelBase> Documents => Workspace.Documents;

        public MainWindowViewModel(ApplicationModel applicationModel) : base()
        {
            this.applicationModel = applicationModel;
            _fileSystem = new FileSystem();
            _fileSystem.AddRootFolder("D:\\Work");
            _fileSystem.AddRootFolder("D:\\Personal");
            _fileSystem.AddRootFolder("D:\\cmder");
            _fileSystem.AddRootFolder("D:\\Programs");
            
            //TestTreeView =  new Library.ViewModels.FileTreeViewModel(_fileSystem);
            Workspace = new WorkspaceViewModel(applicationModel);

            //UnitTests = new UnitTestDocumentViewModel(applicationModel);

            //UnitTestsDataFiles = new DataFilesViewModel(applicationModel);

            //WorkspaceItems = new ObservableCollection<ObservableObject>();
        }

        internal async Task Initialize()
        {
            TestCode = new CodeDocumentViewModel(applicationModel, @"D:\Work\Projects\Relax\UnitTesting\consolidation_unit_tests_test.lua");

            //await UnitTestsDataFiles.Initialize();
            //await TestCode.LoadPathAsync(@"D:\Work\Projects\Relax\UnitTesting\consolidation_unit_tests_test.lua", "develop");
            await TestCode.LoadPathAsync(@"D:\Work\Git\OTDRMonoGlue\Source\TheTool\Metrino.Development.Studio.Test\Assets\lua_parser_test.lua", "develop");
            Code = new CodeDocument();
            Code.Load(@"D:\Work\Git\code-studio\CodeStudio\CodeStudio.Test\Assets\lua_parser_test.lua");
            //Code.Load(@"D:\Work\Projects\Relax\StartOffset\test_start_offset.lua");
        }

        public void Dispose()
        {
            //UnitTestsDataFiles.Dispose();
            TestCode.Dispose();
        }
    }
}