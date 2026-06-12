namespace SaborMercado.Web.Features.Recognition;

public static class EanValidator
{
    public static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.Length is 8 or 13 && IsValidChecksum(digits) ? digits : null;
    }

    private static bool IsValidChecksum(string digits)
    {
        var sum = 0;
        var reverse = digits.Reverse().ToArray();
        for (var i = 1; i < reverse.Length; i++)
        {
            var digit = reverse[i] - '0';
            sum += i % 2 == 1 ? digit * 3 : digit;
        }

        var check = (10 - sum % 10) % 10;
        return check == reverse[0] - '0';
    }
}
