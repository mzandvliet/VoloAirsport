using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using RamjetAnvil.Unity.Utility;
using UnityEngine;

namespace RamjetAnvil.Reactive {

    public static class FileWatching {

        public struct WatcherSettings {
            public string Directory;
            public string SearchPattern;
            public NotifyFilters NotifyFilters;
            public SearchOption SearchOption;

            public static WatcherSettings Create(string directory, 
                string searchPattern = "*.*", 
                SearchOption searchOption = SearchOption.TopDirectoryOnly,
                NotifyFilters notifyFilters = NotifyFilters.LastAccess | NotifyFilters.LastWrite |
                                              NotifyFilters.FileName | NotifyFilters.DirectoryName) {

                return new WatcherSettings {
                    Directory = directory,
                    SearchPattern = searchPattern,
                    NotifyFilters = notifyFilters,
                    SearchOption = searchOption
                };
            }
        }

        public static IObservable<FileSystemEventArgs> FileChanges(WatcherSettings watcherSettings) {
            return Observable.Create<FileSystemEventArgs>(observer => {
                var disp = new CompositeDisposable();

                var watcher = new FileSystemWatcher(watcherSettings.Directory) {
                    NotifyFilter = watcherSettings.NotifyFilters,
                    Filter = watcherSettings.SearchPattern,
                    IncludeSubdirectories = watcherSettings.SearchOption == SearchOption.AllDirectories
                };
                disp.Add(watcher);

                disp.Add(Observable.Merge(
                    Observable.FromEventPattern<RenamedEventHandler, FileSystemEventArgs>(x => watcher.Renamed += x,
                        x => watcher.Renamed -= x),
                    Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(x => watcher.Changed += x,
                        x => watcher.Changed -= x),
                    Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(x => watcher.Created += x,
                        x => watcher.Created -= x),
                    Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(x => watcher.Deleted += x,
                        x => watcher.Deleted -= x))
                    .Select(x => x.EventArgs)
                    .Synchronize(observer)
                    .Subscribe(observer));

                watcher.EnableRaisingEvents = true;

                return disp;
            }).Publish().RefCount();
        }

        public static IEnumerable<FileReference> GetCurrentFiles(WatcherSettings watcherSettings) {
            return Directory
                .GetFiles(watcherSettings.Directory, watcherSettings.SearchPattern, watcherSettings.SearchOption)
                .Select(fullPath => new FileReference {FullPath = fullPath});
        } 
        
        public static IObservable<IList<FileReference>> TrackDirectory(WatcherSettings watcherSettings) {
            return Observable.Create<IList<FileReference>>(observer => {
                var initialFiles = GetCurrentFiles(watcherSettings).ToImmutableList();

                var disposable = FileChanges(watcherSettings)
                    .Scan(initialFiles, (fileList, fileEvent) => {
                        var file = new FileReference {FullPath = fileEvent.FullPath};
                        if (fileEvent.ChangeType == WatcherChangeTypes.Created) {
                            fileList = fileList.Add(file);
                        } else if (fileEvent.ChangeType == WatcherChangeTypes.Renamed) {
                            var renameEvent = (RenamedEventArgs) fileEvent;
                            var oldFile = new FileReference {FullPath = renameEvent.OldFullPath};
                            fileList = fileList.Remove(oldFile);
                            fileList = fileList.Add(file);
                        } else if (fileEvent.ChangeType == WatcherChangeTypes.Deleted) {
                            fileList = fileList.Remove(file);
                        }

                        fileList = fileList.Sort();

                        return fileList;
                    })
                    .Select(fileList => fileList as IList<FileReference>)
                    .Synchronize(observer)
                    .Subscribe(observer);

                observer.OnNext(initialFiles);

                return disposable;
            }).Replay(1).RefCount();
        }
    }

    public struct FileReference : IComparable<FileReference>, IEquatable<FileReference> {

        public string FullPath;

        public string Name {
            get { return Path.GetFileNameWithoutExtension(FullPath); }
        }

        public int CompareTo(FileReference other) {
            return String.CompareOrdinal(Name, other.Name);
        }

        public bool Equals(FileReference other) {
            return string.Equals(FullPath, other.FullPath);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is FileReference && Equals((FileReference) obj);
        }

        public override int GetHashCode() {
            return (FullPath != null ? FullPath.GetHashCode() : 0);
        }

        public override string ToString() {
            return FullPath;
        }

    }
}
