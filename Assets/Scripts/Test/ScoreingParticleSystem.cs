using System;
using System.Collections.Generic;
using Machamy.Utils;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Test
{
    [RequireComponent(typeof(ParticleSystem))]
    public class ScoreingParticleSystem : MonoBehaviour
    {
        [SerializeField, HideInInspector] private ParticleSystem _particleSystem;

        public TestSlider testSlider;
        public TestSliderEnd target;
        public int toSummon = 5;

        [FormerlySerializedAs("scorePerSummon")]
        public int totalSummonScore = 10;

        
        public float _rangeAngle = 30f;
        
        public float emitForceMin = 3f;
        public float emitForceMax = 6f;
        
        private List<Vector4> customData = new List<Vector4>();
        private ParticleSystem.Particle[] particlesArray;
        private int[] scoresArray = new int[10];

        private void Reset()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        private void Start()
        { 
            testSlider = FindAnyObjectByType<TestSlider>();
            target = FindAnyObjectByType<TestSliderEnd>();
        }

        private void LateUpdate()
        {
            // Trigger로는 안잡혀서 수동으로 처리
            if (particlesArray == null || particlesArray.Length < _particleSystem.main.maxParticles)
            {
                particlesArray = new ParticleSystem.Particle[_particleSystem.main.maxParticles];
            }

            int numParticlesAlive = _particleSystem.GetParticles(particlesArray);
            _particleSystem.GetCustomParticleData(customData, ParticleSystemCustomData.Custom1);

            if (testSlider == null) return;
            
            bool updated = false;
            
            for (int i = 0; i < numParticlesAlive; i++)
            {
                var p = particlesArray[i];
                if (p.remainingLifetime <= 0f) continue;
                if (target.BoxCollider.bounds.Intersects(new Bounds(p.position, Vector3.one * p.GetCurrentSize(_particleSystem))))
                {
                    // 파티클 제거
                    p.remainingLifetime = -1f;
                    p.startLifetime = p.remainingLifetime;
                    updated = true;
                    // 점수 추가
                    int scoreToAdd = Mathf.RoundToInt(customData[i].x);
                    testSlider.rangeModel.AddValue(scoreToAdd);
                    LogEx.Log($"Particle collided, adding score: {scoreToAdd}");
                    testSlider.UpdateSliderToRangeModel();
                }
                particlesArray[i] = p;
            }
            if (updated)    
                _particleSystem.SetParticles(particlesArray, numParticlesAlive);
        }

        public void EmitParticles()
        {
            if (testSlider == null) return;
            int remain = totalSummonScore;
            if (scoresArray.Length < toSummon)
            {
                scoresArray = new int[toSummon];
            }

            for (int i = 0; i < toSummon; i++)
            {
                int score = Random.Range(0, totalSummonScore / toSummon);
                if (i == toSummon - 1)
                {
                    score = remain;
                }

                remain -= score;
                scoresArray[i] = score;
            }

            EmitParticles(toSummon, scoresArray);
        }
        
        public void EmitParticles(int cnt, params int[] scores)
        {
            for (int i = 0; i < cnt; i++)
            {
                int score = scores[i];
                var emitParams = new ParticleSystem.EmitParams();
                emitParams.velocity = GetEmitDirection() * Random.Range(emitForceMin, emitForceMax);
                emitParams.startSize = Mathf.Lerp(1f, 3f, (float)score / totalSummonScore);
                _particleSystem.Emit(emitParams, 1);
            }

            _particleSystem.GetCustomParticleData(customData, ParticleSystemCustomData.Custom1);
            int startIndex = customData.Count - cnt;
            for (int i = 0; i < cnt; i++)
            {
                customData[startIndex + i] = new Vector4(scores[i], 0, 0, 1);
            }

            _particleSystem.SetCustomParticleData(customData, ParticleSystemCustomData.Custom1);
        }
        

        public void PlayParticle()
        {
            _particleSystem.Play();
        }

        
        public float GetMidAngle()
        {
            // 타겟쪽 각도의 반대
            Vector3 toTarget = target.transform.position - transform.position;
            float angleToTarget = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
            float emitAngle = angleToTarget + 90f;
            return emitAngle;
        }
        
        public Vector3 GetEmitDirection()
        {
            float angle = GetMidAngle() + Random.Range(-_rangeAngle / 2f, _rangeAngle / 2f);
            Vector3 dir = Quaternion.Euler(0f, 0f, angle) * Vector3.up;
            return dir.normalized;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            float midAngle = GetMidAngle();
            Vector3 dirMid = Quaternion.Euler(0f, 0f, midAngle) * Vector3.up;
            Gizmos.DrawLine(transform.position, transform.position + dirMid * 2f);
            Vector3 dir1 = Quaternion.Euler(0f, 0f, midAngle - _rangeAngle / 2f) * Vector3.up;
            Vector3 dir2 = Quaternion.Euler(0f, 0f, midAngle + _rangeAngle / 2f) * Vector3.up;
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + dir1 * 2f);
            Gizmos.DrawLine(transform.position, transform.position + dir2 * 2f);
        }

        // private List<ParticleSystem.Particle> enter = new ();
        // private List<Vector4> scores = new();


        //
        //     public void OnParticleTrigger()
        //     {
        //         LogEx.Log("OnParticleTrigger called");
        //         int numEnter = _particleSystem.GetTriggerParticles(ParticleSystemTriggerEventType.Enter, enter);
        //         _particleSystem.GetCustomParticleData(scores, ParticleSystemCustomData.Custom1);
        //         
        //         for (int i = 0; i < numEnter; i++)
        //         {
        //             
        //             // 파티클 제거
        //             var p = enter[i];
        //             p.remainingLifetime = 0f;
        //             
        //             // 점수 추가
        //             int scoreToAdd = Mathf.RoundToInt(scores[i].x);
        //             var testSlider = FindAnyObjectByType<TestSlider>();
        //             if (testSlider == null) continue;
        //             testSlider.rangeModel.AddValue(scoreToAdd);
        //             testSlider.UpdateSliderToRangeModel();
        //         }
        //     }
        // }

#if UNITY_EDITOR
        [UnityEditor.CustomEditor(typeof(ScoreingParticleSystem))]
        public class TestParticleEditor : UnityEditor.Editor
        {
            public override void OnInspectorGUI()
            {
                DrawDefaultInspector();
                ScoreingParticleSystem myScript = (ScoreingParticleSystem)target;
                if (GUILayout.Button("Emit Particles"))
                {
                    myScript.EmitParticles();
                }

                if (GUILayout.Button("Play Particle System"))
                {
                    myScript.PlayParticle();
                }
            }
        }
#endif
    }
}