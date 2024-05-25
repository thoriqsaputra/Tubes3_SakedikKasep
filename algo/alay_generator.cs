using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;

public class AlayConverter
{
    private static readonly Dictionary<string, List<string>> replacements = new Dictionary<string, List<string>>()
    {
        {"a", new List<string> {"4", "A", "a"}}, {"b", new List<string> {"B", "b", "8", "13"}},
        {"c", new List<string> {"C", "c"}}, {"d", new List<string> {"D", "d"}},
        {"e", new List<string> {"3", "E", "e"}}, {"f", new List<string> {"F", "f"}},
        {"g", new List<string> {"G", "g", "6", "9"}}, {"h", new List<string> {"H", "h"}},
        {"i", new List<string> {"!", "1", "I", "i"}}, {"j", new List<string> {"J", "j"}},
        {"k", new List<string> {"K", "k"}}, {"l", new List<string> {"L", "l", "1"}},
        {"m", new List<string> {"M", "m"}}, {"n", new List<string> {"N", "n"}},
        {"o", new List<string> {"0", "O", "o"}}, {"p", new List<string> {"P", "p"}},
        {"q", new List<string> {"Q", "q", "9"}}, {"r", new List<string> {"R", "r", "12", "i2", "I2"}},
        {"s", new List<string> {"S", "s", "5"}}, {"t", new List<string> {"T", "t", "7"}},
        {"u", new List<string> {"U", "u"}}, {"v", new List<string> {"V", "v"}},
        {"w", new List<string> {"W", "w"}}, {"x", new List<string> {"X", "x"}},
        {"y", new List<string> {"Y", "y"}}, {"z", new List<string> {"Z", "z", "2"}}
    };

    private static readonly Random random = new Random();
    private static readonly Dictionary<string, string> reverseReplacements;
    private static readonly string regexPattern;

    static AlayConverter()
    {
        reverseReplacements = new Dictionary<string, string>();
        foreach (var pair in replacements)
        {
            foreach (var val in pair.Value)
            {
                if (!reverseReplacements.ContainsKey(val.ToLower()))
                {
                    reverseReplacements[val.ToLower()] = pair.Key;
                }
            }
        }
        reverseReplacements["12"] = "r";
        reverseReplacements["i2"] = "r";
        reverseReplacements["I2"] = "r";
        
        var sortedKeys = reverseReplacements.Keys.OrderByDescending(s => s.Length).ToList();
        regexPattern = string.Join("|", sortedKeys.Select(Regex.Escape));
    }

    public static string ConvertAlay(string fullName, bool useNumberSymbol = true, bool useCaseMix = true, bool useVowelRemoval = false)
    {
        string modifiedName = fullName.ToLower();

        if (useNumberSymbol)
        {
            modifiedName = new string(modifiedName.Select(c => replacements.ContainsKey(c.ToString()) ? replacements[c.ToString()][random.Next(replacements[c.ToString()].Count)][0] : c).ToArray());
        }

        if (useCaseMix)
        {
            modifiedName = new string(modifiedName.Select(c => random.Next(2) == 0 ? char.ToUpper(c) : c).ToArray());
        }

        if (useVowelRemoval)
        {
            var vowels = new HashSet<char> { 'a', 'e', 'i', 'o', 'u' };
            modifiedName = new string(modifiedName.Where(c => !vowels.Contains(c)).ToArray());
        }

        return modifiedName;
    }

    public static string RevertAlay(string alayName)
    {
        Regex regex = new Regex(regexPattern, RegexOptions.IgnoreCase);
        string modifiedName = regex.Replace(alayName, match =>
        {
            string key = match.Value.ToLower();
            if (reverseReplacements.ContainsKey(key))
            {
                return reverseReplacements[key];
            }
            return match.Value;
        });

        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(modifiedName.ToLower());
    }
}
