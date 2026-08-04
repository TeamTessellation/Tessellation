using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace Tessellation.Tests.EditMode
{
    public class ProjectConfigurationTests
    {
        private const string ExpectedFirstScene = "Assets/Scenes/InitialLoadingScene.unity";
        private const string ExpectedApplicationId = "com.tessellation.honeycomb0";

        [Test]
        public void EnabledBuildScenesExistAndStartWithInitialLoadingScene()
        {
            var enabledScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .ToArray();

            Assert.That(enabledScenes, Is.Not.Empty, "At least one build scene must be enabled.");
            Assert.That(enabledScenes[0].path, Is.EqualTo(ExpectedFirstScene));

            foreach (var scene in enabledScenes)
            {
                var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
                Assert.That(sceneAsset, Is.Not.Null, $"Build scene is missing: {scene.path}");
            }
        }

        [Test]
        public void AndroidApplicationIdentifierMatchesPlayConsolePackage()
        {
            var applicationId = PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android);

            Assert.That(applicationId, Is.EqualTo(ExpectedApplicationId));
        }

        [Test]
        public void AndroidBuildTargetsArm64Only()
        {
            Assert.That(PlayerSettings.Android.targetArchitectures, Is.EqualTo(AndroidArchitecture.ARM64));
        }
    }
}
