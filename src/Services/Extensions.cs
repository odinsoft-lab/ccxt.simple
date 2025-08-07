namespace CCXT.Simple.Services;

public static partial class CExtension
{
    public static string ToQueryString2(this Dictionary<string, string> args)
    {
        return args != null ? String.Join("&", args.Select(a => $"{a.Key}={Uri.EscapeDataString((a.Value ?? "").ToString())}"))
                            : "";
    }

    public static string ConvertHexString(this byte[] buffer)
    {
        return BitConverter.ToString(buffer).Replace("-", "");
    }

    public static bool IsNumber(this string s)
    {
        return s.All(char.IsDigit);
    }

    public static bool IsEmpty(this string s)
    {
        return String.IsNullOrEmpty(s);
    }

    public static bool IsNotEmpty(this string s)
    {
        return !String.IsNullOrEmpty(s);
    }

    public static string IsNotEmpty(this string s, string replace)
    {
        return !String.IsNullOrEmpty(s) ? s : replace;
    }

    /// <summary>
    /// 연-월-일T시:분:초.Zone(yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK) 형식으로 변환 합니다.
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    public static string ToDateTimeZoneString(this DateTime d)
    {
        return d.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK");
    }

    /// <summary>
    /// 연-월-일 시:분:초(yyyy-MM-dd HH:mm:ss) 형식으로 변환 합니다.
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    public static string ToDateTimeString(this DateTime d)
    {
        return d.ToString("yyyy-MM-dd HH:mm:ss");
    }
}
