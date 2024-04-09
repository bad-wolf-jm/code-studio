using Metrino.Development.Core;
using System;
using System.Collections.Generic;

namespace Metrino.Development.UI.Core;


public class FuzzySearch
{
    const uint MatchMaximumLength = 1024;
    const double ScoreMax = double.PositiveInfinity;
    const double ScoreMin = double.NegativeInfinity;

    const double ScoreGapLeading = -0.005;
    const double ScoreGapTrailing = -0.005;
    const double ScoreGapInner = -0.01;
    const double ScoreMatchConsecutive = 1.0;
    const double ScpreMatchSlash = 0.9;
    const double ScoreMatchWord = 0.8;
    const double ScoreMatchCapital = 0.7;
    const double ScoreMatchDot = 0.6;

    static readonly Dictionary<char, double>[] _bonusStatesInit = new Dictionary<char, double>[] {
        new Dictionary<char, double> { },
        new Dictionary<char, double> {
            ['/'] = ScpreMatchSlash,
            ['-'] = ScoreMatchWord,
            ['_'] = ScoreMatchWord,
            [' '] = ScoreMatchWord,
            ['.'] = ScoreMatchDot,
        },
        new Dictionary<char, double> {
            ['/'] = ScpreMatchSlash,
            ['-'] = ScoreMatchWord,
            ['_'] = ScoreMatchWord,
            [' '] = ScoreMatchWord,
            ['.'] = ScoreMatchDot,
            ['a'] = ScoreMatchCapital,
            ['b'] = ScoreMatchCapital,
            ['c'] = ScoreMatchCapital,
            ['d'] = ScoreMatchCapital,
            ['e'] = ScoreMatchCapital,
            ['f'] = ScoreMatchCapital,
            ['g'] = ScoreMatchCapital,
            ['h'] = ScoreMatchCapital,
            ['i'] = ScoreMatchCapital,
            ['j'] = ScoreMatchCapital,
            ['k'] = ScoreMatchCapital,
            ['l'] = ScoreMatchCapital,
            ['m'] = ScoreMatchCapital,
            ['n'] = ScoreMatchCapital,
            ['o'] = ScoreMatchCapital,
            ['p'] = ScoreMatchCapital,
            ['q'] = ScoreMatchCapital,
            ['r'] = ScoreMatchCapital,
            ['s'] = ScoreMatchCapital,
            ['t'] = ScoreMatchCapital,
            ['u'] = ScoreMatchCapital,
            ['v'] = ScoreMatchCapital,
            ['w'] = ScoreMatchCapital,
            ['x'] = ScoreMatchCapital,
            ['y'] = ScoreMatchCapital,
            ['z'] = ScoreMatchCapital
        },
    };

    static readonly Dictionary<char, int> _bonusIndexInit = new Dictionary<char, int>
    {
        ['A'] = 2,
        ['B'] = 2,
        ['C'] = 2,
        ['D'] = 2,
        ['E'] = 2,
        ['F'] = 2,
        ['G'] = 2,
        ['H'] = 2,
        ['I'] = 2,
        ['J'] = 2,
        ['K'] = 2,
        ['L'] = 2,
        ['M'] = 2,
        ['N'] = 2,
        ['O'] = 2,
        ['P'] = 2,
        ['Q'] = 2,
        ['R'] = 2,
        ['S'] = 2,
        ['T'] = 2,
        ['U'] = 2,
        ['V'] = 2,
        ['W'] = 2,
        ['X'] = 2,
        ['Y'] = 2,
        ['Z'] = 2,
        ['a'] = 1,
        ['b'] = 1,
        ['c'] = 1,
        ['d'] = 1,
        ['e'] = 1,
        ['f'] = 1,
        ['g'] = 1,
        ['h'] = 1,
        ['i'] = 1,
        ['j'] = 1,
        ['k'] = 1,
        ['l'] = 1,
        ['m'] = 1,
        ['n'] = 1,
        ['o'] = 1,
        ['p'] = 1,
        ['q'] = 1,
        ['r'] = 1,
        ['s'] = 1,
        ['t'] = 1,
        ['u'] = 1,
        ['v'] = 1,
        ['w'] = 1,
        ['x'] = 1,
        ['y'] = 1,
        ['z'] = 1,
        ['0'] = 1,
        ['1'] = 1,
        ['2'] = 1,
        ['3'] = 1,
        ['4'] = 1,
        ['5'] = 1,
        ['6'] = 1,
        ['7'] = 1,
        ['8'] = 1,
        ['9'] = 1
    };

