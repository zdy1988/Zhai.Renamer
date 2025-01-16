using System;
using System.Text.RegularExpressions;

namespace Zhai.Renamer.Core
{
    internal static class ModifierExecutor
    {
        internal static string Clear(this string _) => string.Empty;

        internal static string AppendBefore(this string s, string text)
        {
            return text + s;
        }

        internal static string AppendAfter(this string s, string text)
        {
            return s + text;
        }

        internal static string AppendAtPosition(this string s, string text, int n)
        {
            if (n > s.Length) return s.Insert(s.Length, text);
            if (n < 0) return s.Insert(0, text);
            return s.Insert(n, text);
        }


        internal static string ExtractChinese(this string s)
        {
            var result = "";
            foreach (var match in Regex.Matches(s, @"[\u4e00-\u9fa5]")) result += match;
            return result;
        }

        //String.Insert
        //internal static string AppendAtPosition(this string s, int position, string text)

        internal static string ExtractNumeric(this string s)
        {
            var result = "";
            foreach (var match in Regex.Matches(s, @"\d+")) result += match;
            return result;
        }

        internal static string ExtractAlphanumeric(this string s)
        {
            return Regex.Replace(s, "[^a-zA-Z0-9 ]", "");
        }

        //Clean
        //Strip out all nonalphanumeric characters except whitespaces ( ), periods (.), at symbols (@), and hyphens (-)
        //msdn.microsoft.com/en-us/library/844skk0h(v=vs.110).aspx
        internal static string Clean(this string s)
        {
            return Regex.Replace(s, @"[^\w\.\s@-]", "");
        }

        //Returns a number of characters from the left-hand side of a string
        internal static string KeepLeft(this string s, int n)
        {
            if (n >= s.Length) return s;
            if (n < 0) return string.Empty;

            string result = "";
            for (int i = 0; i < n; i++)
            {
                result += s[i];
            }
            return result;
        }

        //Returns a number of characters from the right-hand side of a string
        internal static string KeepRight(this string s, int n)
        {
            if (n >= s.Length) return s;
            if (n < 0) return string.Empty;

            string result = "";
            for (int i = 0; i < n; i++)
            {
                result = s[s.Length - 1 - i] + result;
            }
            return result;
        }

        //Trims a number of characters from the left hand side of a string
        internal static string TrimLeft(this string s, int n)
        {
            return KeepRight(s, s.Length - n);
        }

        //Trims a number of characters from the right hand side of a string
        internal static string TrimRight(this string s, int n)
        {
            return KeepLeft(s, s.Length - n);
        }

        //stackoverflow.com/questions/1943273/convert-all-first-letter-to-upper-case-rest-lower-for-each-word
        internal static string CapitalizeEachWord(this string s)
        {
            if (s.Length == 0) return string.Empty;

            char[] a = s.ToLower().ToCharArray();

            for (int i = 0; i < a.Length; i++)
            {
                a[i] = i == 0 || a[i - 1] == ' ' ? char.ToUpper(a[i]) : a[i];
            }

            return new string(a);
        }

        //Capitalize only the first character
        internal static string SentenceCase(this string s)
        {
            return s[0].ToString().ToUpper() + s[1..].ToLower();
        }

        //Fill with 0's until reach the length of the specified number
        internal static string CompleteZeros(this int n, int max)
        {
            string number = n.ToString();

            int maxDigits = max.ToString().Length;
            int requiredZeros = maxDigits - number.Length;

            for (int i = 0; i < requiredZeros; i++)
            {
                number = "0" + number;
            }
            return number;
        }

        //Case insensitive search and replace
        //stackoverflow.com/questions/6275980/string-replace-by-ignoring-case
        internal static string ReplaceInsensitive(this string s, string search, string replacement)
        {
            return Regex.Replace(
                s,
                Regex.Escape(search),
                (replacement),
                RegexOptions.IgnoreCase
            );
        }

        //Perform a Regex match on the current string
        internal static bool RegexMatch(this string s, string pattern)
        {
            try
            {
                return new Regex(pattern).IsMatch(s);
                //var match = new Regex(pattern).Match(s);
                //return match.Success;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        //Check if given Regex pattern is valid
        internal static bool ValidPattern(string pattern)
        {
            try
            {
                new Regex(pattern);
                return true;
            }
            catch
            {
                return false;
            }
        }

        //Add spaces to PascalCase string
        //stackoverflow.com/questions/155303/net-how-can-you-split-a-caps-delimited-string-into-an-array
        internal static string SplitPascalCase(this string s)
        {
            return Regex.Replace(
                s,
                "([a-z](?=[A-Z])|[A-Z](?=[A-Z][a-z]))",
                "$1 "
            );
        }

        // 提取时间
        internal static bool RegexMatchTime(this string s, out string time)
        {
            Regex reg = new Regex(@"((?<!\d)((\d{2,4}(\.|年|\/|\-))((((0?[13578]|1[02])(\.|月|\/|\-))((3[01])|([12][0-9])|(0?[1-9])))|(0?2(\.|月|\/|\-)((2[0-8])|(1[0-9])|(0?[1-9])))|(((0?[469]|11)(\.|月|\/|\-))((30)|([12][0-9])|(0?[1-9]))))|((([0-9]{2})((0[48]|[2468][048]|[13579][26])|((0[48]|[2468][048]|[3579][26])00))(\.|年|\/|\-))0?2(\.|月|\/|\-)29))日?(?!\d))");

            time = reg.Match(s).Value;

            return reg.Match(s).Success;
        }
    }
}