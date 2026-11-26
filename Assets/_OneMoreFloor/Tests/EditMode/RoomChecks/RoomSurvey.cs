using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace OneMoreFloor.Tests.EditMode.RoomChecks
{
    /// <summary>The player's collision size and movement limits, read from the real player prefab and settings.</summary>
    public readonly struct PlayerBody
    {
        public readonly float Radius;
        public readonly float Height;
        public readonly float StepOffset;
        public readonly float SlopeLimit;
        public readonly float JumpHeight;

        public PlayerBody(float radius, float height, float stepOffset, float slopeLimit, float jumpHeight)
        {
            Radius = radius;
            Height = height;
            StepOffset = stepOffset;
            SlopeLimit = slopeLimit;
            JumpHeight = jumpHeight;
        }
    }

    /// <summary>One spot on a floor surface where the player can stand.</summary>
    public sealed class FloorSpot
    {
        public Vector3 Position;
        public Vector3 Normal;
        public string Surface;
        public int Column;
        public bool ReachedWalking;
        public bool ReachedWithJumps;
        public int WalkingParent = -1;
    }

    /// <summary>
    /// Maps every spot a player can stand in a room, and which of them they can reach from the start
    /// markers: on foot (steps up to the character's step height, any drop), and with jumps.
    /// Uses the room's physics colliders only, the same ones the character controller collides with.
    /// </summary>
    public sealed class RoomSurvey
    {
        public const int ScratchLayer = 31;

        private readonly PlayerBody _body;
        private readonly float _spacing;
        private readonly int _mask;
        private int _columnsX;
        private int _columnsZ;
        private Vector3 _origin;
        private readonly List<FloorSpot> _spots = new List<FloorSpot>();
        private readonly Dictionary<int, List<int>> _spotsByColumn = new Dictionary<int, List<int>>();

        public RoomSurvey(PlayerBody body, float spacing)
        {
            _body = body;
            _spacing = spacing;
            _mask = ~((1 << ScratchLayer) | (1 << 2) | (1 << LayerMask.NameToLayer("Player")));
        }

        public IReadOnlyList<FloorSpot> Spots => _spots;
        public float Spacing => _spacing;

        public IEnumerable<FloorSpot> ReachableWalking => _spots.Where(spot => spot.ReachedWalking);
        public IEnumerable<FloorSpot> ReachableWithJumps => _spots.Where(spot => spot.ReachedWithJumps);

        public void Map(Bounds area)
        {
            Physics.SyncTransforms();
            _origin = new Vector3(area.min.x, 0f, area.min.z);
            _columnsX = Mathf.CeilToInt(area.size.x / _spacing) + 1;
            _columnsZ = Mathf.CeilToInt(area.size.z / _spacing) + 1;
            float top = area.max.y + 1f;
            float minUp = Mathf.Cos(_body.SlopeLimit * Mathf.Deg2Rad);

            for (int ix = 0; ix < _columnsX; ix++)
            for (int iz = 0; iz < _columnsZ; iz++)
            {
                int column = ix * _columnsZ + iz;
                var from = new Vector3(_origin.x + ix * _spacing, top, _origin.z + iz * _spacing);
                RaycastHit[] hits = Physics.RaycastAll(from, Vector3.down, area.size.y + 2f, _mask, QueryTriggerInteraction.Ignore);
                foreach (RaycastHit hit in hits.OrderByDescending(h => h.point.y))
                {
                    if (hit.normal.y < minUp || !CanStand(hit.point))
                        continue;
                    // A surface with another one lying on it (a sill under a threshold, a floor under a
                    // thin slab) is covered: no foot fits in between.
                    float y = hit.point.y;
                    if (hits.Any(other => other.point.y > y + .001f && other.point.y <= y + _body.StepOffset))
                        continue;
                    if (_spotsByColumn.TryGetValue(column, out List<int> existing) && existing.Any(i => Mathf.Abs(_spots[i].Position.y - hit.point.y) < 0.05f))
                        continue;

                    if (!_spotsByColumn.ContainsKey(column))
                        _spotsByColumn[column] = new List<int>();
                    _spotsByColumn[column].Add(_spots.Count);
                    _spots.Add(new FloorSpot { Position = hit.point, Normal = hit.normal, Surface = hit.collider.name, Column = column });
                }
            }
        }

        public bool CanStand(Vector3 feet, float radiusScale = 0.95f, int mask = 0)
        {
            float radius = _body.Radius * radiusScale;
            // The lower sphere starts at step height, so steps and small edges don't count as blocking.
            Vector3 low = feet + Vector3.up * (_body.StepOffset + _body.Radius);
            Vector3 high = feet + Vector3.up * (_body.Height - _body.Radius);
            return !Physics.CheckCapsule(low, high, radius, mask == 0 ? _mask : mask, QueryTriggerInteraction.Ignore);
        }

        /// <summary>Overlap test for the player's body against an arbitrary layer mask (for example the scratch layer).</summary>
        public bool BodyOverlaps(Vector3 feet, int mask, float radiusScale, out Collider hit)
        {
            float radius = _body.Radius * radiusScale;
            Vector3 low = feet + Vector3.up * (_body.StepOffset + _body.Radius);
            Vector3 high = feet + Vector3.up * (_body.Height - _body.Radius);
            Collider[] found = Physics.OverlapCapsule(low, high, radius, mask, QueryTriggerInteraction.Ignore);
            hit = found.Length > 0 ? found[0] : null;
            return found.Length > 0;
        }

        public void Spread(IEnumerable<Vector3> starts)
        {
            foreach (bool withJumps in new[] { false, true })
            {
                var queue = new Queue<int>();
                foreach (Vector3 start in starts)
                {
                    int nearest = Nearest(start);
                    if (nearest < 0)
                        continue;
                    Mark(nearest, withJumps, -1);
                    queue.Enqueue(nearest);
                }

                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();
                    FloorSpot spot = _spots[current];
                    int ix = spot.Column / _columnsZ, iz = spot.Column % _columnsZ;
                    foreach ((int dx, int dz) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                    {
                        int nx = ix + dx, nz = iz + dz;
                        if (nx < 0 || nz < 0 || nx >= _columnsX || nz >= _columnsZ)
                            continue;
                        if (!_spotsByColumn.TryGetValue(nx * _columnsZ + nz, out List<int> neighbours))
                            continue;

                        foreach (int next in neighbours)
                        {
                            if (IsReached(next, withJumps))
                                continue;
                            float rise = _spots[next].Position.y - spot.Position.y;
                            float limit = withJumps ? Mathf.Max(_body.StepOffset, _body.JumpHeight) : _body.StepOffset;
                            if (rise > limit + 0.01f)
                                continue;
                            Mark(next, withJumps, current);
                            queue.Enqueue(next);
                        }
                    }
                }
            }
        }

        public int Nearest(Vector3 point)
        {
            int best = -1;
            float bestDistance = 1f;
            for (int i = 0; i < _spots.Count; i++)
            {
                float d = Vector3.Distance(_spots[i].Position, point);
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = i;
                }
            }
            return best;
        }

        public List<FloorSpot> WalkingRouteTo(int target)
        {
            var route = new List<FloorSpot>();
            for (int i = target; i >= 0; i = _spots[i].WalkingParent)
                route.Add(_spots[i]);
            route.Reverse();
            return route;
        }

        /// <summary>
        /// Small height edges between neighbouring flat spots players walk on: too low to be a step, high
        /// enough to jolt the character. A ramp isn't an edge: its surface is sloped. Spots reachable only by
        /// jumping (the tops of seat rows, say) don't count.
        /// </summary>
        public List<(FloorSpot low, FloorSpot high)> Lips(float min, float max)
        {
            var lips = new List<(FloorSpot, FloorSpot)>();
            foreach (FloorSpot spot in _spots.Where(s => s.ReachedWalking))
            {
                int ix = spot.Column / _columnsZ, iz = spot.Column % _columnsZ;
                foreach ((int dx, int dz) in new[] { (1, 0), (0, 1) })
                {
                    if (ix + dx >= _columnsX || iz + dz >= _columnsZ)
                        continue;
                    if (!_spotsByColumn.TryGetValue((ix + dx) * _columnsZ + iz + dz, out List<int> neighbours))
                        continue;
                    foreach (int n in neighbours)
                    {
                        FloorSpot other = _spots[n];
                        if (!other.ReachedWalking)
                            continue;
                        float d = Mathf.Abs(other.Position.y - spot.Position.y);
                        bool bothFlat = spot.Normal.y > 0.999f && other.Normal.y > 0.999f;
                        if (bothFlat && d >= min && d <= max)
                            lips.Add(spot.Position.y < other.Position.y ? (spot, other) : (other, spot));
                    }
                }
            }
            return lips;
        }

        private bool IsReached(int index, bool withJumps) => withJumps ? _spots[index].ReachedWithJumps : _spots[index].ReachedWalking;

        private void Mark(int index, bool withJumps, int parent)
        {
            if (withJumps)
            {
                _spots[index].ReachedWithJumps = true;
            }
            else
            {
                _spots[index].ReachedWalking = true;
                _spots[index].WalkingParent = parent;
            }
        }

        public static IEnumerable<Transform> AllTransforms(Scene scene)
        {
            return scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true));
        }

        public static Bounds ColliderBounds(Scene scene)
        {
            Collider[] colliders = AllTransforms(scene).SelectMany(t => t.GetComponents<Collider>()).Where(c => c.enabled && !c.isTrigger).ToArray();
            if (colliders.Length == 0)
                throw new InvalidOperationException("The room has no colliders.");
            Bounds bounds = colliders[0].bounds;
            foreach (Collider collider in colliders)
                bounds.Encapsulate(collider.bounds);
            return bounds;
        }

        public static string Describe(Vector3 p) => $"({p.x:F1}, {p.y:F2}, {p.z:F1})";
    }
}
