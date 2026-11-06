using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OneMoreFloor.ArtReview
{
    // Review rendering only: never writes to the project's shared pipeline assets.
    public sealed class TriviaReviewRenderScope : IDisposable
    {
        readonly RenderPipelineAsset previous;
        readonly int previousSamples;
        public UniversalRenderPipelineAsset Pipeline { get; }

        public TriviaReviewRenderScope()
        {
            previous = QualitySettings.renderPipeline;
            previousSamples = QualitySettings.antiAliasing;
            Pipeline = UnityEngine.Object.Instantiate((UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline);
            Pipeline.name = "Trivia_Transient_4xMSAA";
            Pipeline.msaaSampleCount = 4;
            Pipeline.renderScale = 1;
            QualitySettings.renderPipeline = Pipeline;
        }

        public static void ConfigureCamera(Camera camera)
        {
            camera.allowHDR = true;
            camera.allowMSAA = true;
            camera.allowDynamicResolution = false;
            var data = camera.GetUniversalAdditionalCameraData();
            data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            data.antialiasingQuality = AntialiasingQuality.High;
        }

        public void Dispose()
        {
            QualitySettings.renderPipeline = previous;
            QualitySettings.antiAliasing = previousSamples;
            UnityEngine.Object.DestroyImmediate(Pipeline);
        }
    }
}
