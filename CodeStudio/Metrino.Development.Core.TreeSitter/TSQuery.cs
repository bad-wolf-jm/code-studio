using System.Runtime.InteropServices;

namespace Metrino.Development.Core.TreeSitter;

public sealed class TSQuery : IDisposable
{
    internal IntPtr Ptr { get; private set; }

    public TSQuery(IntPtr ptr)
    {
        Ptr = ptr;
    }

    public void Dispose()
    {
        if (Ptr != IntPtr.Zero)
        {
            c_api.ts_x_query_delete(Ptr);
            Ptr = IntPtr.Zero;
        }
    }

    public uint PatternCount => c_api.ts_x_query_pattern_count(Ptr);
    public uint CaptureCount => c_api.ts_x_query_capture_count(Ptr);
    public uint StringCount => c_api.ts_x_query_string_count(Ptr);

    public uint StartOffsetForPattern(uint patternIndex) 
        => c_api.ts_x_query_start_byte_for_pattern(Ptr, patternIndex) / sizeof(ushort);

    public IntPtr PredicateForPattern(uint patternIndex, out uint length) 
        => c_api.ts_x_query_predicates_for_pattern(Ptr, patternIndex, out length);

    public bool IsPatternRooted(uint patternIndex) 
        => c_api.ts_x_query_is_pattern_rooted(Ptr, patternIndex);

    public bool IsPatternNonLocal(uint patternIndex) 
        => c_api.ts_x_query_is_pattern_non_local(Ptr, patternIndex);

    public bool IsPatternGuaranteedAtOffset(uint offset) 
        => c_api.ts_x_query_is_pattern_guaranteed_at_step(Ptr, offset / sizeof(ushort));

    public string CaptureNameForId(uint id, out uint length) 
        => Marshal.PtrToStringAnsi(c_api.ts_x_query_capture_name_for_id(Ptr, id, out length));

    public ETSQuantifier CaptureQuantifierForId(uint patternId, uint captureId) 
        => c_api.ts_x_query_capture_quantifier_for_id(Ptr, patternId, captureId);

    public string StringValueForId(uint id, out uint length) 
        => Marshal.PtrToStringAnsi(c_api.ts_x_query_string_value_for_id(Ptr, id, out length));

    public void DisableCapture(string captureName) 
        => c_api.ts_x_query_disable_capture(Ptr, captureName, (uint)captureName.Length);

    public void DisablePattern(uint patternIndex) 
        => c_api.ts_x_query_disable_pattern(Ptr, patternIndex);

}
