//////////////////////////////////////////////////////////////////////////////
//
//  Module Name:
//      binding.cs
//
//  Abstract:
//      Wrapper for GitHub Tree-Sitter Library.
//
using System.Runtime.InteropServices;

namespace Metrino.Development.Core.TreeSitter;

public enum ETSInputEncoding
{
    TSInputEncodingUTF8,
    TSInputEncodingUTF16
}

public enum ETSSymbolType
{
    TSSymbolTypeRegular,
    TSSymbolTypeAnonymous,
    TSSymbolTypeAuxiliary,
}

public enum ETSLogType
{
    TSLogTypeParse,
    TSLogTypeLex,
}

public enum ETSQuantifier
{
    TSQuantifierZero = 0, // must match the array initialization value
    TSQuantifierZeroOrOne,
    TSQuantifierZeroOrMore,
    TSQuantifierOne,
    TSQuantifierOneOrMore,
}

public enum ETSQueryPredicateStepType
{
    TSQueryPredicateStepTypeDone,
    TSQueryPredicateStepTypeCapture,
    TSQueryPredicateStepTypeString,
}

public enum ETSQueryError
{
    TSQueryErrorNone = 0,
    TSQueryErrorSyntax,
    TSQueryErrorNodeType,
    TSQueryErrorField,
    TSQueryErrorCapture,
    TSQueryErrorStructure,
    TSQueryErrorLanguage,
}

[StructLayout(LayoutKind.Sequential)]
public struct TSPoint
{
    public uint Row;
    public uint Column;

    public TSPoint(uint row, uint column)
    {
        Row = row;
        Column = column;
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct TSRange
{
    public TSPoint StartPoint;
    public TSPoint EndPoint;
    public uint StartByte;
    public uint EndByte;
}

[StructLayout(LayoutKind.Sequential)]
public struct TSInputEdit
{
    public uint StartByte;
    public uint OldEndByte;
    public uint NewEndByte;
    public TSPoint StartPoint;
    public TSPoint OldEndPoint;
    public TSPoint NewEndPoint;
}

[StructLayout(LayoutKind.Sequential)]
public struct TSQueryCapture
{
    public TSNode Node;
    public uint Index;
}

[StructLayout(LayoutKind.Sequential)]
public struct TSQueryMatch
{
    public uint Id;
    public ushort PatternIndex;
    public ushort CaptureCount;
    public IntPtr Captures;
}

[StructLayout(LayoutKind.Sequential)]
public struct TSQueryPredicateStep
{
    public ETSQueryPredicateStepType Type;
    public uint ValueId;
}

public delegate void TSLogger(ETSLogType logType, string message);

[StructLayout(LayoutKind.Sequential)]
public struct _TSLoggerData
{
    private IntPtr Payload;
    internal TSLogCallback Log;
}

[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void TSLogCallback(IntPtr payload, ETSLogType logType, [MarshalAs(UnmanagedType.LPUTF8Str)] string message);

public class _TSLoggerCode
{
    private TSLogger Logger;

    internal _TSLoggerCode(TSLogger logger)
    {
        Logger = logger;
    }

    internal void LogCallback(IntPtr payload, ETSLogType logType, string message) => Logger(logType, message);
}


