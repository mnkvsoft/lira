using System.ComponentModel;
using Lira.Common;

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

    public static string GetSingleStringValue(this FileBlock block) => block.Lines.JoinWithNewLine();
}
