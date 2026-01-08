using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using Lira.Common;
using Lira.Domain.Configuration.Extensions;

namespace Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Handlers.ParametersParsing;

static class FullParamsParser
{
    public static ParamsParseResult Parse(IReadOnlySet<MethodParameterDefinition> definitions, string str, MethodParameterDefinition? defaultParameter = null )
    {
        if(definitions.Count == 0)
            throw new ArgumentException("Empty definitions");

        if (string.IsNullOrWhiteSpace(str))
            return new ParamsParseResult.Fail("Empty parameters are not allowed");

        if(defaultParameter != null && !definitions.Contains(defaultParameter))
            throw new ArgumentException($"Default parameter {defaultParameter} must be contains in definitions");

        var iterator = new StringIterator(str);
        var result = new HashSet<MethodParameter>(definitions.Count);

        var nameToDefinitionMap = definitions.ToDictionary(def => def.Name, def => def);

        bool firstParamFound = false;
        bool expectedParamInput = false;

        ParamsParseResult? fail;
        MethodParameter? param;

        // ищем начало ввода имени параметра
        while (iterator.MoveTo(currentPredicate: char.IsLetter, available: char.IsWhiteSpace))
        {
            firstParamFound = true;

            if (!iterator.MoveTo(currentPredicate: c => c == ':', available: char.IsLetter))
            {
                if(defaultParameter != null)
                {
                    if (!ReadParamValue(defaultParameter, iterator, out fail, out param))
                        return fail;

                    result.Add(param);
                    return new ParamsParseResult.Success(result);
                }

                if(iterator.MoveTo(c => !char.IsLetter(c)))
                    return new ParamsParseResult.Fail($"Expected ':' after parameter name '{iterator.PopExcludeCurrent()}'");

                return new ParamsParseResult.Fail($"Expected ':' after parameter name '{iterator.PeekFromCurrentToEnd()}'");
            }

            // закончен ввод имени параметра
            var name = iterator.PopExcludeCurrent()?.Trim() ?? throw new Exception("name is null");

            // убрали :
            iterator.PopIncludeCurrent();

            if (!nameToDefinitionMap.TryGetValue(name, out var definition))
                return new ParamsParseResult.Fail($"Unknown parameter: {name}. Available parameters: {string.Join(", ", definitions.Select(d => d.Name))}");

            if (!ReadParamValue(definition, iterator, out fail, out param))
                return fail;

            if (!result.Add(param))
                return new ParamsParseResult.Fail($"Duplicate parameter: {definition.Name}");

            expectedParamInput = iterator.Current == ',';
        }

        if (!firstParamFound)
        {
            if(defaultParameter != null)
            {
                if (!ReadParamValue(defaultParameter, iterator, out fail, out param))
                    return fail;

                result.Add(param);
                return new ParamsParseResult.Success(result);
            }

            return new ParamsParseResult.Fail($"String '{str}' not contains any parameters");
        }

        if(expectedParamInput)
            return new ParamsParseResult.Fail("After ',' the parameter name is expected to be entered");

        var requiredParams = definitions.Where(d => d.IsRequired);

        var missingParams = requiredParams.Where(rp => result.All(x => x.Definition != rp)).Select(x=> x.Name).ToArray();

        if (missingParams.Length != 0)
            return new ParamsParseResult.Fail($"Missing required parameters: {string.Join(", ", missingParams)}");

        return new ParamsParseResult.Success(result);
    }

    private static bool ReadParamValue(
        MethodParameterDefinition definition,
        StringIterator iterator,
        [MaybeNullWhen(true)] out ParamsParseResult fail,
        [MaybeNullWhen(false)] out MethodParameter param)
    {
        param = null;
        fail = null;

        if (definition.Type == typeof(MethodParameter.String))
        {
            if (!iterator.MoveTo('"'))
            {
                fail = new ParamsParseResult.Fail($"Parameter '{definition.Name}' is string and must be start with: '\"'");
                return false;
            }

            // убираем стартовую кавычку
            iterator.PopIncludeCurrent();

            if(!iterator.MoveToCloseStringParameterValue())
            {
                fail = new ParamsParseResult.Fail($"Parameter '{definition.Name}' is string and must be closed with: '\"'");
                return false;
            }

            var value = iterator.PopExcludeCurrent() ?? throw new Exception("value is null");
            iterator.PopIncludeCurrent();

            if(!iterator.MoveTo(currentPredicate: c => c == ',', available: char.IsWhiteSpace))
            {
                if (iterator.HasNext())
                {
                    fail = new ParamsParseResult.Fail(
                        $"Parameter '{definition.Name}' contains unexpected value: '{iterator.PeekFromCurrentToEnd()}'");
                    return false;
                }
            }

            iterator.PopIncludeCurrent();

            var unescapedValue = Regex.Unescape(value);
            param = new MethodParameter.String(definition, unescapedValue);
            return true;
        }

        if (definition.Type == typeof(MethodParameter.Bool))
        {
            var value = iterator.MoveToNextParameterOrEnd();

            if (!(value is "true" or "false"))
            {
                fail = new ParamsParseResult.Fail($"Parameter '{definition.Name}' is boolean and can have values: true, false. Current value: '{value}'");
                return false;
            }

            param = new MethodParameter.Bool(definition, value == "true");
            return true;
        }

        if (definition.Type == typeof(MethodParameter.Dec))
        {
            var value = iterator.MoveToNextParameterOrEnd();

            if (!double.TryParse(value, CultureInfo.InvariantCulture, out var decValue))
            {
                fail = new ParamsParseResult.Fail($"Parameter '{definition.Name}' is decimal but have invalid value: '{value}'");
                return false;
            }

            param = new MethodParameter.Dec(definition, decValue);
            return true;
        }

        if (definition.Type == typeof(MethodParameter.Int))
        {
            var value = iterator.MoveToNextParameterOrEnd();

            if (!int.TryParse(value, out var intValue))
            {
                fail = new ParamsParseResult.Fail($"Parameter '{definition.Name}' is decimal but have invalid value: '{value}'");
                return false;
            }

            param = new MethodParameter.Int(definition, intValue);
            return true;
        }

        throw new Exception($"Parameter '{definition.Name}' has an unknown type: {definition.Type}");
    }
}