using System.Reflection;
using SimpleMockServer.Domain.TextPart.CSharp.Compilation;

namespace SimpleMockServer.Domain.TextPart.CSharp;

record CustomAssembly(Assembly LoadedAssembly, PeImage PeImage);
