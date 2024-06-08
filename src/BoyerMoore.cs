using System;
using System.Collections.Generic;
using System.Linq;

public class BoyerMoore
{
   public static double BmMatch(string text, string pattern)
    {
        Dictionary<char, int> last = BuildLast(pattern);
        int n = text.Length;
        int m = pattern.Length;
        int i = m - 1;
        double maxSimilarity = 0;

        if (i > n - 1)
            return -1;

        int j = m - 1;

        do
        {
            if (pattern[j] == text[i])
            {
                if (j == 0)
                {
                    return 1;
                }
                else
                {
                    i--;
                    j--;
                }
            }
            else
            {
                int lo = last.ContainsKey(text[i]) ? last[text[i]] : -1;
                i = i + m - Math.Min(j, 1 + lo);
                j = m - 1;

                int startIndex = i - m + 1;
                if (startIndex >= 0 && startIndex + m<= text.Length)
                {
                    double similarity = CalculateLevenshteinSimilarity(text.Substring(startIndex, m), pattern);
                    if (similarity > maxSimilarity)
                    {
                        maxSimilarity = similarity;
                    }
                }
            }
        } while (i <= n - 1);

        return maxSimilarity;
    }

    public static Dictionary<char, int> BuildLast(string pattern)
    {
        Dictionary<char, int> last = new Dictionary<char, int>();

        for (int i = 0; i < pattern.Length; i++)
            last[pattern[i]] = i;

        return last;
    }

    private static double CalculateLevenshteinSimilarity(string str1, string str2)
    {
        int len1 = str1.Length;
        int len2 = str2.Length;
        int[,] dp = new int[len1 + 1, len2 + 1];

        for (int i = 0; i <= len1; i++)
        {
            dp[i, 0] = i;
        }

        for (int j = 0; j <= len2; j++)
        {
            dp[0, j] = j;
        }

        for (int i = 1; i <= len1; i++)
        {
            for (int j = 1; j <= len2; j++)
            {
                int cost = (str1[i - 1] == str2[j - 1]) ? 0 : 1;
                dp[i, j] = Math.Min(Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1), dp[i - 1, j - 1] + cost);
            }
        }

        int maxLen = Math.Max(len1, len2);
        double similarity = 1.0 - (double)dp[len1, len2] / maxLen;
        return similarity;
    }
}