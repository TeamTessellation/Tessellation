using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Tessellation.Tests.EditMode
{
    public class BombAndScoreLogicTests
    {
        private Assembly GameAssembly => AppDomain.CurrentDomain.GetAssemblies()
            .Single(assembly => assembly.GetName().Name == "Assembly-CSharp");

        [Test]
        public void ExplosionWithoutChainDestroysAdjacentBombButDoesNotDetonateIt()
        {
            object resolution = Resolve(
                occupied: new[] { (0, 0), (1, 0), (2, 0) },
                bombs: new[] { (0, 0), (1, 0) },
                seeds: new[] { (0, 0) },
                radius: 1,
                chain: false);

            Assert.That(ReadCoordinates(resolution, "DestroyedCoordinates"),
                Is.EquivalentTo(new[] { new Vector2Int(0, 0), new Vector2Int(1, 0) }));
            Assert.That(ReadCoordinates(resolution, "DetonatedBombCoordinates"),
                Is.EquivalentTo(new[] { new Vector2Int(0, 0) }));
        }

        [Test]
        public void ChainExplosionPropagatesAndNeverDuplicatesBombs()
        {
            object resolution = Resolve(
                occupied: new[] { (0, 0), (1, 0), (2, 0) },
                bombs: new[] { (0, 0), (1, 0) },
                seeds: new[] { (0, 0), (0, 0) },
                radius: 1,
                chain: true);

            Assert.That(ReadCoordinates(resolution, "DestroyedCoordinates"),
                Is.EquivalentTo(new[]
                {
                    new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0)
                }));
            Assert.That(ReadCoordinates(resolution, "DetonatedBombCoordinates"), Has.Count.EqualTo(2));
        }

        [Test]
        public void ExplosionUsesHexDistanceAndConfiguredRadius()
        {
            object resolution = Resolve(
                occupied: new[] { (0, 0), (2, 0), (2, -2), (1, 1), (3, 0) },
                bombs: new[] { (0, 0) },
                seeds: new[] { (0, 0) },
                radius: 2,
                chain: false);

            Assert.That(ReadCoordinates(resolution, "DestroyedCoordinates"),
                Is.EquivalentTo(new[]
                {
                    new Vector2Int(0, 0), new Vector2Int(2, 0), new Vector2Int(2, -2),
                    new Vector2Int(1, 1)
                }));
        }

        [Test]
        public void BombRulesApplyAndRemoveRangeImmediateAndChainEffects()
        {
            Type rulesType = GameAssembly.GetType("BombRules", throwOnError: true);
            Type itemType = GameAssembly.GetType("Abilities.eItemType", throwOnError: true);
            object rules = Activator.CreateInstance(rulesType);
            MethodInfo apply = rulesType.GetMethod("Apply");
            MethodInfo remove = rulesType.GetMethod("Remove");

            object immediate = Enum.Parse(itemType, "BombImmediatelyExplosion");
            object chain = Enum.Parse(itemType, "ChainExplosion");
            object extraTurn = Enum.Parse(itemType, "ExtraTurn");
            apply.Invoke(rules, new[] { immediate, (object)1 });
            apply.Invoke(rules, new[] { chain, (object)1 });
            apply.Invoke(rules, new[] { extraTurn, (object)10 });

            Assert.That(ReadProperty<int>(rules, "ExplosionRadius"), Is.EqualTo(3));
            Assert.That(ReadProperty<bool>(rules, "ExplodesImmediately"), Is.True);
            Assert.That(ReadProperty<bool>(rules, "Chains"), Is.True);

            remove.Invoke(rules, new[] { immediate, (object)1 });
            remove.Invoke(rules, new[] { chain, (object)1 });
            Assert.That(ReadProperty<int>(rules, "ExplosionRadius"), Is.EqualTo(1));
            Assert.That(ReadProperty<bool>(rules, "ExplodesImmediately"), Is.False);
            Assert.That(ReadProperty<bool>(rules, "Chains"), Is.False);
        }

        [TestCase("Place", "Boom", true, "BasePlaceScore", false, false)]
        [TestCase("LineClear", "Default", true, "BaseLineClearScore", false, false)]
        [TestCase("LineClear", "Boom", false, "BasePlaceScore", false, false)]
        [TestCase("LineClear", "Bonus", true, "BaseBonusScore", false, false)]
        [TestCase("LineClear", "Double", true, "BaseBonusScore", false, true)]
        [TestCase("LineClear", "Gold", true, "BaseBonusScore", true, false)]
        [TestCase("Burst", "Default", true, "BaseBurstScore", false, false)]
        [TestCase("Burst", "Boom", false, "BasePlaceScore", false, false)]
        [TestCase("Burst", "Bonus", true, "BaseBonusScore", false, false)]
        [TestCase("Burst", "Double", true, "BaseBurstScore", false, false)]
        [TestCase("Burst", "Gold", true, "BaseBurstScore", true, false)]
        public void TileScoreRuleMatchesDesign(
            string eventName,
            string optionName,
            bool awardsScore,
            string scoreTypeName,
            bool awardsCoin,
            bool appliesDouble)
        {
            Type rulesType = GameAssembly.GetType("TileScoreRules", throwOnError: true);
            Type eventType = GameAssembly.GetType("eTileEventType", throwOnError: true);
            Type optionType = GameAssembly.GetType("TileOption", throwOnError: true);
            object rule = rulesType.GetMethod("Get").Invoke(null, new[]
            {
                Enum.Parse(eventType, eventName), Enum.Parse(optionType, optionName)
            });

            Assert.That(ReadProperty<bool>(rule, "AwardsScore"), Is.EqualTo(awardsScore));
            Assert.That(ReadProperty<bool>(rule, "AwardsCoin"), Is.EqualTo(awardsCoin));
            Assert.That(ReadProperty<bool>(rule, "AppliesDoubleMultiplier"), Is.EqualTo(appliesDouble));
            if (awardsScore)
                Assert.That(ReadProperty<object>(rule, "ScoreType").ToString(), Is.EqualTo(scoreTypeName));
        }

        [Test]
        public void BombTilePrefabUsesBombOptionHandler()
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/PoolObj/Tile.prefab");
            Assert.That(prefab, Is.Not.Null);
            GameObject instance = UnityEngine.Object.Instantiate(prefab);

            try
            {
                Type tileType = GameAssembly.GetType("Tile", throwOnError: true);
                Type tileDataType = GameAssembly.GetType("TileData", throwOnError: true);
                Type optionType = GameAssembly.GetType("TileOption", throwOnError: true);
                Component tile = instance.GetComponent(tileType);
                object tileData = Activator.CreateInstance(tileDataType,
                    new[] { Enum.Parse(optionType, "Boom"), (object)1f });

                tileType.GetMethod("Set").Invoke(tile, new[] { tileData });
                object handler = tileType.GetField("TileOptionBase").GetValue(tile);
                Assert.That(handler.GetType().Name, Is.EqualTo("TileOptionBoom"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void ScoreModifiersComposeInOrderAndCanBeUnregistered()
        {
            Type scoreManagerType = GameAssembly.GetType("ScoreManager", throwOnError: true);
            Type eventType = GameAssembly.GetType("eTileEventType", throwOnError: true);
            Type tileType = GameAssembly.GetType("Tile", throwOnError: true);
            Type modifierType = scoreManagerType.GetNestedType("TileScoreModifierDelegate");
            GameObject owner = new GameObject("ScoreManager test owner");
            Component scoreManager = owner.AddComponent(scoreManagerType);

            try
            {
                Delegate multiply = CreateScoreModifier(modifierType, eventType, tileType, OpCodes.Mul, 2);
                Delegate add = CreateScoreModifier(modifierType, eventType, tileType, OpCodes.Add, 3);
                MethodInfo register = scoreManagerType.GetMethod("RegisterScoreModifier");
                MethodInfo unregister = scoreManagerType.GetMethod("UnRegisterScoreModifier");

                register.Invoke(scoreManager, new object[] { multiply });
                register.Invoke(scoreManager, new object[] { add });

                object result = scoreManagerType.GetMethod("CalculateTileScore").Invoke(scoreManager,
                    new[] { Enum.Parse(eventType, "Place"), null, (object)2 });
                Assert.That(result, Is.EqualTo(7));

                unregister.Invoke(scoreManager, new object[] { multiply });
                unregister.Invoke(scoreManager, new object[] { add });
                var modifiers = (ICollection)scoreManagerType.GetField("_tileScoreModifiers").GetValue(scoreManager);
                Assert.That(modifiers.Count, Is.Zero);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(owner);
            }
        }

        private object Resolve(
            IEnumerable<(int x, int y)> occupied,
            IEnumerable<(int x, int y)> bombs,
            IEnumerable<(int x, int y)> seeds,
            int radius,
            bool chain)
        {
            Type coordinateType = GameAssembly.GetType("Coordinate", throwOnError: true);
            Type resolverType = GameAssembly.GetType("ExplosionResolver", throwOnError: true);
            object occupiedList = MakeCoordinateList(coordinateType, occupied);
            object bombList = MakeCoordinateList(coordinateType, bombs);
            object seedList = MakeCoordinateList(coordinateType, seeds);
            return resolverType.GetMethod("Resolve").Invoke(null,
                new[] { occupiedList, bombList, seedList, (object)radius, chain });
        }

        private static object MakeCoordinateList(Type coordinateType, IEnumerable<(int x, int y)> values)
        {
            Type listType = typeof(List<>).MakeGenericType(coordinateType);
            var list = (IList)Activator.CreateInstance(listType);
            foreach ((int x, int y) in values)
                list.Add(Activator.CreateInstance(coordinateType, x, y));
            return list;
        }

        private static List<Vector2Int> ReadCoordinates(object resolution, string propertyName)
        {
            var coordinates = (IEnumerable)resolution.GetType().GetProperty(propertyName).GetValue(resolution);
            var result = new List<Vector2Int>();
            foreach (object coordinate in coordinates)
                result.Add((Vector2Int)coordinate.GetType().GetField("Pos").GetValue(coordinate));
            return result;
        }

        private static T ReadProperty<T>(object value, string propertyName)
        {
            return (T)value.GetType().GetProperty(propertyName).GetValue(value);
        }

        private static Delegate CreateScoreModifier(
            Type delegateType,
            Type eventType,
            Type tileType,
            OpCode operation,
            int operand)
        {
            var method = new DynamicMethod(
                "ScoreModifier",
                typeof(int),
                new[] { eventType, tileType, typeof(int) },
                typeof(BombAndScoreLogicTests).Module,
                skipVisibility: true);
            ILGenerator il = method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_2);
            il.Emit(OpCodes.Ldc_I4, operand);
            il.Emit(operation);
            il.Emit(OpCodes.Ret);
            return method.CreateDelegate(delegateType);
        }
    }
}
