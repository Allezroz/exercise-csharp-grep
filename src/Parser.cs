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
                return !neg;
            }
            return neg;

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

        // Take the string and pattern, handle the stepping and parsing.
        // Can pass an idx by reference to keep the starting point and 'cursor' separate
        // or it can pass the substring up and receive an index integer back or -1 on false? - I like this better
        private static Queue<string> Tokenize(string pattern)
        {
            int idx = 0;
            string tkn = "";
            Queue<string> ret = new Queue<string>();
            
            while (idx < pattern.Length)
            {
                if (pattern[idx] == '\\') // word, digit, etc
                {
                    tkn = tkn + pattern[idx];
                    idx += 1;
                    tkn = tkn + pattern[idx];
                }
                else if (pattern[idx] == '[') // group
                {
                    while (idx < pattern.Length && pattern[idx] != ']')
                    {
                        tkn = tkn + pattern[idx];
                        idx += 1;
                    }
                    tkn = tkn + pattern[idx]; // adds the closing bracket
                }
                else // single character of any sort including anchors
                {
                    tkn = tkn + pattern[idx];
                }

                ret.Enqueue(tkn);
                idx+=1;
                tkn = "";
            }

            return ret;

        }

        public static bool MatchPattern(string input, string pattern, bool DEBUG)
        {
            Queue<string> originalQ = Tokenize(pattern);
            Queue<string> Q = new Queue<string>();
            string pat;
            int INidx = 0;
            int OUTidx = 0;
            bool ret = true;


            while (OUTidx < input.Length)
            {
                ret = true;
                INidx = OUTidx;
                Q = new Queue<string>(originalQ);


                if (DEBUG)
                    foreach (var item in originalQ)
                        Console.WriteLine(item);
                            

                while (Q.Count > 0 && INidx < input.Length && ret == true)
                {
                    pat = Q.Dequeue();

                    if (pat == "^")
                    {
                        if (INidx != 0)
                            return false;
                        pat = Q.Dequeue();
                    }

                    if (pat == "\\d")
                    {
                        ret = Digits(input[INidx], pat);
                    }
                    else if (pat == "\\w")
                    {
                        ret = Alpha(input[INidx], pat);
                    }
                    else if (pat.StartsWith("[^")) // neg fails on fail
                    {
                        if (!Characters(input[INidx], pat))
                            return false;
                    }
                    else
                    {
                        ret = Characters(input[INidx], pat);
                    }

                    INidx += 1;
                }
                // needs a catch for the case where we hit end of string with an end of string anchor
                // needs to handle case with start of string and end of string anchors

                if (INidx == input.Length && Q.Count == 1 && Q.Dequeue() == "$" && ret == true) // EOS anchor
                    return true;

                if (Q.Count == 0 && ret==true) 
                    return true;
                OUTidx += 1;
            }

            return false;

        }


    }
}
