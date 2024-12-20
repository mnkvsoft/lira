﻿namespace Lira.Common.Extensions;

public static class PathExtensions
{
    public static string GetDirectory(this string filePath)
    {
        return new FileInfo(filePath).Directory!.FullName;
    }
}
