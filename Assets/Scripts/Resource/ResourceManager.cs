using System;
using System.Collections.Generic;
using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Resource
{
    public class ResourceManager : Singleton<ResourceManager>
    {
        public override bool IsDontDestroyOnLoad => true;

        
        private Dictionary<string, Object> _resourceCache = new Dictionary<string, Object>();
        private Dictionary<string, List<Object>> _resourceListCache = new Dictionary<string, List<Object>>();

        [SerializeField] private List<string> preloaLabels = new List<string>();
        
        protected override void AfterAwake()
        {
            base.AfterAwake();
            
        }
        
        public async UniTask PreloadResources()
        {
            foreach (var label in preloaLabels)
            {
                var op = Addressables.LoadAssetsAsync<Object>(label, null);
                await op.ToUniTask();
                List<Object> resources = op.Result as List<Object>;
                if (resources != null)
                {
                    foreach (var resource in resources)
                    {
                        string path = resource.name; // Assuming the resource name is unique and can be used as a key
                        _resourceCache.TryAdd(path, resource);
                    }
                    _resourceListCache[label] = resources;
                }else
                {
                    Debug.LogWarning($"No resources found with label: {label}");
                }
            }
        }


        private T LoadResource<T>(string path) where T : Object
        {
            if (_resourceCache.TryGetValue(path, out Object cachedResource))
            {
                return cachedResource as T;
            }

            var op = Addressables.LoadAssetAsync<T>(path);
            T resource = op.WaitForCompletion();
            if (resource != null)
            {
                _resourceCache[path] = resource;
            }
            else
            {
                Debug.LogWarning($"Resource not found at path: {path}");
            }
            return resource;
        }

        public T GetResource<T>(string path) where T : Object
        {
            return LoadResource<T>(path);
        }
        
        public List<T> GetResourcesInFolder<T>(string folderPath) where T : Object
        {
            if (_resourceListCache.TryGetValue(folderPath, out List<Object> cachedList))
            {
                return cachedList.ConvertAll(obj => obj as T);
            }

            var op = Addressables.LoadAssetsAsync<T>(folderPath, null);
            List<T> resources = op.WaitForCompletion() as List<T>;
            if (resources != null)
            {
                _resourceListCache[folderPath] = resources.ConvertAll(obj => obj as Object);
            }
            else
            {
                Debug.LogWarning($"No resources found in folder: {folderPath}");
            }
            return resources;
        }
        
        public List<T> GetAllResourcesByLabel<T>(string label) where T : Object
        {
            if (_resourceListCache.TryGetValue(label, out List<Object> cachedList))
            {
                return cachedList.ConvertAll(obj => obj as T);
            }
            var op = Addressables.LoadAssetsAsync<T>(label, null);
            op.WaitForCompletion();
            List<T> resources = op.Result as List<T>;
            if (resources == null)
            {
                Debug.LogWarning($"No resources found with label: {label}");
                return new List<T>();
            }
            _resourceListCache[label] = resources as List<Object>;
            return resources;
        }



    }
}