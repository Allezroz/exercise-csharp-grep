using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// TO DO: Alternation (cat|dog) line 84
// add handling: pattern opens with ( and contains |, we strip brackets, split on | and check each pattern.

namespace Parser
{
    public enum TokenType
    {
        Characters,
        Wildcard,
        Digit,
        Alpha,
        Group,
        Alternation,
        Backpattern
    }
    public struct Token
    {
        public string pattern;
        public TokenType type;
        public bool optional;
        public bool multiple;
        public bool capturegroup;
        public bool negative;
    }
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

        public static bool CaptureGroup(string input, out string group) // handle alternation and capture groups. We should return both the result as a bool and the contents of the group. The output probably needs to be a list or something.
        {
            group = "";
            return true;
        }

        public static Token GetPattern(string pattern) // this ONLY gets the full pattern to be matched. If it's a capture group, it gets the whole nested shebang. We can deal with cursor positions in the outer method by looking at length.
        {
            // three cases - an open bracket/parenthesis, a forwardslash, or anything else

            // two lookahead cases at the end - + and ?
            Token ret = new Token();
            int len = pattern.Length;

            if (pattern[0] == '\\')
            {
                ret.pattern = pattern[0..1];
                if (ret.pattern[1] == 'd')
                    ret.type = TokenType.Digit;
                else if (ret.pattern[1] == 'w')
                    ret.type = TokenType.Alpha;
                else
                    ret.type = TokenType.Backpattern;
            }
            else if (pattern[0] == '[') // until ]
            {
                int i = 0;
                while (pattern[i] != ']')
                {
                    i++;
                }
                ret.pattern = pattern[1..(i-1)];
                if (ret.pattern.StartsWith('^'))
                {
                    ret.pattern = ret.pattern[1..];
                }
               
            }
            else if (pattern[0] == '(') // count opens and closes, until we zero the set of parenthesis
            {

            }
            else // character string
            {

            }

            if (len > ret.pattern.Length+1 && "?+".Contains(pattern[ret.pattern.Length+1]))
            {
                ret.pattern += pattern[ret.pattern.Length + 1];
            }
            



            if (!"[(".Contains(pattern[0]))  // single character of any sort, wildcard
            {
                ret = ret + pattern[0];
                pattern = pattern.Substring(1);
                if (ret == "\\") // '/' refs. Double digit backreferences will fuck this, if those are even allowed.
                {
                    ret = ret + pattern[0];
                    pattern = pattern.Substring(1);
                }
            }
            else
            {
                // nested backrefs again :-/
                while (pattern.Length > 0 && !ret.EndsWith("]") && !ret.EndsWith(")")) // blocks of any sort, goes until the block ends. Needs matching.
                { // this is okay until we nest
                    ret = ret + pattern[0];
                    pattern = pattern.Substring(1);
                }
            }


            

            return ret;
        }

        public static bool MatchPattern(string input, string pattern) // this needs to be able to accept an empty 'input' string and then check for optionals
        {
            string pat = "";
            bool match = false;
            bool ret = false;

            if (pattern == "")
                return true; // base case - we have exhausted the pattern

            

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
                if (input.Length > 0 && HandlePattern(input[0], pat)) // unless i refactor handlepattern to do everything this is a failing edge case
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
            else if (pat.StartsWith('(') && pat.Contains('|')) // alternation - this will need to be refactored to handle backreferences
            { // we don't record the pattern in the brackets, we record what matches in the brackets. This makes the recursive approach awkward.
              // Maybe it needs to be reserved for the + operator, or we refactor that to be two pointers and step back?

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

// so if I refactored again, how would i do that

// we find the whole 'current' character group with a pointer and use .startswith
// we don't check anything character by character
// we record what is matched by anything in brackets but this is brutal.

// stick with recursive approach
// instead of explicitly one character at a time, we have a method that parses out the next whole token
// this means that for character groups we can capture the whole group and record what it captured in the correct spot. Nested ones are still rough.
// character groups probably need their own recursable method - start with open bracket, return the captured group and/or a success fail, but this is a different 'branch'

// 1. get the full next token
// 2. handle the full token
    // a string and wildcards are fine but should check against the start of the whole string not a single char
    // character alternative group is fine
    // + and ? logic is okay just needs reimplimenting into the capture
    // anything wrapped in brackets needs to be handled in its own spur before folding back in