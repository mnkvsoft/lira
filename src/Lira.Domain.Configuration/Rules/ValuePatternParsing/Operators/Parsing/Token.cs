using System.Text;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Parsing;

abstract record Token
{
    public record StaticData : Token
    {
        public PatternParts Content { get; set; }

        public StaticData(PatternParts content)
        {
            Content = content;
        }

        public override string ToString()
        {
            return "<t>" + Content + "</t>";
        }
    }

    public record Operator : Token
    {
        public List<Token> Content = new();


        private readonly List<OperatorElement> _elements = new();
        public IReadOnlyList<OperatorElement> Elements => _elements;

        public OperatorDefinition Definition { get; }
        public OperatorParameters? Parameters { get; }

        public Operator(OperatorDefinition definition, OperatorParameters? parameters)
        {
            var parametersMode = definition.ParametersMode;

            if(parametersMode == ParametersMode.None && parameters != null)
                throw new ArgumentException($"@{definition.Name}' operator not expected parameters, but found: '{parameters}'");

            if(parametersMode == ParametersMode.Required && parameters == null)
                throw new ArgumentException($"@{definition.Name} operator expected parameters, but not found");

            Definition = definition;
            Parameters = parameters;
        }

        public void AddChildElement(OperatorElement element) => _elements.Add(element);

        public void AddStaticContent(PatternParts staticContent)
        {
            var staticData = new StaticData(staticContent);
            AddContent(staticData);
        }

        public void AddContent(Token token)
        {
            if (_elements.Count == 0)
            {
                Content.Add(token);
            }
            else
            {
                var lastItem = _elements.Last();
                lastItem.AddContent(token);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"<op name='{Definition.Name}' pars='{Parameters}'>");
            foreach (var token in Content)
            {
                sb.Append(token);
            }
            foreach (var child in Elements)
            {
                sb.Append(child);
            }
            sb.Append("</op>");
            return sb.ToString();
        }

        public class OperatorElement
        {
            public AllowedChildElementDefinition Definition { get; }
            public OperatorParameters? Parameters { get; }

            public void AddContent(Token token) => Content.Add(token);

            public List<Token> Content = new();

            public OperatorElement(AllowedChildElementDefinition definition, OperatorParameters? parameters)
            {
                if(definition.ParametersMode == ParametersMode.None && parameters != null)
                    throw new ArgumentException($"Element '@{definition.Name}' for '@{definition.Operator.Name}' operator not expected parameters, but found: '{parameters}'");

                if(definition.ParametersMode == ParametersMode.Required && parameters == null)
                    throw new ArgumentException($"Element '@{definition.Name}' for '@{definition.Operator.Name}' operator expected parameters, but not found");

                Definition = definition;
                Parameters = parameters;
            }

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append($"<i name='{Definition.Name}' pars='{Parameters}'>");
                foreach (var token in Content)
                {
                    sb.Append(token);
                }
                sb.Append("</i>");
                return sb.ToString();
            }
        }
    }
}