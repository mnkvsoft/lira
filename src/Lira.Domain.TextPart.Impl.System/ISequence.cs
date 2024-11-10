using Lira.Common;
using Lira.Common.State;

namespace Lira.Domain.TextPart.Impl.System;

public class SystemSequence() : SequenceStateful(new Int64Sequence(), "seq");