using Avalonia.Controls;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Metrino.Development.UI.ViewModels;

public partial class RepositoryManager : ObservableObject, IDisposable
{
    [ObservableProperty]
    private Branch _currentHead;

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private int _added;

    [ObservableProperty]
    private int _modified;

    [ObservableProperty]
    private int _removed;


    private string _repositoryPath;
    Repository _repository;

    RepositoryStatus _repositoryStatus;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LocalBranches))]
    IEnumerable<Branch> _branches;

    public IEnumerable<Branch> LocalBranches => Branches.Where(x => !x.IsRemote);

    Thread _currentHeadMonitor;
    bool _currentHeadMonitorStop;

    public RepositoryManager(string repositoryPath)
    {
        _repositoryPath = repositoryPath;
        _repository = new Repository(_repositoryPath);
        CurrentHead = _repository.Head;
        _repositoryStatus = _repository.RetrieveStatus();
        IsDirty = _repositoryStatus.IsDirty;
        Branches = _repository.Branches.Select(x => x);

        _currentHeadMonitor = new Thread(() =>
        {
            while (!_currentHeadMonitorStop)
            {
                UpdateRepositoryStatus();

                Thread.Sleep(500);
            }
        });

        _currentHeadMonitorStop = false;
        _currentHeadMonitor.Start();
    }

    private void UpdateRepositoryStatus()
    {
        _repositoryStatus = _repository.RetrieveStatus(new StatusOptions { IncludeIgnored = false, IncludeUntracked = false });
        SetProperty(ref _added, _repositoryStatus.Added.Count(), nameof(Added));
        SetProperty(ref _modified, _repositoryStatus.Modified.Count(), nameof(Modified));
        SetProperty(ref _removed, _repositoryStatus.Removed.Count(), nameof(Removed));
        SetProperty(ref _isDirty, _repositoryStatus.IsDirty, nameof(IsDirty));
        SetProperty(ref _currentHead, _repository.Head, nameof(CurrentHead));
    }

    public void SetCurrentBranch(Branch newHead)
    {
        if (newHead == _repository.Head)
            return;

        if (!newHead.IsTracking)
        {
            var remoteNameLength = newHead.RemoteName.Length + 1;
            var localBranchName = newHead.FriendlyName.Substring(remoteNameLength);

            if (_repository.Branches[localBranchName] == null)
            {
                var localBranch = _repository.CreateBranch(localBranchName, newHead.Tip);
                Branches = _repository.Branches.Select(x => x);
                newHead = _repository.Branches.Update(localBranch, b => b.TrackedBranch = newHead.CanonicalName);
            }
        }

        Commands.Checkout(_repository, newHead);
        CurrentHead = newHead;
    }

    public void Dispose()
    {
        _currentHeadMonitorStop = true;
        _currentHeadMonitor?.Join();
    }
}
