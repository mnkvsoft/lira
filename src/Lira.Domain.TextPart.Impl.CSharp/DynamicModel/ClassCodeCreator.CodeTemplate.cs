using Lira.Common;

namespace Lira.Domain.TextPart.Impl.CSharp.DynamicModel;

static partial class ClassCodeCreator
{
    static class CodeTemplate
    {
        private static readonly string Nl = Constants.NewLine;

        private readonly static string ImportNamespaces =
            "[usingstatic]" + Nl + Nl +

            "using System;" + Nl +
            "using System.Text;" + Nl +
            "using System.Linq;" + Nl +
            "using System.Collections;" + Nl +
            "using System.Collections.Generic;" + Nl + Nl +
            "using System.Threading.Tasks;" + Nl + Nl +
            "using System.IO;" + Nl + Nl +
            "using Lira.Domain.TextPart.Impl.CSharp.DynamicModel;" + Nl +
            "using Lira.Domain;" + Nl +
            "using Lira.Domain.TextPart;" + Nl +
            "using Lira.Domain.TextPart.Impl.CSharp;" + Nl + Nl +

            "[namespaces]" + Nl + Nl +
            "[usings]" + Nl;


        private const string Namespace = "namespace __DynamicGenerated;";

        public readonly static string IObjectTextPart =
            ImportNamespaces + Nl + Nl +
            Namespace + Nl + Nl +
            """
            public sealed class [className] : DynamicObjectBaseGenerate, IObjectTextPart
            {
                Type IObjectTextPart.Type => [type];

                public [className](DependenciesBase dependencies) : base(dependencies)
                {
                }

                public dynamic? Get(RuleExecutingContext [context])
                {
                    var [externalRequestVariableName] = new RequestModel([context].RequestContext.RequestData);
                    var __variablesWriter = GetVariablesWriter([context], readOnly: false);

                    [code]
                }
            }
            """;

        public readonly static string ITransformFunction =
            ImportNamespaces + Nl + Nl +
            Namespace + Nl + Nl +
            """
            public sealed class [className] : ITransformFunction
            {
                Type ITransformFunction.Type => DotNetType.Unknown;

                public dynamic? Transform(dynamic? [input])
                {
                    [code]
                }
            }
            """;

        public readonly static string IMatchFunction =
            ImportNamespaces + Nl + Nl +
            Namespace + Nl + Nl +
            "using Lira.Domain.Matching.Request;" + Nl +
            """
            public sealed class [className] : DynamicObjectBaseMatch, IMatchFunctionTyped
            {
                public Type ValueType => DotNetType.Unknown;

                public [className](DependenciesBase dependencies) : base(dependencies)
                {
                }

                public MatchFunctionRestriction Restriction => MatchFunctionRestriction.Custom;

                public bool IsMatch(RuleExecutingContext context, string? value) => IsMatchTyped(context, value, out _);

                public bool IsMatchTyped(RuleExecutingContext __ctx, string? [input], out dynamic? typedValue)
                {
                    typedValue = value;

                    var __variablesWriter = GetVariablesWriter(__ctx, readOnly: false);
                   [code]
                }
            }
            """;

        public readonly static string IPredicateFunction =
            ImportNamespaces + Nl + Nl +
            Namespace + Nl + Nl +
            "using Lira.Domain.Matching.Request;" + Nl +
            """
            public sealed class [className] : DynamicObjectWithDeclaredPartsBase, IPredicateFunction
            {
                public [className](DependenciesBase dependencies) : base(dependencies)
                {
                }

                public bool IsMatch(RuleExecutingContext __ctx)
                {
                    var [externalRequestVariableName] = new RequestModel(__ctx.RequestContext.RequestData);
                    [code]
                }
            }
            """;

        public readonly static string IRequestMatcher =
            ImportNamespaces + Nl + Nl +
            Namespace + Nl + Nl +
            "using Lira.Domain.Matching.Request;" + Nl +
            "using System.Collections.Immutable;" + Nl +
            """
            public sealed class [className] : DynamicObjectBaseRequestMatcher
            {
                public [className](DependenciesBase dependencies) : base(dependencies)
                {
                }

                protected override bool IsMatchInternal(RuleExecutingContext __ctx)
                {
                    var [externalRequestVariableName] = new RequestModel(__ctx.RequestContext.RequestData);
                    var __variablesWriter = GetVariablesWriter(__ctx, readOnly: false);

                    [code]
                }
            }
            """;

        public readonly static string IAction =
            ImportNamespaces + Nl + Nl +
            Namespace + Nl + Nl +
            "using Lira.Domain.Handling.Actions;" + Nl +
            """

            public sealed class [className] : DynamicObjectBaseAction, IAction
            {
                public [className](DependenciesBase dependencies) : base(dependencies)
                {
                }

                public async Task Execute(RuleExecutingContext [context])
                {
                    var [externalRequestVariableName] = new RequestModel([context].RequestContext.RequestData);
                    var __variablesWriter = GetVariablesWriter([context], readOnly: false);

                    [code]
                }
            }
            """;
    }
}