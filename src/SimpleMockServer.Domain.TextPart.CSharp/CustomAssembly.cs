﻿using System.Reflection;

namespace SimpleMockServer.Domain.TextPart.CSharp;

public record CustomAssembly(Assembly LoadedAssembly, byte[] PeImage);