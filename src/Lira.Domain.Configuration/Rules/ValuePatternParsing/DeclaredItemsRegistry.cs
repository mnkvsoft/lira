using System.Diagnostics.CodeAnalysis;
using Lira.Domain.Configuration.Variables;
using Lira.Domain.TextPart;
using Lira.Domain.TextPart.Impl.Custom;
using Lira.Domain.TextPart.Impl.Custom.FunctionModel;
using Lira.Domain.TextPart.Impl.Custom.VariableModel;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.LocalVariables;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables;
using Lira.Domain.TextPart.Impl.Custom.VariableModel.RuleVariables.Impl;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

public interface IDeclaredItemsRegistryReadonly;

class DeclaredItemsRegistry : IDeclaredItemsRegistryReadonly
{
    private readonly ITextPartsParser _textPartsParser;

    // private readonly Dictionary<string, DeclaredItem> _nameToCompiledMap = new();

    // private readonly Dictionary<string, DeclaredItemDraft> _nameToDraftMap = new ();
    private readonly Dictionary<string, IReference> _nameToReferenceMap = new ();

    // private readonly Dictionary<string, LocalVariable> _nameToLocalVariableMap = new ();
    // private readonly Dictionary<string, ObjectTextPartReference> _nameToLocalVariableReferenceMap = new ();

    private DeclaredItemsRegistry(DeclaredItemsRegistry registry)
    {
        _textPartsParser = registry._textPartsParser;

        // _nameToDraftMap = new Dictionary<string, DeclaredItemDraft>(registry._nameToDraftMap);
        _nameToReferenceMap = new Dictionary<string, IReference>(registry._nameToReferenceMap);
    }

    public DeclaredItemsRegistry(ITextPartsParser textPartsParser)
    {
        _textPartsParser = textPartsParser;
    }

    public static DeclaredItemsRegistry WithoutLocalVariables(IDeclaredItemsRegistryReadonly registry) => new((DeclaredItemsRegistry)registry);

    // public override string ToString()
    // {
    //     var nl = Environment.NewLine;
    //     return
    //         $"Declared functions: {string.Join(", ", _nameToCompiledMap.OfType<Function>().Select(x => x.Name))}" +
    //         nl +
    //         $"Declared variables: {string.Join(", ", _nameToCompiledMap.OfType<RuleVariable>().Select(x => x.Name))}" +
    //         nl +
    //         $"Declared local variables: {string.Join(", ", _nameToCompiledMap.OfType<LocalVariable>().Select(x => x.Name))}"
    //         ;
    // }

    public void AddDraftsRange(IReadOnlyCollection<DeclaredItemDraft> drafts)
    {
        foreach (var draft in drafts)
        {
            var name = draft.Name;

            if (_nameToReferenceMap.ContainsKey(name))
            {
                throw new Exception(
                    $"Declaration '{name}' is defined multiple times");
            }

            DraftReference reference;
            if (name.StartsWith(RuleVariable.Prefix))
            {
                reference = new DraftVariableReference(name, draft);
            }
            else if(name.StartsWith(Function.Prefix))
            {
                reference = new DraftReference(name, draft);
            }
            else
            {
                throw new Exception($"Unknown declaration '{name}'");
            }

            _nameToReferenceMap.Add(name, reference);
        }
    }

    public IObjectTextPart Get(string name)
    {
        if(!_nameToReferenceMap.TryGetValue(name, out var result))
            throw new Exception($"Unknown declaration '{name}'");

        return result.ObjectTextPart ?? throw new Exception($"Reference for declaration '{name}' not set");
    }

    public void SetVariable(string name, RuleExecutingContext context, dynamic value)
    {
        if(!TryFindVariableReference(name, out var variableReference))
            throw new Exception($"Variable '{name}' is not declared");

        variableReference.SetValue(context, value);
    }

    private bool TryFindVariableReference(string name, [MaybeNullWhen(false)]out IVariableReference variableReference)
    {
        variableReference = null;

        if (!IsVariableName(name))
            throw new Exception($"'{name}' is not variable name");

        if (!_nameToReferenceMap.TryGetValue(name, out var reference))
            return false;

        variableReference = (IVariableReference)reference;
        return true;
    }

    private static bool IsVariableName(string name) => name.StartsWith(RuleVariable.Prefix) || name.StartsWith(LocalVariable.Prefix);

    public IVariableReference? TryGetVariable(string maybeVariableDeclaration, ReturnType? returnType)
    {
        if (!IsVariableName(maybeVariableDeclaration))
            return null;

        // todo: check types
        if (TryFindVariableReference(maybeVariableDeclaration, out var variableReference))
            return variableReference;

        if (maybeVariableDeclaration.StartsWith(RuleVariable.Prefix))
        {
            var variable = new InlineRuleVariable(
                maybeVariableDeclaration,
                returnType);

            var reference = new VariableReference(maybeVariableDeclaration, variable);
            _nameToReferenceMap.Add(maybeVariableDeclaration, reference);
            return reference;
        }

        if (maybeVariableDeclaration.StartsWith(LocalVariable.Prefix))
        {
            var variable = new LocalVariable(
                maybeVariableDeclaration,
                returnType);

            var reference = new VariableReference(maybeVariableDeclaration, variable);
            _nameToReferenceMap.Add(maybeVariableDeclaration, reference);
            return reference;
        }

        return null;
    }

