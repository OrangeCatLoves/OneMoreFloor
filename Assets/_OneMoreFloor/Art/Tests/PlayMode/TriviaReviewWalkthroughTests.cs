#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using FishNet;
using NUnit.Framework;
using OneMoreFloor.Game;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace OneMoreFloor.ArtReview.Tests
{
    public class TriviaReviewWalkthroughTests
    {
        const string ScenePath = "Assets/_OneMoreFloor/Art/Floors/Trivia/Review/TriviaArtReview.unity";
        const string EvidenceFolder = "art-source/renders/trivia/round-05";

        [UnityTest]
        public IEnumerator LocalHost_CanLeaveLift_ClimbStage_AndInspectBothViewsWithAA()
        {
            if (!File.Exists(ScenePath)) Assert.Ignore("Build the local Trivia art review scene first; binary art is intentionally excluded from Git.");
            var oldBackground = InputSystem.settings.backgroundBehavior;
            var oldEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            var keyboard = InputSystem.AddDevice<Keyboard>();var mouse = InputSystem.AddDevice<Mouse>();
            try
            {
                EditorSceneManager.LoadSceneInPlayMode(ScenePath,new LoadSceneParameters(LoadSceneMode.Single));
                PlayerController player = null;
                float deadline = Time.realtimeSinceStartup + 20;
                while (!player && Time.realtimeSinceStartup < deadline)
                {
                    player = Object.FindObjectsByType<PlayerController>(FindObjectsSortMode.None).FirstOrDefault(p => p.IsLocalPlayer);
                    yield return null;
                }
                Assert.That(player, Is.Not.Null, "The existing local host must spawn its real player.");
                yield return new WaitForSeconds(.8f);
                Assert.That(InstanceFinder.IsServerStarted && InstanceFinder.IsClientStarted, Is.True);
                var pipeline = (UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline;
                Assert.That(pipeline.msaaSampleCount,Is.EqualTo(4));
                Assert.That(Camera.main.GetUniversalAdditionalCameraData().antialiasing,Is.EqualTo(AntialiasingMode.SubpixelMorphologicalAntiAliasing));
                var probes=Object.FindObjectsByType<ReflectionProbe>(FindObjectsSortMode.None);
                Assert.That(probes.Length,Is.EqualTo(2));
                Assert.That(probes.All(p => p.mode==ReflectionProbeMode.Custom && p.customBakedTexture),Is.True,"Baked cubemap assignments must survive loading the saved scene.");
                Assert.That(LightmapSettings.lightmaps.Length,Is.GreaterThan(0),"Baked room lighting must survive loading the saved scene.");
                Assert.That(LightmapSettings.lightProbes.bakedProbes.Length,Is.EqualTo(113),"The player and moving props need the baked probe volume.");
                Assert.That(RenderSettings.customReflectionTexture,Is.Not.Null,"Outside probe bounds must use the indoor fallback.");

                // Exercise real input through the existing controller on exit.
                float originalX = player.transform.position.x;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W));
                yield return new WaitForSeconds(1.05f);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                yield return new WaitForSeconds(.2f);
                Assert.That(player.transform.position.x,Is.LessThan(originalX-3),"Open lift doors must not block walking out.");

                // Then use the same motor for a deterministic collision route.
                // Camera feel remains the owner's playtest, not an assertion.
                player.enabled = false;
                var motor = player.GetComponent<PlayerMotor>();
                IEnumerator WalkTo(Vector3 target)
                {
                    float until = Time.realtimeSinceStartup + 25;
                    while (Vector2.Distance(new Vector2(player.transform.position.x,player.transform.position.z),new Vector2(target.x,target.z)) > .18f && Time.realtimeSinceStartup < until)
                    {
                        Vector3 delta = target-player.transform.position;delta.y=0;
                        Vector3 direction = delta.normalized;
                        motor.Tick(new MoveCommand(new Vector2(direction.x,direction.z),false,0,false),Mathf.Clamp(Time.deltaTime,1f/120,.05f));
                        yield return null;
                    }
                    Assert.That(Vector2.Distance(new Vector2(player.transform.position.x,player.transform.position.z),new Vector2(target.x,target.z)),Is.LessThan(.3f),"Collision route blocked toward " + target);
                }
                yield return WalkTo(new Vector3(11.2f,0,3));
                yield return WalkTo(new Vector3(11.2f,0,6.4f));
                yield return WalkTo(new Vector3(8.9f,0,6.4f));
                yield return WalkTo(new Vector3(8.9f,.6f,3.8f));
                Assert.That(player.transform.position.y,Is.InRange(.55f,.75f),"Player must climb the three corner steps.");
                yield return WalkTo(new Vector3(6,.6f,1.45f));
                foreach(float x in new[]{4.8f,1.6f,-1.6f,-4.8f})yield return WalkTo(new Vector3(x,.6f,1.45f));
                yield return WalkTo(new Vector3(0,.6f,1.45f));
                player.enabled = true;
                InputSystem.QueueStateEvent(mouse,new MouseState {delta=new Vector2(900,0)});
                yield return null;yield return null;
                InputSystem.QueueStateEvent(mouse,new MouseState());
                yield return new WaitForSeconds(.6f);
                Capture("unity-playtest-third-person");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.V));
                yield return null;yield return null;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                yield return new WaitForSeconds(.6f);
                Assert.That(player.Zoom.IsFirstPerson,Is.True);
                Capture("unity-playtest-first-person");
                // Check every seating-row passage, both side aisles, the rear
                // control position, both gallery stairs and the continuous gallery.
                player.enabled=false;
                yield return WalkTo(new Vector3(6,.6f,1.45f));
                yield return WalkTo(new Vector3(8.9f,.6f,3.8f));
                yield return WalkTo(new Vector3(8.9f,0,6.4f));
                yield return WalkTo(new Vector3(0,0,6.4f));
                for(int row=0;row<7;row++)
                {
                    float z=9.15f+row*2;
                    yield return WalkTo(new Vector3(0,row*.2f,z));
                    yield return WalkTo(new Vector3(9.95f,row*.2f,z));
                    yield return WalkTo(new Vector3(-9.95f,row*.2f,z));
                    yield return WalkTo(new Vector3(0,row*.2f,z));
                }
                yield return WalkTo(new Vector3(0,1.2f,22.3f));
                foreach(float side in new[]{-1f,1f})
                {
                    yield return WalkTo(new Vector3(side*9.95f,1.2f,22.3f));
                    yield return WalkTo(new Vector3(side*9.95f,0,7.1f));
                    yield return WalkTo(new Vector3(side*9.95f,.4f,13.2f));
                    yield return WalkTo(new Vector3(side*12.8f,.4f,13.2f));
                    yield return WalkTo(new Vector3(side*9.95f,.4f,13.2f));
                    yield return WalkTo(new Vector3(side*9.95f,1.2f,22.3f));
                    yield return WalkTo(new Vector3(0,1.2f,22.3f));
                }
                yield return WalkTo(new Vector3(3,1.2f,22.3f));
                yield return WalkTo(new Vector3(3,1.2f,25.1f));
                yield return WalkTo(new Vector3(0,1.2f,25.1f));
                Capture("unity-walk-control");
                foreach(float side in new[]{1f,-1f})
                {
                    yield return WalkTo(new Vector3(side*9.9f,1.2f,25.1f));
                    yield return WalkTo(new Vector3(side*12.45f,1.2f,25.1f));
                    yield return WalkTo(new Vector3(side*12.45f,4.8f,18.6f));
                    Assert.That(player.transform.position.y,Is.InRange(4.75f,4.95f),"Gallery staircase must support the player.");
                    yield return WalkTo(new Vector3(side*13.3f,4.8f,16));
                    yield return WalkTo(new Vector3(side*13.3f,4.8f,0));
                    yield return WalkTo(new Vector3(side*13.3f,4.8f,16));
                    yield return WalkTo(new Vector3(side*10.1f,4.8f,19));
                    yield return WalkTo(new Vector3(side*10.1f,4.8f,24.6f));
                    yield return WalkTo(new Vector3(0,4.8f,24.6f));
                    Capture("unity-walk-gallery-"+side);
                    yield return WalkTo(new Vector3(side*10.1f,4.8f,24.6f));
                    yield return WalkTo(new Vector3(side*10.1f,4.8f,18.6f));
                    yield return WalkTo(new Vector3(side*12.45f,4.8f,18.6f));
                    yield return WalkTo(new Vector3(side*12.45f,1.2f,25.1f));
                    yield return WalkTo(new Vector3(side*9.9f,1.2f,25.1f));
                    yield return WalkTo(new Vector3(0,1.2f,25.1f));
                }
                File.WriteAllText("art-source/trivia/walkthrough-validation.json", "{\"local_host\":true,\"lift_exit_input\":true,\"corner_steps_walked\":true,\"four_player_positions\":true,\"all_seven_row_passages\":true,\"rear_control\":true,\"both_gallery_stairs_and_side_galleries\":true,\"first_and_third_person_captured\":true,\"msaa_samples\":4,\"human_comfort_and_frame_rate\":\"Owner playtest pending\"}");
            }
            finally
            {
                InputSystem.RemoveDevice(keyboard);InputSystem.RemoveDevice(mouse);
                InputSystem.settings.backgroundBehavior=oldBackground;InputSystem.settings.editorInputBehaviorInPlayMode=oldEditorInput;
                if (InstanceFinder.NetworkManager)
                {
                    var network=InstanceFinder.NetworkManager.gameObject;
                    InstanceFinder.ClientManager.StopConnection();InstanceFinder.ServerManager.StopConnection(true);
                    Object.Destroy(network);
                }
                // Unload the quality override synchronously so following tests
                // run with the project's original pipeline, even after a failure.
                GameObject.Find("TriviaReviewQuality")?.SetActive(false);
                var empty = SceneManager.CreateScene("AfterTriviaArtReview");SceneManager.SetActiveScene(empty);
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneByPath(ScenePath));
            }
        }

        static void Capture(string name)
        {
            Directory.CreateDirectory(EvidenceFolder);
            var render = new RenderTexture(1280,720,24,RenderTextureFormat.ARGB32) {antiAliasing=4};render.Create();
            var resolved = new RenderTexture(1280,720,0,RenderTextureFormat.ARGB32);resolved.Create();
            var previous = RenderTexture.active;
            bool asyncCompilation = ShaderUtil.allowAsyncCompilation;
            try
            {
                // The first render request introduces offscreen/MSAA shader
                // variants. Finish their compilation before recording evidence.
                ShaderUtil.allowAsyncCompilation = false;
                RenderPipeline.SubmitRenderRequest(Camera.main,new UniversalRenderPipeline.SingleCameraRequest {destination=render});
                RenderPipeline.SubmitRenderRequest(Camera.main,new UniversalRenderPipeline.SingleCameraRequest {destination=render});
                Graphics.Blit(render,resolved);RenderTexture.active=resolved;
                var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
                image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
                File.WriteAllBytes(EvidenceFolder+"/"+name+".png",image.EncodeToPNG());Object.Destroy(image);
            }
            finally {ShaderUtil.allowAsyncCompilation=asyncCompilation;RenderTexture.active=previous;render.Release();resolved.Release();Object.Destroy(render);Object.Destroy(resolved);}
        }
    }
}
#endif
