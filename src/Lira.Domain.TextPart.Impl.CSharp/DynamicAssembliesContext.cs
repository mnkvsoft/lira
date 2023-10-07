using System.Runtime.Loader;

// ReSharper disable RedundantExplicitArrayCreation

namespace Lira.Domain.TextPart.Impl.CSharp;

record DynamicAssembliesContext(int Revision, AssemblyLoadContext AssemblyLoadContext);
