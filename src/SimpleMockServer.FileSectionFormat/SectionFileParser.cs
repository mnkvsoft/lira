using System.Diagnostics.CodeAnalysis;
using System.Text;
using SimpleMockServer.Common.Extensions;

namespace SimpleMockServer.FileSectionFormat;

public static class SectionFileParser
{
    static class CommentChar
    {
        public const string SingleLine = "//";
        public const string MultiLineStart = "/*";
        public const string MultiLineEnd = "*/";
    }

    private const int MinSectionBlockCharsLength = 3;
    private const char SectionStartChar = '-';
    private const char BlockEndChar = ':';

    public static async Task<IReadOnlyList<FileSection>> Parse(
        string ruleFile,
        IReadOnlyDictionary<string, IReadOnlySet<string>> knownBlockForSections,
        int maxNestingDepth)
    {
        IReadOnlyList<string> lines = await File.ReadAllLinesAsync(ruleFile);

        lines = DeleteEmptyAndComments(lines);

        if (lines.Count == 0)
            return Array.Empty<FileSection>();

        HashSet<int> sectionLengths = GetSectionLengths(lines);

        if (sectionLengths.Count == 0)
            throw new FileBlockFormatException("Sections not found");

        if (sectionLengths.Count > maxNestingDepth)
            throw new FileBlockFormatException($"Max nesting depth: {maxNestingDepth}. Current: {sectionLengths.Count}");

        int[] orderedSectionLengths = sectionLengths.OrderByDescending(x => x).ToArray();
        int index = 0;
        var rootSections = GetSections(orderedSectionLengths, ref index, lines, knownBlockForSections);
        return rootSections;
    }

    private static HashSet<int> GetSectionLengths(IReadOnlyList<string> lines)
    {
        HashSet<int> sectionLengths = new HashSet<int>();

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
        IReadOnlyList<string> lines,
        IReadOnlyDictionary<string, IReadOnlySet<string>> knownBlockForSections)
    {
        int currentLength = orderedSectionLengths[index];

        foreach (var line in lines)
        {
            if (IsSection(line))
            {
                string sectionSplitter = GetStartSectionString(line);
                if (sectionSplitter.Length > currentLength)
                {
                    throw new FileBlockFormatException($"Current section splitter length cannot be more than {currentLength}. Current: {sectionSplitter.Length}");
                }
            }
        }

        var firstLine = lines.First();
        string currentSplitter = new string(SectionStartChar, currentLength);

        if (GetStartSectionString(firstLine) != currentSplitter)
            throw new FileBlockFormatException($"First section must be start'{new string(SectionStartChar, currentLength)}'");

        List<List<string>> sectionsLines = new List<List<string>>();

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

            List<string> sectionBlocksLines = new List<string>();
            List<string> childSectionsBlocksLines = new List<string>();

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
                if (knownBlockForSections.ContainsKey(sectionName) && IsBlock(line, knownBlockForSections[sectionName], out var blockName))
                {
                    currentBlock = new FileBlock(blockName);
                    section!.Blocks.AddOrThrowIfContains(currentBlock);
                    continue;
                }

                if (currentBlock != null)
                    currentBlock.Lines.Add(line);
                else
                    section.LinesWithoutBlock.Add(line);
            }

            if (childSectionsBlocksLines.Count > 0)
            {
                index++;
                section.ChildSections.AddRange(GetSections(orderedSectionLengths, ref index, childSectionsBlocksLines, knownBlockForSections));
            }
        }

        index--;
        return sections;
    }

    private static List<string> GetSubList(List<string> list, int startIndex)
    {
        List<string> subList = new List<string>();

        for (int i = startIndex; i < list.Count; i++)
        {
            subList.Add(list[i]);
        }

        return subList;
    }

    private static IReadOnlyList<string> DeleteEmptyAndComments(IReadOnlyCollection<string> lines)
    {
        List<string> result = new List<string>();
        bool isMultiLineComment = false;

        foreach (var line in lines)
        {
            if (isMultiLineComment)
                continue;

            if (string.IsNullOrWhiteSpace(line))
                continue;

            if (line.Contains(CommentChar.SingleLine))
            {
                (string text, _) = line.SplitToTwoPartsRequired(CommentChar.SingleLine);

                if(!string.IsNullOrWhiteSpace(text))
                    result.Add(line);

                continue;
            }

            if (line.Contains(CommentChar.MultiLineStart))
            {
                isMultiLineComment = true;
                (string text, _) = line.SplitToTwoPartsRequired(CommentChar.MultiLineStart);

                if (!string.IsNullOrWhiteSpace(text))
                    result.Add(line);

                continue;
            }

            if (line.Contains(CommentChar.MultiLineEnd))
            {
                isMultiLineComment = false;

                (_, string text) = line.SplitToTwoPartsRequired(CommentChar.MultiLineEnd);

                if (!string.IsNullOrWhiteSpace(text))
                    result.Add(line);

                continue;
            }

            result.Add(line);
        }

        return result;
    }

    private static bool IsSection(string line)
    {
        return line.StartsWith(new string(SectionStartChar, MinSectionBlockCharsLength));
    }

    private static bool IsBlock(string cleanLine, IReadOnlySet<string> knownBlocks, [MaybeNullWhen(returnValue: false)] out string blockName)
    {
        blockName = null;
        foreach (string block in knownBlocks)
        {
            if (cleanLine.StartsWith(block + BlockEndChar))
            {
                blockName = block;
                return true;
            }
        }
        return false;
    }

    private static string GetName(string cleanLine) => cleanLine.TrimStart(GetStartSectionString(cleanLine)).Trim();

    private static string GetStartSectionString(string line)
    {
        StringBuilder startBlock = new StringBuilder();
        foreach (char c in line)
        {
            if (c == ' ')
            {
                break;
            }
            else if (c == SectionStartChar)
            {
                startBlock.Append(c);
                continue;
            }
            else
            {
                throw new FileBlockFormatException($"Invalid char '{c}' in section line '{line}'");
            }
        }

        return startBlock.ToString();
    }
}
