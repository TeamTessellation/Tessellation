using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Database.DataReader
{
    /// <summary>
    /// 
    /// </summary>
    public class GoogleSheetReader : IDataReader
    {
        private readonly RawJsonReader _rawJsonReader = new RawJsonReader();
        private string loadedJson = string.Empty;
        private List<DataFrame> _dataFrames = new List<DataFrame>();
        public List<DataFrame> DataFrames => _dataFrames;

        public GoogleSheetReader()
        {
        }
        
        public IEnumerator LoadDataFramesRoutine(string url, Action<bool> onComplete, float timeoutDuration = 10f)
        {
            Debug.Log("[GoogleSheetReader] LoadDataFramesRoutine 시작");
            yield return DownloadJsonFromUrlRoutine(url, success =>
            {
                if (success)
                {
                    _dataFrames = _rawJsonReader.ReadJSON(loadedJson);
                    Debug.Log($"[GoogleSheetReader] 데이터프레임 로드 완료: {_dataFrames.Count}개 시트");
                    onComplete?.Invoke(true);
                }
                else
                {
                    Debug.LogError($"[GoogleSheetReader] 데이터프레임 로드 실패 {url}");
                    onComplete?.Invoke(false);
                }
            }, timeoutDuration);
            yield return null;
        }

        /// <summary>
        /// Google Sheet에서 JSON을 다운로드 받아 저장합니다.
        /// PlayMode에서만 동작합니다.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="onComplete"></param>
        /// <param name="timeoutDuration"></param>
        /// <returns></returns>
        public IEnumerator DownloadJsonFromUrlRoutine(string url, Action<bool> onComplete, float timeoutDuration = 10f)
        {
            if(!Application.isPlaying)
            {
                Debug.LogWarning("[GoogleSheetReader] 에디터 모드에서는 DownloadJsonFromUrlRoutine 를 지원하지 않습니다.");
                onComplete?.Invoke(false);
                yield break;
            }
            loadedJson = string.Empty;
            using (var www = UnityWebRequest.Get(url))
            {
                www.timeout = Mathf.CeilToInt(timeoutDuration);
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[GoogleSheetReader] Google Sheet 다운로드 실패: {www.error}");
                    onComplete?.Invoke(false);
                }
                else
                {
                    loadedJson = www.downloadHandler.text;
                    Debug.Log($"[GoogleSheetReader] Google Sheet 다운로드 성공, 길이: {loadedJson.Length}");
                    onComplete?.Invoke(true);
                }
            }
            yield return null;
        }
        
        public List<DataFrame> Read(string link)
        {
            if (string.IsNullOrEmpty(link))
            {
                Debug.LogError("[GoogleSheetReader] path가 비었습니다.");
                return new List<DataFrame>();
            }


            try
            {
                // URL인지 여부
                bool isUrl = Uri.IsWellFormedUriString(link, UriKind.Absolute);
                string fullPath = link;

                if (isUrl)
                {
                    using (var wc = new WebClient())
                    {
                        wc.Encoding = Encoding.UTF8;
                        string json = wc.DownloadString(link);
                        return _rawJsonReader.ReadJSON(json);
                    }
                    return null;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GoogleSheetReader] Read 실패: {ex}");
                return new List<DataFrame>();
            }
        }
        
    }
}
