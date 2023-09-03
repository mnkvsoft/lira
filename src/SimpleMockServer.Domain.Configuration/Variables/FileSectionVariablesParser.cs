﻿using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;
using SimpleMockServer.FileSectionFormat;

namespace SimpleMockServer.Domain.Configuration.Variables;

class FileSectionDeclaredItemsParser
{
    private readonly DeclaredItemsParser _parser;

    public FileSectionDeclaredItemsParser(DeclaredItemsParser parser) => _parser = parser;

    public Task<IReadonlyDeclaredItems> Parse(FileSection variablesSection, ParsingContext parsingContext)
        => _parser.Parse(variablesSection.LinesWithoutBlock, parsingContext);
}
