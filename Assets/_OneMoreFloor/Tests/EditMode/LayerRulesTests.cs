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
    }
}
