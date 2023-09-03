using System.Runtime.Loader;

// ReSharper disable RedundantExplicitArrayCreation

namespace SimpleMockServer.Domain.TextPart.Impl.CSharp;

record DynamicAssembliesContext(int Revision, AssemblyLoadContext AssemblyLoadContext);
