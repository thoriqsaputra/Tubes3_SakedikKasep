using System;
using System.Collections.Generic;

public static class KMPAlgorithm
{
    public static (double similarity, int position) KmpMatch(string text, string pattern)
    {
        int n = text.Length;
        int m = pattern.Length;

        if (m == 0 || n == 0)
            return (0, -1);

        int[] lps = ComputeLPSArray(pattern);
        int i = 0;
        int j = 0;

        double maxSimilarity = 0;
        int bestPosition = -1;
        List<int> potentialMatches = new List<int>();

        while (i < n)
        {
            if (pattern[j] == text[i])
            {
                j++;
                i++;
            }

            if (j == m)
            {
                maxSimilarity = 1.0; // Exact match found
                bestPosition = i - j;
                return (maxSimilarity, bestPosition); // return percentage
            }
            else if (i < n && pattern[j] != text[i])
            {
                if (j != 0)
                    j = lps[j - 1];
                else
                    i++;
            }

            if (j > 0 && j < m)
            {
                potentialMatches.Add(i - j);
            }
        }

        foreach (var start in potentialMatches)
        {
            int end = Math.Min(start + m, n);
            double similarity = CalculateLevenshteinSimilarity(text.Substring(start, end - start), pattern);
            if (similarity > maxSimilarity)
            {
                maxSimilarity = similarity;
                bestPosition = start;
            }
        }

        return (maxSimilarity, bestPosition);
    }

    private static int[] ComputeLPSArray(string pattern)
    {
        int m = pattern.Length;
        int[] lps = new int[m];
        int len = 0;
        int i = 1;

        lps[0] = 0;

        while (i < m)
        {
            if (pattern[i] == pattern[len])
            {
                len++;
                lps[i] = len;
                i++;
            }
            else
            {
                if (len != 0)
                {
                    len = lps[len - 1];
                }
                else
                {
                    lps[i] = 0;
                    i++;
                }
            }
        }

        return lps;
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
