using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using AvaloniaEdit.Utils;

namespace Metrino.Development.Studio.Library.Controls;

public class CompletionListBox : ListBox
{
    internal ScrollViewer ScrollViewer;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ScrollViewer = e.NameScope.Find("PART_ScrollViewer") as ScrollViewer;
    }

    public int FirstVisibleItem
    {
        get
        {
            if (ScrollViewer == null || ScrollViewer.Extent.Height == 0)
            {
                return 0;
            }

            return (int)(ItemCount * ScrollViewer.Offset.Y / ScrollViewer.Extent.Height);
        }
        set
        {
            value = value.CoerceValue(0, ItemCount - VisibleItemCount);
            if (ScrollViewer != null)
            {
                ScrollViewer.Offset = ScrollViewer.Offset.WithY((double)value / ItemCount * ScrollViewer.Extent.Height);
            }
        }
    }

    public int VisibleItemCount
    {
        get
        {
            if (ScrollViewer == null || ScrollViewer.Extent.Height == 0)
            {
                return 10;
            }
            return Math.Max(
                3,
                (int)Math.Ceiling(ItemCount * ScrollViewer.Viewport.Height
                                  / ScrollViewer.Extent.Height));
        }
    }

    public void ClearSelection()
    {
        SelectedIndex = -1;
    }

    public void SelectIndex(int index)
    {
        if (index >= ItemCount)
            index = ItemCount - 1;
        if (index < 0)
            index = 0;
        SelectedIndex = index;
        ScrollIntoView(SelectedItem);
    }

    public void CenterViewOn(int index)
    {
        FirstVisibleItem = index - VisibleItemCount / 2;
    }
}
