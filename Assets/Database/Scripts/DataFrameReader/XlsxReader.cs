using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using Database;
using Database.DataReader;
using UnityEngine;

public class XlsxReader : IDataReader
{
    private readonly string[] KEY_COMMENT = new []{"comment","주석"};
    private readonly string[] KEY_TYPE = new []{"type","타입"};
    private readonly string[] KEY_VARNAME = new []{"varName","variableName","헤더","변수명","변수","header"};
    private readonly string[] KEY_DATA = new []{"data","데이터","value","값"};

    public List<DataFrame> Read(string path)
    {
        Debug.Log($"Read {path}");
        if (!File.Exists(path))
        {
            Debug.LogError("파일이 존재하지 않습니다.");
            return null;
        }

        if (!path.EndsWith(".xlsx"))
        {
            Debug.LogError("파일 확장자가 .xlsx가 아닙니다.");
            return null;
        }


        try
        {
            using FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using ZipArchive zip = new ZipArchive(fileStream, ZipArchiveMode.Read);
            List<DataFrame> dfs = new();
            var sharedStrings = GetSharedStrings(zip);
            var sheetNames = GetSheetNames(zip);
                
            int id = 1;
            foreach (string sheetName in sheetNames)
            {
                if (sheetName.StartsWith("_"))
                    continue;
                var sheet = GetSheet(zip, sheetName, id, sharedStrings);
                if (sheet == null)
                {
                    Debug.LogError($"{sheetName} 시트를 읽는데 실패했습니다.");
                    return null;
                }

                dfs.Add(sheet);
                id += 1;
            }
            if (dfs.Count == 0)
            {
                Debug.LogError("시트가 존재하지 않습니다.");
                return null;
            }
            return dfs;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new List<DataFrame>();
        }
    }
    
    private Dictionary<string,string> GetSharedStrings(ZipArchive zip)
    {
        Dictionary<string,string> sharedStrings = new ();
        var entry = zip.GetEntry("xl/sharedStrings.xml");
        if (entry == null)
        {
            Debug.LogError("sharedStrings.xml 파일이 존재하지 않습니다.");
            return null;
        }
        XmlDocument sharedStringXml = new XmlDocument();
        sharedStringXml.Load(entry.Open());
        XmlNodeList sharedStringNodes = sharedStringXml.GetElementsByTagName("si");
        int cnt = 0;
        foreach (XmlNode node in sharedStringNodes)
        {
            string value = node.InnerText;
            sharedStrings.Add(cnt.ToString(), value);
            cnt += 1;
        }
        return sharedStrings;
    }
    
    private List<string> GetSheetNames(ZipArchive zip)
    {
        List<string> sheetNames = new ();
        var entry = zip.GetEntry("xl/workbook.xml");
        if (entry == null)
        {
            Debug.LogError("workbook.xml 파일이 존재하지 않습니다.");
            return null;
        }
        XmlDocument workbookXml = new XmlDocument();
        workbookXml.Load(entry.Open());
        XmlNodeList sheetNodes = workbookXml.GetElementsByTagName("sheet");
        foreach (XmlNode node in sheetNodes)
        {
            string sheetName = node.Attributes!["name"].Value;
            sheetNames.Add(sheetName);
        }
        return sheetNames;
    }
    
