using System;
using System.IO;
using Parser;

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

