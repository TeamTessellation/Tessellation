using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Collections
{
    /// <summary>
    /// Unity에서 직렬화 가능한 딕셔너리입니다.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    [System.Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [System.Serializable]
        private class SerializableKeyValuePair
        {
            public TKey Key;
            public TValue Value;

            public SerializableKeyValuePair(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
        }
        
        [SerializeField] private TKey defaultKey = default;
        [SerializeField] private List<SerializableKeyValuePair> _items = new List<SerializableKeyValuePair>();
        
        
        
        public void OnBeforeSerialize()
        {
            _items.Clear();
            foreach (var kvp in this)
            {
                _items.Add(new SerializableKeyValuePair(kvp.Key, kvp.Value));
            }

        }
        
        public void OnAfterDeserialize()
        {
            this.Clear();
            foreach (var item in _items)
            {
                this[item.Key] = item.Value;
            }
        }
    }
    
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>), true)]
    public class SerializableDictionaryPropertyDrawer : PropertyDrawer
    {
        // 선택 상태를 인스펙터 간에 안정적으로 유지하기 위해 propertyPath별로 보관
        private static readonly Dictionary<string, int> SelectedIndexPerPath = new();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var items = property.FindPropertyRelative("_items");
            var defaultKey = property.FindPropertyRelative("defaultKey");

            if (!SelectedIndexPerPath.ContainsKey(property.propertyPath))
                SelectedIndexPerPath[property.propertyPath] = -1;

            int selectedIndex = SelectedIndexPerPath[property.propertyPath];

            // 헤더(토글 + 제목 + 개수 + Add/Remove)
            Rect headerRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            property.isExpanded = DrawHeader(headerRect, property, label, items, defaultKey, ref selectedIndex);

            // 본문
            float y = headerRect.yMax + 3f;
            if (property.isExpanded)
            {
                // Default Key 편집 박스
                var dkRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
                DrawDefaultKey(dkRect, defaultKey);
                y += dkRect.height + 4f;

                // 리스트(박스 스타일)
                Rect listRect = new Rect(position.x, y, position.width, GetListHeight(items));
                DrawList(listRect, items, ref selectedIndex);
                y = listRect.yMax;
            }

            // 선택 인덱스 저장
            SelectedIndexPerPath[property.propertyPath] = selectedIndex;
        }

        /// <summary>
        /// 헤더를 그립니다.
        /// </summary>
        private bool DrawHeader(Rect rect, SerializedProperty property, GUIContent label, SerializedProperty items, SerializedProperty defaultKey, ref int selectedIndex)
        {
            using (new EditorGUI.PropertyScope(rect, label, property))
            {
                // 접기/펼치기 토글
                property.isExpanded = EditorGUI.Foldout(
                    new Rect(rect.x, rect.y, 18f, rect.height),
                    property.isExpanded, GUIContent.none, true);

                // 제목 + 개수
                string title = $"{label.text}  (Count: {items.arraySize})";
                Rect titleRect = new Rect(rect.x + 18f, rect.y, rect.width - 150f, rect.height);
                EditorGUI.LabelField(titleRect, title, EditorStyles.boldLabel);
                if(titleRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                {
                    // 제목 클릭 시 전체 선택 해제
                    selectedIndex = -1;
                    GUI.FocusControl(null);
                    Event.current.Use();
                    // 토글
                    property.isExpanded = !property.isExpanded;
                }

                // Add 버튼
                float buttonW = 50f;
                var addRect = new Rect(rect.xMax - buttonW, rect.y, buttonW, rect.height);
                if (GUI.Button(addRect, "Add"))
                {
                    // 중복 Key 체크
                    if (!HasDuplicateKey(items, defaultKey))
                    {
                        items.arraySize++;
                        var elem = items.GetArrayElementAtIndex(items.arraySize - 1);
                        var keyProp = elem.FindPropertyRelative("Key");
                        keyProp.boxedValue = defaultKey.boxedValue; // Add 시 Key 값 지정
                        // Value는 기본값 그대로 유지
                        property.serializedObject.ApplyModifiedProperties();
                        selectedIndex = items.arraySize - 1;
                    }
                    else
                    {
                        EditorApplication.delayCall += () =>
                            EditorUtility.DisplayDialog("Duplicate Key",
                                $"Key '{(defaultKey.boxedValue != null ? defaultKey.boxedValue.ToString() : "null")}' already exists.",
                                "OK");
                    }
                }

                // Remove(선택 삭제) 버튼
                var rmRect = new Rect(addRect.x - 60f, rect.y, 60f, rect.height);
                EditorGUI.BeginDisabledGroup(selectedIndex < 0 || selectedIndex >= items.arraySize);
                if (GUI.Button(rmRect, "Remove"))
                {
                    if (selectedIndex >= 0 && selectedIndex < items.arraySize)
                    {
                        items.DeleteArrayElementAtIndex(selectedIndex);
                        property.serializedObject.ApplyModifiedProperties();
                        selectedIndex = Mathf.Clamp(selectedIndex - 1, -1, items.arraySize - 1);
                    }
                }
                EditorGUI.EndDisabledGroup();
            }

            return property.isExpanded;
        }

        private void DrawDefaultKey(Rect rect, SerializedProperty defaultKey)
        {
            using (new EditorGUI.IndentLevelScope(1))
            {
                // “다음 Add에 사용할 Key”
                EditorGUI.PropertyField(rect, defaultKey, new GUIContent("Next Key (on Add)"), true);
            }
        }

        private float GetListHeight(SerializedProperty items)
        {
            float h = 6f; // 상하 패딩
            for (int i = 0; i < items.arraySize; i++)
            {
                var elem = items.GetArrayElementAtIndex(i);
                var key = elem.FindPropertyRelative("Key");
                var val = elem.FindPropertyRelative("Value");

                float keyH = EditorGUI.GetPropertyHeight(key, true);
                float valH = EditorGUI.GetPropertyHeight(val, true);
                bool singleLine = Mathf.Max(keyH, valH) <= EditorGUIUtility.singleLineHeight + 2f;

                float rowH = singleLine
                    ? Mathf.Max(keyH, valH) + 8f        // 한 줄
                    : keyH + valH + 12f;                 // 두 줄 (Key + Value)
                h += rowH;
            }
            return h + 4f; // 하단 패딩
        }

        private void DrawList(Rect rect, SerializedProperty items, ref int selectedIndex)
        {
            GUI.Box(rect, GUIContent.none, EditorStyles.helpBox);
            float y = rect.y + 4f;
            float x = rect.x + 6f;
            float w = rect.width - 12f;

            float accumulatedHeight = 0f;
            for (int i = 0; i < items.arraySize; i++)
            {
                var elem = items.GetArrayElementAtIndex(i);
                var key = elem.FindPropertyRelative("Key");
                var val = elem.FindPropertyRelative("Value");

                float keyH = EditorGUI.GetPropertyHeight(key, true);
                float valH = EditorGUI.GetPropertyHeight(val, true);
                float rowH = Mathf.Max(keyH, valH) + 8f;

                Rect rowRect = new Rect(x, y, w, rowH);
                
                // 선택 배경
                bool isSelected = (i == selectedIndex);
                if (Event.current.type == EventType.Repaint && isSelected)
                {
                    EditorGUI.DrawRect(rowRect, new Color(0.25f, 0.5f, 1f, 0.15f));
                }



                // 내부 레이아웃: 한 줄이면 "Key | Value | Trash", 두 줄이면 "Key(왼)+Trash(오) / Value(아래 전체)"
                float trashW = 20f;
                float pad = 4f;
                
                bool singleLine = Mathf.Max(keyH, valH) <= EditorGUIUtility.singleLineHeight + 2f;

                if (singleLine)
                {
                    // Key | Value | Trash
                    float col = (rowRect.width - trashW - 6f) * 0.5f;

                    Rect keyRect   = new Rect(rowRect.x,                 rowRect.y + pad, col - 4f, keyH);
                    Rect valRect   = new Rect(rowRect.x + col + 4f,      rowRect.y + pad, col - 4f, valH);
                    Rect trashRect = new Rect(rowRect.x + col * 2 + 8f,  rowRect.y + pad, trashW,   EditorGUIUtility.singleLineHeight);

                    float prevLW = EditorGUIUtility.labelWidth;
                    float keyLabelW = keyRect.width * 0.3f;
                    float valLabelW = valRect.width * 0.3f;

                    // Key
                    EditorGUI.BeginProperty(keyRect, GUIContent.none, key);
                    EditorGUIUtility.labelWidth = keyLabelW;
                    EditorGUI.PropertyField(keyRect, key, new GUIContent("Key"), true);
                    EditorGUI.EndProperty();

                    // Value
                    EditorGUI.BeginProperty(valRect, GUIContent.none, val);
                    EditorGUIUtility.labelWidth = valLabelW;
                    EditorGUI.PropertyField(valRect, val, new GUIContent("Value"), true);
                    EditorGUI.EndProperty();

                    // Trash
                    if (GUI.Button(trashRect, EditorGUIUtility.IconContent("TreeEditor.Trash"), GUIStyle.none))
                    {
                        items.DeleteArrayElementAtIndex(i);
                        selectedIndex = -1;
                        items.serializedObject.ApplyModifiedProperties();
                        return; // 레이아웃 꼬임 방지
                    }

                    EditorGUIUtility.labelWidth = prevLW;
                    y += rowH;
                }
                else
                {
                    // 두 줄: 1) Key(왼) + Trash(오)  2) Value(아래 전체폭)
                    Rect keyRect   = new Rect(rowRect.x,                     rowRect.y + pad, rowRect.width - trashW - 6f, keyH);
                    Rect trashRect = new Rect(rowRect.xMax - trashW - 2f,    rowRect.y + pad, trashW,                      EditorGUIUtility.singleLineHeight);
                    Rect valRect   = new Rect(rowRect.x,     keyRect.yMax + 4f,              rowRect.width,               valH);

                    float prevLW = EditorGUIUtility.labelWidth;

                    // Key
                    EditorGUI.BeginProperty(keyRect, GUIContent.none, key);
                    EditorGUIUtility.labelWidth = Mathf.Min(90f, keyRect.width * 0.35f);
                    EditorGUI.PropertyField(keyRect, key, new GUIContent("Key"), true);
                    EditorGUI.EndProperty();

                    // Trash
                    if (GUI.Button(trashRect, EditorGUIUtility.IconContent("TreeEditor.Trash"), GUIStyle.none))
                    {
                        items.DeleteArrayElementAtIndex(i);
                        selectedIndex = -1;
                        items.serializedObject.ApplyModifiedProperties();
                        return;
                    }

                    // Value (아래 전체 폭)
                    EditorGUI.BeginProperty(valRect, GUIContent.none, val);
                    EditorGUIUtility.labelWidth = Mathf.Min(90f, valRect.width * 0.18f);
                    EditorGUI.PropertyField(valRect, val, new GUIContent("Value"), true);
                    EditorGUI.EndProperty();

                    EditorGUIUtility.labelWidth = prevLW;
                    y += keyH + valH + 12f;
                }


                
                // 클릭으로 선택
                if (Event.current.type == EventType.MouseDown && rowRect.Contains(Event.current.mousePosition))
                {
                    selectedIndex = i;
                    GUI.FocusControl(null);
                    Event.current.Use();
                }
                
                
            }
        }

        private bool HasDuplicateKey(SerializedProperty items, SerializedProperty defaultKey)
        {
            object targetKey = defaultKey.boxedValue;
            for (int i = 0; i < items.arraySize; i++)
            {
                var elem = items.GetArrayElementAtIndex(i);
                var keyProp = elem.FindPropertyRelative("Key");
                if (SerializedObjectEqual(keyProp.boxedValue, targetKey))
                    return true;
            }
            return false;
        }

        // 간단 동등성 비교(직렬화된 값끼리 문자열로도 fallback)
        private bool SerializedObjectEqual(object a, object b)
        {
            if (ReferenceEquals(a, b)) return true;
            if (a == null || b == null) return false;
            if (a.Equals(b)) return true;
            return a.ToString() == b.ToString();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float h = EditorGUIUtility.singleLineHeight + 3f; // 헤더
            if (!property.isExpanded) return h;

            var items = property.FindPropertyRelative("_items");
            // Default Key 라인
            h += EditorGUIUtility.singleLineHeight + 4f;
            // 리스트 높이
            h += GetListHeight(items);
            return h;
        }
    }
#endif
}