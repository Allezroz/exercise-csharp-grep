using System;
using System.IO;
using Parser;


void RunTests()

{
    Test[] tests = 
    [
        new Test(1, "a", "apple", true),
        new Test(2, "a", "dsf", false),
        new Test(3, "\\d", "apple", false),
        new Test(4, "\\d", "12", true),
        new Test(5, "\\w", "%12a", true),
        new Test(6, "\\w", "$%", false),
        new Test(7, "\\w", " ", true),
        new Test(8, "[abc]", "apple", true),
        new Test(9, "[abc]", "dog", false),
        new Test(10, "[^abc]", "apple", false),
        new Test(11, "[^abc]", "dog", true),
    ];
    string outc;
    bool outcome;
    foreach (Test t in tests)
    {
        Console.WriteLine("***");
        Console.WriteLine($"Test {t.n} - {t.pattern} in {t.input} - expected: {t.expected}");
        outcome = Parser.Parser.MatchPattern(t.input, t.pattern) ? true : false;
        Console.WriteLine($"Outcome: {outcome} Expected: {t.expected}");
        outc = outcome == t.expected ? "PASS" : "FAIL";
        Console.WriteLine($"TEST {t.n} {outc}");
    }
    Console.WriteLine("***");

    Environment.Exit(0);
}


// comment this out before submitting
//RunTests();

// This is functionally Main() and will do nothing but handle the console input

if (args[0] != "-E")
{
    Console.WriteLine("Expected first argument to be '-E'");
    Environment.Exit(2);
}

string pattern = args[1];
string inputLine = Console.In.ReadToEnd();

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.Error.WriteLine("Logs from your program will appear here!");
 
 if (Parser.Parser.MatchPattern(inputLine, pattern))
 {
    Environment.Exit(0);
}
 else
 {
    Environment.Exit(1);
}


public struct Test
{
    public int n;
    public string pattern;
    public string input;
    public bool expected;

    public Test(int n, string pattern, string input, bool expected)
    {
        this.n = n;
        this.pattern = pattern;
        this.input = input;
        this.expected = expected;
    }
}