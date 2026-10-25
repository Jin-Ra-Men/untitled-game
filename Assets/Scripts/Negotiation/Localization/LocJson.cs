using System;
using System.Collections.Generic;
using System.Text;

namespace UntitledGame.Negotiation.Localization
{
    /// <summary>Minimal flat JSON object parser (string keys/values only). No Unity/JSON.NET dependency.</summary>
    public static class LocJson
    {
        public static Dictionary<string, string> ParseObject(string json)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            if (string.IsNullOrWhiteSpace(json)) return result;

            var i = 0;
            SkipWs(json, ref i);
            Expect(json, ref i, '{');
            SkipWs(json, ref i);

            if (Peek(json, i) == '}')
                return result;

            while (i < json.Length)
            {
                SkipWs(json, ref i);
                var key = ReadString(json, ref i);
                SkipWs(json, ref i);
                Expect(json, ref i, ':');
                SkipWs(json, ref i);
                var value = ReadString(json, ref i);
                result[key] = value;
                SkipWs(json, ref i);
                if (Peek(json, i) == ',')
                {
                    i++;
                    continue;
                }

                if (Peek(json, i) == '}')
                    break;

                throw new FormatException("Unexpected token in loc JSON near index " + i);
            }

            return result;
        }

        private static void SkipWs(string s, ref int i)
        {
            while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
        }

        private static char Peek(string s, int i) => i < s.Length ? s[i] : '\0';

        private static void Expect(string s, ref int i, char c)
        {
            if (Peek(s, i) != c)
                throw new FormatException("Expected '" + c + "' at " + i);
            i++;
        }

        private static string ReadString(string s, ref int i)
        {
            Expect(s, ref i, '"');
            var sb = new StringBuilder();
            while (i < s.Length)
            {
                var c = s[i++];
                if (c == '"') return sb.ToString();
                if (c != '\\')
                {
                    sb.Append(c);
                    continue;
                }

                if (i >= s.Length) break;
                var e = s[i++];
                switch (e)
                {
                    case '"':
                    case '\\':
                    case '/':
                        sb.Append(e);
                        break;
                    case 'n':
                        sb.Append('\n');
                        break;
                    case 'r':
                        sb.Append('\r');
                        break;
                    case 't':
                        sb.Append('\t');
                        break;
                    case 'u':
                        if (i + 4 > s.Length) throw new FormatException("Bad unicode escape");
                        var hex = s.Substring(i, 4);
                        i += 4;
                        sb.Append((char)Convert.ToInt32(hex, 16));
                        break;
                    default:
                        sb.Append(e);
                        break;
                }
            }

            throw new FormatException("Unterminated string in loc JSON");
        }
    }
}
