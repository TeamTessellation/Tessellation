using System.Collections;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Tessellation.Tests.PlayMode
{
    public class InitialLoadingSceneSmokeTests
    {
        private const string InitialLoadingScene = "InitialLoadingScene";

        [UnityTest]
        public IEnumerator InitialLoadingSceneCanBeLoaded()
        {
            var loadOperation = SceneManager.LoadSceneAsync(InitialLoadingScene, LoadSceneMode.Additive);

            Assert.That(loadOperation, Is.Not.Null);
            while (!loadOperation.isDone)
            {
                yield return null;
            }

            var loadedScene = SceneManager.GetSceneByName(InitialLoadingScene);
            Assert.That(loadedScene.IsValid(), Is.True);
            Assert.That(loadedScene.isLoaded, Is.True);
        }
    }
}
