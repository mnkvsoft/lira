// using SimpleMockServer.Common.Extensions;
// using SimpleMockServer.Domain.Configuration.Rules.ValuePatternParsing;
// using SimpleMockServer.Domain.TextPart;
// using SimpleMockServer.Domain.TextPart.Variables;
//
// namespace SimpleMockServer.Domain.Configuration.Templating;
//
// class TemplatesParser
// {
//     public async Task<TemplateSet> Parse(IReadOnlyCollection<string> lines, ParsingContext parsingContext)
//     {
//         var all = new TemplateSet(parsingContext.Templates);
//         var newContext = parsingContext with { Templates = all };
//         
//         var onlyNew = new VariableSet();
//
//         foreach (var line in lines)
//         {
//             var (name, pattern) = line.SplitToTwoParts(Consts.ControlChars.AssignmentOperator).Trim();
//
//             if (string.IsNullOrEmpty(name))
//                 throw new Exception($"Variable name not defined. Line: {line}");
//
//             if (string.IsNullOrEmpty(pattern))
//                 throw new Exception($"Variable '{name}' not initialized. Line: {line}");
//
//             var parts = await _textPartsParser.Parse(pattern, newContext );
//             
//             all.Add(create(name, parts));
//             onlyNew.Add(create(name, parts));
//         }
//
//         return onlyNew;
//     }
// }
