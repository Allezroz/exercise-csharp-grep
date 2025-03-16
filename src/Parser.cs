using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    
    public static class Parser
    {
        // In specified character pattern
        private static bool Characters(string input, string pattern)
        {
            pattern = pattern.Replace("[", "").Replace("]", "");

            bool neg = (pattern[0] == '^');

            if (neg)
            {
                pattern = pattern[1..];
            }

            foreach (char c in input)
            {
                if (pattern.Contains(c))
                {
                    return !neg;
                }
            }
            return neg;

        }

        // \d digits
        private static bool Digits(string input, string pattern)
        {

            foreach (char c in input)
            {
                if (char.IsDigit(c))
                {
                    return true;
                }
            }
            return false;
        }
        // \w alphanumerics or whitespace
        private static bool Alpha(string input, string pattern)
        {

            foreach (char c in input)
            {
                if (char.IsLetterOrDigit(c) || char.IsWhiteSpace(c))
                {
                    return true;
                }
            }
            return false;
        }

        // Take the string and pattern, handle the stepping and parsing.
        // Can pass an idx by reference to keep the starting point and 'cursor' separate
        // or it can pass the substring up and receive an index integer back or -1 on false? - I like this better

        public static bool MatchPattern(string input, string pattern)
        {
            if (pattern == "\\d")
            {
                return Digits(input, pattern);
            }
            else if (pattern == "\\w")
            {
                return Alpha(input, pattern);
            }
            else
            {
                return Characters(input, pattern);
            }


        }


    }
}
