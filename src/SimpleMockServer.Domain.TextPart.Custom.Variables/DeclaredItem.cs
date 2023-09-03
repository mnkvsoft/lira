// using ArgValidation;
// using SimpleMockServer.Common;
//
// namespace SimpleMockServer.Domain.TextPart.Custom.Variables;
//
// public abstract record DeclaredItem : IObjectTextPart, IUniqueSetItem
// {
//     public string Name { get; }
//     public abstract string EntityName { get; }
//
//     protected DeclaredItem(string name)
//     {
//         Arg.NotNullOrEmpty(name, nameof(name));
//
//
//     public abstract dynamic? Get(RequestData request);
// }