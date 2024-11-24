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
    public  DeclaredItems DeclaredItems { get; private set; }
    public TemplateSet Templates { get; private set; }
    public CustomDicts CustomDicts { get; }
    public string RootPath { get; }
    public string CurrentPath { get; private set; }


    public ParsingContext(IReadonlyParsingContext context,
        string? currentPath = null,
        DeclaredItems? declaredItems = null)
    {
        DeclaredItems = declaredItems ?? new DeclaredItems(context.DeclaredItems);
        Templates = new TemplateSet(context.Templates);
        CustomDicts = new CustomDicts(context.CustomDicts);
        RootPath = context.RootPath;
        CurrentPath = currentPath ?? context.CurrentPath;
    }

    public ParsingContext(DeclaredItems declaredItems, TemplateSet templates, CustomDicts customDicts, string rootPath, string currentPath)
    {
        DeclaredItems = declaredItems;
        Templates = templates;
        CustomDicts = customDicts;
        RootPath = rootPath;
        CurrentPath = currentPath;
    }

    public ParsingContext WithCurrentPath(string currentPath)
    {
        return new ParsingContext(this)
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
