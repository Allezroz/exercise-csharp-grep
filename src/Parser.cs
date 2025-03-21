using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TO DO: Alternation (cat|dog) line 84
// add handling: pattern opens with ( and contains |, we strip brackets, split on | and check each pattern.

namespace Parser
{

    public static class Parser
    {
        // In specified character pattern
        private static bool Characters(char c, string pattern)
        {
            pattern = pattern.Replace("[", "").Replace("]", "");

            bool neg = (pattern[0] == '^');

            if (neg)
            {
                pattern = pattern[1..];
            }

            if (pattern.Contains(c))
            {
                return neg ^ true;
            }
            return neg ^ false;

        }

        // \d digits
        private static bool Digits(char c, string pattern)
        {

            if (char.IsDigit(c))
            {
                return true;
            }
            return false;
        }

        // \w alphanumerics or whitespace
        private static bool Alpha(char c, string pattern)
        {
            if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
            {
                return true;
            }
            return false;
        }

        public static bool HandlePattern(char c, string pat) => pat switch
        {
            "\\d" => Digits(c, pat),
            "\\w" => Alpha(c, pat),
            "." => true, // wildcard any character
            _ => Characters(c, pat)
        };

        public static bool MatchPattern(string input, string pattern) // this needs to be able to accept an empty 'input' string and then check for optionals
        {
            string pat = "";
            bool match = false;
            bool ret = false;

            if (pattern == "")
                return true; // base case - we have exhausted the pattern

            if (!"[(".Contains(pattern[0]))  // single character of any sort, wildcard
            {
                pat = pat + pattern[0];
                pattern = pattern.Substring(1);
                if (pat == "\\") // '/' refs. Double digit backreferences will fuck this, if those are even allowed.
                {
                    pat = pat + pattern[0];
                    pattern = pattern.Substring(1);
                }
            }
            else
            {
                // nested backrefs again :-/
                while (pattern.Length > 0 && !pat.EndsWith("]") && !pat.EndsWith(")")) // blocks of any sort, goes until the block ends
                {
                    pat = pat + pattern[0];
                    pattern = pattern.Substring(1);
                }
            }

            if (pattern.Length > 0 && pattern.StartsWith('?'))
            {

                if (input.Length > 0 && HandlePattern(input[0], pat))
                    ret = MatchPattern(input.Substring(1), pattern.Substring(1));
                else
                    ret = MatchPattern(input, pattern.Substring(1));

            }
            else if (pattern.Length > 0 && pattern.StartsWith('+')) // refactor me to match stylistically
            {
                string remainingPattern = pattern.Substring(1);
                if (input.Length > 0 && HandlePattern(input[0], pat))
                {
                    bool matchMore = MatchPattern(input.Substring(1), pat + "+" + remainingPattern);
                    if (matchMore)
                        return true;
                    return MatchPattern(input.Substring(1), remainingPattern);
                }
                return false;
            }
            else if (pat == "$" && input.Length == 0)
            {
                ret = true;
            }
            else if (pat.StartsWith('(') && pat.Contains('|')) // alternation
            {
                string[] pats = pat.Replace("(", "").Replace(")", "").Split('|');
                for (int idx = 0; idx < pats.Length; idx++)
                    if (input.StartsWith(pats[idx]))
                    {
                        ret = MatchPattern(input.Substring(pats[idx].Length), pattern);
                        break;
                    }
            }
            else if (input.Length > 0 && HandlePattern(input[0], pat)) // any other pattern
                ret = MatchPattern(input.Substring(1), pattern);

            return ret;
        }

        public static bool Grep(string input, string pattern)
        {
            int idx = 0;
            bool ret = false;

            bool IsSOL = pattern.StartsWith('^');
            if (IsSOL)
                pattern = pattern[1..];

            while (idx < input.Length && !ret)
            {
                ret = MatchPattern(input[idx..], pattern);
                idx += 1;
                if (IsSOL && !ret)
                    break;
            }
            return ret;
        }
    }
}

