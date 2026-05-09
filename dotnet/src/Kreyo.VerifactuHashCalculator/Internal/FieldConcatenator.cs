using System.Collections.Generic;
using System.Text;

namespace Kreyo.VerifactuHashCalculator.Internal;

internal static class FieldConcatenator
{
    public static string Concatenate(IEnumerable<(string Name, string? Value)> fields)
    {
        var sb = new StringBuilder();
        bool first = true;
        foreach (var (name, value) in fields)
        {
            if (!first)
            {
                sb.Append('&');
            }
            first = false;
            
            sb.Append(name);
            sb.Append('=');
            sb.Append(value?.Trim() ?? string.Empty);
        }
        return sb.ToString();
    }
}
