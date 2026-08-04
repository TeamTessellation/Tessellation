using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Tessellation.Tests.PlayMode
{
    public class BombRuntimeSmokeTests
    {
        [UnityTest]
        public IEnumerator BombRuntimeTypesLoadInPlayMode()
        {
            yield return null;
            Assembly gameAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .Single(assembly => assembly.GetName().Name == "Assembly-CSharp");

            Assert.That(gameAssembly.GetType("ExplosionResolver"), Is.Not.Null);
            Assert.That(gameAssembly.GetType("BombRules"), Is.Not.Null);
            Assert.That(gameAssembly.GetType("TileOptionBoom"), Is.Not.Null);
            Assert.That(gameAssembly.GetType("TileScoreRules"), Is.Not.Null);
        }
    }
}
