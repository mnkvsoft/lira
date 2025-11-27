using System.Collections.Immutable;
using Lira.Domain.TextPart.Impl.CSharp;
using Lira.Domain.TextPart.Impl.Custom;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

public interface IParsingContext : IReadonlyParsingContext;

public interface IReadonlyParsingContext
{
    IReadOnlySet<DeclaredItem> DeclaredItems { get; }
    string RootPath { get; }
    string CurrentPath { get; }
    FunctionFactoryUsingContext CSharpUsingContext { get; }
}

class ParsingContext : IParsingContext
{
    public string RootPath { get; }
    public DeclaredItems DeclaredItems { get; private set; }
    public string CurrentPath { get; }
    public FunctionFactoryUsingContext CSharpUsingContext { get; }

         public ParsingContext(
         IReadonlyParsingContext context,
         string? currentPath = null,
         DeclaredItems? declaredItems = null,
         FunctionFactoryUsingContext? cSharpUsingContext = null)
     {
         CSharpUsingContext = cSharpUsingContext ?? context.CSharpUsingContext;
         DeclaredItems = declaredItems ?? DeclaredItems.WithoutLocalVariables(context.DeclaredItems);
         RootPath = context.RootPath;
         CurrentPath = currentPath ?? context.CurrentPath;
     }

     public ParsingContext(DeclaredItems declaredItems, FunctionFactoryUsingContext cSharpUsingContext, string rootPath, string currentPath)
     {
         DeclaredItems = declaredItems;
         RootPath = rootPath;
         CurrentPath = currentPath;
         CSharpUsingContext = cSharpUsingContext;
     }

    public void SetDeclaredItems(DeclaredItems declaredItems)
    {
        DeclaredItems = declaredItems;
    }

    IReadOnlySet<DeclaredItem> IReadonlyParsingContext.DeclaredItems => DeclaredItems.ToImmutableHashSet();

    public override string ToString() => DeclaredItems.ToString();
}

internal static class ParsingContextExtensions
{
    public static ParsingContext ToImpl(this IParsingContext context) => (ParsingContext)context;
}