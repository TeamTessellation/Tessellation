using System;
using System.Collections.Generic;
using System.IO;
using Database.DataReader;
using UnityEditor;
using UnityEngine;

namespace Database
{
    
    public class AutomaticWatcher
    {
#if UNITY_EDITOR
        public string Path { get; private set; }
        public string Extension { get; }
        private string TempPrefix = null;

        private FileSystemWatcher _watcher;
        private IDataReader reader;
        private HashSet<string> _modifiedFiles = new HashSet<string>();
        
        public Action<DataFrame> CreateCsharp;

        public AutomaticWatcher(string path,IDataReader reader, string extension, string tempPrefix = null)
        {
            Path = path;
            Extension = extension;
            
            this.reader = reader;
            this.TempPrefix = tempPrefix;

            _watcher = CreateWatcher();
        }

        private FileSystemWatcher CreateWatcher()
        {
            var watcher = new FileSystemWatcher(Path, $"*{Extension}");
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            watcher.Changed += OnDataChanged;
            watcher.Created += OnDataCreated;
            watcher.Deleted += OnDataDeleted;
            watcher.EnableRaisingEvents = true;
            return watcher;
        }

        public void ChangeWatcherDir(string newPath)
        {
            Path = newPath;
            if (_watcher != null)
            {
                _watcher.Path = Path;
                Debug.Log($"Watcher directory changed to: {newPath}");
            }
            else
            {
                CreateWatcher();
            }
        }

        public void DisposeWatcher()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Dispose();
                _watcher = null;
            }
        }

        private void OnDataCreated(object sender, FileSystemEventArgs e)
        {
            if (TempPrefix != null && e.Name.StartsWith(TempPrefix)) return;
            Debug.Log($"File Created: {e.FullPath}");

            var readPath = e.FullPath;
            var dfs = reader.Read(readPath);
            foreach (var df in dfs)
            {
                CreateCsharp(df);
            }

            DelayedAssetRefresh();
        }

        private void OnDataChanged(object sender, FileSystemEventArgs e)
        {
            if (TempPrefix == null)
            {
                var dfs = reader.Read(e.FullPath);
                foreach (var df in dfs)
                {
                    CreateCsharp(df);
                }
                return;
            } 
            if (e.Name.StartsWith("~$")) return;
            Debug.Log($"File Changed: {e.FullPath}");
            _modifiedFiles.Add(e.FullPath);
        }

        private void OnDataDeleted(object sender, FileSystemEventArgs e)
        {
            if (e.Name.StartsWith("~$"))
            {
                OnTempFileRemoved(e.FullPath);
                return;
            }

            Debug.Log($"File Removed: {e.FullPath}");
        }

        private void OnTempFileRemoved(string path)
        {
            string realPath = path.Replace("~$", "");

            if (_modifiedFiles.Contains(realPath))
            {
                EditorApplication.delayCall += () =>
                {
                    if (!_modifiedFiles.Contains(realPath)) return;
                    Debug.Log($"Creating {System.IO.Path.GetFileNameWithoutExtension(realPath)}.cs");

                    _modifiedFiles.Remove(realPath);
                    var dfs = reader.Read(realPath);
                    foreach (var df in dfs)
                    {
                        CreateCsharp(df);
                    }

                    if (_modifiedFiles.Count == 0)
                        DelayedAssetRefresh();
                };
            }
        }



        private static void DelayedAssetRefresh()
        {
            EditorApplication.delayCall += () =>
            {
                Debug.Log("Refreshing AssetDatabase...");
                AssetDatabase.Refresh();
            };
        }
#endif
    }

}
