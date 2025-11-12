using Lira.Common;

namespace Lira.Domain.TextPart.Impl.CSharp.Compilation;

record PeImage(Hash Hash, PeBytes Bytes, string? AdditionInfo);
record PeBytes(byte[] Value);
