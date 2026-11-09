namespace SchoolAdmission.Application.Common;

public static class IdHelper
{
    public const string ClassPrefix = "c_";
    public const string HoldPrefix = "h_";

    public static string ToClassId(Guid id) => $"{ClassPrefix}{id:N}";

    public static string ToHoldId(Guid id) => $"{HoldPrefix}{id:N}";

    public static bool TryParseClassId(string input, out Guid id)
    {
        id = Guid.Empty;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var trimmed = input.Trim();
        if (trimmed.StartsWith(ClassPrefix, StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[ClassPrefix.Length..];
        }

        return Guid.TryParse(trimmed, out id);
    }

    public static bool TryParseHoldId(string input, out Guid id)
    {
        id = Guid.Empty;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var trimmed = input.Trim();
        if (trimmed.StartsWith(HoldPrefix, StringComparison.OrdinalIgnoreCase))
        {
            trimmed = trimmed[HoldPrefix.Length..];
        }

        return Guid.TryParse(trimmed, out id);
    }
}

