using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TINY_Compiler;

public enum Token_Class
{
    If, Int, Float, String, Read, Write, Repeat, Until, Elseif, Else, Then, Return, Endl,
    Semicolon, Comma, LParanthesis, RParanthesis, LCurlyBracket, RCurlyBracket,
    EqualOp, LessThanOp, GreaterThanOp, NotEqualOp, AssignmentOp, AndOp, OrOp,
    PlusOp, MinusOp, MultiplyOp, DivideOp,
    Identifier, Constant, Comment, SingleQuote, DoubleQuote, Main, End
}
namespace TINY_Compiler
{
    public class Token
    {
       public string lex;
       public Token_Class token_type;
    }

    public class Scanner
    {
        public List<Token> Tokens = new List<Token>();
        Dictionary<string, Token_Class> ReservedWords = new Dictionary<string, Token_Class>();
        Dictionary<string, Token_Class> Operators = new Dictionary<string, Token_Class>();

        public Scanner()
        {
            ReservedWords.Add("if", Token_Class.If);
            ReservedWords.Add("int", Token_Class.Int);
            ReservedWords.Add("float", Token_Class.Float);
            ReservedWords.Add("string", Token_Class.String);
            ReservedWords.Add("read", Token_Class.Read);
            ReservedWords.Add("write", Token_Class.Write);
            ReservedWords.Add("repeat", Token_Class.Repeat);
            ReservedWords.Add("until", Token_Class.Until);
            ReservedWords.Add("elseif", Token_Class.Elseif);
            ReservedWords.Add("else", Token_Class.Else);
            ReservedWords.Add("then", Token_Class.Then);
            ReservedWords.Add("return", Token_Class.Return);
            ReservedWords.Add("endl", Token_Class.Endl);
            ReservedWords.Add("main", Token_Class.Main);
            ReservedWords.Add("end", Token_Class.End);
   
            Operators.Add(";", Token_Class.Semicolon);
            Operators.Add(",", Token_Class.Comma);
            Operators.Add("(", Token_Class.LParanthesis);
            Operators.Add(")", Token_Class.RParanthesis);
            Operators.Add("{", Token_Class.LCurlyBracket);
            Operators.Add("}", Token_Class.RCurlyBracket);
            Operators.Add("=", Token_Class.EqualOp);
            Operators.Add("<", Token_Class.LessThanOp);
            Operators.Add(">", Token_Class.GreaterThanOp);
            Operators.Add("!=", Token_Class.NotEqualOp);
            Operators.Add(":=", Token_Class.AssignmentOp);
            Operators.Add("and", Token_Class.AndOp);
            Operators.Add("or", Token_Class.OrOp);
            Operators.Add("+", Token_Class.PlusOp);
            Operators.Add("-", Token_Class.MinusOp);
            Operators.Add("*", Token_Class.MultiplyOp);
            Operators.Add("/", Token_Class.DivideOp);
            Operators.Add("'", Token_Class.SingleQuote);
            Operators.Add("\"", Token_Class.DoubleQuote);
        }

