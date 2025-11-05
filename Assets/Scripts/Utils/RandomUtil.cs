
using System;
using System.Collections.Generic;
using System.Linq;
using Machamy.DeveloperConsole.Attributes;
using UMRandom = Unity.Mathematics.Random;
using URandom = UnityEngine.Random;

namespace Utils
{
    public static class RandomUtil
    {
        public enum RandomType
        {
            Default = 0,
            CardShuffle, 
            
            MAX
        }
        
        private static Dictionary<RandomType, UMRandom> randoms = new Dictionary<RandomType, UMRandom>();
        private static Dictionary<RandomType, uint> initialRandomSeeds = new Dictionary<RandomType, uint>();
        
        
        private static bool _isInitialized = false;
        
        public static void Init()
        {
            _isInitialized = true;
            for (RandomType randomType = RandomType.Default; randomType < RandomType.MAX; randomType++)
            {
                InitSeed(randomType);
            }
        }

        public static void InitSeed(RandomType type, uint initialSeed = 0, uint setSeed = 0)
        {
            if (setSeed == 0)
            {
                setSeed = initialSeed == 0 ? (uint) URandom.Range(int.MinValue, int.MaxValue) : initialSeed;
            }
            randoms[type] = new UMRandom(setSeed);
            initialRandomSeeds[type] = initialSeed;
        }
        
        public static int GetRandomInt(int min, int max, RandomType type = RandomType.Default)
        {
            if (!randoms.TryGetValue(type, out var r))
            {
                InitSeed(type);
            }

            var random = randoms[type];
            int result = random.NextInt(min, max);
            randoms[type] = random; // UMRandom이 struct이기 때문.
            
            return result;
        }

        /// <summary>
        /// Returns a random float within [0.0..1.0]
        /// </summary>
        public static float GetValue(RandomType type = RandomType.Default)
        {
            return GetRandomFloat(0, 1, type);
        }
        
        public static float GetRandomFloat(float min, float max, RandomType type = RandomType.Default)
        {
            if (!randoms.TryGetValue(type, out var r))
            {
                InitSeed(type);
            }
            
            var random = randoms[type];
            float result = random.NextFloat(min, max);
            randoms[type] = random; // UMRandom이 struct이기 때문.
            
            return result;
        }
        
        /// <summary>
        /// <see cref="RandomType"/> 기반의 Fisher–Yates in place 셔플.  
        /// </summary>
        public static void ShuffleListInPlace<T>(this IList<T> list, RandomType type = RandomType.CardShuffle)
        {
            if (list == null) throw new ArgumentNullException(nameof(list));

            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = GetRandomInt(0, i + 1, type);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        [Serializable]
        public class RandomSave
        {
            public RandomType type;
            public uint initialSeed;
            public uint state;
        }
        
        public static List<RandomSave> SerializeRandoms()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("RandomUtil is not initialized. Call Init() first.");
            }
            
            List<RandomSave> saves = new List<RandomSave>();
            foreach (var kvp in randoms)
            {
                saves.Add(new RandomSave
                {
                    type = kvp.Key,
                    initialSeed = initialRandomSeeds[kvp.Key],
                    state = kvp.Value.state
                });
            }
            return saves;
        }
        
        public static void DeserializeRandoms(List<RandomSave> saves)
        {
            foreach (var save in saves)
            {
                InitSeed(save.type, save.initialSeed, save.state);
            }
            _isInitialized = true;
        }
    }
    
}