using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/CreateCustomRenderPipeline")]
public class CustomRenderPipelineAsset : RenderPipelineAsset
{
    [SerializeField]
    private bool _useDynamicBatching = true;
    [SerializeField]
    private bool _useGPUInstancing = true;
    [SerializeField]
    private bool _useSrpBatcher = true;
    [SerializeField]
    private ShadowSettings _shadows;

    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(_useDynamicBatching, _useGPUInstancing, _useSrpBatcher, _shadows);
    }
}