    static readonly int[] _bonusIndex = new int[256];

    static readonly double[][] _bonusState = new double[3][]
    {
        new double[256],
        new double[256],
        new double[256]
    };

    static FuzzySearch()
    {
        for (int i = 0; i < 256; i++)
        {
            if (_bonusIndexInit.ContainsKey((char)i))
                _bonusIndex[i] = _bonusIndexInit[(char)i];
        }

        for (int s = 0; s < 3; s++)
        {
            for (int i = 0; i < 256; i++)
            {
                if (_bonusStatesInit[s].ContainsKey((char)i))
                    _bonusState[s][i] = _bonusStatesInit[s][(char)i];
            }
        }
    }

    static double ComputeBonus(char last_ch, char ch)
    {
        return _bonusState[_bonusIndex[ch]][last_ch];
    }

    struct Match
    {
        public int NeedleLength;
        public int HaystackLength;

        public string LoweredNeedle = "";
        public string LoweredHaystack = "";

        public double[] MatchBonus = new double[MatchMaximumLength];

        public Match()
        {
        }

        public Match(string needle, string haystack)
        {
            NeedleLength = needle.Length;
            HaystackLength = haystack.Length;

            if (HaystackLength > MatchMaximumLength || NeedleLength > HaystackLength)
                return;

            LoweredNeedle = needle.ToLower();
            LoweredHaystack = haystack.ToLower();

            PrecomputeBonus(haystack);
        }

        void PrecomputeBonus(string haystack)
        {
            /* Which positions are beginning of words */
            char lastCh = '/';
            for (int i = 0; i < haystack.Length; i++)
            {
                char ch = haystack[i];

                MatchBonus[i] = ComputeBonus(lastCh, ch);
                lastCh = ch;
            }
        }

        public void MatchRow(int row, double[] currentD, double[] currentM, double[] previousD, double[] previousM)
        {

            int n = NeedleLength;
            int m = HaystackLength;
            int i = row;

            double previousScore = ScoreMin;
            double gapScore = i == n - 1 ? ScoreGapTrailing : ScoreGapInner;

            for (int j = 0; j < m; j++)
            {
                if (LoweredNeedle[i] == LoweredHaystack[j])
                {
                    double score = ScoreMin;
                    if (i == 0)
                    {
                        score = (j * ScoreGapLeading) + MatchBonus[j];
                    }
                    else if (j != 0)
                    {
                        /* i > 0 && j > 0*/
                        score = Math.Max(
                                previousM[j - 1] + MatchBonus[j],
                                /* consecutive match, doesn't stack with match_bonus */
                                previousD[j - 1] + ScoreMatchConsecutive);
                    }

                    currentD[j] = score;
                    currentM[j] = previousScore = Math.Max(score, previousScore + gapScore);
                }
                else
                {
                    currentD[j] = ScoreMin;
                    currentM[j] = previousScore = previousScore + gapScore;
                }
            }
        }
    }

    public static bool HasMatch(string needle, string haystack)
    {
        string loweredNeedle = needle.ToLower();
        string loweredHaystack = haystack.ToLower();

        int L = needle.Length;
        int L1 = haystack.Length;
        for (int i = 0, j=0; i < L; i++)
        {
            j = loweredHaystack.IndexOf(loweredNeedle[i], j);
            if (j == -1)
                return false;
            j++;
            if (j >= L1)
                break;
        }

        return true; ;
    }

