using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;


namespace Database.DataReader
{
    public class RawJsonReader : IDataReader
    {
        public List<DataFrame> Read(string path)
        {
            Debug.Log($"Read {path}");
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    return ReadJSON(sr.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new List<DataFrame>();
            }
        }

        public List<DataFrame> ReadJSON(string json)
        {
            // 1. 시트별로 나누고
            // 2. 시트별로 데이터프레임 생성
            // 2-1. type별로 comment, varName, data를 찾음
            // 2-2. 데이터를 찾아서 데이터프레임에 추가
            
            List<DataFrame> dfs = new List<DataFrame>();
            JObject jObject = JObject.Parse(json);
            foreach (var jSheet in jObject)
            {
                // Debug.Log(jSheet.Key);
                dfs.Add(ReadSheet(jSheet.Key, jSheet.Value));
            }
            
            return dfs;
        }
        
        public DataFrame ReadSheet(string name, JToken jSheet)
        {
            DataFrame df = new DataFrame(name);
            if(jSheet["type"] == null)
                return df;
            JObject jType = (JObject) jSheet["type"];
            Dictionary<string,int> nameIdx = new Dictionary<string, int>();
            int idx = 0;
            List<string> varNames = new List<string>();
            List<string> types = new List<string>();
            foreach (var jVar in jType)
            {
                string varName = jVar.Key;
                if(string.IsNullOrWhiteSpace(varName))
                    continue;
                // Debug.Log(jVar.Key);
                nameIdx[varName] = idx++;
                varNames.Add(varName);
                types.Add(jVar.Value.ToString());
            }

            df.varNames = varNames.ToArray();
            df.types = types.ToArray();
            // for (int i = 0; i < df.types.Length; i++)
            // {
            //     Debug.Log($"{df.varNames[i]} : {df.types[i]}");
            // }
            var comments = new string[types.Count];
            if(jSheet["comment"] != null)
            {
                foreach (var jComment in jSheet["comment"] as JObject)
                {
                    var varName = jComment.Key;
                    if(string.IsNullOrWhiteSpace(varName))
                        continue;
                    if (nameIdx.ContainsKey(varName))
                        comments[nameIdx[varName]] = jComment.Value.ToString();
                }
            }
            df.comments = comments;
            List<string[]> data = new List<string[]>();
            if(jSheet["data"] == null)
            {
                df.data = Array.Empty<string[]>();
                return df;
            }
            
            foreach (JObject jDataSection in jSheet["data"])
            {
                string[] dataLine = new string[types.Count];
                foreach (var jData in jDataSection)
                {
                    var varName = jData.Key;
                    if (nameIdx.ContainsKey(varName))
                        dataLine[nameIdx[varName]] = jData.Value.ToString();
                }
                data.Add(dataLine);
            }
            df.data = data.ToArray();
            return df;
        }
    }
}