    public void StartScanning(string SourceCode)
        {
            for(int i=0; i<SourceCode.Length;i++)
            {
                char CurrentChar = SourceCode[i];
                string CurrentLexeme = "";

                if (CurrentChar == ' ' || CurrentChar == '\r' || CurrentChar == '\n')
                    continue;

                // identifier or reserved word
                else if (char.IsLetter(CurrentChar)) //if you read a character(start of identifier or reserved word)
                {
                    CurrentLexeme = "";
                    while (char.IsLetterOrDigit(CurrentChar))
                    {
                        CurrentLexeme += CurrentChar;
                        if (i < SourceCode.Length)
                            CurrentChar = SourceCode[++i];
                    }
                    i--;
                  
                }
                // 3. String => \"[^\"]*\"
                else if (CurrentChar == '\"')
                {
                    CurrentLexeme += CurrentChar;
                    i++;
                    while (i < SourceCode.Length && SourceCode[i] != '\"')
                    {
                        CurrentLexeme += SourceCode[i];
                        i++;
                    }
                       if (i + 1 < SourceCode.Length)
                         CurrentLexeme += SourceCode[i]; // add closing quote(  ")
                }

                //comment
                else if (CurrentChar == '/' && i + 1 < SourceCode.Length && SourceCode[i + 1] == '*')
                {
                    CurrentLexeme += "/*";
                    i += 2;
                    while (i + 1 < SourceCode.Length && !(SourceCode[i] == '*' && SourceCode[i + 1] == '/'))
                    {
                        CurrentLexeme += SourceCode[i];
                        i++;
                    }
                    if (i + 1 < SourceCode.Length)
                    {
                        CurrentLexeme += "*/";
                        i++;
                    }
                }

                //number
                else if (CurrentChar >= '0' && CurrentChar <= '9')
                {
                    bool used_dot= false; 
                    while ( (i<SourceCode.Length && char.IsDigit(CurrentChar)) || (CurrentChar=='.' && !used_dot))
                    {
                        if(CurrentChar == '.')
                            used_dot = true;
                        CurrentLexeme += CurrentChar;
                        if(i+1<SourceCode.Length)
                            CurrentChar = SourceCode[++i];
                    }
                    i -= 1;
                }

                // 5. Arithmetic Operator => + - * /
                else if ("+-*/".Contains(CurrentChar))
                {
                    CurrentLexeme += CurrentChar;
                }

                // 6. Conditional Operator => < > = <>
                else if ("<>=:".Contains(CurrentChar))
                {
                    char next = i + 1 < SourceCode.Length ? SourceCode[i + 1] : '\0';

                    if ((CurrentChar == '<' && next == '>') || (CurrentChar == ':' && next == '='))
                    {
                        CurrentLexeme = $"{CurrentChar}{next}";
                        i++;
                    }
                    else
                    {
                        CurrentLexeme = CurrentChar.ToString();
                    }
                }

                // 7. Boolean Operator => && ||
                else if ((CurrentChar == '&' && i + 1 < SourceCode.Length && SourceCode[i + 1] == '&') ||
                         (CurrentChar == '|' && i + 1 < SourceCode.Length && SourceCode[i + 1] == '|'))
                {
                    CurrentLexeme += CurrentChar.ToString() + SourceCode[i + 1];
                    i++;
                }
                else
                {
                    CurrentLexeme += CurrentChar;
                }
                FindTokenClass(CurrentLexeme);
            }
            
            TINY_Compiler.TokenStream = Tokens;
        }
        void FindTokenClass(string Lex)
        {

            if (Lex == "")
                return;
            Token Tok = new Token();
            Tok.lex = Lex;

            if (ReservedWords.ContainsKey(Lex))
            {
                Tok.token_type = ReservedWords[Lex];
                Tokens.Add(Tok);
            }
            else if (isIdentifier(Lex))
            {
                Tok.token_type = Token_Class.Identifier;
                Tokens.Add(Tok);
            }
            else if (isConstant(Lex))
            {
                Tok.token_type = Token_Class.Constant;
                Tokens.Add(Tok);
            }
            // is the lex a string ?
            else if (isString(Lex))
            {
                Tok.token_type = Token_Class.String;
                Tokens.Add(Tok);
            }
            // is the lex an operator ?
            else if (Operators.ContainsKey(Lex))
            {
                Tok.token_type = Operators[Lex];
                Tokens.Add(Tok);
            }
            else if (isComment(Lex))
            {
                Tok.token_type = Token_Class.Comment;
                Tokens.Add(Tok);
            }
            else
            {
                Errors.Error_List.Add(Lex);
            }

        }
        private bool isString(string lex)
        {
            var exp = new Regex("^\".*\"$");
            return exp.IsMatch(lex);
        }

        bool isIdentifier(string lex)
        {
            // Check if the lex is an identifier or not.
            var exp = new Regex(@"^[a-zA-Z][a-zA-Z0-9]*$", RegexOptions.Compiled);
            return exp.IsMatch(lex);
        }
        bool isComment(string lex)
        {
            if (lex.Length < 4)
                return false;
            if (lex[0] == '/' && lex[1] == '*' && lex[lex.Length - 2] == '*' && lex[lex.Length - 1] == '/')
                return true;
            return false;
        }
        bool isConstant(string lex)
        {
            // Check if the lex is a constant (Number) or not.
       
            var exp = new Regex(@"^[0-9]+(\.[0-9]+)?$", RegexOptions.Compiled);
            return exp.IsMatch(lex);
        }
    }
}

