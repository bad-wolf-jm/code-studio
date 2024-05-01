using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Metrino.Development.Core.TreeSitter;

public sealed class TSQueryCursor : IDisposable
{
    private IntPtr Ptr { get; set; }

    private TSQueryCursor(IntPtr ptr)
    {
        Ptr = ptr;
    }

    public TSQueryCursor()
    {
        Ptr = c_api.ts_x_query_cursor_new();
    }

    public void Dispose()
    {
        if (Ptr != IntPtr.Zero)
        {
            c_api.ts_x_query_cursor_delete(Ptr);
            Ptr = IntPtr.Zero;
        }
    }

    public void Execute(TSQuery query, TSNode node)
        => c_api.ts_x_query_cursor_exec(Ptr, query.Ptr, node);

    public bool DidExceedMatchLimit => c_api.ts_x_query_cursor_did_exceed_match_limit(Ptr);

    public uint MatchLimit
    {
        get { return c_api.ts_x_query_cursor_match_limit(Ptr); }
        set { c_api.ts_x_query_cursor_set_match_limit(Ptr, value); }
    }

    public void SetRange(uint start, uint end)
        => c_api.ts_x_query_cursor_set_byte_range(Ptr, start * sizeof(ushort), end * sizeof(ushort));

    public void SetPointRange(TSPoint start, TSPoint end)
        => c_api.ts_x_query_cursor_set_point_range(Ptr, start, end);

    public bool NextMatch(out TSQueryMatch match, out TSQueryCapture[] captures)
    {
        captures = null;
        if (c_api.ts_x_query_cursor_next_match(Ptr, out match))
        {
            if (match.CaptureCount > 0)
            {
                captures = new TSQueryCapture[match.CaptureCount];
                for (ushort i = 0; i < match.CaptureCount; i++)
                {
                    var intPtr = match.Captures + Marshal.SizeOf(typeof(TSQueryCapture)) * i;
                    captures[i] = Marshal.PtrToStructure<TSQueryCapture>(intPtr);
                }
            }
            return true;
        }
        return false;
    }

    public void RemoveMatch(uint id)
        => c_api.ts_x_query_cursor_remove_match(Ptr, id);

    public bool NextCapture(out TSQueryMatch match, out uint index) 
        => c_api.ts_x_query_cursor_next_capture(Ptr, out match, out index);

}
