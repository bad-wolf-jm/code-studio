using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;
using AvaloniaEdit.Utils;

namespace Metrino.Development.Studio.Library.Controls;

public class CompletionListItems : TemplatedControl
{
    public CompletionListItems()
    {
        DoubleTapped += OnDoubleTapped;

        CompletionAcceptKeys = new[]
        {
            Key.Enter,
            Key.Tab,
        };
    }


    public bool IsFiltering { get; set; } = true;

    public static readonly StyledProperty<ControlTemplate> EmptyTemplateProperty =
        AvaloniaProperty.Register<CompletionListItems, ControlTemplate>(nameof(EmptyTemplate));

    public ControlTemplate EmptyTemplate
    {
        get => GetValue(EmptyTemplateProperty);
        set => SetValue(EmptyTemplateProperty, value);
    }

    public event EventHandler InsertionRequested;

    public void RequestInsertion(EventArgs e)
    {
        InsertionRequested?.Invoke(this, e);
    }

    private CompletionListBox _listBox;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var list = e.NameScope.Find("PART_ListBox");
        _listBox = e.NameScope.Find("PART_ListBox") as CompletionListBox;
        if (_listBox != null)
        {
            _listBox.ItemsSource = _completionData;
        }
    }

    public CompletionListBox ListBox
    {
        get
        {
            if (_listBox == null)
                ApplyTemplate();

            return _listBox;
        }
    }

    public Key[] CompletionAcceptKeys { get; set; }

    public ScrollViewer ScrollViewer => _listBox?.ScrollViewer;

    private readonly ObservableCollection<ICompletionData> _completionData = new ObservableCollection<ICompletionData>();

    public IList<ICompletionData> CompletionData => _completionData;

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!e.Handled)
        {
            HandleKey(e);
        }
    }

    public void HandleKey(KeyEventArgs e)
    {
        if (_listBox == null)
            return;

        // We have to do some key handling manually, because the default doesn't work with
        // our simulated events.
        // Also, the default PageUp/PageDown implementation changes the focus, so we avoid it.
        switch (e.Key)
        {
            case Key.Down:
                e.Handled = true;
                _listBox.SelectIndex(_listBox.SelectedIndex + 1);
                break;
            case Key.Up:
                e.Handled = true;
                _listBox.SelectIndex(_listBox.SelectedIndex - 1);
                break;
            case Key.PageDown:
                e.Handled = true;
                _listBox.SelectIndex(_listBox.SelectedIndex + _listBox.VisibleItemCount);
                break;
            case Key.PageUp:
                e.Handled = true;
                _listBox.SelectIndex(_listBox.SelectedIndex - _listBox.VisibleItemCount);
                break;
            case Key.Home:
                e.Handled = true;
                _listBox.SelectIndex(0);
                break;
            case Key.End:
                e.Handled = true;
                _listBox.SelectIndex(_listBox.ItemCount - 1);
                break;
            default:
                if (CompletionAcceptKeys.Contains(e.Key) && CurrentList.Count > 0)
                {
                    e.Handled = true;
                    RequestInsertion(e);
                }

                break;
        }
    }

    protected void OnDoubleTapped(object sender, RoutedEventArgs e)
    {
        //TODO TEST
        if (((AvaloniaObject)e.Source).VisualAncestorsAndSelf()
                .TakeWhile(obj => obj != this).Any(obj => obj is ListBoxItem))
        {
            e.Handled = true;
            RequestInsertion(e);
        }
    }

    public ICompletionData SelectedItem
    {
        get => _listBox?.SelectedItem as ICompletionData;
        set
        {
            if (_listBox == null && value != null)
                ApplyTemplate();

            if (_listBox != null) // may still be null if ApplyTemplate fails, or if listBox and value both are null
                _listBox.SelectedItem = value;
        }
    }

    public void ScrollIntoView(ICompletionData item)
    {
        if (_listBox == null)
            ApplyTemplate();

        _listBox?.ScrollIntoView(item);
    }

    public event EventHandler<SelectionChangedEventArgs> SelectionChanged
    {
        add => AddHandler(SelectingItemsControl.SelectionChangedEvent, value);
        remove => RemoveHandler(SelectingItemsControl.SelectionChangedEvent, value);
    }

    private string _currentText;

    private ObservableCollection<ICompletionData> _currentList;

    public List<ICompletionData> CurrentList
    {
        get => ListBox.Items.Cast<ICompletionData>().ToList();
    }

    public void SelectItem(string text)
    {
        if (text == _currentText)
            return;

        if (_listBox == null)
            ApplyTemplate();

        if (IsFiltering)
        {
            SelectItemFiltering(text);
        }
        else
        {
            SelectItemWithStart(text);
        }
        _currentText = text;
    }

    private void SelectItemFiltering(string query)
    {
        // if the user just typed one more character, don't filter all data but just filter what we are already displaying
        var listToFilter = _currentList != null && !string.IsNullOrEmpty(_currentText) && !string.IsNullOrEmpty(query) &&
                           query.StartsWith(_currentText, StringComparison.Ordinal) ?
            _currentList : _completionData;

        var matchingItems =
            from item in listToFilter
            let quality = GetMatchQuality(item.Name, query)
            where quality > 0
            select new { Item = item, Quality = quality };

        // e.g. "DateTimeKind k = (*cc here suggests DateTimeKind*)"
        var suggestedItem = _listBox.SelectedIndex != -1 ? (ICompletionData)_listBox.SelectedItem : null;

        var listBoxItems = new ObservableCollection<ICompletionData>();
        var bestIndex = -1;
        var bestQuality = -1;
        double bestPriority = 0;
        var i = 0;
        foreach (var matchingItem in matchingItems)
        {
            var priority = matchingItem.Item == suggestedItem ? double.PositiveInfinity : 0;
            var quality = matchingItem.Quality;
            if (quality > bestQuality || quality == bestQuality && priority > bestPriority)
            {
                bestIndex = i;
                bestPriority = priority;
                bestQuality = quality;
            }
            listBoxItems.Add(matchingItem.Item);
            i++;
        }

        _currentList = listBoxItems;
        //_listBox.Items = null; Makes no sense? Tooltip disappeared because of this
        _listBox.ItemsSource = listBoxItems;
        SelectIndexCentered(bestIndex);
    }

    private void SelectItemWithStart(string query)
    {
        if (string.IsNullOrEmpty(query))
            return;

        var suggestedIndex = _listBox.SelectedIndex;

        var bestIndex = -1;
        var bestQuality = -1;
        double bestPriority = 0;
        for (var i = 0; i < _completionData.Count; ++i)
        {
            var quality = GetMatchQuality(_completionData[i].Name, query);
            if (quality < 0)
                continue;

            var priority =0;
            bool useThisItem;
            if (bestQuality < quality)
            {
                useThisItem = true;
            }
            else
            {
                if (bestIndex == suggestedIndex)
                {
                    useThisItem = false;
                }
                else if (i == suggestedIndex)
                {
                    // prefer recommendedItem, regardless of its priority
                    useThisItem = bestQuality == quality;
                }
                else
                {
                    useThisItem = bestQuality == quality && bestPriority < priority;
                }
            }
            if (useThisItem)
            {
                bestIndex = i;
                bestPriority = priority;
                bestQuality = quality;
            }
        }
        SelectIndexCentered(bestIndex);
    }

    private void SelectIndexCentered(int bestIndex)
    {
        if (bestIndex < 0)
        {
            _listBox.ClearSelection();
        }
        else
        {
            var firstItem = _listBox.FirstVisibleItem;
            if (bestIndex < firstItem || firstItem + _listBox.VisibleItemCount <= bestIndex)
            {
                // CenterViewOn does nothing as CompletionListItems.ScrollViewer is null
                _listBox.CenterViewOn(bestIndex);
                _listBox.SelectIndex(bestIndex);
            }
            else
            {
                _listBox.SelectIndex(bestIndex);
            }
        }
    }

    private int GetMatchQuality(string itemText, string query)
    {
        if (itemText == null)
            throw new ArgumentNullException(nameof(itemText), "ICompletionData.Text returned null");

        // Qualities:
        //  	8 = full match case sensitive
        // 		7 = full match
        // 		6 = match start case sensitive
        //		5 = match start
        //		4 = match CamelCase when length of query is 1 or 2 characters
        // 		3 = match substring case sensitive
        //		2 = match substring
        //		1 = match CamelCase
        //		-1 = no match
        if (query == itemText)
            return 8;
        if (string.Equals(itemText, query, StringComparison.CurrentCultureIgnoreCase))
            return 7;

        if (itemText.StartsWith(query, StringComparison.CurrentCulture))
            return 6;
        if (itemText.StartsWith(query, StringComparison.CurrentCultureIgnoreCase))
            return 5;

        bool? camelCaseMatch = null;
        if (query.Length <= 2)
        {
            camelCaseMatch = CamelCaseMatch(itemText, query);
            if (camelCaseMatch == true) return 4;
        }

        // search by substring, if filtering (i.e. new behavior) turned on
        if (IsFiltering)
        {
            if (itemText.IndexOf(query, StringComparison.CurrentCulture) >= 0)
                return 3;
            if (itemText.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0)
                return 2;
        }

        if (!camelCaseMatch.HasValue)
            camelCaseMatch = CamelCaseMatch(itemText, query);
        if (camelCaseMatch == true)
            return 1;

        return -1;
    }

    private static bool CamelCaseMatch(string text, string query)
    {
        // We take the first letter of the text regardless of whether or not it's upper case so we match
        // against camelCase text as well as PascalCase text ("cct" matches "camelCaseText")
        var theFirstLetterOfEachWord = text.AsEnumerable()
            .Take(1)
            .Concat(text.AsEnumerable().Skip(1).Where(char.IsUpper));

        var i = 0;
        foreach (var letter in theFirstLetterOfEachWord)
        {
            if (i > query.Length - 1)
                return true;    // return true here for CamelCase partial match ("CQ" matches "CodeQualityAnalysis")
            if (char.ToUpperInvariant(query[i]) != char.ToUpperInvariant(letter))
                return false;
            i++;
        }
        if (i >= query.Length)
            return true;
        return false;
    }
}