using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Lira.Common;

namespace Lira.Domain.TextPart.Impl.CSharp.Compilation;

class PeImagesCache : IDisposable
{
    private static readonly string TempPath = Paths.GetTempSubPath("pe_images");
    private static readonly string ImageExtension = "image";
    private static readonly string AdditionInfoExtension = "ai";

    private bool _wasInit;

    // use concurrent version only for tests
    private readonly ConcurrentDictionary<Hash, PeImageCacheEntry> _hashToEntryMap = new();
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

    public void Add(PeImage peImage)
    {
        if (_hashToEntryMap.ContainsKey(peImage.Hash))
            return;

        _hashToEntryMap.TryAdd(
            peImage.Hash,
            new PeImageCacheEntry(peImage) { IsNew = true });
    }

    private void InitCacheIfNeed()
    {
        if (_wasInit)
            return;

        var sw = Stopwatch.StartNew();

        if (!Directory.Exists(TempPath))
            Directory.CreateDirectory(TempPath);

        var imageFiles = Directory.GetFiles(TempPath, $"*.{ImageExtension}");

        foreach (string imageFilePath in imageFiles)
        {
            string fileName = Path.GetFileName(imageFilePath);
            var strHash = Path.GetFileNameWithoutExtension(fileName);

            var typeFileName = strHash + "." + AdditionInfoExtension;

            string? type = null;
            if (File.Exists(Path.Combine(TempPath, typeFileName)))
                type = File.ReadAllText(Path.Combine(TempPath, typeFileName));

            var hash = Hash.Parse(strHash);
            var peImage = new PeImage(hash, new PeBytes(File.ReadAllBytes(imageFilePath)), type);
            _hashToEntryMap.TryAdd(hash, new PeImageCacheEntry(peImage));
        }

        _wasInit = true;
        var nl = Constants.NewLine;
        _logger.LogDebug("PE image cache." + nl +
                         $"Directory: {TempPath}" + nl +
                         $"Count: {imageFiles.Length}" + nl +
                         $"Duration: {(int)sw.ElapsedMilliseconds} ms");
    }

    public void Dispose()
    {
        try
        {
            foreach (var pair in _hashToEntryMap)
            {
                var hash = pair.Key;
                var entry = pair.Value;

                var imageFilePath = Path.Combine(TempPath, hash + "." + ImageExtension);
                var additionInfoFilePath = Path.Combine(TempPath, hash + "." + AdditionInfoExtension);

                if (entry.IsNew)
                {
                    File.WriteAllBytes(imageFilePath, entry.PeImage.Bytes.Value);

                    if(entry.PeImage.AdditionInfo != null)
                        File.WriteAllText(additionInfoFilePath, entry.PeImage.AdditionInfo);
                }
                else if (!entry.WasUsed)
                {
                    File.Delete(imageFilePath);

                    if (File.Exists(additionInfoFilePath))
                        File.Delete(additionInfoFilePath);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while disposing PE image cache");
        }
    }
}