    public static double Score(string needle, string haystack)
    {
        if (string.IsNullOrEmpty(needle))
            return ScoreMin;

        Match match = new Match(needle, haystack);

        int n = match.NeedleLength;
        int m = match.HaystackLength;

        if (m > MatchMaximumLength || n > m)
        {
            /*
             * Unreasonably large candidate: return no score
             * If it is a valid match it will still be returned, it will
             * just be ranked below any reasonably sized candidates
             */
            return ScoreMin;
        }
        else if (n == m)
        {
            /* Since this method can only be called with a haystack which
             * matches needle. If the lengths of the strings are equal the
             * strings themselves must also be equal (ignoring case).
             */
            return ScoreMax;
        }

        /*
         * D[][] Stores the best score for this position ending with a match.
         * M[][] Stores the best possible score at this position.
         */
        double[][] D = new double[][] { new double[MatchMaximumLength], new double[MatchMaximumLength] };
        double[][] M = new double[][] { new double[MatchMaximumLength], new double[MatchMaximumLength] };

        double[] previousD, previousM;
        double[] currentD, currentM;

        previousD = D[0];
        previousM = M[0];
        currentD = D[1];
        currentM = M[1];

        for (int i = 0; i < n; i++)
        {
            match.MatchRow(i, currentD, currentM, previousD, previousM);

            (currentD, previousD) = (previousD, currentD);
            (currentM, previousM) = (previousM, currentM);
        }

        return previousM[m - 1];
    }

    public static double MatchPositions(string needle, string haystack, int[] positions)
    {
        if (string.IsNullOrEmpty(needle))
            return ScoreMin;

        var match = new Match(needle, haystack);

        int n = match.NeedleLength;
        int m = match.HaystackLength;

        if (m > MatchMaximumLength || n > m)
        {
            /*
             * Unreasonably large candidate: return no score
             * If it is a valid match it will still be returned, it will
             * just be ranked below any reasonably sized candidates
             */
            return ScoreMin;
        }
        else if (n == m)
        {
            /* Since this method can only be called with a haystack which
             * matches needle. If the lengths of the strings are equal the
             * strings themselves must also be equal (ignoring case).
             */
            if (positions != null)
                for (int i = 0; i < n; i++)
                    positions[i] = i;

            return ScoreMax;
        }

        double[][] D = new double[n][];//,MATCH_MAX_LEN];
        for (int k = 0; k < n; k++)
            D[k] = new double[MatchMaximumLength];

        double[][] M = new double[n][];//,MATCH_MAX_LEN];
        for (int k = 0; k < n; k++)
            M[k] = new double[MatchMaximumLength];

        double[] last_D, last_M;
        double[] curr_D, curr_M;

        curr_D = D[0];
        curr_M = M[0];
        last_D = curr_D;
        last_M = curr_M;

        for (int i = 0; i < n; i++)
        {
            curr_D = D[i];
            curr_M = M[i];

            match.MatchRow(i, curr_D, curr_M, last_D, last_M);

            last_D = curr_D;
            last_M = curr_M;
        }

        /* backtrace to find the positions of optimal matching */
        if (positions != null)
        {
            bool matchRequired = false;
            for (int i = n - 1, j = m - 1; i >= 0; i--)
            {
                for (; j >= 0; j--)
                {
                    /*
                     * There may be multiple paths which result in
                     * the optimal weight.
                     *
                     * For simplicity, we will pick the first one
                     * we encounter, the latest in the candidate
                     * string.
                     */
                    if (D[i][j] != ScoreMin && (matchRequired || D[i][j] == M[i][j]))
                    {
                        /* If this score was determined using SCORE_MATCH_CONSECUTIVE, the previous character MUST be a match */
                        matchRequired = i != 0 && j != 0 && M[i][j] == (D[i - 1][j - 1] + ScoreMatchConsecutive);
                        positions[i] = j--;
                        break;
                    }
                }
            }
        }

        double result = M[n - 1][m - 1];

        return result;
    }

    public static List<TextSpan> PositionsToSpans(string str, int[] positions)
    {
        var result = new List<TextSpan>();
        var matchIndex = 0;
        var stringIndex = 0;
        var offset = 0;

        var strLen = str.Length;
        var positionsLength = positions.Length;

        while (stringIndex < strLen)
        {
            if (positions[matchIndex] == stringIndex)
            {
                result.Add(new TextSpan(false, offset, stringIndex - offset));

                offset = stringIndex;
                while (stringIndex < strLen && matchIndex < positionsLength && positions[matchIndex] == stringIndex)
                {
                    matchIndex++;
                    stringIndex++;
                }

                result.Add(new TextSpan(true, offset, stringIndex - offset));
                offset = stringIndex;
            }
            else
            {
                stringIndex++;
            }
        }
        result.Add(new TextSpan(false, offset, stringIndex - offset));

        return result;
    }
}
