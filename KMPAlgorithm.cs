using System;

public static class KMPAlgorithm
{
    public static (double similarity, int position) KmpMatch(string text, string pattern)
    {
        int n = text.Length;
        int m = pattern.Length;
        int[] b = ComputeBorder(pattern);
        int i = 0;
        int j = 0;

        double maxSimilarity = 0;
        int bestPosition = -1;

        while (i < n)
        {
            if (pattern[j] == text[i])
            {
                if (j == m - 1)
                {
                    double similarity = CalculateLevenshteinSimilarity(text.Substring(i - m + 1, m), pattern);
                    if (similarity > maxSimilarity)
                    {
                        maxSimilarity = similarity;
                        bestPosition = i - m + 1;
                    }
                    j = b[j];
                }
                else
                {
                    i++;
                    j++;
                }
            }
            else if (j > 0)
            {
                j = b[j - 1];
            }
            else
            {
                i++;
            }
        }

        return (maxSimilarity, bestPosition);
    }

    public static int[] ComputeBorder(string pattern)
    {
        int[] b = new int[pattern.Length];
        b[0] = 0;

        int m = pattern.Length;
        int j = 0;
        int i = 1;

        while (i < m)
        {
            if (pattern[i] == pattern[j])
            {
                j++;
                b[i] = j;
                i++;
            }
            else
            {
                if (j != 0)
                {
                    j = b[j - 1];
                }
                else
                {
                    b[i] = 0;
                    i++;
                }
            }
        }
        return b;
    }

    public static double CalculateLevenshteinSimilarity(string str1, string str2)
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