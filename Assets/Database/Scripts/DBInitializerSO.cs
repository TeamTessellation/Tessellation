using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Database.DataReader;
using Database.Generated;
using UnityEditor;
using UnityEngine;

namespace Database.Generated
{
    
}
namespace Database
{
    [CreateAssetMenu(fileName = "DBInitializer", menuName = "Database/DBInitializer")]
    public class DBInitializerSO : ScriptableObject
    {
        
        
#if UNITY_EDITOR
        private string ThisDirPath => Path.GetDirectoryName(AssetDatabase.GetAssetPath(this));

        [Header("Path")] 
        public string dataDirPath = "./Raw";
        public string targetClassPath = "./Generated";
        public string targetPath = "./Json";

        [Header("Final Path (read only)")] 
        public string thisDirPath;
        public string dataDirFullPath;
        public string targetClassFullPath;
        public string targetFullPath;

        private string prevDataDirPath;

        public string DataDirPath => !dataDirPath.StartsWith("./") ? dataDirPath : Path.Combine(ThisDirPath, dataDirPath.Substring(2));
        public string TargetClassPath => !targetClassPath.StartsWith("./") ? targetClassPath : Path.Combine(ThisDirPath, targetClassPath.Substring(2));
        public string TargetDirPath => !targetPath.StartsWith("./") ? targetPath : Path.Combine(Application.streamingAssetsPath, targetPath.Substring(2));
        
        [Space]
        [Header("Config")]
        public bool doSaveCsharp = true;
        public List<string> dbFindTarget = new List<string>() {"name","identifier"};
        [Header("Xlsx Config")] 
        public string xlsxExtension = ".xlsx";
        public bool autoLoadXlsx = false;

        [Header("JSON Config")] 
        public string jsonExtension = ".json";
        public bool autoLoadJson = false;

        [Header("Google Sheet Config")] 
        public string googleSheetUrl;

        private AutomaticWatcher _xlsxWatcher;
        private AutomaticWatcher _jsonWatcher;

        private void OnEnable()
        {
            
            if (autoLoadXlsx)
            {
                _xlsxWatcher = new AutomaticWatcher(DataDirPath, new XlsxReader(), xlsxExtension, "~$");
                _xlsxWatcher.CreateCsharp += WriteClassDefinition;
            }
            if (autoLoadJson)
            {
                _jsonWatcher = new AutomaticWatcher(DataDirPath, new RawJsonReader(), jsonExtension);
                _jsonWatcher.CreateCsharp += WriteClassDefinition;
            }
        }

        private void OnValidate()
        {
            thisDirPath = ThisDirPath;
            dataDirFullPath = DataDirPath;
            targetClassFullPath = TargetClassPath;

            if (autoLoadXlsx)
                _xlsxWatcher?.ChangeWatcherDir(DataDirPath);
            else 
                _xlsxWatcher?.DisposeWatcher();

            if (autoLoadJson)
                _jsonWatcher?.ChangeWatcherDir(DataDirPath);
            else
                _jsonWatcher?.DisposeWatcher();
        }

        private void OnDisable()
        {
            EditorApplication.update -= EditorUpdate;
            _xlsxWatcher?.DisposeWatcher();
            _jsonWatcher?.DisposeWatcher();
        }

        HashSet<string> onProcessing = new HashSet<string>();
        private void EditorUpdate()
        {
            
            while(_modifiedFiles.Count > 0)
            {
                var enumerator = _modifiedFiles.GetEnumerator();
                if (!enumerator.MoveNext())
                    return;

                var file = enumerator.Current; 
                var df = file.Value;
                Debug.Log($"File Changed: {df}");
                _modifiedFiles.Remove(df.name);
                onProcessing.Add(df.name);
                EditorApplication.delayCall += () =>
                {
                    if(!onProcessing.Contains(df.name))
                        return;
                    Debug.Log($"Creating {df.name}.json");
                    _modifiedFiles.Remove(df.name);
                    WriteJson(df);
                    if(_modifiedFiles.Count == 0)
                        AssetDatabase.Refresh();
                };
            }
        }
        public void LoadAll(string extension)
        {
            var files = Directory.GetFiles(DataDirPath, $"*{extension}");
            if (files.Length == 0)
            {
                Debug.LogWarning($"No files with extension {extension} found in {DataDirPath}");
                return;
            }
            List<string> classNames = new List<string>();
            foreach (var file in files)
            {
                if (extension == xlsxExtension)
                {
                    var dfs = new XlsxReader().Read(file);
                    foreach (var df in dfs)
                    {
                        WriteClassDefinition(df);
                        classNames.Add(df.name);
                    }
                }
                else if (extension == jsonExtension)
                {
                    var dfs = new RawJsonReader().Read(file);
                    foreach (var df in dfs)
                    {
                        WriteClassDefinition(df);
                        classNames.Add(df.name);
                    }
                }
            }
            WriteDatabaseClass(classNames);
            
            AssetDatabase.Refresh();
        }
        
        public void DownloadGoogleSheet()
        {
            autoLoadJson = false;
            
            var dfs = new GoogleSheetReader().Read(googleSheetUrl);
            List<string> classNames = new List<string>();
            foreach (var df in dfs)
            {
                WriteClassDefinition(df);
                classNames.Add(df.name);
            }
            WriteDatabaseClass(classNames);
            AssetDatabase.Refresh();
        }
        
        Dictionary<string, DataFrame> _modifiedFiles = new ();
        private void WriteClassDefinition(DataFrame df)
        {
            EditorApplication.update -= EditorUpdate;
            EditorApplication.update += EditorUpdate;
            _modifiedFiles[df.name] = df;
            
            if (!doSaveCsharp)
                return;
            string cs = ClassDefinitionFactory.GenerateClassDefinition(df);
            string fileName = Path.Combine(TargetClassPath, df.name + ".cs");
            File.WriteAllText(fileName, cs);
            Debug.Log($"Class Definition Generated: {fileName}");
        }

        private void WriteDatabaseClass(List<string> classNames)
        {
            if (!doSaveCsharp)
                return;
            string cs = DatabaseClassDefinitionFactory.GenerateDatabaseClass("McDatabase", classNames, dbFindTarget);
            string fileName = Path.Combine(TargetClassPath, "McDatabase.cs");
            File.WriteAllText(fileName, cs);
            Debug.Log($"Database Class Generated: {fileName}");
        }
        
        private void WriteJson(DataFrame df)
        {
            if(!Directory.Exists(TargetDirPath))
                Directory.CreateDirectory(TargetDirPath);
            string fileName = Path.Combine(TargetDirPath, df.name + ".json");
            object[] obj = ClassInstanceFactory.CreateInstance(df) ?? throw new Exception($"Failed to create instance for {df.name}");

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(obj, Newtonsoft.Json.Formatting.Indented);

            File.WriteAllText(fileName, json);
            Debug.Log($"Json : {json}");
            Debug.Log($"Json Generated: {fileName}");
        }


#endif
    }
}
