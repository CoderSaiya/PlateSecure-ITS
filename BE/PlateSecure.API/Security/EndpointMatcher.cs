namespace PlateSecure.Security;

public static class EndpointMatcher
{
    public static bool IsMatch(string pattern, string path)
    {
        if (pattern.EndsWith("*"))
        {
            var prefix = pattern.Substring(0, pattern.Length - 1).ToLower();
            return path.StartsWith(prefix);
        }
        return string.Equals(pattern, path, StringComparison.OrdinalIgnoreCase);
    }
}
