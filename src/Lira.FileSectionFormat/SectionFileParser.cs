using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Lira.Common.Extensions;

namespace Lira.FileSectionFormat;

public static class SectionFileParser
{
    private const int MinSectionBlockCharsLength = 3;
    private const char SectionStartChar = '-';
    private const char BlockStartChar = '~';
    static readonly string StartSection = new(SectionStartChar, MinSectionBlockCharsLength);

    public static async Task<FileSectionRoot> Parse(string ruleFile)
    {
        string text = await File.ReadAllTextAsync(ruleFile);
        var lines =  text.DeleteComments();

        if (lines.Count == 0)
            return FileSectionRoot.Empty;

        var linesWithoutSections = new List<string>();
        if (!IsSection(lines[0]))
        {
            foreach (var line in lines)
            {
                if (IsSection(line))
                {
                    break;
                }

                linesWithoutSections.Add(line);
            }
        }

        var sections = GetSections(lines.Skip(linesWithoutSections.Count).ToArray());
        return new FileSectionRoot(linesWithoutSections.TrimEmptyLines().ToImmutableArray(), sections.ToImmutableList());
    }

    private static IImmutableList<FileSection> GetSections(IReadOnlyList<string> lines)
    {
        var sectionLengths = GetSectionLengths(lines);

        if (sectionLengths.Count == 0)
            throw new FileBlockFormatException("Sections not found");

        int[] orderedSectionLengths = sectionLengths.OrderByDescending(x => x).ToArray();
        int index = 0;

        return GetSections(orderedSectionLengths, ref index, lines).Select(x => x.Build()).ToImmutableList();
    }

    private static HashSet<int> GetSectionLengths(IReadOnlyList<string> lines)
    {
        var sectionLengths = new HashSet<int>();

        foreach (var line in lines)
        {
            if (IsSection(line))
            {
                string startSection = GetStartSectionString(line);
                sectionLengths.Add(startSection.Length);
            }
        }

        return sectionLengths;
    }

    private static IReadOnlyList<FileSectionBuilder> GetSections(
        int[] orderedSectionLengths,
        ref int index,
        IReadOnlyList<string> lines)
    {
        int currentLength = orderedSectionLengths[index];

        foreach (var line in lines)
        {
            if (IsSection(line))
            {
                string sectionSplitter = GetStartSectionString(line);
                if (sectionSplitter.Length > currentLength)
                {
                    throw new FileBlockFormatException(
                        $"Current section splitter length cannot be more than {currentLength}. Current: {sectionSplitter.Length}");
                }
            }
        }

        var firstLine = lines.First();
        var currentSplitter = new string(SectionStartChar, currentLength);

        if (GetStartSectionString(firstLine) != currentSplitter)
            throw new FileBlockFormatException($"First section must be start'{new string(SectionStartChar, currentLength)}'");

        var sectionsLines = new List<List<string>>();

        List<string>? currentPortion = null;

        foreach (var line in lines)
        {
            if (line.StartsWith(currentSplitter))
            {
                currentPortion = [ line ];
                sectionsLines.Add(currentPortion);
            }
            else
            {
                currentPortion!.Add(line);
            }
        }

        var sections = new List<FileSectionBuilder>();
        foreach (var sectionLines in sectionsLines)
        {
            var nameAndKey = CleanFromSectionPrefix(sectionLines[0]).Split(':');
            string sectionName = nameAndKey[0];
            string? sectionKey = nameAndKey.Length == 1 ? null : nameAndKey[1];
            var section = new FileSectionBuilder(sectionName, sectionKey);

            sections.Add(section);

            var withoutNameLines = GetSubList(sectionLines, 1);

            var sectionBlocksLines = new List<string>();
            var childSectionsBlocksLines = new List<string>();

            bool childBlockFound = false;
            foreach (var line in withoutNameLines)
            {
                if (childBlockFound)
                {
                    childSectionsBlocksLines.Add(line);
                }
                else if (IsSection(line))
                {
                    childBlockFound = true;
                    childSectionsBlocksLines.Add(line);
                }
                else
                {
                    sectionBlocksLines.Add(line);
                }
            }

            FileBlockBuilder? currentBlock = null;
            foreach (var line in sectionBlocksLines)
            {
                if (IsBlock(line, out var blockName, out var blockKey))
                {
                    if (currentBlock != null)
                        section.Blocks.AddOrThrowIfContains(currentBlock.Build());

                    currentBlock = new FileBlockBuilder(blockName, blockKey);

                    continue;
                }

                if (currentBlock != null)
                    currentBlock.Add(line);
                else
                    section.LinesWithoutBlock.Add(line);
            }

            if (currentBlock != null)
            {
                section.Blocks.AddOrThrowIfContains(currentBlock.Build());
            }

            if (childSectionsBlocksLines.Count > 0)
            {
                index++;
                section.ChildSections.AddRange(GetSections(orderedSectionLengths, ref index, childSectionsBlocksLines));
            }
        }

        index--;
        return sections;
    }

    private static List<string> GetSubList(List<string> list, int startIndex)
    {
        var subList = new List<string>();

        for (int i = startIndex; i < list.Count; i++)
        {
            subList.Add(list[i]);
        }

        return subList;
    }

    private static bool IsSection(string line)
    {
        return line.StartsWith(StartSection);
    }

    private static bool IsBlock(string cleanLine,
        [MaybeNullWhen(returnValue: false)] out string blockName, out string? blockKey)
    {
        blockName = null;
        blockKey = null;

        if (cleanLine.StartsWith(BlockStartChar + " "))
        {
            (blockName, blockKey) = cleanLine.TrimStart(BlockStartChar).SplitToTwoParts(":").Trim();
            return true;
        }

        return false;
    }

    private static string CleanFromSectionPrefix(string cleanLine) => cleanLine.TrimStart(GetStartSectionString(cleanLine)).Trim();

    private static string GetStartSectionString(string line)
    {
        var startBlock = new StringBuilder();
        foreach (char c in line)
        {
            if (c == ' ')
            {
                break;
            }

            if (c == SectionStartChar)
            {
                startBlock.Append(c);
                continue;
            }

            throw new FileBlockFormatException($"Invalid char '{c}' in section line '{line}'");
        }

        return startBlock.ToString();
    }
}
