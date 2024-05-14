using CommunityToolkit.Mvvm.ComponentModel;
using Metrino.Development.Core;
using Metrino.Development.UI.Core;
using System.Collections.ObjectModel;

namespace CodeStudio.Core;

public interface IFilterable
{
    string SearchKey { get; }
}

public class FilterListItem<T>
{
    public T Value { get; set; }
    public List<TextSpan> TextSpans { get; set; }
}

public partial class FuzzyFilter<T> : ObservableObject where T : IFilterable, new()
{
    [ObservableProperty]
    string _filterText = string.Empty;

    [ObservableProperty]
    ObservableCollection<FilterListItem<T>> _filteredList = new ObservableCollection<FilterListItem<T>>();

    ObservableCollection<T> _list = new ObservableCollection<T>();
    public IEnumerable<T> List => _list;

    public FuzzyFilter()
    {
        PropertyChanged += FuzzyFilter_PropertyChanged;
    }

    private void FuzzyFilter_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FilterText))
        {
            FilteredList.Clear();
            if (FilterText == "")
            {
                foreach (var entry in List)
                    FilteredList.Add(new FilterListItem<T> { Value = entry });

                return;
            }

            var list = List.Where(x => FuzzySearch.HasMatch(FilterText, x.SearchKey));
            var filterResults = new List<Tuple<double, T, List<TextSpan>>>();
            foreach (var element in List)
            {
                int[] positions = new int[element.SearchKey.Length];
                var score = FuzzySearch.MatchPositions(FilterText, element.SearchKey, positions);
                if (score > 0.0)
                {
                    var textSpans = FuzzySearch.PositionsToSpans(element.SearchKey, positions);
                    filterResults.Add(new Tuple<double, T, List<TextSpan>>(score, element, textSpans));
                }
            }

            var orderedFilteredResults = filterResults.OrderBy(x => x.Item1).Reverse();

            foreach (var x in orderedFilteredResults)
                FilteredList.Add(new FilterListItem<T> { Value = x.Item2, TextSpans = x.Item3 });
        }
    }
}
