using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Database.DataReader;
using Newtonsoft.Json;
using UnityEngine.Networking;

#if UNITY_EDITOR
using Abilities;
using Database.Generated;
using Machamy.Utils;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
#endif

namespace Database
{
    /// <summary>
    /// 플레이타임에 로드하는 클래스
    /// </summary>
    public class DatabaseManager : MonoBehaviour
    {
       
        private static bool s_isInstanced = false;
        
        private static bool s_isInitialized = false;
        public static bool s_IsInstanced => s_isInstanced;
        public static bool s_IsInitialized => s_isInitialized;

        [SerializeField] private McDatabase mcDatabase = new McDatabase();
        [SerializeField] private bool isInitialized = false;
        [Header("Path Settings")]
        [SerializeField] private string googleSheetUrl = "URL_HERE"; // 구글 시트 URL
        [SerializeField] private string localJsonPath = "Database/";
        [SerializeField] private string streamingAssetPath = "Json/";
        [Header("Settings")]
        [SerializeField, Tooltip("인터넷/로컬 유무와 상관없이 streaming으로 작동")] private bool forceUseStreamingAsset = false;
        [SerializeField] private int timeoutSeconds = 10;

        // 스프라이트 캐시를 위한 Dictionary //
        /// <summary>
        /// 로드된 스프라이트를 관리하는 캐시. Key: 이미지 URL, Value: 로드된 Sprite
        /// </summary>


        public event Action OnInitialized;
        public string FinalLocalPath => Path.Combine(Application.persistentDataPath, localJsonPath);
        public string FinalStreamingAssetPath => Path.Combine(Application.streamingAssetsPath, streamingAssetPath);

        public McDatabase Database => mcDatabase;
        public bool IsInitialized => isInitialized;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

#if UNITY_EDITOR
            OnInitialized += ExportAbilitiesToSO;
#endif
        }

        private void Start()
        {
// #if UNITY_EDITOR
            if (isInitialized)
            {
                Debug.LogWarning("[DatabaseManager] 이미 초기화되었습니다.");
                return;
            }
            s_isInstanced = true;
            StartCoroutine(InitializeRoutine());
            // #endif
        }

        [ContextMenu("Clear Database")]
        public void Clear()
        {
            mcDatabase.ClearAll();
            isInitialized = false;
        }

        public void Initialize()
        {
            if (isInitialized)
            {
                // Debug.LogWarning("[DatabaseManager] 이미 초기화되었습니다.");
                return;
            }
            StartCoroutine(InitializeRoutine());
        }

        [ContextMenu("Force Initialize In Play Mode")]
        public void ForceInitialize()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[DatabaseManager] 에디터 모드에서는 이 기능을 사용할 수 없습니다.");
                return;
            }
            isInitialized = false;
            StartCoroutine(InitializeRoutine());
        }



#if UNITY_EDITOR
        [ContextMenu("Initialize In Edit Mode")]
        public void InitializeInEditMode()
        {
            if (Application.isPlaying)
            {
                Debug.LogWarning("[DatabaseManager] 플레이 모드에서는 이 기능을 사용할 수 없습니다.");
                return;
            }
            isInitialized = false;
            EditorCoroutineUtility.StartCoroutine(InitializeRoutine(), this);
        }
