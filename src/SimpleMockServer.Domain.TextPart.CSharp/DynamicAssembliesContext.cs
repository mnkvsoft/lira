using System.Runtime.Loader;

// ReSharper disable RedundantExplicitArrayCreation

namespace SimpleMockServer.Domain.TextPart.CSharp;

record DynamicAssembliesContext(int Revision, AssemblyLoadContext AssemblyLoadContext);
