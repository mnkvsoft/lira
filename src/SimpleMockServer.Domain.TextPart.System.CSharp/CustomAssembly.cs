using System.Reflection;
using SimpleMockServer.Domain.TextPart.System.CSharp.Compilation;

namespace SimpleMockServer.Domain.TextPart.System.CSharp;

record CustomAssembly(Assembly LoadedAssembly, PeImage PeImage);
