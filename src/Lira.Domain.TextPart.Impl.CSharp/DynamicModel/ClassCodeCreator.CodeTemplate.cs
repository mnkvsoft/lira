using Lira.Common;

namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

static partial class ClassCodeCreator
{
    static class CodeTemplate
    {
        private static readonly string Nl = Constants.NewLine;

        private readonly static string ImportNamespaces =
            "using System;" + Nl +
            "using System.IO;" + Nl +
            "using System.Text;" + Nl +
            "using System.Linq;" + Nl +
            "using System.Collections;" + Nl +
            "using System.Collections.Generic;" + Nl + Nl +
            "using System.Threading.Tasks;" + Nl + Nl +
            "using Lira.Domain.TextPart.Impl.CSharp.DynamicModel;" + Nl +
            "using Lira.Domain;" + Nl +
            "using Lira.Domain.TextPart;" + Nl +
            "using Lira.Domain.TextPart.Impl.CSharp;" + Nl +
            "using static Lira.Domain.TextPart.Impl.CSharp.Functions.JsonUtils;" + Nl +
            "[namespaces]";

        private const string Namespace = "namespace __DynamicGenerated;";

        public readonly static string IObjectTextPart =
            ImportNamespaces + Nl + Nl +
            "[usingstatic]" + Nl + Nl +
            Namespace + Nl + Nl +
            @"
public sealed class [className] : DynamicObjectBaseGenerate, IObjectTextPart
{
    public [className](Dependencies dependencies) : base(dependencies)
    {
    }

    public dynamic? Get(RuleExecutingContext [context])
    {
        var [externalRequestVariableName] = new RequestModel([context].RequestContext.RequestData);
        dynamic bag = new Bag([context], readOnly: true);

        [code]

        string [repeat](IObjectTextPart part, string separator = "","", int? count = null, int? from = null, int? to = null)
        {
            int cnt;
            if(count != null)
                cnt = count.Value;
            else if(from != null)
                cnt = Random.Shared.Next(from.Value, to.Value + 1);
            else
                cnt = Random.Shared.Next(3, 9);
            return Repeat([context], part, separator, cnt);
        }

        string? value(string name)
        {
            return [context].GetValue(name);
        }
    }
}";

        public readonly static string ITransformFunction =
            "[namespaces]" + Nl + Nl +
            ImportNamespaces + Nl + Nl +
            "[usingstatic]" + Nl + Nl +
            Namespace + Nl + Nl +
            @"
public sealed class [className] : ITransformFunction
{
    public dynamic? Transform(dynamic? [input])
    {
        [code]
    }
}
";

        public readonly static string IMatchFunction =
            "[namespaces]" + Nl + Nl +
            ImportNamespaces + Nl + Nl +
            "[usingstatic]" + Nl + Nl +
            Namespace + Nl + Nl +
            "using Lira.Domain.Matching.Request;" + Nl +
            @"

public sealed class [className] : DynamicObjectBaseMatch, IMatchFunction
{
    public [className](DependenciesBase dependencies) : base(dependencies)
    {
    }

    public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Custom;
    public bool IsMatch(RuleExecutingContext __ctx, string? [input])
    {
        dynamic bag = new Bag(__ctx, readOnly: false);
        [code]
    }
}
";

        public readonly static string IRequestMatcher =
            "[namespaces]" + Nl + Nl +
            ImportNamespaces + Nl + Nl +
            "[usingstatic]" + Nl + Nl +
            Namespace + Nl + Nl +
            "using Lira.Domain.Matching.Request;" + Nl +
            "using System.Collections.Immutable;" + Nl +
            @"

public sealed class [className] : DynamicObjectBaseRequestMatcher
{
    public [className](DependenciesBase dependencies) : base(dependencies)
    {
    }

    protected override async Task<bool> IsMatchInternal(RuleExecutingContext __ctx)
    {
        [code]

        string? value(string name)
        {
            return __ctx.GetValue(name);
        }
    }
}
";

        public readonly static string IAction =
            "[namespaces]" + Nl + Nl +
            ImportNamespaces + Nl + Nl +
            "[usingstatic]" + Nl + Nl +
            Namespace + Nl + Nl +
            "using Lira.Domain.Actions;" + Nl +
            @"

public sealed class [className] : DynamicObjectBaseAction, IAction
{
    public [className](Dependencies dependencies) : base(dependencies)
    {
    }

    public async Task Execute(RuleExecutingContext [context])
    {
        var [externalRequestVariableName] = new RequestModel([context].RequestContext.RequestData);
        dynamic bag = new Bag([context], readOnly: false);

        [code]

        string? value(string name)
        {
            return [context].GetValue(name);
        }
    }
}
";
    }
}