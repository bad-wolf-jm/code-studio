using System.Runtime.InteropServices;

namespace Metrino.Development.Core.TreeSitter;

public sealed class TSCursor : IDisposable
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TSTreeCursor
    {
        private IntPtr Tree;
        private IntPtr Id;
        private uint Context0;
        private uint Context1;
    }


    private IntPtr Ptr;
    private TSTreeCursor cursor;
    public TSLanguage Lang { get; private set; }

    public TSCursor(TSTreeCursor cursor, TSLanguage lang)
    {
        this.cursor = cursor;
        Lang = lang;
        Ptr = new IntPtr(1);
    }

    public TSCursor(TSNode node, TSLanguage lang)
    {
        cursor = c_api.ts_x_tree_cursor_new(node);
        Lang = lang;
        Ptr = new IntPtr(1);
    }

    public void Dispose()
    {
        if (Ptr != IntPtr.Zero)
        {
            c_api.ts_x_tree_cursor_delete(ref cursor);
            Ptr = IntPtr.Zero;
        }
    }

    public void Reset(TSNode node) => c_api.ts_x_tree_cursor_reset(ref cursor, node);

    public TSNode CurrentNode => c_api.ts_x_tree_cursor_current_node(ref cursor);

    public string CurrentField => Lang.Fields[CurrentFieldId];

    public string CurrentSymbol
    {
        get
        {
            ushort symbol = c_api.ts_x_tree_cursor_current_node(ref cursor).Symbol;
            return (symbol != ushort.MaxValue) ? Lang.Symbols[symbol] : "ERROR";
        }
    }

    public ushort CurrentFieldId => c_api.ts_x_tree_cursor_current_field_id(ref cursor);

    public bool GotoParent() => c_api.ts_x_tree_cursor_goto_parent(ref cursor);

    public bool GotoNextSibling() 
        => c_api.ts_x_tree_cursor_goto_next_sibling(ref cursor);

    public bool GotoFirstChild() 
        => c_api.ts_x_tree_cursor_goto_first_child(ref cursor);

    public long GotoFirstChildForOffset(uint offset) 
        => c_api.ts_x_tree_cursor_goto_first_child_for_byte(ref cursor, offset * sizeof(ushort));

    public long GotoCirstChildForPoinnt(TSPoint point) 
        => c_api.ts_x_tree_cursor_goto_first_child_for_point(ref cursor, point);

    public TSCursor copy() 
        => new TSCursor(c_api.ts_x_tree_cursor_copy(ref cursor), Lang);

}
