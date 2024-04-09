using System.Runtime.InteropServices;

namespace Metrino.Development.Core.TreeSitter;

[StructLayout(LayoutKind.Sequential)]
public struct TSNode
{
    private uint context0;
    private uint context1;
    private uint context2;
    private uint context3;
    public IntPtr id;
    private IntPtr tree;

    public void Clear()
    {
        id = IntPtr.Zero;
        tree = IntPtr.Zero;
    }

    public bool is_zero => (id == IntPtr.Zero && tree == IntPtr.Zero);

    public string Type() => Marshal.PtrToStringAnsi(c_api.ts_x_node_type(this));

    public string Type(TSLanguage lang) => lang.SymbolName(Symbol);

    public ushort Symbol => c_api.ts_x_node_symbol(this);

    public uint StartOffset => c_api.ts_x_node_start_byte(this) / sizeof(ushort);

    public TSPoint StartPoint
    {
        get
        {
            var pt = c_api.ts_x_node_start_point(this);
            return new TSPoint(pt.Row, pt.Column / sizeof(ushort));
        }
    }

    public uint EndOffset => c_api.ts_x_node_end_byte(this) / sizeof(ushort);

    public TSPoint EndPoint
    {
        get
        {
            var pt = c_api.ts_x_node_end_point(this);
            return new TSPoint(pt.Row, pt.Column / sizeof(ushort));
        }
    }

    public string ToString()
    {
        var dat = c_api.ts_x_node_string(this);
        var str = Marshal.PtrToStringAnsi(dat);
        c_api.ts_x_node_string_free(dat);

        return str;
    }

    public bool IsNull => c_api.ts_x_node_is_null(this);

    public bool IsNamed => c_api.ts_x_node_is_named(this);

    public bool IsMissing => c_api.ts_x_node_is_missing(this);

    public bool IsExtra => c_api.ts_x_node_is_extra(this);

    public bool HasChanges => c_api.ts_x_node_has_changes(this);

    public bool HasError => c_api.ts_x_node_has_error(this);

    public TSNode Parent => c_api.ts_x_node_parent(this);

    public TSNode Child(uint index)
        => c_api.ts_x_node_child(this, index);

    public IntPtr FieldNameForChild(uint index)
        => c_api.ts_x_node_field_name_for_child(this, index);

    public uint ChildCount
        => c_api.ts_x_node_child_count(this);

    public TSNode NamedChild(uint index)
        => c_api.ts_x_node_named_child(this, index);

    public uint NamesChildCount => c_api.ts_x_node_named_child_count(this);

    public TSNode ChildByFieldName(string field_name)
        => c_api.ts_x_node_child_by_field_name(this, field_name, (uint)field_name.Length);

    public TSNode ChildByFieldId(ushort fieldId)
        => c_api.ts_x_node_child_by_field_id(this, fieldId);

    public TSNode NextSibling()
        => c_api.ts_x_node_next_sibling(this);

    public TSNode PreviousSibling()
        => c_api.ts_x_node_prev_sibling(this);

    public TSNode NextNamedSibling()
        => c_api.ts_x_node_next_named_sibling(this);

    public TSNode PreviousNamedSibling()
        => c_api.ts_x_node_prev_named_sibling(this);

    public TSNode FirstChildForOffset(uint offset)
        => c_api.ts_x_node_first_child_for_byte(this, offset * sizeof(ushort));

    public TSNode FirstNamedChildForOffset(uint offset)
        => c_api.ts_x_node_first_named_child_for_byte(this, offset * sizeof(ushort));

    public TSNode DescendentForOffsetRange(uint start, uint end)
        => c_api.ts_x_node_descendant_for_byte_range(this, start * sizeof(ushort), end * sizeof(ushort));

    public TSNode DescendentForPointRange(TSPoint start, TSPoint end)
        => c_api.ts_x_node_descendant_for_point_range(this, start, end);

    public TSNode NamesDescendentForOffsetRange(uint start, uint end)
        => c_api.ts_x_node_named_descendant_for_byte_range(this, start * sizeof(ushort), end * sizeof(ushort));

    public TSNode NamesDescendentForPointRange(TSPoint start, TSPoint end)
        => c_api.ts_x_node_named_descendant_for_point_range(this, start, end);

    public bool Eq(TSNode other)
        => c_api.ts_x_node_eq(this, other);


    public string Text(string data)
    {
        uint beg = StartOffset;
        uint end = EndOffset;

        return data.Substring((int)beg, (int)(end - beg));
    }

}
