using System.Collections.Immutable;
using Lira.Domain.Configuration.Templating;
using Lira.Domain.TextPart.Impl.Custom.CustomDicModel;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

public interface IParsingContext
{
}

public interface IReadonlyParsingContext
{
    IReadonlyDeclaredItems DeclaredItems { get; }
    IReadOnlyCollection<Template> Templates { get; }
    IReadOnlyCustomDicts CustomDicts { get; }
    string RootPath { get; }
    string CurrentPath { get; }
}

class ParsingContext : IParsingContext, IReadonlyParsingContext
{
    public CustomDicts CustomDicts { get; }
    public string RootPath { get; }
    public  DeclaredItems DeclaredItems { get; private set; }
    public string CurrentPath { get; private init; }
    public TemplateSet Templates { get; set; }
    public ImmutableList<string> GlobalFileLines { get; } // without section

    public ParsingContext(
        IReadonlyParsingContext context,
        ImmutableList<string> globalFileLines,
        string? currentPath = null,
        DeclaredItems? declaredItems = null)
    {
        GlobalFileLines = globalFileLines;
        DeclaredItems = declaredItems ?? new DeclaredItems(context.DeclaredItems);
        Templates = new TemplateSet(context.Templates);
        CustomDicts = new CustomDicts(context.CustomDicts);
        RootPath = context.RootPath;
        CurrentPath = currentPath ?? context.CurrentPath;
    }

    public ParsingContext(DeclaredItems declaredItems, TemplateSet templates, CustomDicts customDicts, string rootPath, string currentPath, ImmutableList<string> globalFileLines)
    {
        DeclaredItems = declaredItems;
        Templates = templates;
        CustomDicts = customDicts;
        RootPath = rootPath;
        CurrentPath = currentPath;
        GlobalFileLines = globalFileLines;
    }

    public ParsingContext WithCurrentPath(string currentPath)
    {
        return new ParsingContext(this, globalFileLines: ImmutableList<string>.Empty)
        {
            CurrentPath = currentPath
        };
    }

    public void SetDeclaredItems(DeclaredItems declaredItems)
    {
        DeclaredItems = declaredItems;
    }

    IReadOnlyCustomDicts IReadonlyParsingContext.CustomDicts => CustomDicts;
    IReadOnlyCollection<Template> IReadonlyParsingContext.Templates => Templates.ToArray();
    IReadonlyDeclaredItems IReadonlyParsingContext.DeclaredItems => DeclaredItems;

    public void SetTemplates(TemplateSet templates)
    {
        Templates = templates;
    }
}

internal static class ParsingContextExtensions
{
    public static ParsingContext ToImpl(this IReadonlyParsingContext context) => (ParsingContext)context;
}
