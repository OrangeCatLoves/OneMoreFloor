using System;

namespace OneMoreFloor.Game
{
    /// <summary>
    /// Works out how far behind the player the camera should be. A distance of 0 is first person.
    /// Scrolling in past the closest third-person distance becomes first person, and scrolling out
    /// from first person returns to it. Toggling returns to the last third-person distance.
    /// </summary>
    public sealed class ViewZoom
    {
        private const float FirstPersonDistance = 0f;

        private readonly float _closest;
        private readonly float _farthest;
        private readonly float _perNotch;
        private float _lastThirdPerson;

        public ViewZoom(float startDistance, float closestThirdPerson, float farthest, float perNotch)
        {
            if (closestThirdPerson <= 0f)
                throw new ArgumentOutOfRangeException(nameof(closestThirdPerson), closestThirdPerson, "Must be above 0.");
            if (farthest < closestThirdPerson)
                throw new ArgumentOutOfRangeException(nameof(farthest), farthest, "Must not be closer than the closest distance.");

            _closest = closestThirdPerson;
            _farthest = farthest;
            _perNotch = perNotch;
            Distance = Math.Min(Math.Max(startDistance, closestThirdPerson), farthest);
            _lastThirdPerson = Distance;
        }

        /// <summary>The distance the camera should settle at, in metres.</summary>
        public float Distance { get; private set; }

        public bool IsFirstPerson => Distance == FirstPersonDistance;

        /// <summary>Applies scroll wheel movement. Positive notches zoom in, negative notches zoom out.</summary>
        public void Scroll(float notches)
        {
            if (notches == 0f)
                return;

            if (IsFirstPerson)
            {
                if (notches < 0f)
                    SetThirdPerson(_closest);
                return;
            }

            float next = Distance - notches * _perNotch;
            if (next < _closest)
                Distance = FirstPersonDistance;
            else
                SetThirdPerson(Math.Min(next, _farthest));
        }

        /// <summary>Switches between first person and the last third-person distance.</summary>
        public void ToggleView()
        {
            Distance = IsFirstPerson ? _lastThirdPerson : FirstPersonDistance;
        }

        private void SetThirdPerson(float distance)
        {
            Distance = distance;
            _lastThirdPerson = distance;
        }
    }
}
