using Lira.Domain.TextPart.Impl.CSharp;
using Lira.Domain.TextPart.Impl.Custom.CustomDicModel;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

public interface IParsingContext;

public interface IReadonlyParsingContext
{
    IDeclaredItemsRegistryReadonly DeclaredItems { get; }
    IReadOnlyCustomDicts CustomDicts { get; }
    string RootPath { get; }
    string CurrentPath { get; }
    FunctionFactoryUsingContext CSharpUsingContext { get; }
}

class ParsingContext : IParsingContext, IReadonlyParsingContext
{
    public CustomDicts CustomDicts { get; }
    public string RootPath { get; }
    public DeclaredItemsRegistry DeclaredItemsRegistry { get; private set; }
    public string CurrentPath { get; private init; }
    public FunctionFactoryUsingContext CSharpUsingContext { get; }

         public ParsingContext(
         IReadonlyParsingContext context,
         string? currentPath = null,
         DeclaredItemsRegistry? declaredItems = null,
         FunctionFactoryUsingContext? cSharpUsingContext = null)
     {
         CSharpUsingContext = cSharpUsingContext ?? context.CSharpUsingContext;
         DeclaredItemsRegistry = declaredItems ?? DeclaredItemsRegistry.WithoutLocalVariables(context.DeclaredItems);
         CustomDicts = new CustomDicts(context.CustomDicts);
         RootPath = context.RootPath;
         CurrentPath = currentPath ?? context.CurrentPath;
     }

     public ParsingContext(DeclaredItemsRegistry declaredItemsRegistry, FunctionFactoryUsingContext cSharpUsingContext, CustomDicts customDicts, string rootPath, string currentPath)
     {
         DeclaredItemsRegistry = declaredItemsRegistry;
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

    public void SetDeclaredItems(DeclaredItemsRegistry declaredItemsRegistry)
    {
        DeclaredItemsRegistry = declaredItemsRegistry;
    }

    IReadOnlyCustomDicts IReadonlyParsingContext.CustomDicts => CustomDicts;
    IDeclaredItemsRegistryReadonly IReadonlyParsingContext.DeclaredItems => DeclaredItemsRegistry;

    public override string ToString()
    {
        var nl = Environment.NewLine;
        return
            DeclaredItemsRegistry + nl +
            $"Custom dictionaries: {string.Join(", ", CustomDicts.GetRegisteredNames())}";
    }
}

internal static class ParsingContextExtensions
{
    public static ParsingContext ToImpl(this IParsingContext context) => (ParsingContext)context;
}