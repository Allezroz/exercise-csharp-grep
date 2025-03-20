using System;
using System.IO;


// This is functionally Main() and will do nothing but handle the console input

if (args.Length == 0)
        Tests.Tests.RunTests();

if (args[0] != "-E")
{
    Console.WriteLine("Expected first argument to be '-E'");
    Environment.Exit(2);
}

string pattern = args[1];
string inputLine = Console.In.ReadToEnd();

Console.Error.WriteLine("Logs from your program will appear here!");
 
 if (Parser.Parser.Grep(inputLine, pattern))
 {
    Environment.Exit(0);
}
 else
 {
    Environment.Exit(1);
}


