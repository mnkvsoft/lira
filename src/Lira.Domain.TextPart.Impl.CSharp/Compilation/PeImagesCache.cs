using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Lira.Common;

namespace Lira.Domain.TextPart.Impl.CSharp.Compilation;

class PeImagesCache : IDisposable
{
    private static readonly string TempPath = Path.Combine(Path.GetTempPath(), "Lira");
    
    private bool _wasInit;
    private readonly Dictionary<Hash, PeImageCacheEntry> _hashToEntryMap = new();
    private readonly ILogger _logger;

    public PeImagesCache(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(GetType());
    }

    record PeImageCacheEntry(PeImage PeImage)
    {
        public bool WasUsed;
        public bool IsNew;
    }

    public bool TryGet(Hash hash, [MaybeNullWhen(false)] out PeImage peImage)
    {
        InitCacheIfNeed();
        if (_hashToEntryMap.TryGetValue(hash, out var cacheEntry))
        {
            cacheEntry.WasUsed = true;
            peImage = cacheEntry.PeImage;
            return true;
        }

        peImage = null;
        return false;
    }

    public void Add(Hash hash, PeImage peImage)
    {
        if(_hashToEntryMap.ContainsKey(hash))
            return;
        
        _hashToEntryMap.Add(
            hash,
            new PeImageCacheEntry(peImage) { IsNew = true });
    }

    private void InitCacheIfNeed()
    {
        if (_wasInit)
            return;

        var sw = Stopwatch.StartNew();
        
        if (!Directory.Exists(TempPath)) 
            Directory.CreateDirectory(TempPath);
        
        var files = Directory.GetFiles(TempPath);

        foreach (string filePath in files)
        {
            string strHash = filePath.GetFileName();
            var hash = Hash.Parse(strHash);
            var peImage = new PeImage(File.ReadAllBytes(filePath));
            _hashToEntryMap.Add(hash, new PeImageCacheEntry(peImage));    
        }
        
        _wasInit = true;
        var nl = Constants.NewLine;
        _logger.LogDebug("PE image cache." + nl +
            $"Directory: {TempPath}" + nl +
            $"Count: {files.Length}" + nl +
            $"Duration: {(int)sw.ElapsedMilliseconds} ms");
    }

    public void Dispose()
    {
        foreach (var pair in _hashToEntryMap)
        {
            var hash = pair.Key;
            var entry = pair.Value;

            string filePath = Path.Combine(TempPath, hash.ToString());
            
            if (entry.IsNew)
            {
                IgnoreIoExceptionForTests(() => File.WriteAllBytes(filePath, entry.PeImage.Bytes));
                continue;
            }

            if (entry.WasUsed)
                continue;
            
            IgnoreIoExceptionForTests(() => File.Delete(filePath));
        }
    }

    // todo: when running tests in parallel, similar errors occur, think about how to make it more graceful
    static void IgnoreIoExceptionForTests(Action action)
    {
        try
        {
            action();
        }
        catch (IOException e) when(e.Message.Contains("The process cannot access the file"))
        {
        }
        catch (UnauthorizedAccessException e) when(e.Message.Contains("Access to the path"))
        {
        }
    }
}
