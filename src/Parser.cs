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

                
                idx+=1;

                // handle one or more and zero or one

                if (idx < pattern.Length && "+?".Contains(pattern[idx]))
                {
                    tkn = tkn + pattern[idx];
                    idx += 1;
                }

                ret.Enqueue(tkn);
                tkn = "";
            }

            return ret;

        }
        public static bool HandlePattern(char c, string pat) => pat switch
        {
            "\\d" => Digits(c, pat),
            "\\w" => Alpha(c, pat),
            _ => Characters(c, pat)
        };

        public static bool MatchPattern(string input, string pattern, bool DEBUG)
        {
            Queue<string> originalQ = Tokenize(pattern);
            Queue<string> Q = new Queue<string>();
            string pat;
            int INidx = 0;
            int OUTidx = 0;
            bool ret = true;
            bool OneOrMore;
            bool ZeroOrOne;
            int matches;

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
                    
                    // Start of Line
                    if (pat == "^")
                    {
                        if (INidx != 0)
                            return false;
                        pat = Q.Dequeue();
                    }

                    // One or More handling
                    matches = 0;
                    OneOrMore = false;
                    ZeroOrOne = false;

                    if (pat.EndsWith('+'))
                    {
                        OneOrMore = true;
                        pat = pat.Remove(pat.Length - 1);
                    }
                    else if (pat.EndsWith('?'))
                    {
                        ZeroOrOne = true;
                        pat = pat.Remove(pat.Length - 1);
                    }

                    // Patterns
                    while ((matches == 0 || OneOrMore) && ret && INidx < input.Length)
                    {
                        ret = HandlePattern(input[INidx], pat);
                        matches += ret ? 1 : 0;

                        if (OneOrMore && ret)
                            INidx += 1;
                        else if ((OneOrMore && matches >= 1) || (ZeroOrOne && matches == 0))
                        {
                            INidx -= 1;
                            ret = true;
                            break;
                        }
                    }
                    

                    INidx += 1;
                }
                // needs a catch for the case where we hit end of string with an end of string anchor
                // needs to handle case with start of string and end of string anchors
                
                if (Q.Count == 0 && ret == true)
                    return true;
                if (INidx == input.Length && Q.Count == 1 && Q.Dequeue() == "$" && ret == true) // EOS anchor
                    return true;

                OUTidx += 1;
            }

            return false;

        }


    }
}
