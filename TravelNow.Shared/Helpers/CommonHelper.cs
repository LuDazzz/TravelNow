using System.Net.Mail;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace TravelNow.Shared.Helper;

public static class CommonHelper
{
    /// <summary>
    ///     Extract data between a symbol
    ///     Get first group
    /// </summary>
    /// <param name="text">Source</param>
    /// <param name="pattern">Pattern to search</param>
    /// <param name="idx">Index of matched data</param>
    /// <returns>Text result</returns>
    public static string RegexGroupValue(this string text, string pattern, int? idx = null)
    {
        var regex = new Regex(pattern);
        var match = regex.Match(text);
        return idx != null ? match.Groups[idx.Value].Value : match.Value;
    }

    /// <summary>
    ///     Extract data between a symbol
    ///     Get last group
    /// </summary>
    /// <param name="text">Source</param>
    /// <param name="pattern">Pattern to search</param>
    /// <param name="idx">Index of matched data</param>
    /// <returns>Text result</returns>
    public static string RegexGroupValueLast(this string text, string pattern, int? idx = null)
    {
        var regex = new Regex(pattern);
        var matches = regex.Matches(text);
        return idx != null ? matches.Last().Groups[idx.Value].Value : matches.Last().Value;
    }

    /// <summary>
    ///     Compute an object
    /// </summary>
    /// <param name="toCompute">Object to compute</param>
    /// <returns>Hash</returns>
    public static string ComputeHash(this object toCompute)
    {
        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(toCompute.ToJson().ToByteArray());

        var sb = new StringBuilder();
        foreach (var hashByte in hashBytes) sb.Append(hashByte.ToString("x2"));

        return sb.ToString();
    }

    /// <summary>
    ///     Compute a string
    /// </summary>
    /// <param name="text">String to compute</param>
    /// <returns>Hash</returns>
    public static string ComputeHash(this string text)
    {
        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(text.ToByteArray());

        var sb = new StringBuilder();
        foreach (var hashByte in hashBytes) sb.Append(hashByte.ToString("x2"));

        return sb.ToString();
    }

    /// <summary>
    ///     Compare 2 hash
    /// </summary>
    /// <param name="source">Source</param>
    /// <param name="toCompare">Hash to compare</param>
    /// <returns>Compare result</returns>
    public static bool CompareHash(string source, string toCompare)
    {
        return string.IsNullOrEmpty(source) ? string.IsNullOrEmpty(toCompare) : source.Equals(toCompare);
    }

    /// <summary>
    ///     Convert 0 to empty, usually use for data exportation
    /// </summary>
    /// <param name="source">Source</param>
    /// <param name="format">Format</param>
    /// <returns>Convert result</returns>
    public static string ConvertZeroToEmpty(this double? source, string format = "0.00")
    {
        if (source == null) return string.Empty;
        return source == 0 ? string.Empty : source.Value.ToString(format);
    }

    /// <summary>
    ///     Parse double
    /// </summary>
    /// <param name="source">Source</param>
    /// <param name="func">Function</param>
    /// <param name="defaultValue">Default value</param>
    /// <returns>Parse result, default value if parse error</returns>
    public static double? TryParseDoubleNullable(this string source, Func<string, string>? func,
        double? defaultValue = 0.0)
    {
        if (func != null) source = func(source);

        var canParse = double.TryParse(source, out var result);
        return canParse ? result : defaultValue;
    }

    /// <summary>
    ///     Convert to integer
    /// </summary>
    /// <param name="value">Value</param>
    /// <returns>Int</returns>
    public static int ToInt(this Enum value)
    {
        return Convert.ToInt32(value);
    }

    /// <summary>
    ///     Convert double to string
    /// </summary>
    /// <param name="source">Source</param>
    /// <param name="format">Format</param>
    /// <returns>Double as string</returns>
    public static string DoubleToString(this double? source, string format = "0.00")
    {
        if (source == null) return string.Empty;
        return source != 0 ? source.Value.ToString(format) : "0";
    }

