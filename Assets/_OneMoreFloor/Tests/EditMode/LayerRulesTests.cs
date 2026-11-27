using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace OneMoreFloor.Tests.EditMode
{
    public class LayerRulesTests
    {
        // Core holds the game rules as plain C#, so it must never depend on Unity or networking.
        [Test]
        public void Core_DoesNotReferenceUnityOrFishNet()
        {
            string[] forbidden = Assembly.Load("OneMoreFloor.Core")
                .GetReferencedAssemblies()
                .Select(reference => reference.Name)
                .Where(name => name.StartsWith("Unity") || name.StartsWith("FishNet"))
                .ToArray();

            Assert.That(forbidden, Is.Empty);
        }

        // A floor's rules are plain C# with unit tests, like Core.
        [Test]
        public void FloorRules_DoNotReferenceUnityOrFishNet()
        {
            string[] forbidden = Assembly.Load("OneMoreFloor.Floors.Trivia.Rules")
                .GetReferencedAssemblies()
                .Select(reference => reference.Name)
                .Where(name => name.StartsWith("Unity") || name.StartsWith("FishNet"))
                .ToArray();

            Assert.That(forbidden, Is.Empty);
        }

        // Floors talk to the game only through the Floor contract (FloorSDK), and never to each other.
        [Test]
        public void Floors_DoNotReferenceTheGameOrOtherFloors()
        {
            string[] forbidden = Assembly.Load("OneMoreFloor.Floors.Trivia")
                .GetReferencedAssemblies()
                .Select(reference => reference.Name)
                .Where(name => name == "OneMoreFloor.Game" || name == "OneMoreFloor.Dev"
                    || (name.StartsWith("OneMoreFloor.Floors.") && !name.StartsWith("OneMoreFloor.Floors.Trivia")))
                .ToArray();

            Assert.That(forbidden, Is.Empty);
        }

        // The Floor Tester is for development only: the game never depends on it, and release builds leave it out.
        [Test]
        public void TheFloorTester_IsLeftOutOfReleaseBuilds()
        {
            string[] gameReferences = Assembly.Load("OneMoreFloor.Game").GetReferencedAssemblies().Select(reference => reference.Name).ToArray();
            Assert.That(gameReferences, Has.No.Member("OneMoreFloor.Dev"));

            string asmdef = System.IO.File.ReadAllText("Assets/_OneMoreFloor/Dev/OneMoreFloor.Dev.asmdef");
            Assert.That(asmdef, Does.Contain("\"UNITY_EDITOR || DEVELOPMENT_BUILD\""));
        }
    }
}
