using Lira.Domain.Configuration.Rules.ValuePatternParsing.Operators.Handlers.ParametersParsing;

namespace Lira.Domain.Configuration.UnitTests;

public class FullParamsParserTests
{
    #region common

    [TestCase(
        """
        boolParam: true,
        strParam: "some string value",
        intParam: 1,
        decParam: 2.3
        """,
        "[" +
            "[boolParam:bool = true], " +
            "[strParam:str = 'some string value'], " +
            "[intParam:int = 1], " +
            "[decParam:dec = 2.3]" +
        "]")]
    [TestCase(
        """
        boolParam: true,
        intParam: 1,
        decParam: 2.3,
        strParam: "some string value"
        """,
        "[" +
            "[boolParam:bool = true], " +
            "[intParam:int = 1], " +
            "[decParam:dec = 2.3], " +
            "[strParam:str = 'some string value']" +
        "]")]
    [TestCase(
        """
        decParam: 2.3,
        strParam: "some string value",
        intParam: 1,
        boolParam: true
        """,
        "[" +
            "[decParam:dec = 2.3], " +
            "[strParam:str = 'some string value'], " +
            "[intParam:int = 1], " +
            "[boolParam:bool = true]" +
        "]")]
    public void HappyPath(string str, string expected)
    {
        var boolDef = MethodParameterDefinition.Bool("boolParam", isRequired: true);
        var strDef = MethodParameterDefinition.Str("strParam", isRequired: true);
        var intDef = MethodParameterDefinition.Int("intParam", isRequired: true);
        var decDef = MethodParameterDefinition.Dec("decParam", isRequired: true);

        var result = FullParamsParser.Parse(
            new HashSet<MethodParameterDefinition> { boolDef, strDef, intDef, decDef },
            str);

        Assert.That(result.ToString(), Is.EqualTo(expected));
    }

    [Test]
    public void MissingRequiredParam()
    {
        var boolDef = MethodParameterDefinition.Bool("boolParam", isRequired: true);
        var strDef = MethodParameterDefinition.Str("strParam", isRequired: true);

        var result = FullParamsParser.Parse(
            new HashSet<MethodParameterDefinition> { boolDef, strDef  },
            "boolParam: true");

        Assert.That(result.ToString(), Is.EqualTo("[fail: Missing required parameters: strParam]"));
    }

    [Test]
    public void NotRegisteredParam()
    {
        var boolDef = MethodParameterDefinition.Bool("boolParam", isRequired: true);
        var decDef = MethodParameterDefinition.Dec("decParam", isRequired: true);

        var result = FullParamsParser.Parse(
            new HashSet<MethodParameterDefinition> { boolDef, decDef },
            """
            boolParam: true,
            strParam: "some string value",
            intParam: 1
            """);

        Assert.That(result.ToString(), Is.EqualTo("[fail: Unknown parameter: strParam. Available parameters: boolParam, decParam]"));
    }

    [TestCase(
        "asdf boolParam: true",
        "Expected ':' after parameter name 'asdf'")]
    [TestCase(
        "asdf, boolParam: true",
        "Expected ':' after parameter name 'asdf'")]
    [TestCase(
        "asdf",
        "Expected ':' after parameter name 'asdf'")]
    [TestCase(
        "asd fgh jkl",
        "Expected ':' after parameter name 'asd'")]
    [TestCase(
        "boolParam: true,",
        "After ',' the parameter name is expected to be entered")]
    public void Invalid(string invalidValue, string expectedMessage)
    {
        var result = FullParamsParser.Parse(
            new HashSet<MethodParameterDefinition> { MethodParameterDefinition.Bool("boolParam", isRequired: true)},
            invalidValue);

        Assert.That(result, Is.InstanceOf<ParamsParseResult.Fail>());

        var fail = (ParamsParseResult.Fail)result;
        Assert.That(fail.Message, Is.EqualTo(expectedMessage));
    }

    #endregion

  #region bool

  [TestCase(
      "boolParam: true asd",
      "Parameter 'boolParam' is boolean and can have values: true, false. Current value: 'true asd'")]
  [TestCase(
      "boolParam: true12",
      "Parameter 'boolParam' is boolean and can have values: true, false. Current value: 'true12'")]
  [TestCase(
      "boolParam: True",
      "Parameter 'boolParam' is boolean and can have values: true, false. Current value: 'True'")]
  [TestCase(
      "boolParam: False",
      "Parameter 'boolParam' is boolean and can have values: true, false. Current value: 'False'")]
  public void BoolInvalid(string invalidValue, string expectedMessage)
  {
      var result = FullParamsParser.Parse(
          new HashSet<MethodParameterDefinition> { MethodParameterDefinition.Bool("boolParam", isRequired: true)},
          invalidValue);

      Assert.That(result, Is.InstanceOf<ParamsParseResult.Fail>());

      var fail = (ParamsParseResult.Fail)result;
      Assert.That(fail.Message, Is.EqualTo(expectedMessage));
  }

  [TestCase(
      "boolParam: true",
      "true")]
  [TestCase(
      "boolParam: false",
      "false")]
  [TestCase(
      "true",
      "true")]
  [TestCase(
      "false",
      "false")]
  public void BoolValid(string str, string expectedValue)
  {
      var parameterDefinition = MethodParameterDefinition.Bool("boolParam", isRequired: true);
      var result = FullParamsParser.Parse(
          new HashSet<MethodParameterDefinition> { parameterDefinition },
          str,
          parameterDefinition);

      Assert.That(result.ToString(), Is.EqualTo($"[[boolParam:bool = {expectedValue}]]"));
  }

