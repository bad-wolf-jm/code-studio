using static Metrino.Development.Core.TreeSitter.TSCursor;
using System.Runtime.InteropServices;

namespace Metrino.Development.Core.TreeSitter;

public sealed class c_api
{
    const string DllPath = "TreeSitterLibrary.dll";

    #region PInvoke
    /**
    * Create a new tree cursor starting from the given node.
    *
    * A tree cursor allows you to walk a syntax tree more efficiently than is
    * possible using the `TSNode` functions. It is a mutable object that is always
    * on a certain syntax node, and can be moved imperatively to different nodes.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSTreeCursor ts_x_tree_cursor_new(TSNode node);

    /**
    * Delete a tree cursor, freeing all of the memory that it used.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ts_x_tree_cursor_delete(ref TSTreeCursor cursor);

    /**
    * Re-initialize a tree cursor to start at a different node.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ts_x_tree_cursor_reset(ref TSTreeCursor cursor, TSNode node);

    /**
    * Get the tree cursor's current node.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSNode ts_x_tree_cursor_current_node(ref TSTreeCursor cursor);

    /**
    * Get the field name of the tree cursor's current node.
    *
    * This returns `NULL` if the current node doesn't have a field.
    * See also `ts_x_node_child_by_field_name`.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ts_x_tree_cursor_current_field_name(ref TSTreeCursor cursor);

    /**
    * Get the field id of the tree cursor's current node.
    *
    * This returns zero if the current node doesn't have a field.
    * See also `ts_x_node_child_by_field_id`, `ts_x_language_field_id_for_name`.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern ushort ts_x_tree_cursor_current_field_id(ref TSTreeCursor cursor);

    /**
    * Move the cursor to the parent of its current node.
    *
    * This returns `true` if the cursor successfully moved, and returns `false`
    * if there was no parent node (the cursor was already on the root node).
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ts_x_tree_cursor_goto_parent(ref TSTreeCursor cursor);

    /**
    * Move the cursor to the next sibling of its current node.
    *
    * This returns `true` if the cursor successfully moved, and returns `false`
    * if there was no next sibling node.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ts_x_tree_cursor_goto_next_sibling(ref TSTreeCursor cursor);

    /**
    * Move the cursor to the first child of its current node.
    *
    * This returns `true` if the cursor successfully moved, and returns `false`
    * if there were no children.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ts_x_tree_cursor_goto_first_child(ref TSTreeCursor cursor);

    /**
    * Move the cursor to the first child of its current node that extends beyond
    * the given byte offset or point.
    *
    * This returns the index of the child node if one was found, and returns -1
    * if no such child was found.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern long ts_x_tree_cursor_goto_first_child_for_byte(ref TSTreeCursor cursor, uint byteOffset);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern long ts_x_tree_cursor_goto_first_child_for_point(ref TSTreeCursor cursor, TSPoint point);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSTreeCursor ts_x_tree_cursor_copy(ref TSTreeCursor cursor);
    #endregion

    #region PInvoke
    /**
    * Create a new query from a string containing one or more S-expression
    * patterns. The query is associated with a particular language, and can
    * only be run on syntax nodes parsed with that language.
    *
    * If all of the given patterns are valid, this returns a `TSQuery`.
    * If a pattern is invalid, this returns `NULL`, and provides two pieces
    * of information about the problem:
    * 1. The byte offset of the error is written to the `error_offset` parameter.
    * 2. The type of error is written to the `error_type` parameter.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ts_x_query_new(IntPtr language, [MarshalAs(UnmanagedType.LPUTF8Str)] string source, uint source_len, out uint error_offset, out ETSQueryError error_type);

    /**
    * Get the number of distinct node types in the language.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint ts_x_language_symbol_count(IntPtr language);

    /**
    * Get a node type string for the given numerical id.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ts_x_language_symbol_name(IntPtr language, ushort symbol);

    /**
    * Get the numerical id for the given node type string.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern ushort ts_x_language_symbol_for_name(IntPtr language, [MarshalAs(UnmanagedType.LPUTF8Str)] string str, uint length, bool is_named);

    /**
    * Get the number of distinct field names in the language.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint ts_x_language_field_count(IntPtr language);

    /**
    * Get the field name string for the given numerical id.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ts_x_language_field_name_for_id(IntPtr language, ushort fieldId);

    /**
    * Get the numerical id for the given field name string.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern ushort ts_x_language_field_id_for_name(IntPtr language, [MarshalAs(UnmanagedType.LPUTF8Str)] string str, uint length);

    /**
    * Check whether the given node type id belongs to named nodes, anonymous nodes,
    * or a hidden nodes.
    *
    * See also `ts_x_node_is_named`. Hidden nodes are never returned from the API.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern ETSSymbolType ts_x_language_symbol_type(IntPtr language, ushort symbol);

    /**
    * Get the ABI version number for this language. This version number is used
    * to ensure that languages were generated by a compatible version of
    * Tree-sitter.
    *
    * See also `ts_x_parser_set_language`.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint ts_x_language_version(IntPtr language);
    #endregion

    #region PInvoke
    /**
    * Get the node's type as a null-terminated string.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ts_x_node_type(TSNode node);

    /**
    * Get the node's type as a numerical id.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern ushort ts_x_node_symbol(TSNode node);

    /**
    * Get the node's start byte.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint ts_x_node_start_byte(TSNode node);

    /**
    * Get the node's start position in terms of rows and columns.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSPoint ts_x_node_start_point(TSNode node);

    /**
    * Get the node's end byte.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint ts_x_node_end_byte(TSNode node);

    /**
    * Get the node's end position in terms of rows and columns.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSPoint ts_x_node_end_point(TSNode node);

    /**
    * Get an S-expression representing the node as a string.
    *
    * This string is allocated with `malloc` and the caller is responsible for
    * freeing it using `free`.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ts_x_node_string(TSNode node);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ts_x_node_string_free(IntPtr str);

    /**
    * Check if the node is null. Functions like `ts_x_node_child` and
    * `ts_x_node_next_sibling` will return a null node to indicate that no such node
    * was found.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ts_x_node_is_null(TSNode node);

    /**
    * Check if the node is *named*. Named nodes correspond to named rules in the
    * grammar, whereas *anonymous* nodes correspond to string literals in the
    * grammar.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ts_x_node_is_named(TSNode node);

    /**
    * Check if the node is *missing*. Missing nodes are inserted by the parser in
    * order to recover from certain kinds of syntax errors.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ts_x_node_is_missing(TSNode node);

    /**
    * Check if the node is *extra*. Extra nodes represent things like comments,
    * which are not required the grammar, but can appear anywhere.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ts_x_node_is_extra(TSNode node);

    /**
    * Check if a syntax node has been edited.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ts_x_node_has_changes(TSNode node);

    /**
    * Check if the node is a syntax error or contains any syntax errors.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ts_x_node_has_error(TSNode node);

    /**
    * Get the node's immediate parent.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSNode ts_x_node_parent(TSNode node);

    /**
    * Get the node's child at the given index, where zero represents the first
    * child.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSNode ts_x_node_child(TSNode node, uint index);

    /**
    * Get the field name for node's child at the given index, where zero represents
    * the first child. Returns NULL, if no field is found.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ts_x_node_field_name_for_child(TSNode node, uint index);

    /**
    * Get the node's number of children.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint ts_x_node_child_count(TSNode node);

    /**
    * Get the node's number of *named* children.
    *
    * See also `ts_x_node_is_named`.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSNode ts_x_node_named_child(TSNode node, uint index);

    /**
    * Get the node's number of *named* children.
    *
    * See also `ts_x_node_is_named`.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint ts_x_node_named_child_count(TSNode node);

    /**
    * Get the node's child with the given numerical field id.
    *
    * You can convert a field name to an id using the
    * `ts_x_language_field_id_for_name` function.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSNode ts_x_node_child_by_field_name(TSNode self, [MarshalAs(UnmanagedType.LPUTF8Str)] string field_name, uint field_name_length);

    /**
    * Get the node's child with the given numerical field id.
    *
    * You can convert a field name to an id using the
    * `ts_x_language_field_id_for_name` function.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSNode ts_x_node_child_by_field_id(TSNode self, ushort fieldId);

    /**
    * Get the node's next / previous sibling.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSNode ts_x_node_next_sibling(TSNode self);
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSNode ts_x_node_prev_sibling(TSNode self);

    /**
    * Get the node's next / previous *named* sibling.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSNode ts_x_node_next_named_sibling(TSNode self);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSNode ts_x_node_prev_named_sibling(TSNode self);

    /**
    * Get the node's first child that extends beyond the given byte offset.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSNode ts_x_node_first_child_for_byte(TSNode self, uint byteOffset);

    /**
    * Get the node's first named child that extends beyond the given byte offset.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSNode ts_x_node_first_named_child_for_byte(TSNode self, uint byteOffset);

    /**
    * Get the smallest node within this node that spans the given range of bytes
    * or (row, column) positions.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSNode ts_x_node_descendant_for_byte_range(TSNode self, uint startByte, uint endByte);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSNode ts_x_node_descendant_for_point_range(TSNode self, TSPoint startPoint, TSPoint endPoint);

    /**
    * Get the smallest named node within this node that spans the given range of
    * bytes or (row, column) positions.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSNode ts_x_node_named_descendant_for_byte_range(TSNode self, uint startByte, uint endByte);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSNode ts_x_node_named_descendant_for_point_range(TSNode self, TSPoint startPoint, TSPoint endPoint);

    /**
    * Check if two nodes are identical.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ts_x_node_eq(TSNode node1, TSNode node2);
    #endregion

    #region PInvoke
    //[DllImport("tree-sitter-cpp.dll", CallingConvention = CallingConvention.Cdecl)]
    //public static extern IntPtr tree_sitter_cpp();

    //[DllImport("tree-sitter-c-sharp.dll", CallingConvention = CallingConvention.Cdecl)]
    //public static extern IntPtr tree_sitter_c_sharp();

    //[DllImport("tree-sitter-rust.dll", CallingConvention = CallingConvention.Cdecl)]
    //public static extern IntPtr tree_sitter_rust();


    /**
    * Create a new parser.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ts_x_parser_new();

    /**
    * Delete the parser, freeing all of the memory that it used.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ts_x_parser_delete(IntPtr parser);

    /**
    * Set the language that the parser should use for parsing.
    *
    * Returns a boolean indicating whether or not the language was successfully
    * assigned. True means assignment succeeded. False means there was a version
    * mismatch: the language was generated with an incompatible version of the
    * Tree-sitter CLI. Check the language's version using `ts_x_language_version`
    * and compare it to this library's `TREE_SITTER_LANGUAGE_VERSION` and
    * `TREE_SITTER_MIN_COMPATIBLE_LANGUAGE_VERSION` constants.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ts_x_parser_set_language(IntPtr parser, IntPtr language);

    /**
    * Get the parser's current language.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ts_x_parser_language(IntPtr parser);

    /**
    * Set the ranges of text that the parser should include when parsing.
    *
    * By default, the parser will always include entire documents. This function
    * allows you to parse only a *portion* of a document but still return a syntax
    * tree whose ranges match up with the document as a whole. You can also pass
    * multiple disjoint ranges.
    *
    * The second and third parameters specify the location and length of an array
    * of ranges. The parser does *not* take ownership of these ranges; it copies
    * the data, so it doesn't matter how these ranges are allocated.
    *
    * If `length` is zero, then the entire document will be parsed. Otherwise,
    * the given ranges must be ordered from earliest to latest in the document,
    * and they must not overlap. That is, the following must hold for all
    * `i` < `length - 1`: ranges[i].end_byte <= ranges[i + 1].start_byte
    *
    * If this requirement is not satisfied, the operation will fail, the ranges
    * will not be assigned, and this function will return `false`. On success,
    * this function returns `true`
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    //[return: MarshalAs(UnmanagedType.I1)]
    public static extern bool ts_x_parser_set_included_ranges(IntPtr parser, [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] TSRange[] ranges, uint length);

    /**
    * Get the ranges of text that the parser will include when parsing.
    *
    * The returned pointer is owned by the parser. The caller should not free it
    * or write to it. The length of the array will be written to the given
    * `length` pointer.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)]
    public static extern TSRange[] ts_x_parser_included_ranges(IntPtr parser, out uint length);

    /**
    * Use the parser to parse some source code stored in one contiguous buffer.
    * The first two parameters are the same as in the `ts_x_parser_parse` function
    * above. The second two parameters indicate the location of the buffer and its
    * length in bytes.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ts_x_parser_parse_string(IntPtr parser, IntPtr oldTree, [MarshalAs(UnmanagedType.LPUTF8Str)] string input, uint length);

    /**
    * Use the parser to parse some source code stored in one contiguous buffer.
    * The first two parameters are the same as in the `ts_x_parser_parse` function
    * above. The second two parameters indicate the location of the buffer and its
    * length in bytes.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    //public static extern IntPtr ts_x_parser_parse_string_encoding(IntPtr parser, IntPtr oldTree, [MarshalAs(UnmanagedType.LPUTF8Str)] string input, uint length, TSInputEncoding encoding);
    public static extern IntPtr ts_x_parser_parse_string_encoding(IntPtr parser, IntPtr oldTree, [MarshalAs(UnmanagedType.LPWStr)] string input, uint length, ETSInputEncoding encoding);

    /**
    * Instruct the parser to start the next parse from the beginning.
    *
    * If the parser previously failed because of a timeout or a cancellation, then
    * by default, it will resume where it left off on the next call to
    * `ts_x_parser_parse` or other parsing functions. If you don't want to resume,
    * and instead intend to use this parser to parse some other document, you must
    * call `ts_x_parser_reset` first.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ts_x_parser_reset(IntPtr parser);

    /**
    * Set the maximum duration in microseconds that parsing should be allowed to
    * take before halting.
    *
    * If parsing takes longer than this, it will halt early, returning NULL.
    * See `ts_x_parser_parse` for more information.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ts_x_parser_set_timeout_micros(IntPtr parser, ulong timeout);

    /**
    * Get the duration in microseconds that parsing is allowed to take.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong ts_x_parser_timeout_micros(IntPtr parser);

    /**
    * Set the parser's current cancellation flag pointer.
    *
    * If a non-null pointer is assigned, then the parser will periodically read
    * from this pointer during parsing. If it reads a non-zero value, it will
    * halt early, returning NULL. See `ts_x_parser_parse` for more information.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ts_x_parser_set_cancellation_flag(IntPtr parser, ref IntPtr flag);

    /**
    * Get the parser's current cancellation flag pointer.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ts_x_parser_cancellation_flag(IntPtr parser);

    /**
    * Set the logger that a parser should use during parsing.
    *
    * The parser does not take ownership over the logger payload. If a logger was
    * previously assigned, the caller is responsible for releasing any memory
    * owned by the previous logger.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ts_x_parser_set_logger(IntPtr parser, _TSLoggerData logger);
    #endregion

    #region PInvoke
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ts_x_query_delete(IntPtr query);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint ts_x_query_pattern_count(IntPtr query);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint ts_x_query_capture_count(IntPtr query);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint ts_x_query_string_count(IntPtr query);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint ts_x_query_start_byte_for_pattern(IntPtr query, uint patternIndex);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ts_x_query_predicates_for_pattern(IntPtr query, uint patternIndex, out uint length);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ts_x_query_is_pattern_rooted(IntPtr query, uint patternIndex);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ts_x_query_is_pattern_non_local(IntPtr query, uint patternIndex);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ts_x_query_is_pattern_guaranteed_at_step(IntPtr query, uint byteOffset);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ts_x_query_capture_name_for_id(IntPtr query, uint id, out uint length);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern ETSQuantifier ts_x_query_capture_quantifier_for_id(IntPtr query, uint patternId, uint captureId);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ts_x_query_string_value_for_id(IntPtr query, uint id, out uint length);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ts_x_query_disable_capture(IntPtr query, [MarshalAs(UnmanagedType.LPUTF8Str)] string captureName, uint captureNameLength);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ts_x_query_disable_pattern(IntPtr query, uint patternIndex);
    #endregion

    #region PInvoke
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ts_x_query_cursor_new();

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ts_x_query_cursor_delete(IntPtr cursor);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ts_x_query_cursor_exec(IntPtr cursor, IntPtr query, TSNode node);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ts_x_query_cursor_did_exceed_match_limit(IntPtr cursor);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern uint ts_x_query_cursor_match_limit(IntPtr cursor);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ts_x_query_cursor_set_match_limit(IntPtr cursor, uint limit);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ts_x_query_cursor_set_byte_range(IntPtr cursor, uint start_byte, uint end_byte);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ts_x_query_cursor_set_point_range(IntPtr cursor, TSPoint start_point, TSPoint end_point);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ts_x_query_cursor_next_match(IntPtr cursor, out TSQueryMatch match);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ts_x_query_cursor_remove_match(IntPtr cursor, uint id);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool ts_x_query_cursor_next_capture(IntPtr cursor, out TSQueryMatch match, out uint capture_index);
    #endregion

    #region PInvoke
    /**
    * Create a shallow copy of the syntax tree. This is very fast.
    *
    * You need to copy a syntax tree in order to use it on more than one thread at
    * a time, as syntax trees are not thread safe.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ts_x_tree_copy(IntPtr tree);

    /**
    * Delete the syntax tree, freeing all of the memory that it used.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ts_x_tree_delete(IntPtr tree);

    /**
    * Get the root node of the syntax tree.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSNode ts_x_tree_root_node(IntPtr tree);

    /**
    * Get the root node of the syntax tree, but with its position
    * shifted forward by the given offset.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern TSNode ts_x_tree_root_node_with_offset(IntPtr tree, uint offsetBytes, TSPoint offsetPoint);

    /**
    * Get the language that was used to parse the syntax tree.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ts_x_tree_language(IntPtr tree);

    /**
    * Get the array of included ranges that was used to parse the syntax tree.
    *
    * The returned pointer must be freed by the caller.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ts_x_tree_included_ranges(IntPtr tree, out uint length);

    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ts_x_tree_included_ranges_free(IntPtr ranges);

    /**
    * Edit the syntax tree to keep it in sync with source code that has been
    * edited.
    *
    * You must describe the edit both in terms of byte offsets and in terms of
    * (row, column) coordinates.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern void ts_x_tree_edit(IntPtr tree, ref TSInputEdit edit);

    /**
    * Compare an old edited syntax tree to a new syntax tree representing the same
    * document, returning an array of ranges whose syntactic structure has changed.
    *
    * For this to work correctly, the old syntax tree must have been edited such
    * that its ranges match up to the new tree. Generally, you'll want to call
    * this function right after calling one of the `ts_x_parser_parse` functions.
    * You need to pass the old tree that was passed to parse, as well as the new
    * tree that was returned from that function.
    *
    * The returned array is allocated using `malloc` and the caller is responsible
    * for freeing it using `free`. The length of the array will be written to the
    * given `length` pointer.
    */
    [DllImport(DllPath, CallingConvention = CallingConvention.Cdecl)]
    public static extern IntPtr ts_x_tree_get_changed_ranges(IntPtr old_tree, IntPtr new_tree, out uint length);
    #endregion
}
