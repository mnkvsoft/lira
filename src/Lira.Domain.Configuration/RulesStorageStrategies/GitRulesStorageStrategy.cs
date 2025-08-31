using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using Lira.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Lira.Domain.Configuration.RulesStorageStrategies;

public class GitRulesStorageStrategy : IRulesStorageStrategy, IDisposable
{
    private readonly string _repositoryUrl;
    private readonly string _localPath;
    private readonly string _branchName;
    private readonly TimeSpan _pollingInterval;
    private readonly CredentialsHandler? _credentialsHandler;
    private CancellationTokenSource? _cts;
    private bool _wasInit;
    private readonly ILogger<GitRulesStorageStrategy> _logger;

    public GitRulesStorageStrategy(
        IConfiguration configuration,
        ILogger<GitRulesStorageStrategy> logger)
    {
        _repositoryUrl = configuration.GetValue<string>("GitRepository") ??
                         throw new ArgumentException("GitRepository cannot be null.");
        _branchName = configuration.GetValue<string>("GitBranch") ??
                      throw new ArgumentException("GitBranch cannot be null.");
        _pollingInterval = TimeSpan.FromSeconds(configuration.GetValue<int?>("GitPollingIntervalSeconds") ?? 60);

        var localPath = Paths.GetTempSubPath("git_repo");
        _localPath = localPath;

        _logger = logger;
        _credentialsHandler = null;

        var repositoryDirectory = configuration.GetValue<string>("GitRepositoryDirectory") ?? string.Empty;
        Path = System.IO.Path.Combine(localPath, repositoryDirectory);
    }

    public string Path { get; }
    public event Func<Task>? OnChanged;

    public void InitIfNeed()
    {
        if (_wasInit)
            return;

        ReCreateRepository(_localPath);

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        // Запускаем фоновую задачу для периодического обновления

        _ = UpdatePeriodically(_pollingInterval, token);
        _logger.LogInformation($"Repository was cloned and polling was started with interval: {_pollingInterval}");

        _wasInit = true;
    }

    private void CloneRepository(string localPath)
    {
        var cloneOptions = new CloneOptions
        {
            BranchName = _branchName,
            Checkout = true,
            FetchOptions = { CredentialsProvider = _credentialsHandler }
        };

        Repository.Clone(_repositoryUrl, localPath, cloneOptions);
    }

    private async Task UpdatePeriodically(TimeSpan interval, CancellationToken cancellationToken)
    {
        await Task.Yield();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(interval, cancellationToken);
                var result = await UpdateRepositoryAsync(cancellationToken);

                switch (result)
                {
                    case UpdateRepositoryResult.ChangesExists:
                        _logger.LogInformation($"Git repository was changed, changes is loaded. Commit: {GetCurrentCommitInfo()}");
                        await OnChanges();
                        break;
                    case UpdateRepositoryResult.PushForce:
                        {
                            _logger.LogInformation("Was force push, recreating repository...");
                            ReCreateRepository(_localPath);
                            await OnChanges();
                            break;
                        }
                    case UpdateRepositoryResult.NoChanges:
                        // nothing
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("Unknown result in cloning git repository: " + result);
                }

            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An exception occurred while trying to update the repository: " + ex.Message);
            }
        }
    }

    private async Task OnChanges()
    {
        if (OnChanged != null)
            await OnChanged.Invoke();
    }

    enum UpdateRepositoryResult
    {
        ChangesExists,
        PushForce,
        NoChanges
    }

    private Task<UpdateRepositoryResult> UpdateRepositoryAsync(CancellationToken cancellationToken)
    {
        return Task.Run(() =>
        {
            using var repo = new Repository(_localPath);

            var remote = repo.Network.Remotes["origin"];
            var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);

            var fetchOptions = new FetchOptions
            {
                CredentialsProvider = _credentialsHandler,
                Prune = true
            };

            Commands.Fetch(repo, remote.Name, refSpecs, fetchOptions, "Fetch updates");

            var branch = repo.Branches[$"origin/{_branchName}"];
            if (branch == null)
                throw new Exception($"Branch {_branchName} does not exist");

            var localBranch = repo.Branches[_branchName];
            if (localBranch == null)
            {
                localBranch = repo.CreateBranch(_branchName, branch.Tip);
                repo.Branches.Update(localBranch, b => b.TrackedBranch = branch.CanonicalName);
            }

            Commands.Checkout(repo, localBranch);

            var signature = new Signature("Auto Updater", "updater@example.com", DateTimeOffset.Now);
            var mergeResult = repo.Merge(branch.Tip, signature, new MergeOptions
            {
                FastForwardStrategy = FastForwardStrategy.FastForwardOnly
            });

            _logger.LogDebug("Git merge result: " + mergeResult.Status);

            if (mergeResult.Status == MergeStatus.FastForward)
                return UpdateRepositoryResult.ChangesExists;

            if (mergeResult.Status != MergeStatus.UpToDate)
                throw new Exception($"Git merge failed: {mergeResult.Status}");

            if (localBranch.Tip != branch.Tip)
                return UpdateRepositoryResult.PushForce;

            return UpdateRepositoryResult.NoChanges;
        }, cancellationToken);
    }

    private string GetCurrentCommitInfo()
    {
        using var repo = new Repository(_localPath);
        var localBranch = repo.Branches[_branchName];
        var tip = localBranch.Tip;
        return $"{tip.Sha} ({tip.MessageShort})";
    }

    private void ReCreateRepository(string localPath)
    {
        _logger.LogInformation($"Begin cloning git repository '{_repositoryUrl}' to: {_localPath}");
        if (Directory.Exists(localPath))
        {
            RemoveReadOnlyAttributeRecursive(localPath);
            Directory.Delete(localPath, recursive: true);
        }

        Directory.CreateDirectory(localPath);

        CloneRepository(localPath);
        _logger.LogInformation($"Repository was cloned. Commit: {GetCurrentCommitInfo()}");
    }

    static void RemoveReadOnlyAttributeRecursive(string directoryPath)
    {
        // Обрабатываем файлы в текущем каталоге
        foreach (string file in Directory.GetFiles(directoryPath))
        {
            FileAttributes attributes = File.GetAttributes(file);
            if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                File.SetAttributes(file, attributes & ~FileAttributes.ReadOnly);
        }

        // Рекурсивно обрабатываем подкаталоги
        foreach (string subDirectory in Directory.GetDirectories(directoryPath))
        {
            RemoveReadOnlyAttributeRecursive(subDirectory);
        }
    }

    public void Dispose() => _cts?.Dispose();
}