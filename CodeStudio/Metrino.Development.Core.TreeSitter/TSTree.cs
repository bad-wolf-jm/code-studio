namespace Metrino.Development.Core.TreeSitter;

public sealed class TSTree : IDisposable
{
    internal IntPtr Ptr { get; private set; }

    public TSTree(IntPtr ptr)
    {
        Ptr = ptr;
    }

    public void Dispose()
    {
        if (Ptr != IntPtr.Zero)
        {
            c_api.ts_x_tree_delete(Ptr);
            Ptr = IntPtr.Zero;
        }
    }

    public TSTree Copy()
    {
        var ptr = c_api.ts_x_tree_copy(Ptr);

        return ptr != IntPtr.Zero ? new TSTree(ptr) : null;
    }
    public TSNode RootNode => c_api.ts_x_tree_root_node(Ptr);

    public TSNode RootNodeWithOffset(uint offsetBytes, TSPoint offsetPoint) 
        => c_api.ts_x_tree_root_node_with_offset(Ptr, offsetBytes, offsetPoint);

    public TSLanguage Language
    {
        get
        {
            var ptr = c_api.ts_x_tree_language(Ptr);
            return ptr != IntPtr.Zero ? new TSLanguage(ptr) : null;
        }
    }

    public void Edit(TSInputEdit edit) 
        => c_api.ts_x_tree_edit(Ptr, ref edit);
}
