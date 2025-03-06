using Lira.Common;

namespace Lira.Domain.TextPart.Impl.CSharp.Compilation;

record PeImage(Hash Hash, PeBytes Bytes);
record PeBytes(byte[] Value);
