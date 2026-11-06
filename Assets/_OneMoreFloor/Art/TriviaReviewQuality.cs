using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace OneMoreFloor.ArtReview
{
    // Presentation settings for the art walk-through. Uses the existing game's
    // player/network/input code; it does not implement floor gameplay.
    public sealed class TriviaReviewQuality : MonoBehaviour
    {
        RenderPipelineAsset previous;
        UniversalRenderPipelineAsset reviewPipeline;
        int previousSamples;

        void OnEnable()
        {
            previous = QualitySettings.renderPipeline;
            previousSamples = QualitySettings.antiAliasing;
            reviewPipeline = Instantiate((UniversalRenderPipelineAsset)GraphicsSettings.currentRenderPipeline);
            reviewPipeline.name = "Trivia_Review_4xMSAA";
            reviewPipeline.msaaSampleCount = 4;
            reviewPipeline.renderScale = 1;
            QualitySettings.renderPipeline = reviewPipeline;
        }

        void OnDisable()
        {
            if (QualitySettings.renderPipeline == reviewPipeline)
            {
                QualitySettings.renderPipeline = previous;
                QualitySettings.antiAliasing = previousSamples;
            }
            if (reviewPipeline) Destroy(reviewPipeline);
        }
    }
}
