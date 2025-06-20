namespace Lira.Domain.Configuration.Parsing;

public abstract record Token
{
    public record StaticData(string Content) : Token;

    public record Operator(OperatorDefinition Definition, string? Parameters) : Token
    {
        public void AddContent(Token token) => _content.Add(token);
        private readonly List<Token> _content = new();
        public IReadOnlyCollection<Token> Content => _content;


        public void AddChildElement(OperatorElement element) => _children.Add(element);
        private readonly List<OperatorElement> _children = new();
        public IReadOnlyCollection<OperatorElement> Children => _children;


        public class OperatorElement
        {
            public AllowedChildElementDefinition Definition { get; }
            public string? Parameters { get; }

            public void AddContent(Token token) => _content.Add(token);
            private readonly List<Token> _content = new();
            public IReadOnlyCollection<Token> Content => _content;

            public OperatorElement(AllowedChildElementDefinition definition, string? parameters)
            {
                if(string.IsNullOrWhiteSpace(parameters) && definition.ParametersIsRequired)
                    throw new ArgumentException($"Element '{definition.Name}' for operator {definition.Operator.Name}' expected parameters, but not found");

                Definition = definition;
                Parameters = parameters;
            }
        }
    }
}