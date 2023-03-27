namespace SimpleMockServer.Domain.Functions.Native.UnitTests;

public class CallChainParser_Tests
{
    [Test]
    public void Valid_OneCall_WithoutArguments()
    {
        var chain = CallChainParser.Parse("Now()");
        Assert.AreEqual(1, chain.Count);

        var call = chain.Single();
        Assert.AreEqual("Now", call.Name);

        var args = call.Argumens;

        Assert.AreEqual(0, args.Count);
    }

    [Test]
    public void Valid_OneCall_WithoutSpaces()
    {
        var chain = CallChainParser.Parse("Sequence('name',1,1.3,true)");
        Assert.AreEqual(1, chain.Count);

        var call = chain.Single();
        Assert.AreEqual("Sequence", call.Name);

        var args = call.Argumens;

        Assert.AreEqual(4, args.Count);
        Assert.AreEqual("name", args[0]);
        Assert.AreEqual("1", args[1]);
        Assert.AreEqual("1.3", args[2]);
        Assert.AreEqual("true", args[3]);
    }

    [Test]
    public void Valid_OneCall_SpacesInArgs()
    {
        var chain = CallChainParser.Parse("Sequence(  'name' , 1 , 1.3 , true   )");
        Assert.AreEqual(1, chain.Count);

        var call = chain.Single();
        Assert.AreEqual("Sequence", call.Name);

        var args = call.Argumens;

        Assert.AreEqual(4, args.Count);
        Assert.AreEqual("name", args[0]);
        Assert.AreEqual("1", args[1]);
        Assert.AreEqual("1.3", args[2]);
        Assert.AreEqual("true", args[3]);
    }

    [Test]
    public void Valid_OneCall_SpacesInCallName()
    {
        var chain = CallChainParser.Parse("Sequence ('name', 1, 1.3, true)");
        Assert.AreEqual(1, chain.Count);

        var call = chain.Single();
        Assert.AreEqual("Sequence", call.Name);

        var args = call.Argumens;

        Assert.AreEqual(4, args.Count);
        Assert.AreEqual("name", args[0]);
        Assert.AreEqual("1", args[1]);
        Assert.AreEqual("1.3", args[2]);
        Assert.AreEqual("true", args[3]);
    }

    [Test]
    public void Valid_CallChain_WithotSpaces()
    {
        var chain = CallChainParser.Parse("Sequence('name').Add(1)");
        Assert.AreEqual(2, chain.Count);

        var call = chain[0];
        Assert.AreEqual("Sequence", call.Name);

        var args = call.Argumens;
        Assert.AreEqual(1, args.Count);
        Assert.AreEqual("name", args[0]);


        call = chain[1];
        Assert.AreEqual("Add", call.Name);

        args = call.Argumens;

        Assert.AreEqual(1, args.Count);
        Assert.AreEqual("1", args[0]);
    }

    [Test]
    public void Valid_CallChain_WithSpaces()
    {
        var chain = CallChainParser.Parse("One ('how many', false, 4.56 ) . Two(1, 'sdf') .Foo()");
        Assert.AreEqual(3, chain.Count);

        var call = chain[0];
        Assert.AreEqual("One", call.Name);

        var args = call.Argumens;
        Assert.AreEqual(3, args.Count);
        Assert.AreEqual("how many", args[0]);
        Assert.AreEqual("false", args[1]);
        Assert.AreEqual("4.56", args[2]);


        call = chain[1];
        Assert.AreEqual("Two", call.Name);

        args = call.Argumens;

        Assert.AreEqual(2, args.Count);
        Assert.AreEqual("1", args[0]);
        Assert.AreEqual("sdf", args[1]);


        call = chain[2];
        Assert.AreEqual("Foo", call.Name);

        args = call.Argumens;

        Assert.AreEqual(0, args.Count);
    }

    [Test]
    public void Invalid_OneCall_Empty()
    {
        var exc = Assert.Throws<CallChainParsingException>(() => CallChainParser.Parse("    "));
        Assert.AreEqual("Empty chain", exc.Message);
    }


    [Test]
    public void Invalid_OneCall_WithoutBrackets()
    {
        var exc = Assert.Throws<CallChainParsingException>(() => CallChainParser.Parse("WithourBrackets"));
        Assert.True(exc.Message.Contains("Missing brackets"));
    }

    [Test]
    public void Invalid_CallChain_WithoutBrackets()
    {
        var exc = Assert.Throws<CallChainParsingException>(() => CallChainParser.Parse("WithBrackets().WithoutBrackets"));
        Assert.True(exc.Message.Contains("Missing brackets"));
    }

    [Test]
    public void Invalid_OneCall_OnlyOpenBracket()
    {
        var exc = Assert.Throws<CallChainParsingException>(() => CallChainParser.Parse("Function("));
        Assert.True(exc.Message.Contains("Missing close bracket"));
    }

    [Test]
    public void Invalid_CallChain_OnlyOpenBracket()
    {
        var exc = Assert.Throws<CallChainParsingException>(() => CallChainParser.Parse("Function1().Function2("));
        Assert.True(exc.Message.Contains("Missing close bracket"));
    }


    [Test]
    public void Invalid_OneCall_NotEndArgumets()
    {
        var exc = Assert.Throws<CallChainParsingException>(() => CallChainParser.Parse("Function(12,"));
        Assert.True(exc.Message.Contains("Missing ')'"));
    }

    [Test]
    public void Invalid_CallChain_NotEndArgumets()
    {
        var exc = Assert.Throws<CallChainParsingException>(() => CallChainParser.Parse("Function(12).Function(45,"));
        Assert.True(exc.Message.Contains("Missing ')'"));
    }


    [Test]
    public void Invalid_OneCall_NotEndStringArgument()
    {
        var exc = Assert.Throws<CallChainParsingException>(() => CallChainParser.Parse("Function('dsfs sdfsf ()"));
        Assert.True(exc.Message.Contains("Not ended argument"));
    }

    [Test]
    public void Invalid_CallChain_NotEndStringArgument()
    {
        var exc = Assert.Throws<CallChainParsingException>(() => CallChainParser.Parse("Function('dsfs sdfsf ()').Func('dfdfdfdf "));
        Assert.True(exc.Message.Contains("Not ended argument"));
    }


    [Test]
    public void Invalid_CallChain_MissingDot()
    {
        var exc = Assert.Throws<CallChainParsingException>(() => CallChainParser.Parse("Function(1) Func(2)"));
        Assert.True(exc.Message.Contains("Expected '.' but"));
    }
}