    private DataFrame GetSheet(ZipArchive zip, string sheetName,int sheetId, Dictionary<string,string> sharedStrings)
    {
        DataFrame df = new DataFrame(sheetName);
        var entry = zip.GetEntry($"xl/worksheets/sheet{sheetId}.xml");
        if (entry == null)
        {
            Debug.LogError($"sheet{sheetId}.xml 파일이 존재하지 않습니다.");
            return null;
        }
        XmlDocument sheetXml = new XmlDocument();
        sheetXml.Load(entry.Open());
        string[][] rawSheet = GetRawSheet(sheetXml, sharedStrings);
        
        // 각 줄마다 체크
        for (int i = 0; i < rawSheet.Length; i++)
        {
            string[] row = rawSheet[i];
            // Debug.Log(i+":"+row[1]);
            if(KEY_COMMENT.Any((e) => row[0].ToLowerInvariant() == e))
            {
                df.comments = row.Skip(1).ToArray();
            }else if(KEY_TYPE.Any((e) => row[0].ToLowerInvariant() == e))
            {
               df.types = row.Skip(1).ToArray();
            }else if (KEY_VARNAME.Any((e) => row[0].ToLowerInvariant() == e))
            {
                df.varNames = row.Skip(1).ToArray();
            }
            else if (KEY_DATA.Any((e) => row[0].ToLowerInvariant() == e))
            {
                // df.data = new string[rawSheet.Length - i, rawSheet[i].Length];
                // for (int j = 0; j < rawSheet.Length - i; j++)
                // {
                //     for (int k = 0; k < rawSheet[i].Length; k++)
                //     {
                //         df.data[j, k] = rawSheet[i + j][k];
                //     }
                // }
                // 위에서부터 i개, 왼쪽에서 1개 제외
                df.data = rawSheet.Skip(i).Select(e => e.Skip(1).ToArray()).ToArray();
                Debug.Log( rawSheet.Length+" "+df.data.Length);
                break;
            }
        }
        
        return df;
    }

    private static string[][] GetRawSheet(XmlDocument sheetXml, Dictionary<string,string> sharedStrings)
    {
        XmlNodeList rowNodes = sheetXml.GetElementsByTagName("row");
        string span = sheetXml.GetElementsByTagName("dimension")[0].Attributes!["ref"].Value;
        string cRaw;
        int maxRow;
        (cRaw,maxRow) = GetRowColumn(span.Split(':')[1]);
        var maxColumn = ColumnNameToIndex(cRaw);
        string[][] rawSheet = new string[maxRow][];
        for (int i = 0; i < maxRow; i++)
        {
            rawSheet[i] = new string[maxColumn+1];
        }
        
        for (int i = 0; i < rowNodes.Count; i++)
        {
            XmlNode rowNode = rowNodes[i];
            // 줄 단위 for문
            int row = int.Parse(rowNode.Attributes!["r"].Value) - 1;
            XmlNodeList cellNodes = rowNode.ChildNodes;
            for (int j = 0; j < cellNodes.Count; j++)
            {
                XmlNode cellNode = cellNodes[j];
                // 셀 단위 for문
                string cellId = cellNode.Attributes!["r"].Value;
                ReadOnlySpan<char> columnName = GetColumnName(cellId);
                int column = ColumnNameToIndex(columnName);
                string value = cellNode.InnerText;
                // Debug.Log($"[{cellId}]row: {row}, column: {column}, value: {value} {sharedStrings[value]}");
                if(cellNode.Attributes!["t"] != null && cellNode.Attributes!["t"].Value == "s")
                {
                    // 문자열은 xml 참조
                    value = sharedStrings[value];
                }
                // 나머지 type은 그대로 저장해도 무방함
                // inlineStr : 문자열
                // str : 문자열(함수 출력)
                // b : boolean
                // n : number
                // e : error
                // Debug.Log($"[{cellId}]row: {row}, column: {column}, value: {value}");
                rawSheet[row][column] = value;
            }
        }
        return rawSheet;
    }
    
    public static ReadOnlySpan<char> GetColumnName(ReadOnlySpan<char> cellId){
        int i = 0;
        while (i < cellId.Length && char.IsLetter(cellId[i]))
        {
            i += 1;
        }
        return cellId.Slice(0, i);
    }

    //TODO 문자열 조작 최적화
    public static Tuple<string,int> GetRowColumn(string cellId){
        StringBuilder sb = new StringBuilder();
        int i = 0;
        while (i< cellId.Length && char.IsLetter(cellId[i]))
        {
            sb.Append(cellId[i]);
            i += 1;
        }
        string columnName = sb.ToString();
        int row = int.Parse(cellId.Substring(i));
        return new Tuple<string, int>(columnName, row);
    }
    
    public static int ColumnNameToIndex(ReadOnlySpan<char> columnName)
    {
        int index = 0; ;
        foreach (char c in columnName)
        {
            index = index * 26 + (c - 'A' + 1);
        }
        return index - 1;
    }
}
