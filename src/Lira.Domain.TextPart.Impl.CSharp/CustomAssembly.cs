﻿using System.Reflection;
using Lira.Domain.TextPart.Impl.CSharp.Compilation;

namespace Lira.Domain.TextPart.Impl.CSharp;

record CustomAssembly(Assembly LoadedAssembly, PeImage PeImage);
