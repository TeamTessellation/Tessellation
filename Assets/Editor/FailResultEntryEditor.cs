using UnityEditor;
using UnityEngine;
using UI.OtherUIs;
using Player;

[CustomEditor(typeof(FailResultEntry))]
public class FailResultEntryEditor : Editor
{
    private SerializedProperty failResultUIProp;
    private SerializedProperty failCountTextProp;
    private SerializedProperty variableKeyStringProp;
    private SerializedProperty moveDirectionProp;

    private void OnEnable()
    {
        failResultUIProp = serializedObject.FindProperty("failResultUI");
        failCountTextProp = serializedObject.FindProperty("failCountText");
        variableKeyStringProp = serializedObject.FindProperty("variableKeyString");
        moveDirectionProp = serializedObject.FindProperty("<MoveDirection>k__BackingField");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(failResultUIProp);
        EditorGUILayout.PropertyField(failCountTextProp);

        // VariableKey를 enum으로 표시하되, 문자열로 저장
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Variable Key", EditorStyles.boldLabel);
        
        // 현재 문자열을 enum으로 파싱
        PlayerStatus.VariableKey currentKey = PlayerStatus.VariableKey.TotalScore;
        if (!string.IsNullOrEmpty(variableKeyStringProp.stringValue))
        {
            if (System.Enum.TryParse<PlayerStatus.VariableKey>(variableKeyStringProp.stringValue, out var parsedKey))
            {
                currentKey = parsedKey;
            }
        }

        // enum 필드로 표시
        PlayerStatus.VariableKey newKey = (PlayerStatus.VariableKey)EditorGUILayout.EnumPopup("Variable Key", currentKey);

        // 변경되었다면 문자열로 저장
        if (newKey != currentKey)
        {
            variableKeyStringProp.stringValue = newKey.ToString();
        }

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(moveDirectionProp, new GUIContent("Move Direction"));

        serializedObject.ApplyModifiedProperties();
    }
}

