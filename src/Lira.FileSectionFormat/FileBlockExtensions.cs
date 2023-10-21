using System.ComponentModel;
using System.Text;

namespace Lira.FileSectionFormat;
public static class FileBlockExtensions
{
    public static string GetSingleLine(this FileBlock block)
    {
        var lines = block.Lines;
        if (lines.Count == 0)
            throw new InvalidOperationException($"Block '{block.Name}' not contais lines");

        if (lines.Count > 1)
            throw new InvalidOperationException($"Block '{block.Name}' contais more than one lines. Lines: " + string.Join(", ", lines.Select(l => $"'{l}'")));

        return lines[0];
    }
    
    public static T? GetValue<T>(this FileBlock block)
    {
        var line = GetSingleLine(block);
        TypeConverter tc = TypeDescriptor.GetConverter(typeof(T));
        return (T?)tc.ConvertFrom(line);
    }

    public static string GetStringValue(this FileBlock block)
    {
        var sb = new StringBuilder();

        for (int i = 0; i < block.Lines.Count; i++)
        {
            if (i > 0)
                sb.Append(Environment.NewLine);
            sb.Append(block.Lines[i]);
        }

        return sb.ToString();
    }
}
