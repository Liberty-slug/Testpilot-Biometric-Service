using System;
using System.Linq;

namespace BiometricService
{
    public static class TemplateUtils
    {
        public static string? NormalizeBase64(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var trimmed = value.Trim();

            // Strip data URL prefix if present (data:*;base64,)
            var commaIndex = trimmed.IndexOf(',');
            if (trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase) && commaIndex >= 0)
                trimmed = trimmed[(commaIndex + 1)..];

            // Remove whitespace/newlines
            trimmed = new string(trimmed.Where(c => !char.IsWhiteSpace(c)).ToArray());

            // Handle URL-safe base64 variants
            trimmed = trimmed.Replace('-', '+').Replace('_', '/');

            // Fix padding if needed
            var mod = trimmed.Length % 4;
            if (mod == 1)
                return null; // invalid base64
            if (mod > 1)
                trimmed = trimmed.PadRight(trimmed.Length + (4 - mod), '=');

            return trimmed;
        }

        public static byte[]? TryDecodeBase64(string base64)
        {
            try
            {
                return Convert.FromBase64String(base64);
            }
            catch
            {
                return null;
            }
        }
    }
}
