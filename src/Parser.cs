using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    
    public static class Parser
    {
        // One or more specified characters
        private static int Characters(string input, string pattern)
        {
            return input.IndexOf(pattern);
        }

        // One or more \d digits
        private static int Digits(string input, string pattern)
        {
            int idx = 0;

            while (idx < input.Length)
            {
                if (char.IsDigit(input[idx]))
                {
                    return idx;
                }
                idx++;
            }

            return -1;
        }

        // Take the string and pattern, handle the stepping and parsing.
        // Can pass an idx by reference to keep the starting point and 'cursor' separate
        // or it can pass the substring up and receive an index integer back or -1 on false? - I like this better

        public static bool MatchPattern(string input, string pattern)
        {
            if (pattern.Length == 1)
            {
                return Characters(input, pattern) != -1;
            }
            else if (pattern == "\\d")
            {
                return Digits(input, pattern) != -1;
            }
            else
            {
                throw new ArgumentException($"Unhandled pattern: {pattern}");
            }


        }


    }
}
