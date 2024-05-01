using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Messaging;
using Metrino.Development.Studio.Library.Messages;
using Metrino.Development.Studio.Library.ViewModels;
using Metrino.Development.UI.ViewModels;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;

namespace Metrino.Development.Studio.Library.Controls;

public class WorkspaceControl : TemplatedControl
{
    //public static readonly PseudoClass CustomPseudoClass = PseudoClass.Parse(":isempty");

    public static readonly StyledProperty<ObservableCollection<DocumentViewModelBase>> ItemsSourceProperty
        = AvaloniaProperty.Register<WorkspaceControl, ObservableCollection<DocumentViewModelBase>>(nameof(ItemsSource));

    public ObservableCollection<DocumentViewModelBase> ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly StyledProperty<int> SelectedIndexProperty
        = AvaloniaProperty.Register<WorkspaceControl, int>(nameof(SelectedIndex));

    public int SelectedIndex
    {
        get => GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public static readonly StyledProperty<string> SelectedPathProperty
        = AvaloniaProperty.Register<WorkspaceControl, string>(nameof(SelectedPath));

    public string SelectedPath
    {
        get => GetValue(SelectedPathProperty);
        set => SetValue(SelectedPathProperty, value);
    }

    public static readonly StyledProperty<DocumentViewModelBase?> SelectedDocumentProperty
        = AvaloniaProperty.Register<WorkspaceControl, DocumentViewModelBase?>(nameof(SelectedDocument));

    public DocumentViewModelBase? SelectedDocument
    {
        get => GetValue(SelectedDocumentProperty);
        set => SetValue(SelectedDocumentProperty, value);
    }

    public static readonly StyledProperty<DocumentViewModelBase?> OverlayContentProperty
        = AvaloniaProperty.Register<WorkspaceControl, DocumentViewModelBase?>(nameof(OverlayContent));

    public DocumentViewModelBase? OverlayContent
    {
        get => GetValue(OverlayContentProperty);
        set => SetValue(OverlayContentProperty, value);
    }


    public static readonly StyledProperty<ICommand> RunCurrentProperty
        = AvaloniaProperty.Register<WorkspaceControl, ICommand>(nameof(RunCurrent));

    public ICommand RunCurrent
    {
        get => GetValue(RunCurrentProperty);
        set => SetValue(RunCurrentProperty, value);
    }

    public static readonly StyledProperty<ICommand> DebugCurrentProperty
        = AvaloniaProperty.Register<WorkspaceControl, ICommand>(nameof(DebugCurrent));

    public ICommand DebugCurrent
    {
        get => GetValue(DebugCurrentProperty);
        set => SetValue(DebugCurrentProperty, value);
    }

    public static readonly StyledProperty<ICommand> SaveAllProperty
        = AvaloniaProperty.Register<WorkspaceControl, ICommand>(nameof(SaveAll));

    public ICommand SaveAll
    {
        get => GetValue(SaveAllProperty);
        set => SetValue(SaveAllProperty, value);
    }


    public static readonly StyledProperty<ICommand> SaveCurrentProperty
        = AvaloniaProperty.Register<WorkspaceControl, ICommand>(nameof(SaveCurrent));

    public ICommand SaveCurrent
    {
        get => GetValue(SaveCurrentProperty);
        set => SetValue(SaveCurrentProperty, value);
    }

    public static readonly StyledProperty<RepositoryManager> GitBranchesProperty
        = AvaloniaProperty.Register<WorkspaceControl, RepositoryManager>(nameof(GitBranches));

    public RepositoryManager GitBranches
    {
        get => GetValue(GitBranchesProperty);
        set => SetValue(GitBranchesProperty, value);
    }

    public static readonly StyledProperty<bool> ShowOverlayProperty
        = AvaloniaProperty.Register<WorkspaceControl, bool>(nameof(ShowOverlay));

    public bool ShowOverlay
    {
        get => GetValue(ShowOverlayProperty);
        set => SetValue(ShowOverlayProperty, value);
    }


    public WorkspaceControl()
    {
        AddHandler(DragDrop.DropEvent, OnDrop);

        ItemsSourceProperty.Changed.AddClassHandler<WorkspaceControl>(OnItemsSourcePropertyChanged);
        SelectedIndexProperty.Changed.AddClassHandler<WorkspaceControl>(OnSelectedIndexChanged);
    }

    private void OnItemsSourcePropertyChanged(WorkspaceControl control, AvaloniaPropertyChangedEventArgs args)
    {
        if (ItemsSource == null)
        {
            PseudoClasses.Add(":isempty");
            return;
        }

        if (ItemsSource.Count > 0)
        {
            SelectedIndex = 0;
        }
        else
        {
            PseudoClasses.Add(":isempty");
            ItemsSource.CollectionChanged += InitializeSelectedItem;
        }

        ItemsSource.CollectionChanged += OnItemsSourceCollectionChanged;

    }

    private void OnItemsSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Remove)
        {
        }

        PseudoClasses.Set(":isempty", ItemsSource.Count == 0);
    }

    private void InitializeSelectedItem(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            SelectedIndex = 0;

            SelectedPath = ItemsSource[SelectedIndex].Path;
            SelectedDocument = ItemsSource[SelectedIndex];
            ItemsSource.CollectionChanged -= InitializeSelectedItem;
        }
    }

    private void OnSelectedIndexChanged(WorkspaceControl control, AvaloniaPropertyChangedEventArgs args)
    {
        if ((SelectedIndex >= 0) && (SelectedIndex < ItemsSource.Count))
        {
            SelectedPath = ItemsSource[SelectedIndex].Path;
            SelectedDocument = ItemsSource[SelectedIndex];
        }
        else
        {
            SelectedPath = string.Empty;
            SelectedDocument = null;
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var documents = e.NameScope.Find<ContentControl>("Documents");
        foreach (var dataTemplate in DataTemplates)
            documents?.DataTemplates.Add(dataTemplate);

        var overlay0 = e.NameScope.Find<ContentControl>("Overlay0");
        foreach (var dataTemplate in DataTemplates)
            overlay0?.DataTemplates.Add(dataTemplate);

        var overlay1 = e.NameScope.Find<ContentControl>("Overlay1");
        foreach (var dataTemplate in DataTemplates)
            overlay1?.DataTemplates.Add(dataTemplate);
    }

    private static void OnDrop(object? sender, DragEventArgs e)
    {
        var items = e.Data.GetFileNames();
        if (items == null)
            return;

        foreach (var item in items)
            WeakReferenceMessenger.Default.Send(new OpenFileMessage(item));
    }
}