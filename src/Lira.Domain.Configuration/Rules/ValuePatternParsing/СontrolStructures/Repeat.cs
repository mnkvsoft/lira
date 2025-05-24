using System.Text;
using Lira.Domain.TextPart;

// ReSharper disable once CheckNamespace
namespace Lira.Domain.Configuration.Rules.ValuePatternParsing;

partial class TextPartsParser
{
    class RepeatControlStructure(
        RepeatControlStructure.ICountIterationFactory countIterationFactory,
        IReadOnlyCollection<IObjectTextPart> parts)
        : IObjectTextPart
    {
        public interface ICountIterationFactory
        {
            int Get(RuleExecutingContext context);
        }

        public async Task<dynamic?> Get(RuleExecutingContext context)
        {
            var count = countIterationFactory.Get(context);

            var sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                var part = await parts.Generate(context);
                sb.Append(part?.ToString());
            }

            return sb.ToString();
        }

        public ReturnType ReturnType => ReturnType.String;
    }

    class RandomControlStructure : IObjectTextPart
    {
        private IReadOnlyCollection<IObjectTextPart> _parts;
        public RandomControlStructure(IReadOnlyCollection<IObjectTextPart> parts)
        {
            var a = parts.
        }

        public async Task<dynamic?> Get(RuleExecutingContext context)
        {
            var count = countIterationFactory.Get(context);

            var sb = new StringBuilder();
            for (int i = 0; i < count; i++)
            {
                var part = await _parts.Generate(context);
                sb.Append(part?.ToString());
            }

            return sb.ToString();
        }

        public ReturnType? ReturnType => ReturnType.String;
    }
}
