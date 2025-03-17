using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests
{

    static class Tests
    {
        

        public static void RunTests(bool DEBUG)

        {
            Test[] tests =
            [
                new Test(1, "a", "apple", true, "Single Char"),
                new Test(2, "a", "dsf", false, "Single Char"),
                new Test(3, "\\d", "apple", false, "Single Digit"),
                new Test(4, "\\d", "12", true, "Single Digit"),
                new Test(5, "\\w", "%12a", true, "Single Word WC"),
                new Test(6, "\\w", "$%", false, "Single Word WC"),
                new Test(7, "\\w", " ", true, "Single Word WC space"),
                new Test(8, "[abc]", "apple", true, "Char Group"),
                new Test(9, "[abc]", "dog", false, "Char Group"),
                new Test(10, "[^abc]", "apple", false, "NOT Char Group"),
                new Test(11, "[^abc]", "dog", true, "NOT Char Group"),
                new Test(12, "\\w\\w", "dog", true, "Multiple Word"),
                new Test(13, "\\d \\d", "dog", false, "Multiple fail case"),
                new Test(14, "\\w[abo]\\w", "dog", true, "Word and Group pass"),
                new Test(15, "do[^g]", "dog", false, "Word and Group fail"),
                new Test(16, "do[^d]", "dog", true, "Word and Group pass"),
                new Test(17, "^do[^d]", "dog", true, "Start anchor pass"),
                new Test(18, "^do[^d]", "gdog", false, "Start anchor fail"),
                new Test(19, "do[^d]$", "gdog", true, "End anchor pass"),
                new Test(20, "do[^g]$", "gdog", false, "End anchor fail (char parse)"),
                new Test(21, "gdo$", "gdog", false, "End anchor fail (not EOS)"),
                new Test(22, "^gdog$", "gdog", true, "Double Anchor Pass"),
        ];
            string outc;
            bool outcome;
            foreach (Test t in tests)
            {

                outcome = Parser.Parser.MatchPattern(t.input, t.pattern, DEBUG) ? true : false;

                if (DEBUG || !(outcome == t.expected)) Console.WriteLine("***");
                if (DEBUG || !(outcome == t.expected)) Console.WriteLine($"Test {t.n} - {t.pattern} in {t.input} - Description: {t.description}");
                if (DEBUG || !(outcome == t.expected)) Console.WriteLine($"Outcome: {outcome} Expected: {t.expected}");
                outc = outcome == t.expected ? "PASS" : "FAIL";
                Console.WriteLine($"TEST {t.n} {outc}");
            }
            Console.WriteLine("***");

            Environment.Exit(2);
        }
    }
    public struct Test
    {
        public int n;
        public string pattern;
        public string input;
        public bool expected;
        public string description;

        public Test(int n, string pattern, string input, bool expected, string description)
        {
            this.n = n;
            this.pattern = pattern;
            this.input = input;
            this.expected = expected;
            this.description = description;
        }
    }
}
