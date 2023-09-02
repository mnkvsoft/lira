using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using SimpleMockServer.Common;

namespace SimpleMockServer.Domain.TextPart.System.CSharp.Compilation;

class PeImagesCache : IDisposable
{
    private static readonly string TempPath = Path.Combine(Path.GetTempPath(), "SimpleMockServer");
    
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
        var nl = Environment.NewLine;
        _logger.LogInformation("PE image cache." + nl +
            $"Directory: {TempPath}" + nl +
            $"Count: {files.Length}" + nl +
            $"Duration: {(int)sw.ElapsedMilliseconds} ms");
    }

    public void Dispose()
    {
        // logging
        
        foreach (var pair in _hashToEntryMap)
        {
            var hash = pair.Key;
            var entry = pair.Value;

            string filePath = Path.Combine(TempPath, hash.ToString());
            
            if (entry.IsNew)
            {
                File.WriteAllBytes(filePath, entry.PeImage.Bytes);
                continue;
            }

            if (entry.WasUsed)
                continue;
            
            File.Delete(filePath);
        }
    }
}
