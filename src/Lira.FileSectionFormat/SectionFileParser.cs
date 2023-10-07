using System.Diagnostics.CodeAnalysis;
using System.Text;
using Lira.Common.Extensions;

namespace Lira.FileSectionFormat;

public static class SectionFileParser
{
    private const int MinSectionBlockCharsLength = 3;
    private const char SectionStartChar = '-';
    private const char BlockStartChar = '~';

    public static async Task<IReadOnlyList<FileSection>> Parse(string ruleFile)
    {
        string text = await File.ReadAllTextAsync(ruleFile);
        var lines =  TextCleaner.DeleteEmptiesAndComments(text);

        if (lines.Count == 0)
            return Array.Empty<FileSection>();

        var sectionLengths = GetSectionLengths(lines);

        if (sectionLengths.Count == 0)
            throw new FileBlockFormatException("Sections not found");

        int[] orderedSectionLengths = sectionLengths.OrderByDescending(x => x).ToArray();
        int index = 0;
        var rootSections = GetSections(orderedSectionLengths, ref index, lines);
        return rootSections;
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

    static IReadOnlyList<FileSection> GetSections(
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
        string currentSplitter = new string(SectionStartChar, currentLength);

        if (GetStartSectionString(firstLine) != currentSplitter)
            throw new FileBlockFormatException($"First section must be start'{new string(SectionStartChar, currentLength)}'");

        var sectionsLines = new List<List<string>>();

        List<string>? currentPortion = null;

        foreach (var line in lines)
        {
            if (line.StartsWith(currentSplitter))
            {
                currentPortion = new List<string>();
                currentPortion.Add(line);
                sectionsLines.Add(currentPortion);
            }
            else
            {
                currentPortion!.Add(line);
            }
        }

        var sections = new List<FileSection>();
        foreach (var sectionLines in sectionsLines)
        {
            string sectionName = GetName(sectionLines[0]);
            var section = new FileSection(sectionName);

            sections.Add(section);

            var withoutNameLines = GetSubList(sectionLines, 1);

            var sectionBlocksLines = new List<string>();
            var childSectionsBlocksLines = new List<string>();

            bool childBlockFinded = false;
            foreach (var line in withoutNameLines)
            {
                if (childBlockFinded)
                {
                    childSectionsBlocksLines.Add(line);
                }
                else if (IsSection(line))
                {
                    childBlockFinded = true;
                    childSectionsBlocksLines.Add(line);
                }
                else
                {
                    sectionBlocksLines.Add(line);
                }
            }

            FileBlock? currentBlock = null;
            foreach (var line in sectionBlocksLines)
            {
                if (IsBlock(line, out var blockName, out var shortBlockValue))
                {
                    currentBlock = new FileBlock(blockName);
                    if(shortBlockValue != null)
                        currentBlock.Add(shortBlockValue);
                    section.Blocks.AddOrThrowIfContains(currentBlock);
                    continue;
                }

                if (currentBlock != null)
                    currentBlock.Add(line);
                else
                    section.LinesWithoutBlock.Add(line);
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
        return line.StartsWith(new string(SectionStartChar, MinSectionBlockCharsLength));
    }

    private static bool IsBlock(string cleanLine,
        [MaybeNullWhen(returnValue: false)] out string blockName, out string? shortBlockValue)
    {
        blockName = null;
        shortBlockValue = null;

        if (cleanLine.StartsWith(BlockStartChar + " "))
        {
            (blockName, shortBlockValue) = cleanLine.TrimStart(BlockStartChar).SplitToTwoParts(":").Trim();
            return true;
        }

        return false;
    }

    private static string GetName(string cleanLine) => cleanLine.TrimStart(GetStartSectionString(cleanLine)).Trim();

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
