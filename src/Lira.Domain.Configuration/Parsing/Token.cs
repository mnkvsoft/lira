using System.Text;
using Lira.Common;
using Lira.Common.Extensions;

namespace Lira.Domain.Configuration.Parsing;

public abstract record Token
{
    public record StaticData : Token
    {
        public string Content { get; set; }

        public StaticData(string content)
        {
            Content = content;
        }

        public override string ToString()
        {
            return "<t>" + Content + "</t>";
        }
    }

    public record Operator(OperatorDefinition Definition, string? Parameters) : Token
    {
        public void AddStaticContent(string staticContent)
        {
            // var indented = string.Join(Constants.NewLine, staticContent
            //                                         .Split(Constants.NewLine)
            //                                         .AlignIndents(NewLineIndent));

            // var staticData = new StaticData(indented);
            var staticData = new StaticData(staticContent);
            AddContent(staticData);
        }

        public void AddContent(Token token)
        {
            if (_elements.Count == 0)
            {
                _content.Add(token);
            }
            else
            {
                var lastItem = _elements.Last();
                lastItem.AddContent(token);
            }
        }

        private readonly List<Token> _content = new();
        public IReadOnlyList<Token> Content => _content;

        // public void ClearContentIfWhitespace()
        // {
        //     if(_content.IsWhiteSpace())
        //         _content.Clear();
        // }

        public void AddChildElement(OperatorElement element) => _elements.Add(element);
        private readonly List<OperatorElement> _elements = new();
        public IReadOnlyCollection<OperatorElement> Elements => _elements;

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
            public string? Parameters { get; }

            public void AddContent(Token token) => _content.Add(token);
            private readonly List<Token> _content = new();
            public IReadOnlyCollection<Token> Content => _content;

            public OperatorElement(AllowedChildElementDefinition definition, string? parameters)
            {
                if(definition.ParametersMode == ParametersMode.None && !string.IsNullOrEmpty(parameters))
                    throw new ArgumentException($"Element '{definition.Name}' for operator {definition.Operator.Name}' not expected parameters, but found: '{parameters}'");

                if(definition.ParametersMode == ParametersMode.Required && !string.IsNullOrWhiteSpace(parameters))
                    throw new ArgumentException($"Element '{definition.Name}' for operator {definition.Operator.Name}' expected parameters, but not found");

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