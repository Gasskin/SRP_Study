using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline: RenderPipeline
{
    private CameraRenderer _renderer = new();

    private bool _useDynamicBatching;
    private bool _useGPUInstancing;
    private bool _useSrpBatcher;
    private ShadowSettings _shadows;
    
    public CustomRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSrpBatcher, ShadowSettings shadows)
    {
        _useDynamicBatching = useDynamicBatching;
        _useGPUInstancing = useGPUInstancing;
        _useSrpBatcher = useSrpBatcher;
        _shadows = shadows;
        GraphicsSettings.lightsUseLinearIntensity = true;
    }
    
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            _renderer.Render(context, camera, _useDynamicBatching, _useGPUInstancing, _useSrpBatcher, _shadows);
        }
    }
}