    /// <summary>
    ///     Init array
    /// </summary>
    /// <param name="count">Array size</param>
    /// <param name="defaultValue">Init value</param>
    /// <returns>Array</returns>
    public static T[] InitArray<T>(int count, T defaultValue)
    {
        return Enumerable.Repeat(defaultValue, count).ToArray();
    }

    /// <summary>
    ///     Convert to byte array
    /// </summary>
    /// <param name="source">Source</param>
    /// <returns>Byte array</returns>
    public static byte[] ToByteArray(this string source)
    {
        return Encoding.UTF8.GetBytes(source);
    }

    /// <summary>
    ///     Convert to utf8
    /// </summary>
    /// <param name="data">Data</param>
    /// <returns>String</returns>
    public static string ToUtf8String(this byte[] data)
    {
        return Encoding.UTF8.GetString(data);
    }

    /// <summary>
    ///     Remove spaces
    /// </summary>
    /// <param name="source">Source</param>
    /// <returns>String with no space</returns>
    public static string RemoveSpaces(this string source)
    {
        return source.Replace(" ", "");
    }

    /// <summary>
    ///     Check file existence
    /// </summary>
    /// <param name="filePath">FilePath</param>
    /// <param name="cancelToken">Cancellation token</param>
    /// <returns>True if the file exists. Otherwise, False</returns>
    public static async Task<bool> CheckFileExistence(string filePath, CancellationToken? cancelToken = null)
    {
        var attempt = 5;
        while (attempt > 0)
        {
            if (File.Exists(filePath)) return true;

            if (cancelToken.HasValue)
                await Task.Delay(3000, cancelToken.Value);
            else
                await Task.Delay(3000);

            attempt--;
        }

        return false;
    }

    public static double MRound(this double number, double multiple)
    {
        if (multiple == 0) throw new ArgumentException("Multiple cannot be zero.");

        return Math.Round(number / multiple) * multiple;
    }

    public static string ConvertDashToDefaultValue(this string value)
    {
        return value == "-" ? "0" : value;
    }

    public static string ConvertToValidDoubleType(this string value)
    {
        return value == "-" || string.IsNullOrEmpty(value) ? "0" : value;
    }

    public static string SignFormat(this double value, bool plusZero = false)
    {
        return value.ToString(!plusZero ? "+#;-#;0" : "+#;-#;+0");
    }

    public static string GetSign(this double value, bool plusZero = false)
    {
        if (plusZero) return value >= 0 ? "+" : "-";

        if (value == 0) return string.Empty;

        return value > 0 ? "+" : "-";
    }

    public static bool IsEqual(this double value, double compareTo, double precision = double.Epsilon)
    {
        return Math.Abs(value - compareTo) < precision;
    }

    /// <summary>
    ///     Extract userId nullable from claims.
    /// </summary>
    /// <param name="claims">Logged in claims</param>
    /// <returns>UserId</returns>
    public static Guid GetUserIdNullable(this IEnumerable<Claim> claims)
    {
        if (claims.Count() == 0) return Guid.Empty;

        var claimValue = claims.FirstOrDefault(x => x.Type == "nameid" || x.Type == ClaimTypes.NameIdentifier)?.Value;
        return claimValue != null ? Guid.Parse(claimValue) : Guid.Empty;
    }

    /// <summary>
    ///     Extract userId from claims.
    /// </summary>
    /// <param name="claims">Logged in claims</param>
    /// <returns>UserId</returns>
    public static long GetUserId(this IEnumerable<Claim> claims)
    {
        var claimValue = claims.First(x => x.Type == "nameid" || x.Type == ClaimTypes.NameIdentifier).Value;
        return long.Parse(claimValue);
    }

    public static bool IsValidEmail(this string email)
    {
        var text = email.Trim();
        if (text.EndsWith(".")) return false;

        try
        {
            return new MailAddress(email).Address == text;
        }
        catch
        {
            return false;
        }
    }
}
