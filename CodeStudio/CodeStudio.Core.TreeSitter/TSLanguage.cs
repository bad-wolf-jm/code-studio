using System.Runtime.InteropServices;

namespace Metrino.Development.Core.TreeSitter;

public sealed class TSLanguage : IDisposable
{
    internal IntPtr Ptr { get; private set; }

    public TSLanguage(IntPtr ptr)
    {
        Ptr = ptr;

        Symbols = new string[SymbolCount + 1];
        for (ushort i = 0; i < Symbols.Length; i++)
        {
            Symbols[i] = Marshal.PtrToStringAnsi(c_api.ts_x_language_symbol_name(Ptr, i));
        }

        Fields = new string[FieldCount + 1];
        FieldIds = new Dictionary<string, ushort>((int)FieldCount + 1);

        for (ushort i = 0; i < Fields.Length; i++)
        {
            Fields[i] = Marshal.PtrToStringAnsi(c_api.ts_x_language_field_name_for_id(Ptr, i));
            if (Fields[i] != null)
            {
                FieldIds.Add(Fields[i], i); // TODO: check for dupes, and throw if found
            }
        }
    }

    [DllImport("TreeSitter-CSharp.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr tree_sitter_c_sharp();

    [DllImport("TreeSitter-Json.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr tree_sitter_json();

    [DllImport("TreeSitter-Julia.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr tree_sitter_julia();

    [DllImport("TreeSitter-Lua.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr tree_sitter_lua();

    [DllImport("TreeSitter-Matlab.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr tree_sitter_matlab();

    [DllImport("TreeSitter-Python.dll", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr tree_sitter_python();

    public static TSLanguage FromLanguageName(string language)
    {
        switch (language)
        {
            case "c-sharp":
                return new TSLanguage(tree_sitter_c_sharp());
            case "julia":
                return new TSLanguage(tree_sitter_julia());
            case "lua":
                return new TSLanguage(tree_sitter_lua());
            case "json":
                return new TSLanguage(tree_sitter_json());
            case "matlab":
                return new TSLanguage(tree_sitter_matlab());
            case "pyhton":
                return new TSLanguage(tree_sitter_python());
        }

        return new TSLanguage(IntPtr.Zero);
    }

    public void Dispose()
    {
        if (Ptr != IntPtr.Zero)
        {
            //ts_x_query_cursor_delete(Ptr);
            Ptr = IntPtr.Zero;
        }
    }

    public TSQuery QueryNew(string source, out uint error_offset, out ETSQueryError error_type)
    {
        var ptr = c_api.ts_x_query_new(Ptr, source, (uint)source.Length, out error_offset, out error_type);
        return ptr != IntPtr.Zero ? new TSQuery(ptr) : null;
    }

    private string[] symbols;
    public string[] Symbols { get => symbols; set => symbols = value; }
    
    private string[] fields;
    public string[] Fields { get => fields; set => fields = value; }
    
    private Dictionary<string, ushort> fieldIds;
    public Dictionary<string, ushort> FieldIds { get => fieldIds; set => fieldIds = value; }

    public uint SymbolCount => c_api.ts_x_language_symbol_count(Ptr);

    public string SymbolName(ushort symbol) 
        => (symbol != ushort.MaxValue) ? Symbols[symbol] : "ERROR";

    public ushort SymbolForName(string str, bool is_named) 
        => c_api.ts_x_language_symbol_for_name(Ptr, str, (uint)str.Length, is_named);

    public uint FieldCount 
        => c_api.ts_x_language_field_count(Ptr);

    public string FieldNameForId(ushort fieldId) 
        => Fields[fieldId];

    public ushort FieldIdForName(string str) 
        => c_api.ts_x_language_field_id_for_name(Ptr, str, (uint)str.Length);

    public ETSSymbolType SymbolType(ushort symbol) 
        => c_api.ts_x_language_symbol_type(Ptr, symbol);

}
