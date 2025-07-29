using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline: RenderPipeline
{
    private CameraRenderer _renderer = new();

    private bool _useDynamicBatching;
    private bool _useGPUInstancing;
    private bool _useSRPBatcher;
    
    public CustomRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher)
    {
        _useDynamicBatching = useDynamicBatching;
        _useGPUInstancing = useGPUInstancing;
        _useSRPBatcher = useSRPBatcher;
    }
    
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            _renderer.Render(context, camera, _useDynamicBatching, _useGPUInstancing, _useSRPBatcher);
        }
    }
}