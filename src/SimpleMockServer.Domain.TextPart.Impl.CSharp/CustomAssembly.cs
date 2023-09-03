using System.Reflection;
using SimpleMockServer.Domain.TextPart.Impl.CSharp.Compilation;

namespace SimpleMockServer.Domain.TextPart.Impl.CSharp;

record CustomAssembly(Assembly LoadedAssembly, PeImage PeImage);
