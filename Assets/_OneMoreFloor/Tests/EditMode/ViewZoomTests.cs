using NUnit.Framework;
using OneMoreFloor.Game;

namespace OneMoreFloor.Tests.EditMode
{
    public class ViewZoomTests
    {
        private static ViewZoom NewZoom() => new ViewZoom(startDistance: 4f, closestThirdPerson: 1.5f, farthest: 7f, perNotch: 1f);

        [Test]
        public void StartsInThirdPerson()
        {
            ViewZoom zoom = NewZoom();

            Assert.That(zoom.IsFirstPerson, Is.False);
            Assert.That(zoom.Distance, Is.EqualTo(4f));
        }

        [Test]
        public void ScrollingIn_MovesTheCameraCloser()
        {
            ViewZoom zoom = NewZoom();

            zoom.Scroll(1f);

            Assert.That(zoom.Distance, Is.EqualTo(3f));
            Assert.That(zoom.IsFirstPerson, Is.False);
        }

        [Test]
        public void ScrollingAllTheWayIn_BecomesFirstPerson()
        {
            ViewZoom zoom = NewZoom();

            for (int i = 0; i < 10; i++)
                zoom.Scroll(1f);

            Assert.That(zoom.IsFirstPerson, Is.True);
            Assert.That(zoom.Distance, Is.EqualTo(0f));
        }

        [Test]
        public void ScrollingOut_StopsAtTheFarthestDistance()
        {
            ViewZoom zoom = NewZoom();

            for (int i = 0; i < 10; i++)
                zoom.Scroll(-1f);

            Assert.That(zoom.Distance, Is.EqualTo(7f));
        }

        [Test]
        public void ScrollingOutFromFirstPerson_ReturnsToTheClosestThirdPerson()
        {
            ViewZoom zoom = NewZoom();
            for (int i = 0; i < 10; i++)
                zoom.Scroll(1f);

            zoom.Scroll(-1f);

            Assert.That(zoom.IsFirstPerson, Is.False);
            Assert.That(zoom.Distance, Is.EqualTo(1.5f));
        }

        [Test]
        public void ScrollingInFromFirstPerson_StaysInFirstPerson()
        {
            ViewZoom zoom = NewZoom();
            zoom.ToggleView();

            zoom.Scroll(1f);

            Assert.That(zoom.IsFirstPerson, Is.True);
        }

        [Test]
        public void SmallTrackpadScrolls_AddUp()
        {
            ViewZoom zoom = NewZoom();

            for (int i = 0; i < 10; i++)
                zoom.Scroll(0.1f);

            Assert.That(zoom.Distance, Is.EqualTo(3f).Within(0.001f));
        }

        [Test]
        public void Toggle_SwitchesToFirstPersonAndBackToTheLastDistance()
        {
            ViewZoom zoom = NewZoom();
            zoom.Scroll(-2f);

            zoom.ToggleView();
            Assert.That(zoom.IsFirstPerson, Is.True);

            zoom.ToggleView();
            Assert.That(zoom.Distance, Is.EqualTo(6f));
        }

        [Test]
        public void StartDistance_IsKeptInsideTheAllowedRange()
        {
            Assert.That(new ViewZoom(0.5f, 1.5f, 7f, 1f).Distance, Is.EqualTo(1.5f));
            Assert.That(new ViewZoom(20f, 1.5f, 7f, 1f).Distance, Is.EqualTo(7f));
        }
    }
}
