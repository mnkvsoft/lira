using System.Reflection;
using Lira.Domain.TextPart.Impl.CSharp.Compilation;

namespace Lira.Domain.TextPart.Impl.CSharp;

record CsFilesAssembly(Assembly Loaded, PeImage PeImage);
