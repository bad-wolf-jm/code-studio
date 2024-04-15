namespace Metrino.Development.Core.TreeSitter;

public sealed class TSParser : IDisposable
{
    private IntPtr Ptr { get; set; }

    public TSParser()
    {
        Ptr = c_api.ts_x_parser_new();
    }

    public static TSParser FromLanguageName(string language)
    {
        var parser = new TSParser();
        parser.Setlanguage(TSLanguage.FromLanguageName(language));

        return parser;
    }

    public void Dispose()
    {
        if (Ptr != IntPtr.Zero)
        {
            c_api.ts_x_parser_delete(Ptr);
            Ptr = IntPtr.Zero;
        }
    }

    public bool Setlanguage(TSLanguage language) 
        => c_api.ts_x_parser_set_language(Ptr, language.Ptr);

    public TSLanguage Language()
    {
        var ptr = c_api.ts_x_parser_language(Ptr);
        return ptr != IntPtr.Zero ? new TSLanguage(ptr) : null;
    }

    public bool SetIncludedRanges(TSRange[] ranges) 
        => c_api.ts_x_parser_set_included_ranges(Ptr, ranges, (uint)ranges.Length);

    public TSRange[] IncludedRanges()
    {
        uint length;
        return c_api.ts_x_parser_included_ranges(Ptr, out length);
    }

    public TSTree ParseString(TSTree oldTree, string input)
    {
        var ptr = c_api.ts_x_parser_parse_string_encoding(Ptr, oldTree != null ? oldTree.Ptr : IntPtr.Zero,
                                                    input, (uint)input.Length * 2, ETSInputEncoding.TSInputEncodingUTF16);
        return ptr != IntPtr.Zero ? new TSTree(ptr) : null;
    }

    public void Reset() 
        => c_api.ts_x_parser_reset(Ptr);

    public void SetTimeoutMicros(ulong timeout) 
        => c_api.ts_x_parser_set_timeout_micros(Ptr, timeout);

    public ulong TimeoutMicros() 
        => c_api.ts_x_parser_timeout_micros(Ptr);

    public void SetLogger(TSLogger logger)
    {
        var code = new _TSLoggerCode(logger);
        var data = new _TSLoggerData { Log = logger != null ? new TSLogCallback(code.LogCallback) : null };

        c_api.ts_x_parser_set_logger(Ptr, data);
    }
}
