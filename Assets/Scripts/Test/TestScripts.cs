using System;
using Collections;
using Machamy.Utils;
using SaveLoad;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Test
{
    public class TestScripts : MonoBehaviour, ISavable
    {
        public SerializableDictionary<string, int> testDictionary = new SerializableDictionary<string, int>();
        public SerializableDictionary<string, Sprite> spriteDictionary = new SerializableDictionary<string, Sprite>();
        public SerializableDictionary<string, ScriptableObject> scriptableObjectDictionary = new SerializableDictionary<string, ScriptableObject>();
        public VariableContainer variableContainer = new VariableContainer();


        public int testInt = 0;

        private void OnEnable()
        {
            SaveManager.RegisterPendingSavable(this);
        }


        public void LoadData(SaveData data)
        {
            var testVariable = data["TestInt"];
            if (testVariable != null)
            {
                testInt = testVariable.IntValue;
            }
            else
            {
                testInt = 0;
            }
        }

        public void SaveData(ref SaveData data)
        {
            var testVariableRef = data["TestInt"];
            if (testVariableRef != null)
            {
                testVariableRef.IntValue = testInt;
            }
            else
            {
                data.SaveVariable("TestInt", testInt);
            }
        }
    }

    [CustomEditor(typeof(TestScripts))]
    public class TestScriptsEditor : Editor
    {
        static int appCounter = 0;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (!Application.isPlaying)
            {
                appCounter = 0;
                return;
            }
            if (appCounter < 3)
            {
                appCounter++;
                return;
            }
            var testScripts = target as TestScripts;
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Save Test Data"))
                {
                    testScripts.testInt += 1;
                    HistoryManager.Instance.SaveCurrentState();
                }
                if(HistoryManager.Instance.SaveHistory.Count > 0)
                {
                    if (GUILayout.Button("Load Last Saved Data"))
                    {
                        HistoryManager.Instance.LoadAndPopLastSave();
                        LogEx.Log($"Loaded TestInt: {testScripts.testInt}");
                    }
                }
            }
        }
    }
}