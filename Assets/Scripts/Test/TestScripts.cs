using System;
using Collections;
using DataAnalysis;
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
      
        public VariableContainer variableContainer = new VariableContainer();


        public int testInt = 0;

        private void OnEnable()
        {
            SaveManager.RegisterPendingSavable(this);
        }
        
        [ContextMenu("TrackGameStart")]
        public void TrackGameStart()
        {
            AnalyticsManager.Instance.TrackGameStart();
        }
        [ContextMenu("TrackGameOver")]
        public void TrackGameOver()
        {
            AnalyticsManager.Instance.TrackGameOver(true, 120, "TestPlayer");
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
            using (new EditorGUILayout.VerticalScope("box"))
            {
                if (GUILayout.Button("Track Game Start"))
                {
                    testScripts.TrackGameStart();
                }

                if (GUILayout.Button("Track Game Over"))
                {
                    testScripts.TrackGameOver();
                }
            }
            
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