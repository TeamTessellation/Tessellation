using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace Database
{
    [CustomEditor(typeof(DBInitializerSO))]
    public class DBInitializerEditor: UnityEditor.Editor
    {
        #region code
 private const string gsCode = @"/** Apps Script **/
function doGet() {
  const jsonOutput = exportSheetToJson_XlsxStyle();
  return ContentService.createTextOutput(jsonOutput).setMimeType(ContentService.MimeType.JSON);
}

/** ----- 설정: XlsxReader와 같은 키 집합 ----- */
const KEY_COMMENT = [""comment"",""주석""];
const KEY_TYPE    = [""type"",""타입""];
const KEY_VARNAME = [""varname"",""variablename"",""헤더"",""변수명"",""변수"",""header""];
const KEY_DATA    = [""data"",""데이터"",""value"",""값""];

/** 메인 */
function exportSheetToJson_XlsxStyle() {
  const ss = SpreadsheetApp.getActiveSpreadsheet();
  const sheets = ss.getSheets();
  const result = {};

  sheets.forEach((sheet) => {
    const name = sheet.getName();
    if (name.startsWith(""_"") || name.startsWith(""NOEX_"")) return;

    const values = sheet.getDataRange().getValues();
    if (!values || values.length === 0) return;

    // 시트 전체에서 comment/type/varName/data 줄 탐색
    // 각 줄은 첫 셀에 키가 있고, 오른쪽으로 값/헤더가 이어짐
    let comments = null, types = null, headers = null, dataStartRow = -1;

    for (let r = 0; r < values.length; r++) {
      const row = values[r];
      const key = toKey(row[0]);
      if (!key) continue;

      if (isIn(key, KEY_COMMENT)) {
        comments = row.slice(1).map(safeToString);
      } else if (isIn(key, KEY_TYPE)) {
        types = row.slice(1).map(safeToString);
      } else if (isIn(key, KEY_VARNAME)) {
        headers = row.slice(1).map(safeToString);
      } else if (isIn(key, KEY_DATA)) {
        dataStartRow = r; // 이 줄 포함하여 아래로 데이터
        break;
      }
    }

    // 필수 체크
    if (!headers || !types || dataStartRow < 0) return;

    // 길이 맞추기(오른쪽 패딩)
    const maxCols = Math.max(headers.length, types.length, comments ? comments.length : 0);
    headers = padRight(headers, maxCols, """");
    types   = padRight(types,   maxCols, """");
    if (!comments) comments = new Array(maxCols).fill("""");
    else comments = padRight(comments, maxCols, """");

    // ----- 메타 구축(배열 열 묶기) -----
    const commentData = {};
    const typeData    = {};
    const seenArrayBase = {};

    for (let c = 0; c < headers.length; c++) {
      const h = headers[c];
      if (!h || h.startsWith(""NOEX_"")) continue;
      const t = (types[c] || """").trim();
      const com = comments[c] || """";

      const m = safeMatch(/^arr(\d+)_(.+)$/, h);
      if (m) {
        const baseName = ""arr_"" + m[2];
        if (!seenArrayBase[baseName]) {
          seenArrayBase[baseName] = true;
          commentData[baseName] = com;
          typeData[baseName] = normalizeArrayElementType(t); // 요소 타입 저장
        }
      } else {
        commentData[h] = com;
        typeData[h] = t || ""string"";
      }
    }

    // ----- 데이터 파싱 -----
    const dataRows = [];
    for (let r = dataStartRow; r < values.length; r++) {
      const row = values[r];
      if (!row || row.length === 0) continue;

      // 첫 셀은 ""data/값"" 키이므로 버리고 오른쪽부터 실제 열
      const cells = row.slice(1);
      const rowData = {};
      const buckets = {}; // baseName -> []

      for (let c = 0; c < headers.length; c++) {
        const header = headers[c];
        if (!header || header.startsWith(""NOEX_"")) continue;

        const rawValue = (c < cells.length) ? cells[c] : null;
        const arr = safeMatch(/^arr(\d+)_(.+)$/, header);

        if (arr) {
          const arrIndex = parseInt(arr[1], 10);
          const baseName = ""arr_"" + arr[2];
          if (!buckets[baseName]) buckets[baseName] = [];
          const elemType = typeData[baseName] || ""string"";
          buckets[baseName][arrIndex] = parseByType(elemType, rawValue);
        } else {
          const declared = typeData[header] || ""string"";
          if (isListType(declared)) {
            rowData[header] = parseListCell(declared, rawValue);
          } else {
            rowData[header] = parseByType(declared, rawValue);
          }
        }
      }

      // 배열 버킷 합치기
      Object.keys(buckets).forEach((base) => {
        rowData[base] = compactOrNullPad(buckets[base]);
      });

      dataRows.push(rowData);
    }

    result[name] = {
      comment: commentData,
      type: typeData,
      data: dataRows
    };
  });

  return JSON.stringify(result, null, 2);
}

/* ================== 유틸 ================== */

function toKey(v) {
  if (v == null) return """";
  return String(v).toLowerCase().trim();
}
function isIn(k, arr) { return arr.some(x => toKey(x) === k); }
function safeToString(v) { return (v == null) ? """" : String(v); }
function padRight(a, n, fill) { const out = a.slice(); while (out.length < n) out.push(fill); return out; }

function safeMatch(re, str) {
  if (typeof str !== ""string"") return null;
  const m = re.exec(str);
  return (m && m.length >= 1) ? m : null;
}

function isListType(typeStr) {
  if (typeof typeStr !== ""string"") return false;
  return /^List<\s*[^>]+\s*>$/i.test(typeStr.trim());
}
function getListElementType(typeStr) {
  if (!isListType(typeStr)) return null;
  const m = typeStr.match(/^List<\s*([^>]+)\s*>$/i);
  return m ? m[1].trim() : null;
}
function normalizeArrayElementType(typeStr) {
  if (typeof typeStr !== ""string"") return ""string"";
  const t = typeStr.trim();
  if (isListType(t)) {
    const elem = getListElementType(t);
    return elem || ""string"";
  }
  return t || ""string"";
}

function parseListCell(listType, rawValue) {
  const elemType = getListElementType(listType) || ""string"";
  if (rawValue == null || rawValue === """") return [];
  const str = (typeof rawValue === ""string"") ? rawValue : String(rawValue);
  const parts = str.split("","").map(s => s.trim());
  return parts.map(p => parseByType(elemType, p));
}

function parseByType(typeStr, rawValue) {
  const t = (typeStr || ""string"").toLowerCase();
  if (rawValue === """" || rawValue == null) return null;

  if (t === ""int"" || t === ""integer"") {
    if (typeof rawValue === ""number"") return Math.trunc(rawValue);
    const iv = parseInt(String(rawValue).trim(), 10);
    return isNaN(iv) ? null : iv;
  }
  if (t === ""float"" || t === ""double"" || t === ""number"") {
    if (typeof rawValue === ""number"") return rawValue;
    const fv = parseFloat(String(rawValue).trim());
    return isNaN(fv) ? null : fv;
  }
  if (t === ""bool"" || t === ""boolean"") {
    if (typeof rawValue === ""boolean"") return rawValue;
    const s = String(rawValue).trim().toLowerCase();
    if ([""true"",""1"",""y"",""yes""].includes(s)) return true;
    if ([""false"",""0"",""n"",""no""].includes(s)) return false;
    return null;
  }
  if (t === ""json"") {
    try {
      if (typeof rawValue === ""object"") return rawValue;
      return JSON.parse(String(rawValue));
    } catch (e) { return null; }
  }
  // default string
  return (typeof rawValue === ""string"") ? rawValue : String(rawValue);
}

function compactOrNullPad(arr) {
  let max = -1;
  for (let i = 0; i < arr.length; i++) if (arr[i] !== undefined) max = i;
  if (max < 0) return [];
  const out = new Array(max + 1);
  for (let j = 0; j <= max; j++) out[j] = (arr[j] === undefined ? null : arr[j]);
  return out;
}


";


        #endregion

        public override void OnInspectorGUI()
        {
            // 기본 인스펙터 필드를 그립니다.
            DrawDefaultInspector();

            DBInitializerSO target = (DBInitializerSO)this.target;

            EditorGUILayout.Space(10); // 보기 좋게 간격 추가

            // 인스펙터에 Google Sheet 관련 버튼들을 그립니다.
            if (GUILayout.Button("Download Google Sheet JSON"))
            {
                if (string.IsNullOrEmpty(target.googleSheetUrl))
                {
                    EditorUtility.DisplayDialog("Error", "Google Sheet URL is not set.", "OK");
                }
                else
                {
                    try
                    {
                        // DBInitializerSO에 DownloadGoogleSheet 메소드가 public 이어야 합니다.
                        // 만약 public이 아니라면 아래 target.GetType() 코드를 사용하세요.
                        var method = target.GetType().GetMethod("DownloadGoogleSheet", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        method?.Invoke(target, null);
                    }
                    catch (Exception ex)
                    {
                        EditorUtility.DisplayDialog("Error", $"Failed to download Google Sheet JSON: {ex.Message}", "OK");
                    }
                }
            }

            if (GUILayout.Button("Get .gs code"))
            {
                EditorGUIUtility.systemCopyBuffer = gsCode;
                EditorUtility.DisplayDialog("Copy to Clipboard", "Google Apps Script code copied to clipboard", "OK");
            }


        }

    }
}
