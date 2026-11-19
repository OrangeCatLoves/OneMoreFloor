using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NUnit.Framework;
using OneMoreFloor.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace OneMoreFloor.Tests.EditMode.RoomChecks
{
    /// <summary>
    /// Checks the Trivia room's build before the owner playtests it: where players can go, whether they can
    /// walk into visible walls, the route from the lift to the podiums, upright screens and brightness.
    /// Each failure lists the offending objects and positions. The room's art is local-only until the owner
    /// approves it, so these tests are skipped where the scene is missing.
    /// They gate each art round, so they don't run with the everyday tests. Run them on request:
    /// open Editor: unity command test run --mode editor --filter RoomChecks --filter-type category --include-explicit true
    /// closed Editor: unity test . --mode EditMode --filter TriviaRoomChecks
    /// </summary>
    [TestFixture, Explicit("Art-round gate: run on request."), Category("RoomChecks")]
    public class TriviaRoomChecks
    {
        private const string ScenePath = "Assets/_OneMoreFloor/Art/Floors/Trivia/Review/TriviaArtReview.unity";
        private const string PlayerPrefabPath = "Assets/_OneMoreFloor/Game/Player/Player.prefab";
        private const string MovementSettingsPath = "Assets/_OneMoreFloor/Data/MovementSettings.asset";
        private const string ReportPath = "Temp/OneMoreFloor/TriviaRoomChecks.txt";

        private const float GridSpacing = 0.25f;
        private const float EyeHeight = 1.6f;

        // Height edges between flat floor spots that jolt the character: above a seam, below a step.
        private const float SmallestLip = 0.005f;
        private const float LargestLip = 0.10f;

        // Live screens must face straight out. Degrees from vertical.
        private const float ScreenTiltLimit = 2f;

        // Average display brightness (0 = black, 1 = white) from the player's views. The owner chose 0.35 on
        // 20 Nov 2026: a well-lit TV studio, about 3x round 2's 0.12. 0 would mean "not agreed yet".
        private const float MinimumAverageBrightness = 0.35f;

        // Visible meshes the player may pass through by design: light beams, haze, glows, decals.
        private static readonly Regex NotSolid = new Regex("Beam|Haze|Glow|Decal|Fx|FX_|Volumetric|Cone", RegexOptions.IgnoreCase);
        private static readonly Regex LiveScreen = new Regex(@"^SM_Trivia_\w*Screen(_\d+)?$");

        private Scene _scene;
        private Scene _previousActive;
        private bool _openedHere;
        private RoomSurvey _survey;
        private PlayerBody _body;
        private readonly StringBuilder _report = new StringBuilder();

        [OneTimeSetUp]
        public void MapTheRoom()
        {
            if (!File.Exists(ScenePath))
                Assert.Ignore($"The Trivia room isn't on this machine ({ScenePath}): its art is local-only.");

            var controller = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath).GetComponent<CharacterController>();
            var movement = AssetDatabase.LoadAssetAtPath<MovementSettings>(MovementSettingsPath);
            _body = new PlayerBody(controller.radius, controller.height, controller.stepOffset, controller.slopeLimit, movement.JumpHeight);

            _previousActive = SceneManager.GetActiveScene();
            Scene alreadyOpen = SceneManager.GetSceneByPath(ScenePath);
            _openedHere = !alreadyOpen.isLoaded;
            _scene = _openedHere ? EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive) : alreadyOpen;
            SceneManager.SetActiveScene(_scene);

            Bounds area = RoomSurvey.ColliderBounds(_scene);
            _survey = new RoomSurvey(_body, GridSpacing);
            _survey.Map(area);
            _survey.Spread(Markers("MK_Spawn_").Select(t => t.position));

            _report.AppendLine($"Trivia room checks. Player radius {_body.Radius} m, height {_body.Height} m, step {_body.StepOffset} m, jump {_body.JumpHeight} m.");
            _report.AppendLine($"Standing spots: {_survey.Spots.Count}; reachable on foot {_survey.ReachableWalking.Count()}, with jumps {_survey.ReachableWithJumps.Count()}.");
            _report.AppendLine(Summarise("Surfaces players can reach (counting jumps)", _survey.ReachableWithJumps.ToList(), spot => spot.Surface));
        }

        [OneTimeTearDown]
        public void CloseTheRoom()
        {
            if (_scene.IsValid())
            {
                if (_previousActive.IsValid() && _previousActive.isLoaded)
                    SceneManager.SetActiveScene(_previousActive);
                if (_openedHere)
                    EditorSceneManager.CloseScene(_scene, true);
            }

            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, _report.ToString());
        }

        private IEnumerable<Transform> Markers(string prefix)
        {
            return RoomSurvey.AllTransforms(_scene).Where(t => t.name.StartsWith(prefix));
        }

        [Test]
        public void TheRoomHasItsMarkers()
        {
            Assert.That(Markers("MK_Spawn_").Count(), Is.EqualTo(4), "Four MK_Spawn markers in the lift.");
            Assert.That(Markers("MK_Objective_").Count(), Is.EqualTo(4), "Four MK_Objective markers at the podiums.");
            Assert.That(Markers("MK_Exit").Count(), Is.EqualTo(1), "One MK_Exit marker at the lift.");
            Assert.That(Markers("MK_PlayArea_").Count(), Is.GreaterThan(0),
                "MK_PlayArea_nn boxes (an empty whose position is the box centre and whose scale is its size in metres) mark where players may go.");
        }

        [Test]
        public void PlayersCanOnlyReachTheApprovedPlayArea()
        {
            List<Bounds> areas = Markers("MK_PlayArea_").Select(t => new Bounds(t.position, t.lossyScale)).ToList();
            Assume.That(areas, Is.Not.Empty, "Needs MK_PlayArea markers (see TheRoomHasItsMarkers).");

            List<FloorSpot> outside = _survey.ReachableWithJumps
                .Where(spot => !areas.Any(area => area.Contains(spot.Position + Vector3.up * 0.1f)))
                .ToList();

            string message = Summarise("Players can reach spots outside every MK_PlayArea box (counting jumps)", outside, spot => spot.Surface);
            _report.AppendLine(message);
            Assert.That(outside, Is.Empty, message);
        }

        [Test]
        public void PlayersCantWalkIntoVisibleWallsOrProps()
        {
            // Copies every visible mesh onto a scratch layer as a collider, then puts the player's body on every
            // spot it can reach. Any overlap means a visible surface has no matching collision.
            var scratch = new List<GameObject>();
            try
            {
                foreach (MeshRenderer renderer in RoomSurvey.AllTransforms(_scene).Select(t => t.GetComponent<MeshRenderer>()).Where(r => r != null && r.enabled && r.gameObject.activeInHierarchy))
                {
                    string name = renderer.name;
                    if (name.StartsWith("COL_") || name.StartsWith("MK_") || NotSolid.IsMatch(name))
                        continue;
                    var filter = renderer.GetComponent<MeshFilter>();
                    if (filter == null || filter.sharedMesh == null)
                        continue;

                    var copy = new GameObject(name) { hideFlags = HideFlags.HideAndDontSave, layer = RoomSurvey.ScratchLayer };
                    copy.transform.SetPositionAndRotation(renderer.transform.position, renderer.transform.rotation);
                    copy.transform.localScale = renderer.transform.lossyScale;
                    copy.AddComponent<MeshCollider>().sharedMesh = filter.sharedMesh;
                    scratch.Add(copy);
                }
                Physics.SyncTransforms();

                var inside = new List<(FloorSpot spot, string mesh)>();
                foreach (FloorSpot spot in _survey.ReachableWithJumps)
                    if (_survey.BodyOverlaps(spot.Position, 1 << RoomSurvey.ScratchLayer, 0.9f, out Collider hit))
                        inside.Add((spot, hit.name));

                string message = Summarise("The player can stand inside these visible meshes (their collision is missing or set back)",
                    inside.Select(i => i.spot).ToList(), spot => inside.First(i => i.spot == spot).mesh);
                _report.AppendLine(message);
                Assert.That(inside, Is.Empty, message);
            }
            finally
            {
                foreach (GameObject copy in scratch)
                    Object.DestroyImmediate(copy);
            }
        }

        [Test]
        public void EveryPodiumCanBeReachedOnFootFromTheLift()
        {
            var missing = new List<string>();
            foreach (Transform objective in Markers("MK_Objective_"))
            {
                int spot = _survey.Nearest(objective.position);
                if (spot < 0 || !_survey.Spots[spot].ReachedWalking)
                    missing.Add($"{objective.name} at {RoomSurvey.Describe(objective.position)}");
                else
                    _report.AppendLine($"{objective.name}: walking route of {_survey.WalkingRouteTo(spot).Count} spots.");
            }

            Assert.That(missing, Is.Empty, "No walking route (steps up to the step height, no jumps) from the lift to: " + string.Join("; ", missing));
        }

        [Test]
        public void TheFloorHasNoSmallLips()
        {
            var lips = _survey.Lips(SmallestLip, LargestLip);
            string message = Summarise($"Small edges of {SmallestLip * 100f:0.#}-{LargestLip * 100f:0} cm between flat floor spots jolt the camera; make them flush, a ramp or a real step",
                lips.Select(l => l.low).ToList(),
                low => { var lip = lips.First(l => l.low == low); return $"{lip.low.Surface} -> {lip.high.Surface} {(lip.high.Position.y - lip.low.Position.y) * 100f:0.0} cm"; });
            _report.AppendLine(message);
            Assert.That(lips, Is.Empty, message);
        }

        [Test]
        public void LiveScreensStandUpright()
        {
            var tilted = new List<string>();
            foreach (MeshFilter filter in RoomSurvey.AllTransforms(_scene).Select(t => t.GetComponent<MeshFilter>()).Where(f => f != null && f.sharedMesh != null && LiveScreen.IsMatch(f.name)))
            {
                Vector3 normal = AverageNormal(filter);
                float tilt = Mathf.Abs(90f - Vector3.Angle(normal, Vector3.up));
                _report.AppendLine($"{filter.name}: faces {normal:F2}, {tilt:F1} degrees from upright.");
                if (tilt > ScreenTiltLimit)
                    tilted.Add($"{filter.name} is tilted {tilt:F1} degrees");
            }

            Assert.That(tilted, Is.Empty, string.Join("; ", tilted));
        }

        [Test]
        public void TheRoomIsBrightEnough()
        {
            Transform exit = Markers("MK_Exit").First();
            Vector3 stage = Markers("MK_Objective_").Aggregate(Vector3.zero, (sum, t) => sum + t.position) / 4f;
            var views = new List<(string name, Vector3 from, Vector3 to)>
            {
                ("lift door to stage", exit.position + Vector3.up * EyeHeight, stage + Vector3.up * 1.2f),
                ("halfway down to the stage", Vector3.Lerp(exit.position, stage, 0.5f) + Vector3.up * EyeHeight, stage + Vector3.up * 1.2f),
            };
            foreach (Transform objective in Markers("MK_Objective_").OrderBy(t => t.name))
                views.Add(($"{objective.name} to audience", objective.position + Vector3.up * EyeHeight, objective.position + Vector3.up * EyeHeight + objective.forward * 10f));

            var lines = new List<string>();
            float total = 0f;
            foreach (var view in views)
            {
                (float average, float dark) = MeasureView(view.from, view.to);
                total += average;
                lines.Add($"{view.name}: brightness {average:F2}, {dark * 100f:F0}% near-black");
            }
            float overall = total / views.Count;
            string message = $"Average brightness {overall:F2} (0 black, 1 white).\n  " + string.Join("\n  ", lines);
            _report.AppendLine(message);

            if (MinimumAverageBrightness <= 0f)
                Assert.Inconclusive("No brightness target agreed with the owner yet. " + message);
            Assert.That(overall, Is.GreaterThanOrEqualTo(MinimumAverageBrightness), message);
        }

        private (float average, float darkShare) MeasureView(Vector3 from, Vector3 to)
        {
            const int Width = 480, Height = 270;
            var template = RoomSurvey.AllTransforms(_scene).Select(t => t.GetComponent<Camera>()).FirstOrDefault(c => c != null && c.CompareTag("MainCamera"));
            var go = new GameObject("BrightnessCamera") { hideFlags = HideFlags.HideAndDontSave };
            var target = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            try
            {
                var camera = go.AddComponent<Camera>();
                if (template != null)
                    camera.CopyFrom(template);
                camera.fieldOfView = 60f;
                camera.transform.SetPositionAndRotation(from, Quaternion.LookRotation(to - from));
                camera.targetTexture = target;
                camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;

                RenderPipeline.SubmitRenderRequest(camera, new RenderPipeline.StandardRequest { destination = target });

                var previous = RenderTexture.active;
                RenderTexture.active = target;
                var pixels = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
                pixels.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                RenderTexture.active = previous;

                float sum = 0f;
                int dark = 0;
                Color32[] data = pixels.GetPixels32();
                foreach (Color32 c in data)
                {
                    float luma = (0.2126f * c.r + 0.7152f * c.g + 0.0722f * c.b) / 255f;
                    sum += luma;
                    if (luma < 0.1f)
                        dark++;
                }
                Object.DestroyImmediate(pixels);
                return (sum / data.Length, (float)dark / data.Length);
            }
            finally
            {
                Object.DestroyImmediate(go);
                target.Release();
                Object.DestroyImmediate(target);
            }
        }

        private static Vector3 AverageNormal(MeshFilter filter)
        {
            Mesh mesh = filter.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Vector3 sum = Vector3.zero;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 a = filter.transform.TransformPoint(vertices[triangles[i]]);
                Vector3 b = filter.transform.TransformPoint(vertices[triangles[i + 1]]);
                Vector3 c = filter.transform.TransformPoint(vertices[triangles[i + 2]]);
                sum += Vector3.Cross(b - a, c - a); // Length = twice the triangle's area.
            }
            return sum.normalized;
        }

        private static string Summarise(string title, List<FloorSpot> spots, System.Func<FloorSpot, string> label)
        {
            if (spots.Count == 0)
                return title + ": none.";

            var lines = spots.GroupBy(label)
                .OrderByDescending(group => group.Count())
                .Take(15)
                .Select(group =>
                {
                    Bounds b = new Bounds(group.First().Position, Vector3.zero);
                    foreach (FloorSpot s in group)
                        b.Encapsulate(s.Position);
                    return $"  {group.Key}: {group.Count()} spots, x {b.min.x:F1} to {b.max.x:F1}, z {b.min.z:F1} to {b.max.z:F1}, floor height {b.min.y:F2} to {b.max.y:F2} m";
                });
            return $"{title}: {spots.Count} spots.\n" + string.Join("\n", lines);
        }
    }
}
