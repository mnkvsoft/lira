using System.Text;
using Lira.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Lira.Domain.TextPart.Impl.CSharp.Compilation;

record UsageAssemblies(IReadOnlyCollection<PeImage> Runtime, IReadOnlyCollection<string> AssembliesLocations);

record PeImage(Hash Hash, byte[] Bytes);

static class CodeCompiler
{
    public static CompileResult Compile(CompileUnit compileUnit)
    {
        var compilation = CreateCompilation(compileUnit);

        using var ms = new MemoryStream();

        var emitResult = compilation.Emit(ms);

        if (!emitResult.Success)
            return new CompileResult.Fault(CreateFaultMessage(emitResult));

        ms.Seek(0, SeekOrigin.Begin);

        var bytes = ms.ToArray();
        return new CompileResult.Success(new PeImage(compileUnit.GetHash(), bytes));
    }

    private static CSharpCompilation CreateCompilation(CompileUnit compileUnit)
    {
        var syntaxTrees = compileUnit.Codes.Select(code => CSharpSyntaxTree.ParseText(code));

        var compilation = CSharpCompilation.Create(
            compileUnit.AssemblyName,
            syntaxTrees: syntaxTrees,
            references: GetReferences(compileUnit.UsageAssemblies),
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        return compilation;
    }

    private static IReadOnlyCollection<MetadataReference> GetReferences(UsageAssemblies? usageAssemblies)
    {
        //The location of the .NET assemblies
        var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

        if (assemblyPath == null)
            throw new Exception("Assembly path is null");

        var result = Dlls
            .Select(dllName => MetadataReference.CreateFromFile(Path.Combine(assemblyPath, dllName)))
            .ToList();

        if (usageAssemblies != null)
        {
            result.AddRange(usageAssemblies.AssembliesLocations.Select(location =>  MetadataReference.CreateFromFile(location)));
            result.AddRange(usageAssemblies.Runtime.Select(peImage => MetadataReference.CreateFromImage(peImage.Bytes)));
        }

        return result;
    }

    private static string CreateFaultMessage(EmitResult result)
    {
        var failures = result.Diagnostics.Where(diagnostic =>
            diagnostic.IsWarningAsError ||
            diagnostic.Severity == DiagnosticSeverity.Error)
            .ToArray();

        var sb = new StringBuilder();
        sb.AppendLine("Errors: ");


        if (!failures.Any())
        {
            sb.AppendLine("- unknown error while compiling code");
        }
        else
        {
            foreach (var diagnostic in failures)
            {
                sb.AppendLine($"- {diagnostic.Id} {diagnostic.GetMessage()}");
            }
        }

        return sb.ToString();
    }

    private static readonly string[] Dlls =
    [
        "System.Private.CoreLib.dll",
        "Microsoft.CSharp.dll",

        // these assemblies passed to csc.exe from "dotnet build" command by default
        "mscorlib.dll",
        "netstandard.dll",
        "System.AppContext.dll",
        "System.Buffers.dll",
        "System.Collections.Concurrent.dll",
        "System.Collections.dll",
        "System.Collections.Immutable.dll",
        "System.Collections.NonGeneric.dll",
        "System.Collections.Specialized.dll",
        "System.ComponentModel.Annotations.dll",
        "System.ComponentModel.DataAnnotations.dll",
        "System.ComponentModel.dll",
        "System.ComponentModel.EventBasedAsync.dll",
        "System.ComponentModel.Primitives.dll",
        "System.ComponentModel.TypeConverter.dll",
        "System.Configuration.dll",
        "System.Console.dll",
        "System.Core.dll",
        "System.Data.Common.dll",
        "System.Data.DataSetExtensions.dll",
        "System.Data.dll",
        "System.Diagnostics.Contracts.dll",
        "System.Diagnostics.Debug.dll",
        "System.Diagnostics.DiagnosticSource.dll",
        "System.Diagnostics.FileVersionInfo.dll",
        "System.Diagnostics.Process.dll",
        "System.Diagnostics.StackTrace.dll",
        "System.Diagnostics.TextWriterTraceListener.dll",
        "System.Diagnostics.Tools.dll",
        "System.Diagnostics.TraceSource.dll",
        "System.Diagnostics.Tracing.dll",
        "System.dll",
        "System.Drawing.dll",
        "System.Drawing.Primitives.dll",
        "System.Dynamic.Runtime.dll",
        "System.Formats.Asn1.dll",
        "System.Formats.Tar.dll",
        "System.Globalization.Calendars.dll",
        "System.Globalization.dll",
        "System.Globalization.Extensions.dll",
        "System.IO.Compression.Brotli.dll",
        "System.IO.Compression.dll",
        "System.IO.Compression.FileSystem.dll",
        "System.IO.Compression.ZipFile.dll",
        "System.IO.dll",
        "System.IO.FileSystem.AccessControl.dll",
        "System.IO.FileSystem.dll",
        "System.IO.FileSystem.DriveInfo.dll",
        "System.IO.FileSystem.Primitives.dll",
        "System.IO.FileSystem.Watcher.dll",
        "System.IO.IsolatedStorage.dll",
        "System.IO.MemoryMappedFiles.dll",
        "System.IO.Pipes.AccessControl.dll",
        "System.IO.Pipes.dll",
        "System.IO.UnmanagedMemoryStream.dll",
        "System.Linq.dll",
        "System.Linq.Expressions.dll",
        "System.Linq.Parallel.dll",
        "System.Linq.Queryable.dll",
        "System.Memory.dll",
        "System.Net.dll",
        "System.Net.Http.dll",
        "System.Net.Http.Json.dll",
        "System.Net.HttpListener.dll",
        "System.Net.Mail.dll",
        "System.Net.NameResolution.dll",
        "System.Net.NetworkInformation.dll",
        "System.Net.Ping.dll",
        "System.Net.Primitives.dll",
        "System.Net.Quic.dll",
        "System.Net.Requests.dll",
        "System.Net.Security.dll",
        "System.Net.ServicePoint.dll",
        "System.Net.Sockets.dll",
        "System.Net.WebClient.dll",
        "System.Net.WebHeaderCollection.dll",
        "System.Net.WebProxy.dll",
        "System.Net.WebSockets.Client.dll",
        "System.Net.WebSockets.dll",
        "System.Numerics.dll",
        "System.Numerics.Vectors.dll",
        "System.ObjectModel.dll",
        "System.Reflection.DispatchProxy.dll",
        "System.Reflection.dll",
        "System.Reflection.Emit.dll",
        "System.Reflection.Emit.ILGeneration.dll",
        "System.Reflection.Emit.Lightweight.dll",
        "System.Reflection.Extensions.dll",
        "System.Reflection.Metadata.dll",
        "System.Reflection.Primitives.dll",
        "System.Reflection.TypeExtensions.dll",
        "System.Resources.Reader.dll",
        "System.Resources.ResourceManager.dll",
        "System.Resources.Writer.dll",
        "System.Runtime.CompilerServices.Unsafe.dll",
        "System.Runtime.CompilerServices.VisualC.dll",
        "System.Runtime.dll",
        "System.Runtime.Extensions.dll",
        "System.Runtime.Handles.dll",
        "System.Runtime.InteropServices.dll",
        "System.Runtime.InteropServices.JavaScript.dll",
        "System.Runtime.InteropServices.RuntimeInformation.dll",
        "System.Runtime.Intrinsics.dll",
        "System.Runtime.Loader.dll",
        "System.Runtime.Numerics.dll",
        "System.Runtime.Serialization.dll",
        "System.Runtime.Serialization.Formatters.dll",
        "System.Runtime.Serialization.Json.dll",
        "System.Runtime.Serialization.Primitives.dll",
        "System.Runtime.Serialization.Xml.dll",
        "System.Security.AccessControl.dll",
        "System.Security.Claims.dll",
        "System.Security.Cryptography.Algorithms.dll",
        "System.Security.Cryptography.Cng.dll",
        "System.Security.Cryptography.Csp.dll",
        "System.Security.Cryptography.dll",
        "System.Security.Cryptography.Encoding.dll",
        "System.Security.Cryptography.OpenSsl.dll",
        "System.Security.Cryptography.Primitives.dll",
        "System.Security.Cryptography.X509Certificates.dll",
        "System.Security.dll",
        "System.Security.Principal.dll",
        "System.Security.Principal.Windows.dll",
        "System.Security.SecureString.dll",
        "System.ServiceModel.Web.dll",
        "System.ServiceProcess.dll",
        "System.Text.Encoding.CodePages.dll",
        "System.Text.Encoding.dll",
        "System.Text.Encoding.Extensions.dll",
        "System.Text.Encodings.Web.dll",
        "System.Text.Json.dll",
        "System.Text.RegularExpressions.dll",
        "System.Threading.Channels.dll",
        "System.Threading.dll",
        "System.Threading.Overlapped.dll",
        "System.Threading.Tasks.Dataflow.dll",
        "System.Threading.Tasks.dll",
        "System.Threading.Tasks.Extensions.dll",
        "System.Threading.Tasks.Parallel.dll",
        "System.Threading.Thread.dll",
        "System.Threading.ThreadPool.dll",
        "System.Threading.Timer.dll",
        "System.Transactions.dll",
        "System.Transactions.Local.dll",
        "System.ValueTuple.dll",
        "System.Web.dll",
        "System.Web.HttpUtility.dll",
        "System.Windows.dll",
        "System.Xml.dll",
        "System.Xml.Linq.dll",
        "System.Xml.ReaderWriter.dll",
        "System.Xml.Serialization.dll",
        "System.Xml.XDocument.dll",
        "System.Xml.XmlDocument.dll",
        "System.Xml.XmlSerializer.dll",
        "System.Xml.XPath.dll",
        "System.Xml.XPath.XDocument.dll",
        "WindowsBase.dll"
    ];
}
