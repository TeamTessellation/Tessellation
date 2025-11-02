using System;
using Collections;
using Cysharp.Threading.Tasks;
using DataAnalysis;
using Machamy.Utils;
using PriortyExecEvent;
using SaveLoad;
using Unity.VisualScripting;
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
            
            ExecEventBus<TestExecEventArgs>.Register(OnTestEvent);
            ExecEventBus<TestExecEventArgs>.Register(OnTestEvent2);
        }

        private void OnDisable()
        {
            ExecEventBus<TestExecEventArgs>.Unregister(OnTestEvent);
            ExecEventBus<TestExecEventArgs>.Unregister(OnTestEvent2);
        }

        [ContextMenu("InvokeTestEvent")]
        public async void InvokeTestEvent()
        {
            using var args = new TestExecEventArgs { Value = UnityEngine.Random.Range(1, 100) };
            LogEx.Log($"Invoking TestExecEvent with Value: {args.Value}");
            await ExecEventBus<TestExecEventArgs>.Invoke(args);
        }
        
        private ExecAction<TestExecEventArgs> _highPriorityAction = async (args) =>
        {
            await UniTask.Delay(50);
            LogEx.Log($"High Priority Action Executed with Value: {args.Value}");
        };
        private ExecAction<TestExecEventArgs> _firstPriorityAction = async (args) =>
        {
            await UniTask.Delay(100);
            LogEx.Log($"First Priority Action Executed with Value: {args.Value}");
        };

        private ExecAction<TestExecEventArgs> _secondPriorityAction = async (args) =>
        {
            await UniTask.Delay(150);
            LogEx.Log($"Second Priority Action Executed with Value: {args.Value}");
        };
        private ExecAction<TestExecEventArgs> _thirdPriorityAction = async (args) =>
        {
            await UniTask.Delay(200);
            LogEx.Log($"Third Priority Action Executed with Value: {args.Value}");
        };
        
        public void OnTestEvent(ExecQueue<TestExecEventArgs> queue, TestExecEventArgs args)
        {
            LogEx.Log($"Received TestExecEvent with Value: {args.Value}");
            
            queue.Enqueue(2, _secondPriorityAction);
            queue.Enqueue(1, _firstPriorityAction);
            queue.Enqueue(3, _thirdPriorityAction);
        }
        
        public void OnTestEvent2(ExecQueue<TestExecEventArgs> queue, TestExecEventArgs args)
        {
            LogEx.Log($"Received TestExecEvent with Value: {args.Value}");
            queue.Enqueue(0, _highPriorityAction);
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