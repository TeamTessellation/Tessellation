using System;
using Collections;
using Cysharp.Threading.Tasks;
using DataAnalysis;
using Machamy.Utils;
using ExecEvents;
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
        #region Fields
        
        public VariableContainer variableContainer = new VariableContainer();
        public int testInt = 0;
        
        #endregion

        #region Unity Lifecycle
        
        private void OnEnable()
        {
            SaveManager.RegisterPendingSavable(this);
            
            ExecDynamicEventBus<TestExecEventArgs>.Register(OnTestEvent);
            ExecDynamicEventBus<TestExecEventArgs>.Register(OnTestEvent2);
            ExecStaticEventBus<TestExecEventArgs>.Register(1, async (args) =>
            {
                await UniTask.Delay(500);
                LogEx.Log($"Static Event Bus Handler Executed with Value: {args.Value}");
            });
        }

        private void OnDisable()
        {
            ExecDynamicEventBus<TestExecEventArgs>.Unregister(OnTestEvent);
            ExecDynamicEventBus<TestExecEventArgs>.Unregister(OnTestEvent2);
        }
        
        #endregion

        #region ExecEvent Test

        [ContextMenu("ExecEvent/InvokeTestEvent")]
        public async void InvokeTestEvent()
        {
            using var args = new TestExecEventArgs { Value = UnityEngine.Random.Range(1, 100) };
            LogEx.Log($"Invoking TestExecEvent with Value: {args.Value}");
            await ExecEventBus<TestExecEventArgs>.InvokeSequentially(args);
        }
        
        [ContextMenu("ExecEvent/InvokeSequentially")]
        public async void InvokeTestEventSequentially()
        {
            using var args = TestExecEventArgs.Get();
            args.Value = UnityEngine.Random.Range(1, 100);
            LogEx.Log($"[Test] InvokeSequentially with Value: {args.Value}");
            await ExecEventBus<TestExecEventArgs>.InvokeSequentially(args);
        }
        
        [ContextMenu("ExecEvent/InvokeMerged")]
        public async void InvokeTestEventMerged()
        {
            using var args = TestExecEventArgs.Get();
            args.Value = UnityEngine.Random.Range(1, 100);
            LogEx.Log($"[Test] InvokeMerged with Value: {args.Value}");
            await ExecEventBus<TestExecEventArgs>.InvokeMerged(args);
        }
        
        [ContextMenu("ExecEvent/InvokeDynamicOnly")]
        public async void InvokeTestEventDynamicOnly()
        {
            using var args = TestExecEventArgs.Get();
            args.Value = UnityEngine.Random.Range(1, 100);
            LogEx.Log($"[Test] InvokeDynamic with Value: {args.Value}");
            await ExecDynamicEventBus<TestExecEventArgs>.Invoke(args);
        }
        
        [ContextMenu("ExecEvent/InvokeStaticOnly")]
        public async void InvokeTestEventStaticOnly()
        {
            using var args = TestExecEventArgs.Get();
            args.Value = UnityEngine.Random.Range(1, 100);
            LogEx.Log($"[Test] InvokeStatic with Value: {args.Value}");
            await ExecStaticEventBus<TestExecEventArgs>.Invoke(args);
        }
        
        [ContextMenu("ExecEvent/TestBreakChain")]
        public async void TestBreakChain()
        {
            using var args = TestExecEventArgs.Get();
            args.Value = 999;
            LogEx.Log($"[Test] TestBreakChain - 두 번째 액션에서 중단됨");
            await ExecEventBus<TestExecEventArgs>.InvokeSequentially(args);
        }
        
        [ContextMenu("ExecEvent/TestExtraPriorities")]
        public async void TestExtraPriorities()
        {
            // 임시로 Static 핸들러 추가
            ExecStaticEventBus<TestExecEventArgs>.Register(10, async args => 
                LogEx.Log($"[Static] Priority (10) - No Extra"));
            ExecStaticEventBus<TestExecEventArgs>.Register(10, async args => 
                LogEx.Log($"[Static] Priority (10, 5) - Extra [5]"), 5);
            ExecStaticEventBus<TestExecEventArgs>.Register(10, async args => 
                LogEx.Log($"[Static] Priority (10, 5, 3) - Extra [5, 3]"), 5, 3);
            ExecStaticEventBus<TestExecEventArgs>.Register(10, async args => 
                LogEx.Log($"[Static] Priority (10, 8) - Extra [8]"), 8);
            
            using var args = TestExecEventArgs.Get();
            args.Value = 100;
            LogEx.Log($"[Test] TestExtraPriorities - Extra Priority 비교 테스트");
            await ExecStaticEventBus<TestExecEventArgs>.Invoke(args);
            
            // 정리
            ExecStaticEventBus<TestExecEventArgs>.ClearHandlers();
            // 기본 Static 핸들러 다시 등록
            ExecStaticEventBus<TestExecEventArgs>.Register(1, async (eventArgs) =>
            {
                await UniTask.Delay(500);
                LogEx.Log($"[Action] Priority (1) Static Event Bus Handler Executed with Value: {eventArgs.Value}");
            });
        }
        
        [ContextMenu("ExecEvent/TestExecutionState")]
        public async void TestExecutionState()
        {
            LogEx.Log($"[Test] IsExecuting (Before): {ExecEventBus<TestExecEventArgs>.IsExecuting}");
            
            using var args = TestExecEventArgs.Get();
            args.Value = 50;
            var task = ExecEventBus<TestExecEventArgs>.InvokeSequentially(args);
            
            await UniTask.Delay(100);
            LogEx.Log($"[Test] IsExecuting (During): {ExecEventBus<TestExecEventArgs>.IsExecuting}");
            
            await task;
            LogEx.Log($"[Test] IsExecuting (After): {ExecEventBus<TestExecEventArgs>.IsExecuting}");
        }
        
        // Action 정의
        private ExecAction<TestExecEventArgs> _highPriorityAction = async (args) =>
        {
            await UniTask.Delay(500);
            LogEx.Log($"[Action] High Priority (0) Executed with Value: {args.Value}");
        };
        
        private ExecAction<TestExecEventArgs> _firstPriorityAction = async (args) =>
        {
            await UniTask.Delay(500);
            LogEx.Log($"[Action] First Priority (1) Executed with Value: {args.Value}");
            
            // BreakChain 테스트용
            if (args.Value == 999)
            {
                LogEx.Log($"[Action] BreakChain 설정! 이후 액션은 실행되지 않습니다.");
                args.BreakChain = true;
            }
        };

        private ExecAction<TestExecEventArgs> _secondPriorityAction = async (args) =>
        {
            await UniTask.Delay(500);
            LogEx.Log($"[Action] Second Priority (2) Executed with Value: {args.Value}");
        };
        
        private ExecAction<TestExecEventArgs> _thirdPriorityAction = async (args) =>
        {
            await UniTask.Delay(500);
            LogEx.Log($"[Action] Third Priority (3) Executed with Value: {args.Value}");
        };
        
        // Event Handlers
        public void OnTestEvent(ExecQueue<TestExecEventArgs> queue, TestExecEventArgs args)
        {
            LogEx.Log($"[%method%:Handler] OnTestEvent - Enqueueing 3 actions");
            
            queue.Enqueue(2, _secondPriorityAction);
            queue.Enqueue(1, _firstPriorityAction);
            queue.Enqueue(3, _thirdPriorityAction);
        }
        
        public void OnTestEvent2(ExecQueue<TestExecEventArgs> queue, TestExecEventArgs args)
        {
            LogEx.Log($"[Handler] OnTestEvent2 - Enqueueing high priority action");
            queue.Enqueue(0, _highPriorityAction);
        }
        
        #endregion

        #region Analytics Test

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
        
        #endregion

        #region ISavable Implementation

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
        
        #endregion
    }

    #region Editor

#if UNITY_EDITOR
    [CustomEditor(typeof(TestScripts))]
    public class TestScriptsEditor : Editor
    {
        private static int _appCounter;
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            if (!Application.isPlaying)
            {
                _appCounter = 0;
                return;
            }
            
            if (_appCounter < 3)
            {
                _appCounter++;
                return;
            }
            
            var testScripts = target as TestScripts;
            if (testScripts == null) return;
            
            // Analytics Test Buttons
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Analytics Test", EditorStyles.boldLabel);
                
                if (GUILayout.Button("Track Game Start"))
                {
                    testScripts.TrackGameStart();
                }

                if (GUILayout.Button("Track Game Over"))
                {
                    testScripts.TrackGameOver();
                }
            }
            
            // Save/Load Test Buttons
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Save/Load Test", EditorStyles.boldLabel);
                
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Save Test Data"))
                    {
                        testScripts.testInt += 1;
                        HistoryManager.Instance.SaveCurrentState();
                    }
                    
                    if (HistoryManager.Instance.SaveHistory.Count > 0)
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
#endif
    
    #endregion
}