  [Test]
  public void BoolDefault()
  {
      var defParam = MethodParameterDefinition.Bool("boolParam", isRequired: true);
      var result = FullParamsParser.Parse(
          new HashSet<MethodParameterDefinition> { defParam },
          "true",
          defParam);

      Assert.That(result.ToString(), Is.EqualTo(
          "[[boolParam:bool = true]]"));
  }

  #endregion



    [TestCase(
        "strParam: \"not closed",
        "Parameter 'strParam' is string and must be closed with: '\"'")]
    [TestCase(
        "strParam: \"not closed\"asd",
        "Parameter 'strParam' contains unexpected value: '\"asd'")]
    [TestCase(
        "strParam: \"with escaped quote \\\"",
        "Parameter 'strParam' is string and must be closed with: '\"'")]
    [TestCase(
        "\"with escaped quote",
        "Parameter 'strParam' is string and must be closed with: '\"'")]
    public void StrInvalid(string invalidValue, string expectedMessage)
    {
        var parameterDefinition = MethodParameterDefinition.Str("strParam", isRequired: true);
        var result = FullParamsParser.Parse(
            new HashSet<MethodParameterDefinition> { parameterDefinition },
            invalidValue,
            parameterDefinition);

        Assert.That(result.ToString(), Is.EqualTo($"[fail: {expectedMessage}]"));
    }

    [TestCase(
        "strParam: \"it's valid value\"",
        "it's valid value")]
    [TestCase(
        "strParam: \"with escaped \\\n\\\r\"",
        "with escaped \n\r")]
    public void StrValid(string str, string expectedValue)
    {
        var parameterDefinition = MethodParameterDefinition.Str("strParam", isRequired: true);
        var result = FullParamsParser.Parse(
            new HashSet<MethodParameterDefinition> { parameterDefinition },
            str,
            parameterDefinition);

        Assert.That(result.ToString(), Is.EqualTo($"[[strParam:str = '{expectedValue}']]"));
    }

    [TestCase(
        "intParam: 1.0",
        "Parameter 'intParam' is decimal but have invalid value: '1.0'")]
    [TestCase(
        "intParam: 2as",
        "Parameter 'intParam' is decimal but have invalid value: '2as'")]
    [TestCase(
        "intParam: as2",
        "Parameter 'intParam' is decimal but have invalid value: 'as2'")]
    [TestCase(
        "intParam: as",
        "Parameter 'intParam' is decimal but have invalid value: 'as'")]
    [TestCase(
        "2.0",
        "Parameter 'intParam' is decimal but have invalid value: '2.0'")]
    [TestCase(
        "\"2\"",
        "Parameter 'intParam' is decimal but have invalid value: '\"2\"'")]
    [TestCase(
        "'2'",
        "Parameter 'intParam' is decimal but have invalid value: ''2''")]
    public void IntInvalid(string invalidValue, string expectedMessage)
    {
        var parameterDefinition = MethodParameterDefinition.Int("intParam", isRequired: true);
        var result = FullParamsParser.Parse(
            new HashSet<MethodParameterDefinition> { parameterDefinition },
            invalidValue,
            parameterDefinition);

        Assert.That(result.ToString(), Is.EqualTo($"[fail: {expectedMessage}]"));
    }

    [TestCase(
        "intParam: 1",
        "1")]
    public void IntValid(string invalidValue, string expectedMessage)
    {
        var parameterDefinition = MethodParameterDefinition.Int("intParam", isRequired: true);
        var result = FullParamsParser.Parse(
            new HashSet<MethodParameterDefinition> { parameterDefinition },
            invalidValue,
            parameterDefinition);

        Assert.That(result.ToString(), Is.EqualTo($"[[intParam:int = {expectedMessage}]]"));
    }

    [TestCase(
        "decParam: 2.45as",
        "Parameter 'decParam' is decimal but have invalid value: '2.45as'")]
    [TestCase(
        "decParam: as2",
        "Parameter 'decParam' is decimal but have invalid value: 'as2'")]
    [TestCase(
        "decParam: asd",
        "Parameter 'decParam' is decimal but have invalid value: 'asd'")]
    [TestCase(
        "asd",
        "Parameter 'decParam' is decimal but have invalid value: 'asd'")]
    [TestCase(
        "\"2.45\"",
        "Parameter 'decParam' is decimal but have invalid value: '\"2.45\"'")]
    [TestCase(
        "'2'",
        "Parameter 'decParam' is decimal but have invalid value: ''2''")]
    public void DecInvalid(string invalidValue, string expectedMessage)
    {
        var parameterDefinition = MethodParameterDefinition.Dec("decParam", isRequired: true);
        var result = FullParamsParser.Parse(
            new HashSet<MethodParameterDefinition> { parameterDefinition },
            invalidValue,
            parameterDefinition);

        Assert.That(result.ToString(), Is.EqualTo($"[fail: {expectedMessage}]"));
    }

    [TestCase("decParam: 2.45", "2.45")]
    [TestCase("decParam: 2", "2")]
    public void DecValid(string invalidValue, string value)
    {
        var parameterDefinition = MethodParameterDefinition.Dec("decParam", isRequired: true);
        var result = FullParamsParser.Parse(
            new HashSet<MethodParameterDefinition> { parameterDefinition },
            invalidValue,
            parameterDefinition);

        Assert.That(result.ToString(), Is.EqualTo($"[[decParam:dec = {value}]]"));
    }
}