    public IReference? TryGetFunctionReference(string functionName)
    {
        return TryGetReference(functionName);

        // var mayBeFunction = TryGetReference(functionName);
        //
        // if (mayBeFunction?.ObjectTextPart is Function)
        //     return mayBeFunction;
        //
        // return null;
    }

    private IReference? TryGetReference(string name)
    {
        _nameToReferenceMap.TryGetValue(name, out var result);
        return result;
    }

    public void AddVariablesRange(IReadOnlyCollection<Variable> variables)
    {
        foreach (var variable in variables)
        {
            _nameToReferenceMap.Add(variable.Name, new VariableReference(variable.Name, variable));
        }
    }

    public IReadOnlySet<string> GetAllDeclaredNames() => _nameToReferenceMap.Select(x => x.Key).ToHashSet();

    public void Merge(DeclaredItemsRegistry registry)
    {
        foreach (var pair in registry._nameToReferenceMap)
        {
            if(!_nameToReferenceMap.ContainsKey(pair.Key))
                _nameToReferenceMap.Add(pair.Key, pair.Value);
        }
    }

    public async Task Compile(IParsingContext ctx)
    {
        var notCompiledReferences = _nameToReferenceMap.Where(pair => pair.Value is DraftReference { Compiled: null }).Select(x => x.Value).Cast<DraftReference>().ToArray();
        foreach (var reference in notCompiledReferences)
        {
            var draft = reference.Draft;
            var compiled = await _textPartsParser.Parse(draft.Pattern, ctx);
            reference.Compiled = new ObjectTextPartsComposite(compiled, draft.ReturnType ?? (compiled.IsString ? ReturnType.String : null));
        }
    }

    public void Add(InlineRuleVariable variable)
    {
        _nameToReferenceMap.Add(variable.Name, new VariableReference(variable.Name, variable));
    }

    record ObjectTextPartsComposite(ObjectTextParts Parts, ReturnType? ReturnType) : IObjectTextPart
    {
        public Task<dynamic?> Get(RuleExecutingContext context) => Parts.Generate(context);
    }
}

class ObjectTextPartWithSaveVariable(IObjectTextPart objectTextPart, IVariableReference variable) : IObjectTextPart
{
    public async Task<dynamic?> Get(RuleExecutingContext context)
    {
        var value = await objectTextPart.Get(context);
        variable.SetValue(context, value);
        return value;
    }

    public ReturnType? ReturnType => objectTextPart.ReturnType;
}

interface IVariableReference : IReference
{
    void SetValue(RuleExecutingContext context, dynamic value);
}

class VariableReference(string name, Variable inlineDeclareVariable) : Reference(name), IVariableReference
{
    public void SetValue(RuleExecutingContext context, dynamic value) => inlineDeclareVariable.SetValue(context, value);

    protected override IObjectTextPart GetObjectTextPart() => inlineDeclareVariable;
}

class DraftReference(string name, DeclaredItemDraft variableDraft) : Reference(name)
{
    public DeclaredItemDraft Draft { get; } = variableDraft;
    public IObjectTextPart? Compiled { get; set; }
    protected override IObjectTextPart? GetObjectTextPart() => Compiled;
}

class DraftVariableReference(string name, DeclaredItemDraft variableDraft) : DraftReference(name, variableDraft), IVariableReference
{
    private DeclaredRuleVariable? _compiledVariable;

    private DeclaredRuleVariable? CompiledVariable
    {
        get
        {
            if (_compiledVariable == null)
            {
                _compiledVariable = new DeclaredRuleVariable(Name,
                    [Compiled ?? throw new Exception("Not compiled")], ReturnType);
            }

            return _compiledVariable;
        }
    }

    public void SetValue(RuleExecutingContext context, dynamic value)
    {
        if (CompiledVariable == null)
            throw new Exception($"Variable '{Name}' was not compiled from draft");

        CompiledVariable.SetValue(context,value);
    }

    protected override IObjectTextPart? GetObjectTextPart() => CompiledVariable;
}

interface IReference : IObjectTextPart
{
    IObjectTextPart? ObjectTextPart { get; }
}

abstract class Reference(string name)
{
    protected string Name { get; } = name;

    protected abstract IObjectTextPart? GetObjectTextPart();

    public IObjectTextPart? ObjectTextPart => GetObjectTextPart();

    public Task<dynamic?> Get(RuleExecutingContext context) =>
        GetObjectTextPart()?.Get(context) ?? throw CreateReferenceNotSetException();

    private Exception CreateReferenceNotSetException() => new($"Reference for declaration '{Name}' not set");

    public ReturnType? ReturnType
    {
        get
        {
            var objectTextPart = GetObjectTextPart();
            if (objectTextPart == null)
                throw CreateReferenceNotSetException();

            return objectTextPart.ReturnType;
        }
    }
}

// record VariableReference(string Name, Variable? Variable) : ObjectTextPartReference(Name, Variable)
// {
//     public void SetValue(RuleExecutingContext ctx, dynamic? value)
//     {
//         if (Variable == null)
//             throw new Exception($"Reference for variable '{Name}' not set");
//         Variable.SetValue(ctx, value);
//     }
// }