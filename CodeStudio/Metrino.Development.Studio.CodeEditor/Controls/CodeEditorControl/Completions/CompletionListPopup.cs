using System;
using Avalonia;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using Avalonia.Controls;
using Avalonia.Input;

namespace Metrino.Development.Studio.Library.Controls;

public class CompletionListPopup : TextAreaPopup
{
    private PopupWithCustomPosition _toolTip;
    private CompletionTipContentControl _toolTipContent;

    public CompletionListItems CompletionList { get; }

    public CompletionListPopup(TextArea textArea) : base(textArea)
    {
        CompletionList = new CompletionListItems();
        // keep height automatic
        CloseAutomatically = true;
        MaxHeight = 225;
        Width = 375;
        Child = CompletionList;
        // prevent user from resizing window to 0x0
        MinHeight = 15;
        MinWidth = 30;

        _toolTipContent = new CompletionTipContentControl();

        _toolTip = new PopupWithCustomPosition
        {
            IsLightDismissEnabled = true,
            PlacementTarget = this,
            Child = _toolTipContent,
        };

        LogicalChildren.Add(_toolTip);

        //_toolTip.Closed += (o, e) => ((Popup)o).Child = null;

        AttachEvents();
    }

    protected override void OnClosed()
    {
        base.OnClosed();

        if (_toolTip != null)
        {
            _toolTip.IsOpen = false;
            _toolTip = null;
            _toolTipContent = null;
        }
    }

    #region ToolTip handling

    private void CompletionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_toolTipContent == null) return;

        var item = CompletionList.SelectedItem;
        var description = item?.Description;


        if (description != null && Host is Control placementTarget && CompletionList.CurrentList != null)
        {
            _toolTipContent.Content = description;

            double yOffset = 0;
            var selectedIndex = CompletionList.ListBox.SelectedIndex;

            var itemContainer = CompletionList.ListBox.ContainerFromIndex(selectedIndex);

            if (itemContainer != null)
            {
                _toolTip.Placement = PlacementMode.RightEdgeAlignedTop;
                var position = itemContainer.TranslatePoint(new Point(0, 0), placementTarget);
                if (position.HasValue) yOffset = position.Value.Y;
            }
            else
            {
                //When scrolling down the container is not always ready
                //If that happens we align the tooltip at the bottom or top
                if (CompletionList.ListBox.FirstVisibleItem < selectedIndex)
                {
                    _toolTip.Placement = PlacementMode.RightEdgeAlignedBottom;
                }
                else
                {
                    _toolTip.Placement = PlacementMode.RightEdgeAlignedTop;
                }
            }

            _toolTip.Offset = new Point(2, yOffset);
            _toolTip.PlacementTarget = placementTarget;
            _toolTip.IsOpen = true;
        }
        else
        {
            _toolTip.IsOpen = false;
        }
    }

    #endregion

    private void CompletionList_InsertionRequested(object sender, EventArgs e)
    {
        Hide();
        // The window must close before Complete() is called.
        // If the Complete callback pushes stacked input handlers, we don't want to pop those when the CC window closes.
        var item = CompletionList.SelectedItem;
        item?.Complete(TextArea, new AnchorSegment(TextArea.Document, StartOffset, EndOffset - StartOffset), e);
    }

    private void AttachEvents()
    {
        CompletionList.InsertionRequested += CompletionList_InsertionRequested;
        CompletionList.SelectionChanged += CompletionList_SelectionChanged;
        TextArea.Caret.PositionChanged += CaretPositionChanged;
        TextArea.PointerWheelChanged += TextArea_MouseWheel;
        TextArea.TextInput += TextArea_PreviewTextInput;
    }

    protected override void DetachEvents()
    {
        CompletionList.InsertionRequested -= CompletionList_InsertionRequested;
        CompletionList.SelectionChanged -= CompletionList_SelectionChanged;
        TextArea.Caret.PositionChanged -= CaretPositionChanged;
        TextArea.PointerWheelChanged -= TextArea_MouseWheel;
        TextArea.TextInput -= TextArea_PreviewTextInput;
        base.DetachEvents();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!e.Handled)
        {
            CompletionList.HandleKey(e);
        }
    }

    private void TextArea_PreviewTextInput(object sender, TextInputEventArgs e)
    {
        e.Handled = RaiseEventPair(this, null, TextInputEvent,
                                   new TextInputEventArgs { Text = e.Text });
    }

    private void TextArea_MouseWheel(object sender, PointerWheelEventArgs e)
    {
        e.Handled = RaiseEventPair(GetScrollEventTarget(),
                                   null, PointerWheelChangedEvent, e);
    }

    private Control GetScrollEventTarget()
    {
        if (CompletionList == null)
            return this;
        return CompletionList.ScrollViewer ?? CompletionList.ListBox ?? (Control)CompletionList;
    }

    public bool CloseAutomatically { get; set; }

    protected override bool CloseOnFocusLost => CloseAutomatically;

    public bool CloseWhenCaretAtBeginning { get; set; }

    private void CaretPositionChanged(object sender, EventArgs e)
    {
        var offset = TextArea.Caret.Offset;
        if (offset == StartOffset)
        {
            if (CloseAutomatically && CloseWhenCaretAtBeginning)
            {
                Hide();
            }
            else
            {
                CompletionList.SelectItem(string.Empty);

                if (CompletionList.ListBox.ItemCount == 0) IsVisible = false;
                else IsVisible = true;
            }
            return;
        }
        if (offset < StartOffset || offset > EndOffset)
        {
            if (CloseAutomatically)
            {
                Hide();
            }
        }
        else
        {
            var document = TextArea.Document;
            if (document != null)
            {
                CompletionList.SelectItem(document.GetText(StartOffset, offset - StartOffset));

                if (CompletionList.ListBox.ItemCount == 0) IsVisible = false;
                else IsVisible = true;
            }
        }
    }
}