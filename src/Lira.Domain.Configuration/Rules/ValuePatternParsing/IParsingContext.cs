using Lira.Domain.Configuration.Templating;
using Lira.Domain.TextPart.Impl.CSharp;
using Lira.Domain.TextPart.Impl.Custom.CustomDicModel;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

public interface IParsingContext;

public interface IReadonlyParsingContext
{
    IReadonlyDeclaredItems DeclaredItems { get; }
    IReadOnlyCollection<Template> Templates { get; }
    IReadOnlyCustomDicts CustomDicts { get; }
    string RootPath { get; }
    string CurrentPath { get; }
    FunctionFactoryUsingContext CSharpUsingContext { get; }
}

class ParsingContext : IParsingContext, IReadonlyParsingContext
{
    public CustomDicts CustomDicts { get; }
    public string RootPath { get; }
    public DeclaredItems DeclaredItems { get; private set; }
    public string CurrentPath { get; private init; }
    public TemplateSet Templates { get; set; }
    public FunctionFactoryUsingContext CSharpUsingContext { get; }

         public ParsingContext(
         IReadonlyParsingContext context,
         string? currentPath = null,
         DeclaredItems? declaredItems = null,
         FunctionFactoryUsingContext? cSharpUsingContext = null)
     {
         CSharpUsingContext = cSharpUsingContext ?? context.CSharpUsingContext;
         DeclaredItems = declaredItems ?? new DeclaredItems(context.DeclaredItems);
         Templates = new TemplateSet(context.Templates);
         CustomDicts = new CustomDicts(context.CustomDicts);
         RootPath = context.RootPath;
         CurrentPath = currentPath ?? context.CurrentPath;
     }

     public ParsingContext(DeclaredItems declaredItems, FunctionFactoryUsingContext cSharpUsingContext, TemplateSet templates, CustomDicts customDicts, string rootPath, string currentPath)
     {
         DeclaredItems = declaredItems;
         Templates = templates;
         CustomDicts = customDicts;
         RootPath = rootPath;
         CurrentPath = currentPath;
         CSharpUsingContext = cSharpUsingContext;
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

    public override string ToString()
    {
        var nl = Environment.NewLine;
        return
            DeclaredItems + nl +
            $"Custom dictionaries: {string.Join(", ", CustomDicts.GetRegisteredNames())}";
    }
}

internal static class ParsingContextExtensions
{
    public static ParsingContext ToImpl(this IReadonlyParsingContext context) => (ParsingContext)context;
}

//
//
// using Lira.Domain.Configuration.Templating;
// using Lira.Domain.TextPart.Impl.CSharp;
// using Lira.Domain.TextPart.Impl.Custom.CustomDicModel;
//
// namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;
//
// public interface IParsingContext;
//
// public interface IReadonlyParsingContext
// {
//     IReadonlyDeclaredItems DeclaredItems { get; }
//     IReadOnlyCollection<Template> Templates { get; }
//     IReadOnlyCustomDicts CustomDicts { get; }
//     string RootPath { get; }
//     string CurrentPath { get; }
//     public FunctionFactoryUsingContext CSharpUsingContext { get; }
//
// }
//
// class ParsingContext : IParsingContext, IReadonlyParsingContext
// {
//     public CustomDicts CustomDicts { get; }
//     public string RootPath { get; }
//     public  DeclaredItems DeclaredItems { get; private set; }
//     public string CurrentPath { get; private init; }
//     public TemplateSet Templates { get; set; }
//
//     public FunctionFactoryUsingContext CSharpUsingContext { get; }
//
//     public ParsingContext(
//         IReadonlyParsingContext context,
//         string? currentPath = null,
//         DeclaredItems? declaredItems = null,
//         FunctionFactoryUsingContext? cSharpUsingContext = null)
//     {
//         CSharpUsingContext = cSharpUsingContext ?? context.CSharpUsingContext;
//         DeclaredItems = declaredItems ?? new DeclaredItems(context.DeclaredItems);
//         Templates = new TemplateSet(context.Templates);
//         CustomDicts = new CustomDicts(context.CustomDicts);
//         RootPath = context.RootPath;
//         CurrentPath = currentPath ?? context.CurrentPath;
//     }
//
//     public ParsingContext(DeclaredItems declaredItems, FunctionFactoryUsingContext cSharpUsingContext, TemplateSet templates, CustomDicts customDicts, string rootPath, string currentPath)
//     {
//         DeclaredItems = declaredItems;
//         Templates = templates;
//         CustomDicts = customDicts;
//         RootPath = rootPath;
//         CurrentPath = currentPath;
//         CSharpUsingContext = cSharpUsingContext;
//     }
//
//     public ParsingContext WithCurrentPath(string currentPath)
//     {
//         return new ParsingContext(this)
//         {
//             CurrentPath = currentPath
//         };
//     }
//
//     public void SetDeclaredItems(DeclaredItems declaredItems)
//     {
//         DeclaredItems = declaredItems;
//     }
//
//     IReadOnlyCustomDicts IReadonlyParsingContext.CustomDicts => CustomDicts;
//     IReadOnlyCollection<Template> IReadonlyParsingContext.Templates => Templates.ToArray();
//     IReadonlyDeclaredItems IReadonlyParsingContext.DeclaredItems => DeclaredItems;
//
//     public void SetTemplates(TemplateSet templates)
//     {
//         Templates = templates;
//     }
// }
//
// internal static class ParsingContextExtensions
// {
//     public static ParsingContext ToImpl(this IReadonlyParsingContext context) => (ParsingContext)context;
// }