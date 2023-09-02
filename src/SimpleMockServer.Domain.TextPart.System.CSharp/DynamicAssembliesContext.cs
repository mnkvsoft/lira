using System.Runtime.Loader;

// ReSharper disable RedundantExplicitArrayCreation

namespace SimpleMockServer.Domain.TextPart.System.CSharp;

record DynamicAssembliesContext(int Revision, AssemblyLoadContext AssemblyLoadContext);
