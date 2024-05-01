using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace Metrino.Development.Core.TreeSitter;

public sealed class TSQuery : IDisposable
{
    internal IntPtr Ptr { get; private set; }

    public PredicateConjunction[] Predicates;

    public TSQuery(IntPtr ptr)
    {
        Ptr = ptr;

        Predicates = new PredicateConjunction[PatternCount];

        for (int i = 0; i < PatternCount; i++)
        {
            Predicates[i] = PredicatesForPattern((uint)i);
        }
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

    public struct PredicateOperand
    {
        public ETSQueryPredicateStepType Type;
        public string Value;

        public PredicateOperand(TSQuery q, TSQueryPredicateStep step)
        {
            Type = step.Type;
            switch (step.Type)
            {
                case ETSQueryPredicateStepType.TSQueryPredicateStepTypeString:
                    {
                        uint len;
                        Value = q.StringValueForId(step.ValueId, out len);
                    }
                    break;
                case ETSQueryPredicateStepType.TSQueryPredicateStepTypeCapture:
                    {
                        uint len;
                        Value = q.CaptureNameForId(step.ValueId, out len);
                    }
                    break;
                default:
                    Value = string.Empty;
                    break;
            }
        }
    }

    public class Predicate
    {
        public string Op = string.Empty;
        public List<PredicateOperand> Rands;

        public Predicate(TSQuery q, List<TSQueryPredicateStep> desc)
        {
            var op = desc[0];

            switch (op.Type)
            {
                case ETSQueryPredicateStepType.TSQueryPredicateStepTypeString:
                    {
                        uint len;
                        Op = q.StringValueForId(op.ValueId, out len);
                    }
                    break;
                default:
                    Op = string.Empty;
                    break;
            }

            Rands = new List<PredicateOperand>();
            for (int i = 1; i < desc.Count; i++)
                Rands.Add(new PredicateOperand(q, desc[i]));
        }

        public bool Test(Dictionary<string, string> captures)
        {
            bool isPositive = Op switch
            {
                "eq?" or "any-eq?" or "match?" or "any-match?" or "is?" or "any-of?" => true,
                _ => false
            };

            if (Rands[0].Type != ETSQueryPredicateStepType.TSQueryPredicateStepTypeCapture)
                throw new Exception();

            switch (Op)
            {
                case "eq?":
                    if (Rands[1].Type == ETSQueryPredicateStepType.TSQueryPredicateStepTypeCapture)
                        if (captures.ContainsKey(Rands[0].Value) && captures.ContainsKey(Rands[1].Value))
                            return captures[Rands[0].Value] == captures[Rands[1].Value];
                    return false;
                case "not-eq?":
                    if (Rands[1].Type == ETSQueryPredicateStepType.TSQueryPredicateStepTypeCapture)
                        if (captures.ContainsKey(Rands[0].Value) && captures.ContainsKey(Rands[1].Value))
                            return captures[Rands[0].Value] != captures[Rands[1].Value];
                    return true;
                case "any-eq?":
                    return false;
                case "any-not-eq?":
                    return false;
                case "match?":
                    if (Rands[1].Type == ETSQueryPredicateStepType.TSQueryPredicateStepTypeString)
                    {
                        if (captures.ContainsKey(Rands[0].Value))
                        {
                            var regex = new Regex(Rands[1].Value);
                            return regex.IsMatch(captures[Rands[0].Value]);
                        }
                    }
                    return true;
                case "not-match?":
                    return false;
                case "any-match?":
                    return false;
                case "any-not-match?":
                    return false;
                case "is?":
                    return false;
                case "is-not?":
                    return false;
                case "any-of?":
                    {
                        List<string> list = Rands.Skip(1).Select(x => x.Value).ToList();
                        var captureValue = captures[Rands[0].Value];
                        return list.Contains(captureValue);
                    }
                case "not-any-of?":
                    {
                        List<string> list = Rands.Skip(1).Select(x => x.Value).ToList();
                        var captureValue = captures[Rands[0].Value];
                        return !list.Contains(captureValue);
                    }
            }

            return false;
        }
    }

    public class PredicateConjunction
    {
        public List<Predicate> Predicates;

        public PredicateConjunction(List<Predicate> preds)
        {
            Predicates = preds;
        }

        public bool Test(TSQuery q, TSQueryCapture[] captures, string text)
        {
            var captureText = new Dictionary<string, string>();

            foreach (var capture in captures)
            {
                uint length;
                var name = q.CaptureNameForId(capture.Index, out length);

                var captureStart = (int)capture.Node.StartOffset;
                var captureEnd = (int)capture.Node.EndOffset;
                var captureData = text.Substring(captureStart, captureEnd - captureStart);

                captureText[name] = captureData;
            }

            foreach (var p in Predicates)
                if (!p.Test(captureText))
                    return false;

            return true;
        }
    }

    public PredicateConjunction PredicatesForPattern(uint patternIndex)
    {
        uint stepCount;
        IntPtr predicateStepsPtr = c_api.ts_x_query_predicates_for_pattern(Ptr, patternIndex, out stepCount);
        if (predicateStepsPtr != IntPtr.Zero)
        {
            var size = (long)stepCount * Marshal.SizeOf(typeof(TSQueryPredicateStep));
            if (size == 0) return new PredicateConjunction(new List<Predicate>());

            var predicateStepsStaging = new byte[size];

            Marshal.Copy(predicateStepsPtr, predicateStepsStaging, 0, (int)size);
            var predicateSteps = new TSQueryPredicateStep[stepCount];

            unsafe
            {
                fixed (TSQueryPredicateStep* p = &predicateSteps[0])
                fixed (byte* q = &predicateStepsStaging[0])
                    Buffer.MemoryCopy(q, p, size, size);
            }

            List<List<TSQueryPredicateStep>> predicateDescriptions = new List<List<TSQueryPredicateStep>>();
            var predicate = new List<TSQueryPredicateStep>();
            for (int i = 0; i < stepCount; i++)
            {
                var step = predicateSteps[i];
                if (step.Type == ETSQueryPredicateStepType.TSQueryPredicateStepTypeDone)
                {
                    if (predicate.Count > 0)
                    {
                        predicateDescriptions.Add(predicate);
                        predicate = new List<TSQueryPredicateStep>();
                    }

                    continue;
                }

                predicate.Add(step);
            }

            var preds = predicateDescriptions.Select(x => new Predicate(this, x)).ToList();
            return new PredicateConjunction(preds);
        }

        return new PredicateConjunction(new List<Predicate>());
    }

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