#endif

        private IEnumerator InitializeRoutine()
        {
            Debug.Log("[DatabaseManager] 초기화 시작");
            Debug.Log("[DatabaseManager] DB 데이터 삭제");
            mcDatabase.ClearAll();


            yield return LoadDatabase();
            // yield return new WaitForSeconds(1f); // 테스트용 딜레이



            Debug.Log("[DatabaseManager] 초기화 완료");
            isInitialized = s_isInitialized = true;
            OnInitialized?.Invoke();
        }

        private IEnumerator LoadDatabase()
        {
            if (forceUseStreamingAsset)
            {
                Debug.Log("[DatabaseManager] 강제 스트리밍 에셋 모드. 로컬 파일을 로드합니다.");
                yield return LoadLocalDf();
            }
            else if (String.IsNullOrEmpty(googleSheetUrl))
            {
                Debug.Log("[DatabaseManager] 구글 시트 URL이 비었습니다. 로컬 파일을 로드합니다.");
                yield return LoadLocalDf();
            }
            else if (IsInternetAvailable())
            {
                Debug.Log("[DatabaseManager] 인터넷 연결됨. 구글 시트를 로드합니다.");
                bool isSuccess = false;
                GoogleSheetReader reader = new GoogleSheetReader();
                yield return reader.LoadDataFramesRoutine(googleSheetUrl, success => isSuccess = success, timeoutSeconds);
                if (isSuccess)
                {
                    mcDatabase.InitializeAll(reader.DataFrames);
                    Debug.Log($"[DatabaseManager] 구글 시트 데이터 로드 성공. 항목 수: {reader.DataFrames.Count}");
                }
                else
                {
                    Debug.LogWarning("[DatabaseManager] 구글 시트 데이터 로드 실패. 로컬 파일을 로드합니다.");
                    yield return LoadLocalDf();
                }
            }
            else
            {
                Debug.Log("[DatabaseManager] 인터넷 연결안됨. 로컬 파일을 로드합니다.");
                yield return LoadLocalDf();
            }
        }
        
        private IEnumerator LoadLocalDf()
        {
            string json = null;
            List<string> targetClassNames = mcDatabase.ClassNames;
            List<DataFrame> dataFrames = new List<DataFrame>();
            foreach (var className in targetClassNames)
            {
                string fileName = $"{className}.df";
                string localPath = Path.Combine(FinalLocalPath, fileName);
                string streamingPath = Path.Combine(FinalStreamingAssetPath, fileName);
                Debug.Log($"[DatabaseManager] 로컬 파일 로드 시도: {localPath}");
                yield return LoadSavedFile(localPath, result => json = result);
                if (string.IsNullOrEmpty(json))
                {
                    Debug.Log($"[DatabaseManager] 스트리밍 에셋 로드 시도: {streamingPath}");
                    yield return LoadStreamingAsset(streamingPath, result => json = result);
                }
                if (!string.IsNullOrEmpty(json))
                {
                    DataFrame df = JsonConvert.DeserializeObject<DataFrame>(json);
                    if (df != null)
                    {
                        dataFrames.Add(df);
                    }
                    else
                    {
                        Debug.LogWarning($"[DatabaseManager] {className} 데이터 프레임 역직렬화 실패.");
                    }
                }
                else
                {
                    Debug.LogWarning($"[DatabaseManager] {className} 데이터 로드 실패. 파일이 존재하지 않음.");
                }

            }
            
            mcDatabase.InitializeAll(dataFrames);
            
        }

        private IEnumerator LoadLocalJson()
        {
            string json = null;
            List<string> targetClassNames = mcDatabase.ClassNames;
            foreach (var className in targetClassNames)
            {
                string fileName = $"{className}.json";
                string localPath = Path.Combine(FinalLocalPath, fileName);
                string streamingPath = Path.Combine(FinalStreamingAssetPath, fileName);
                if (forceUseStreamingAsset)
                {
                    Debug.Log($"[DatabaseManager] 강제 스트리밍 에셋 모드. 스트리밍 에셋 로드 시도: {streamingPath}");
                    yield return LoadStreamingAsset(streamingPath, result => json = result);
                }
                else
                {
                    Debug.Log($"[DatabaseManager] 로컬 파일 로드 시도: {localPath}");
                    yield return LoadSavedFile(localPath, result => json = result);
                    if (string.IsNullOrEmpty(json))
                    {
                        Debug.Log($"[DatabaseManager] 스트리밍 에셋 로드 시도: {streamingPath}");
                        yield return LoadStreamingAsset(streamingPath, result => json = result);
                    }
                }

                if (!string.IsNullOrEmpty(json))
                {
                    Type type = mcDatabase.GetTypeByName(className);
                    if (type == null)
                    {
                        Debug.LogWarning($"[DatabaseManager] {className}에 해당하는 타입을 찾을 수 없습니다.");
                        continue;
                    }
                    mcDatabase.AddInstancesFromJsonList(className, json);
                }
                else
                {
                    Debug.LogWarning($"[DatabaseManager] {className} 데이터 로드 실패. 파일이 존재하지 않음.");
                }
            }
            Debug.Log($"[DatabaseManager] 로컬 JSON 길이: {json?.Length ?? 0}");
            yield break;
        }
        private IEnumerator LoadSavedFile(string path, Action<string> onComplete)
        {
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                onComplete?.Invoke(json);
            }
            else
            {
                onComplete?.Invoke(null);
            }
            yield break;
        }

        private IEnumerator LoadStreamingAsset(string path, Action<string> onComplete)
        {
            if (Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.WebGLPlayer)
            {
                using UnityWebRequest www = UnityWebRequest.Get(path);
                yield return www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[DatabaseManager] 스트리밍 에셋 로드 실패: {www.error}");
                    onComplete?.Invoke(null);
                }
                else
                {
                    onComplete?.Invoke(www.downloadHandler.text);
                }
            }
            else
            {
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    onComplete?.Invoke(json);
                }
                else
                {
                    onComplete?.Invoke(null);
                }
            }

        }


        private bool IsInternetAvailable()
        {
            return Application.internetReachability != NetworkReachability.NotReachable;
        }

        private IEnumerator Timer(float time, Action onTimeout)
        {
            float elapsed = 0f;
            while (elapsed < time)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            onTimeout?.Invoke();
        }


   
        /// <summary>
        /// 일반 Google Drive 공유 링크를 직접 다운로드 가능한 URL로 변환합니다.
        /// </summary>
        /// <param name="anyUrl">검사 및 변환할 URL</param>
        /// <returns>변환된 URL. 변환이 필요 없거나 실패 시 원본 URL 반환.</returns>
        private string ConvertToGoogleDriveDownloadUrl(string anyUrl)
        {
            // URL이 유효한지, 그리고 우리가 변환하려는 공유 링크 형태인지 확인합니다.
            if (string.IsNullOrEmpty(anyUrl) || !anyUrl.Contains("/file/d/"))
            {
                return anyUrl; // 변환할 필요가 없는 URL(이미 uc?id 형식이거나 다른 URL)은 그대로 반환
            }

            try
            {
                // URL을 "/d/" 기준으로 잘라 파일 ID를 포함한 뒷부분을 얻습니다.
                string[] parts = anyUrl.Split(new string[] { "/d/" }, StringSplitOptions.None);
                // 뒷부분에서 다시 "/" 기준으로 잘라 순수한 파일 ID만 추출합니다.
                string fileId = parts[1].Split('/')[0];

                // 추출된 파일 ID를 직접 다운로드 URL 형식에 맞춰 조합합니다.
                return $"https://drive.google.com/uc?id={fileId}";
            }
            catch (Exception ex)
            {
                // URL 형식이 예상과 달라 오류가 발생할 경우를 대비합니다.
                Debug.LogWarning($"Google Drive URL 변환 실패: {anyUrl} | Error: {ex.Message}. 원본 URL을 사용합니다.");
                return anyUrl; // 실패 시 원본 URL 반환
            }
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// OnInitialized 이후 (데이터베이스 초기화 완료 후) 호출된다.
        /// 데이터를 가져다 AbilityDataSO를 만든다
        /// </summary>
        private void ExportAbilitiesToSO()
        {
            string abilitySOFolder = "Assets/Resources/Abilities/AbilityDataSO";

            int createdCount = 0;
            int updatedCount = 0;

            foreach (var itemData in mcDatabase.ItemDataList)
            {
                // 현재 SO가 있는지 판단
                string assetPath = Path.Combine(abilitySOFolder, $"{itemData.ItemID}.asset");
                AbilityDataSO dataSO = AssetDatabase.LoadAssetAtPath<AbilityDataSO>(assetPath);

                // 없다면 새로운 SO 생성
                if (dataSO == null)
                {
                    dataSO = ScriptableObject.CreateInstance<AbilityDataSO>();
                    AssetDatabase.CreateAsset(dataSO, assetPath);
                    createdCount++;
                }
                else
                {
                    updatedCount++;
                }
                
                UpdateAbilitySO(dataSO, itemData);
                EditorUtility.SetDirty(dataSO);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            foreach (var itemData in mcDatabase.ItemDataList)
            {
                string assetPath = Path.Combine(abilitySOFolder, $"{itemData.ItemID}.asset");
                AbilityDataSO dataSO = AssetDatabase.LoadAssetAtPath<AbilityDataSO>(assetPath);

                if (dataSO == null) continue;
                
                UpdateAbilitySORef(dataSO, itemData);
                EditorUtility.SetDirty(dataSO);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log($"[AbilityDataSO 생성 완료] 생성 : {createdCount}, 갱신 : {updatedCount}");
        }

        private void UpdateAbilitySO(AbilityDataSO dataSO, ItemData itemData)
        {
            // 기본 정보
            dataSO.ItemType = itemData.eItemType;
            dataSO.Rarity = itemData.Rarity;
            dataSO.ItemName = GetItemName(itemData.itemNameID);
            dataSO.ItemID = itemData.ItemID;
            dataSO.Description = GetItemDescription(itemData.DescriptionID, itemData.input);
            dataSO.ItemIcon = GetItemSprite(itemData.eItemType);
            dataSO.input = itemData.input;
            dataSO.CanAppearInShop = itemData.CanAppearInShop;
            dataSO.ItemPrice = itemData.ItemPrice;
            dataSO.CanAppearInShop = itemData.CanAppearInShop;
            dataSO.IsSynthesisItem = itemData.IsSynthesisItem;
            
            
            // conflictingItems와 isSynthesisItem은 AssetDatabase.SaveAssets 다 끝나고 해야할듯
        }

        private Sprite GetItemSprite(eItemType itemType)
        {
            string abilityIconFolder = "Assets/Resources/Abilities/AbilityIcons";
            string itemTypeName = itemType.ToString();

            string iconPath = Path.Combine(abilityIconFolder, $"{itemTypeName}.png");
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);

            if (sprite == null)
            {
                iconPath = Path.Combine(abilityIconFolder, "Default.png");
                sprite = AssetDatabase.LoadAssetAtPath<Sprite>(iconPath);
                
                LogEx.LogError($"{itemTypeName}에 대한 Sprite 필요!");
            }

            return sprite;
        }
        
        private string GetItemName(string itemNameID)
        {
            foreach (var stringData in mcDatabase.StringDataList)
            {
                if (itemNameID == stringData.ItemID)
                {
                    return stringData.kr;
                }
            }
            return "__NULL__";
        }

        private string GetItemDescription(string itemDescriptionID, List<float> input)
        {
            foreach (var stringData in mcDatabase.StringDataList)
            {
                if (itemDescriptionID == stringData.ItemID)
                {
                    string template = stringData.kr;
            
                    if (input != null)
                    {
                        for (int i = 0; i < input.Count; i++)
                        {
                            float value = input[i];
                            string formattedValue;
                    
                            // 정수면 소수점 없이, 실수면 소수점 포함
                            if (value == (int)value)
                                formattedValue = value.ToString("F0");
                            else
                                formattedValue = value.ToString("F2");
                    
                            template = template.Replace($"{{{i}}}", formattedValue);
                        }
                    }
            
                    return template;
                }
            }
    
            return "NULL";
        }
        private void UpdateAbilitySORef(AbilityDataSO dataSO, ItemData itemData)
        {
            string abilitySOFolder = "Assets/Resources/Abilities/AbilityDataSO";
            
            // Conflict 설정
            if (itemData.ConflictingItems != null && itemData.ConflictingItems.Count > 0)
            {
                dataSO.ConflictingItems = new AbilityDataSO[itemData.ConflictingItems.Count];
                for(int i = 0; i < itemData.ConflictingItems.Count; i++)
                {
                    string assetPath = Path.Combine(abilitySOFolder, $"{itemData.ConflictingItems[i]}.asset");  
                    AbilityDataSO conflictDataSO = AssetDatabase.LoadAssetAtPath<AbilityDataSO>(assetPath);
                    dataSO.ConflictingItems[i] = conflictDataSO;
                }
            }
            else dataSO.ConflictingItems = Array.Empty<AbilityDataSO>();
            
            // SynthesisRequirements 설정
            if (itemData.SynthesisRequirements != null && itemData.SynthesisRequirements.Count > 0)
            {
                Debug.Log(itemData.SynthesisRequirements[0]);
                dataSO.SynthesisRequirements = new AbilityDataSO[itemData.SynthesisRequirements.Count];
                for (int i = 0; i < itemData.SynthesisRequirements.Count; i++)
                {
                    Debug.Log(itemData.SynthesisRequirements[0]);
                    string assetPath = Path.Combine(abilitySOFolder, $"{itemData.SynthesisRequirements[i]}.asset");
                    AbilityDataSO synthesisDataSO = AssetDatabase.LoadAssetAtPath<AbilityDataSO>(assetPath);
                    dataSO.SynthesisRequirements[i] = synthesisDataSO;
                }
            }
            else dataSO.SynthesisRequirements = Array.Empty<AbilityDataSO>();
        }
#endif
    }
    
}