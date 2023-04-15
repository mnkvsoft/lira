namespace SimpleMockServer.FileSectionFormat;

public static class FileSectionExtensions
{
    public static FileSection GetSingleChildSection(this FileSection section, string name)
    {
        var sections = section.ChildSections.Where(x => x.Name == name).ToArray();
        if (sections.Length == 0)
            throw new InvalidOperationException($"Section '{section.Name}' not contais child section '{name}'");

        if (sections.Length > 1)
            throw new InvalidOperationException($"Section '{section.Name}' contais more than one child sections '{name}'");

        return sections[0];
    }

    public static string GetSingleLine(this FileSection section)
    {
        var lines = section.LinesWithoutBlock;
        if (lines.Count == 0)
            throw new InvalidOperationException($"Section '{section.Name}' not contais lines");

        if (lines.Count > 1)
            throw new InvalidOperationException($"Section '{section.Name}' contais more than one lines. Lines: " + string.Join(", ", lines.Select(l => $"'{l}'")));

        return lines[0];
    }

    public static FileBlock GetBlockRequired(this FileSection section, string name)
    {
        var block = section.Blocks.FirstOrDefault(x => x.Name == name);

        if (block == null)
            throw new InvalidOperationException($"Block '{name}' not found in section '{section.Name}'");

        return block;
    }

    public static FileBlock? GetBlock(this FileSection section, string name)
    {
        return section.Blocks.FirstOrDefault(x => x.Name == name);
    }

    public static T? GetBlockValue<T>(this FileSection section, string blockName)
    {
        return section
                  .GetBlockRequired(blockName)
                  .GetValue<T>();
    }

    public static T? GetBlockValueOrDefault<T>(this FileSection section, string blockName)
    {
        var block = section.GetBlock(blockName);
        if (block == null)
            return default(T);

        return block.GetValue<T>();
    }

    public static string GetStringValueFromRequiredBlock(this FileSection section, string blockName)
    {
        var block = section.GetBlockRequired(blockName);
        return block.GetStringValue();
    }

    public static string GetStringValueFromBlockOrEmpty(this FileSection section, string blockName)
    {
        var block = section.GetBlockOrNull(blockName);
        if (block == null)
            return string.Empty;
        return block.GetStringValue();
    }

    public static IReadOnlyCollection<string> GetLinesFromBlockOrEmpty(this FileSection section, string blockName)
    {
        return section.GetBlockOrNull(blockName)?.Lines ?? (IReadOnlyCollection<string>)Array.Empty<string>();
    }

    public static FileBlock? GetBlockOrNull(this FileSection section, string name)
    {
        return section.Blocks.FirstOrDefault(x => x.Name == name);
    }
}
