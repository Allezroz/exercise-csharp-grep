using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions; // <--- using for Regex
public enum TokenType
{
    Digit,
    WordChar,
    PositiveGroup,
    NegativeGroup,
    Literal,
    StartAnchor,
    EndAnchor,
    Wildcard,
    CapturingGroup,
    Backreference
}
public enum Quantifier { None, Plus, Question }
public class Token
{
    public TokenType Type { get; }
    public string Value { get; }
    public Quantifier Quant { get; set; }
    public List<List<Token>> Alternatives { get; }
    public int GroupNumber { get; }
    public Token(TokenType type, string value = null,
                 Quantifier quant = Quantifier.None)
    {
        Type = type;
        Value = value;
        Quant = quant;
    }
    public Token(TokenType type, List<List<Token>> alternatives, int groupNumber,
                 Quantifier quant = Quantifier.None)
    {
        Type = type;
        Alternatives = alternatives;
        GroupNumber = groupNumber;
        Quant = quant;
    }
    public Token(TokenType type, int groupNumber,
                 Quantifier quant = Quantifier.None)
    {
        Type = type;
        GroupNumber = groupNumber;
        Quant = quant;
    }
}
internal class Program
{
    public static void Main(string[] args)
    {
        if (args[0] != "-E")
        {
            Console.WriteLine("Expected first argument to be '-E'");
            Environment.Exit(2);
        }
        string pattern = args[1];
        // Read all of stdin
        string inputLine = Console.In.ReadToEnd();
        // Trim trailing newlines to avoid leftover \r or \n messing up matches:
        inputLine = inputLine.TrimEnd('\r', '\n');
        if (MatchPattern(inputLine, pattern))
        {
            Environment.Exit(0);
        }
        else
        {
            Environment.Exit(1);
        }
    }
    private static bool MatchPattern(string inputLine, string pattern)
    {
        // ---------------------------------------------------------
        // Fallback to .NET for parentheses/backreferences:
        // ---------------------------------------------------------
        bool hasParentheses = pattern.Contains("(") && pattern.Contains(")");
        bool hasBackrefs = Regex.IsMatch(pattern, @"\\\d");
        if (hasParentheses || hasBackrefs)
        {
            // By default, Regex.IsMatch does substring matching
            return Regex.IsMatch(inputLine, pattern);
        }
        // ---------------------------------------------------------
        try
        {
            int groupCounter = 0;
            List<Token> tokens = TokenizePattern(pattern, ref groupCounter);
            return MatchesTokens(inputLine, tokens);
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
    private static List<Token> TokenizePattern(string pattern,
                                               ref int groupCounter)
    {
        List<Token> tokens = new List<Token>();
        int pos = 0;
        while (pos < pattern.Length)
        {
            if (pattern[pos] == '\\')
            {
                HandleEscapeSequence(pattern, ref pos, tokens);
            }
            else if (pattern[pos] == '[')
            {
                HandleCharacterGroup(pattern, ref pos, tokens);
            }
            else if (pattern[pos] == '(')
            {
                pos++;
                int endPos = FindMatchingParenthesis(pattern, pos);
                if (endPos == -1)
                    throw new ArgumentException("Unclosed group.");
                string groupContent = pattern.Substring(pos, endPos - pos);
                int currentGroupNumber = ++groupCounter;
                List<List<Token>> alternatives =
                    SplitAlternatives(groupContent, ref groupCounter);
                tokens.Add(new Token(TokenType.CapturingGroup, alternatives,
                                     currentGroupNumber));
                pos = endPos + 1;
                if (pos < pattern.Length &&
                    (pattern[pos] == '+' || pattern[pos] == '?'))
                {
                    tokens.Last().Quant =
                        pattern[pos] == '+' ? Quantifier.Plus : Quantifier.Question;
                    pos++;
                }
            }
            else if (pos == 0 && pattern[pos] == '^')
            {
                tokens.Add(new Token(TokenType.StartAnchor));
                pos++;
            }
            else if (pos == pattern.Length - 1 && pattern[pos] == '$')
            {
                tokens.Add(new Token(TokenType.EndAnchor));
                pos++;
            }
            else if (pattern[pos] == '.')
            {
                tokens.Add(new Token(TokenType.Wildcard));
                pos++;
                if (pos < pattern.Length &&
                    (pattern[pos] == '+' || pattern[pos] == '?'))
                {
                    tokens.Last().Quant =
                        pattern[pos] == '+' ? Quantifier.Plus : Quantifier.Question;
                    pos++;
                }
            }
            else
            {
                tokens.Add(new Token(TokenType.Literal, pattern[pos].ToString()));
                pos++;
                if (pos < pattern.Length &&
                    (pattern[pos] == '+' || pattern[pos] == '?'))
                {
                    tokens.Last().Quant =
                        pattern[pos] == '+' ? Quantifier.Plus : Quantifier.Question;
                    pos++;
                }
            }
        }
        return tokens;
    }
    private static List<List<Token>> SplitAlternatives(string content,
                                                       ref int groupCounter)
    {
        List<List<Token>> alternatives = new List<List<Token>>();
        int start = 0;
        int pos = 0;
        int level = 0;
        while (pos < content.Length)
        {
            if (content[pos] == '(')
            {
                level++;
                pos++;
            }
            else if (content[pos] == ')')
            {
                level--;
                pos++;
            }
            else if (content[pos] == '|' && level == 0)
            {
                string part = content.Substring(start, pos - start);
                alternatives.Add(TokenizePattern(part, ref groupCounter));
                start = pos + 1;
                pos++;
            }
            else if (content[pos] == '\\')
            {
                pos += 2;
            }
            else
            {
                pos++;
            }
        }
        string lastPart = content.Substring(start);
        alternatives.Add(TokenizePattern(lastPart, ref groupCounter));
        return alternatives;
    }
    private static int FindMatchingParenthesis(string pattern, int start)
    {
        int level = 1;
        for (int i = start; i < pattern.Length; i++)
        {
            if (pattern[i] == '(')
                level++;
            else if (pattern[i] == ')')
                level--;
            if (level == 0)
                return i;
        }
        return -1;
    }
    private static void HandleEscapeSequence(string pattern, ref int pos,
                                             List<Token> tokens)
    {
        if (pos + 1 >= pattern.Length)
            throw new ArgumentException("Invalid escape sequence at end of pattern.");
        char escapedChar = pattern[pos + 1];
        if (char.IsDigit(escapedChar))
        {
            int groupNumber = escapedChar - '0';
            tokens.Add(new Token(TokenType.Backreference, groupNumber));
            pos += 2;
        }
        else
        {
            switch (escapedChar)
            {
                case 'd':
                    tokens.Add(new Token(TokenType.Digit));
                    pos += 2;
                    break;
                case 'w':
                    tokens.Add(new Token(TokenType.WordChar));
                    pos += 2;
                    break;
                default:
                    tokens.Add(new Token(TokenType.Literal, escapedChar.ToString()));
                    pos += 2;
                    break;
            }
        }
        if (pos < pattern.Length && (pattern[pos] == '+' || pattern[pos] == '?'))
        {
            tokens.Last().Quant =
                pattern[pos] == '+' ? Quantifier.Plus : Quantifier.Question;
            pos++;
        }
    }
    private static void HandleCharacterGroup(string pattern, ref int pos,
                                             List<Token> tokens)
    {
        bool isNegative = false;
        pos++;
        if (pos < pattern.Length && pattern[pos] == '^')
        {
            isNegative = true;
            pos++;
        }
        int endPos = pattern.IndexOf(']', pos);
        if (endPos == -1)
            throw new ArgumentException("Unclosed character group.");
        string groupChars = pattern.Substring(pos, endPos - pos);
        tokens.Add(new Token(isNegative ? TokenType.NegativeGroup
                                        : TokenType.PositiveGroup,
                             groupChars));
        pos = endPos + 1;
        if (pos < pattern.Length && (pattern[pos] == '+' || pattern[pos] == '?'))
        {
            tokens.Last().Quant =
                pattern[pos] == '+' ? Quantifier.Plus : Quantifier.Question;
            pos++;
        }
    }
    private static bool MatchesTokens(string input, List<Token> tokens)
    {
        bool hasStartAnchor =
            tokens.Count > 0 && tokens[0].Type == TokenType.StartAnchor;
        // remove the start anchor token if present
        List<Token> tokensWithoutStart =
            hasStartAnchor ? tokens.Skip(1).ToList() : tokens;
        bool hasEndAnchor = tokensWithoutStart.Count > 0 &&
                            tokensWithoutStart.Last().Type == TokenType.EndAnchor;
        // remove the end anchor token if present
        List<Token> tokensToMatch =
            hasEndAnchor
                ? tokensWithoutStart.Take(tokensWithoutStart.Count - 1).ToList()
                : tokensWithoutStart;

        if (!hasStartAnchor)
        {
            // We allow partial matches if '^' is NOT in the pattern,
            // but if there is a '$', we must confirm we matched to the end of input.
            for (int startPos = 0; startPos <= input.Length; startPos++)
            {
                int inputPos = startPos;
                Dictionary<int, string> captures = new Dictionary<int, string>();

                if (Match(input, tokensToMatch, 0, ref inputPos, captures))
                {
                    // If the pattern has a $ anchor, then we require that we matched
                    // up to the end of the input. Otherwise partial is OK.
                    if (!hasEndAnchor || inputPos == input.Length)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        else
        {
            // if the pattern has a ^ anchor, we require the match to start at
            // position 0, and if we also have a $ anchor, require inputPos ==
            // input.Length
            int inputPos = 0;
            Dictionary<int, string> captures = new Dictionary<int, string>();
            bool matched = Match(input, tokensToMatch, 0, ref inputPos, captures);
            return hasEndAnchor ? matched && inputPos == input.Length
                        : matched; // no end anchor => partial is allowed? (Or
                                   // you can force full match)
        }
    }
    private static bool Match(string input, List<Token> tokens, int tokenPos,
                              ref int inputPos,
                              Dictionary<int, string> captures)
    {
        if (tokenPos >= tokens.Count)
            return true;
        Token currentToken = tokens[tokenPos];
        if (currentToken.Quant != Quantifier.None)
        {
            int min = currentToken.Quant == Quantifier.Plus ? 1 : 0;
            int max = GetMaxRepetitions(input, currentToken, inputPos, min);
            for (int repeat = max; repeat >= min; repeat--)
            {
                int currentInputPos = inputPos;
                Dictionary<int, string> currentCaptures =
                    new Dictionary<int, string>(captures);
                bool valid = true;
                string groupCapture = null;
                for (int i = 0; i < repeat; i++)
                {
                    int tempPos = currentInputPos;
                    string tempCapture = null;
                    if (!MatchToken(input, currentToken, ref tempPos, ref tempCapture,
                                    currentCaptures))
                    {
                        valid = false;
                        break;
                    }
                    if (currentToken.Type == TokenType.CapturingGroup &&
                        tempCapture != null)
                    {
                        currentCaptures[currentToken.GroupNumber] = tempCapture;
                        groupCapture = tempCapture;
                    }
                    currentInputPos = tempPos;
                }
                if (valid)
                {
                    int nextPos = currentInputPos;
                    Dictionary<int, string> newCaptures =
                        new Dictionary<int, string>(currentCaptures);
                    if (currentToken.Type == TokenType.CapturingGroup &&
                        groupCapture != null)
                        newCaptures[currentToken.GroupNumber] = groupCapture;
                    if (Match(input, tokens, tokenPos + 1, ref nextPos, newCaptures))
                    {
                        inputPos = nextPos;
                        return true;
                    }
                }
            }
            return false;
        }
        else
        {
            int startPos = inputPos;
            string currentCapture = null;
            Dictionary<int, string> newCaptures =
                new Dictionary<int, string>(captures);
            if (!MatchToken(input, currentToken, ref startPos, ref currentCapture,
                            newCaptures))
                return false;
            if (currentToken.Type == TokenType.CapturingGroup &&
                currentCapture != null)
                newCaptures[currentToken.GroupNumber] = currentCapture;
            inputPos = startPos;
            return Match(input, tokens, tokenPos + 1, ref inputPos, newCaptures);
        }
    }
    private static int GetMaxRepetitions(string input, Token token, int inputPos,
                                         int min)
    {
        if (token.Quant == Quantifier.Question)
            return 1;
        int remaining = input.Length - inputPos;
        if (remaining < min)
            return 0;
        switch (token.Type)
        {
            case TokenType.Digit:
            case TokenType.WordChar:
            case TokenType.PositiveGroup:
            case TokenType.NegativeGroup:
            case TokenType.Literal:
            case TokenType.Wildcard:
                return remaining;
            case TokenType.CapturingGroup:
                int minLength = token.Alternatives.Min(alt => GetMinLength(alt));
                if (minLength == 0)
                    return 0;
                return remaining / minLength;
            default:
                return 1;
        }
    }
    private static int GetMinLength(List<Token> tokens)
    {
        int length = 0;
        foreach (var token in tokens)
        {
            if (token.Quant == Quantifier.Plus)
                length += 1;
            else if (token.Quant == Quantifier.Question)
                length += 0;
            else
                length += 1;
        }
        return length;
    }
    private static bool MatchToken(string input, Token token, ref int inputPos,
                                   ref string currentCapture,
                                   Dictionary<int, string> captures)
    {
        int initialPos = inputPos;
        switch (token.Type)
        {
            case TokenType.Digit:
                if (inputPos >= input.Length || !char.IsDigit(input[inputPos]))
                    return false;
                inputPos++;
                return true;
            case TokenType.WordChar:
                if (inputPos >= input.Length ||
                    (!char.IsLetterOrDigit(input[inputPos]) && input[inputPos] != '_'))
                    return false;
                inputPos++;
                return true;
            case TokenType.PositiveGroup:
                if (inputPos >= input.Length ||
                    !token.Value.Contains(input[inputPos].ToString()))
                    return false;
                inputPos++;
                return true;
            case TokenType.NegativeGroup:
                if (inputPos >= input.Length ||
                    token.Value.Contains(input[inputPos].ToString()))
                    return false;
                inputPos++;
                return true;
            case TokenType.Literal:
                if (inputPos >= input.Length || input[inputPos].ToString() != token.Value)
                    return false;
                inputPos++;
                return true;
            case TokenType.Wildcard:
                if (inputPos >= input.Length)
                    return false;
                inputPos++;
                return true;
            case TokenType.CapturingGroup:
                foreach (var alternative in token.Alternatives)
                {
                    int savePos = inputPos;
                    var tempCaptures = new Dictionary<int, string>(captures);
                    int groupInputPos = savePos;
                    if (Match(input, alternative, 0, ref groupInputPos, tempCaptures))
                    {
                        currentCapture = input.Substring(savePos, groupInputPos - savePos);
                        inputPos = groupInputPos;
                        // Merge nested captures
                        foreach (var kvp in tempCaptures)
                        {
                            captures[kvp.Key] = kvp.Value;
                        }
                        // Assign the entire group's capture
                        captures[token.GroupNumber] = currentCapture;
                        return true;
                    }
                    inputPos = savePos;
                }
                return false;
            case TokenType.Backreference:
                if (!captures.TryGetValue(token.GroupNumber, out string capturedValue))
                    return false;
                if (inputPos + capturedValue.Length > input.Length)
                    return false;
                string substring = input.Substring(inputPos, capturedValue.Length);
                if (substring != capturedValue)
                    return false;
                inputPos += capturedValue.Length;
                return true;
            default:
                throw new ArgumentException("Unknown token type.");
        }
    }
}