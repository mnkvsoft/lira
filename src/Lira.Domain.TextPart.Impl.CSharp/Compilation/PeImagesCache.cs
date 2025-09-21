using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Lira.Common;

namespace Lira.Domain.TextPart.Impl.CSharp.Compilation;

class PeImagesCache : IDisposable
{
    private static readonly string TempPath = Paths.GetTempSubPath("pe_images");

    private bool _wasInit;
    private readonly Dictionary<Hash, PeImageCacheEntry> _hashToEntryMap = new();
    private readonly ILogger _logger;
    private readonly State _state;

    public class State
    {
        public DateTime? LastClean { get; set; }
    }

    public PeImagesCache(ILoggerFactory loggerFactory, State state)
    {
        _state = state;
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

        _hashToEntryMap.Add(
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

        var files = Directory.GetFiles(TempPath);

        foreach (string filePath in files)
        {
            string strHash = Path.GetFileName(filePath);
            var hash = Hash.Parse(strHash);
            var peImage = new PeImage(hash, new PeBytes(File.ReadAllBytes(filePath)));
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
        try
        {
            var notUsed = new List<string>(_hashToEntryMap.Keys.Count);

            foreach (var pair in _hashToEntryMap)
            {
                var hash = pair.Key;
                var entry = pair.Value;

                string filePath = Path.Combine(TempPath, hash.ToString());

                if (entry.IsNew)
                {
                    IgnoreIoExceptionForTests(() => File.WriteAllBytes(filePath, entry.PeImage.Bytes.Value));
                    continue;
                }

                if (entry.WasUsed)
                    continue;

                notUsed.Add(filePath);
            }

            // cleaning

            if (_state.LastClean == null)
            {
                _state.LastClean = DateTime.UtcNow;
                return;
            }

            // todo: clean immediately
            if (DateTime.UtcNow - _state.LastClean.Value > TimeSpan.FromDays(1))
            {
                foreach (var notUsedFile in notUsed)
                {
                    File.Delete(notUsedFile);
                }

                _state.LastClean = DateTime.UtcNow;
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while disposing PE image cache");
        }
    }

    // todo: when running tests in parallel, similar errors occur, think about how to make it more graceful
    static void IgnoreIoExceptionForTests(Action action)
    {
        try
        {
            action();
        }
        catch (IOException e) when (e.Message.Contains("The process cannot access the file"))
        {
        }
        catch (UnauthorizedAccessException e) when (e.Message.Contains("Access to the path"))
        {
        }
    }
}