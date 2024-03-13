using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RWGallary
{
    public static class JsonUtils
    {
        public static List<Token> Tokenize(string jsonString)
        {
            List<Token> tokens = new List<Token>();

            // JSON 문자열을 문자 단위로 순회하면서 토큰화
            StringBuilder sb = new StringBuilder();
            bool inString = false; // 문자열 안에 있는지 여부를 나타내는 플래그
            char prev = '\0';
            foreach (char c in jsonString)
            {
                if (prev != '\\' && c == '"')
                {
                    // 큰 따옴표를 만나면 문자열 안/밖을 번갈아가며 처리
                    inString = !inString;
                }
                else if (inString)
                {
                    // 문자열 안에 있는 경우에는 문자를 그대로 추가
                    sb.Append(c);
                }
                else if (c == ',' || c == '{' || c == '}' || c == '[' || c == ']' || c == ':')
                {
                    // 문자열 밖에서 쉼표나 중괄호를 만나면 이전까지의 문자열을 토큰으로 추가
                    if (sb.Length > 0)
                    {
                        tokens.Add(new Token(sb.ToString()));
                        sb.Clear();
                    }
                }
                else if (!char.IsWhiteSpace(c))
                {
                    // 공백이 아닌 문자는 일반 문자열에 추가
                    sb.Append(c);
                }

                prev = c;
            }

            // 마지막에 남은 문자열도 토큰으로 추가
            if (sb.Length > 0)
            {
                tokens.Add(new Token(sb.ToString()));
            }

            return tokens;
        }

        public enum TokenType
        {
            String,
            Number,
            True,
            False,
        }

        // JSON 토큰을 나타내는 클래스
        public class Token
        {
            public TokenType Type { get; private set; }
            public string Value { get; private set; }

            public int IntValue => int.Parse(Value);

            public Token(string value)
            { 
                if (value == "true")
                {
                    Type = TokenType.True;
                }
                else if (value == "false")
                {
                    Type = TokenType.False;
                }
                else if (int.TryParse(value, out _))
                {
                    Type = TokenType.Number;
                }
                else
                {
                    Type = TokenType.String;
                }

                Value = value;
            }
        }
    }
}
