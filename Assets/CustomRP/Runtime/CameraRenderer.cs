using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

public partial class CameraRenderer
{
    private ScriptableRenderContext _context;
    private Camera _camera;
    private CommandBuffer _buffer = new();

    private CullingResults _cullingResults;

    private static ShaderTagId _unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");


    private bool _useDynamicBatching;
    private bool _useGPUInstancing;
    private bool _useSRPBatcher;

    public void Render(ScriptableRenderContext context, Camera camera, bool useDynamicBatching, bool useGPUInstancing, bool useSRPBatcher)
    {
        _context = context;
        _camera = camera;
        _useDynamicBatching = useDynamicBatching;
        _useGPUInstancing = useGPUInstancing;
        _useSRPBatcher = useSRPBatcher;
        
        GraphicsSettings.useScriptableRenderPipelineBatching = _useSRPBatcher;

        PrepareBuff();
        PrepareForSceneWindow();

        if (!Cull())
        {
            return;
        }

        Setup();
        DrawVisibleGeometry();
        DrawUnsupportedShaders();
        DrawGizoms();
        Submit();
    }


    private bool Cull()
    {
        if (_camera.TryGetCullingParameters(out var p))
        {
            _cullingResults = _context.Cull(ref p);
            return true;
        }
        return false;
    }


    private void Setup()
    {
        _context.SetupCameraProperties(_camera);
        // _buffer.BeginSample(BUFFER_NAME);
        var flags = _camera.clearFlags;
        _buffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, flags == CameraClearFlags.Color,
            flags == CameraClearFlags.Color ? _camera.backgroundColor.linear : Color.clear);
        ExecuteBuffer();
    }


    private void DrawVisibleGeometry()
    {
        // 天空盒
        var skyBoxRendererList = _context.CreateSkyboxRendererList(_camera);
        _buffer.DrawRendererList(skyBoxRendererList);
        
        // 不透明物体
        var sortingOpaque = new SortingSettings(_camera) { criteria = SortingCriteria.CommonOpaque };
        var drawingSettingsOpaque = new DrawingSettings(_unlitShaderTagId, sortingOpaque)
        {
            enableInstancing = _useGPUInstancing,
            enableDynamicBatching = _useDynamicBatching,
        };
        var filteringOpaque = new FilteringSettings(RenderQueueRange.opaque);
        
        var opaqueParams = new RendererListParams(_cullingResults, drawingSettingsOpaque, filteringOpaque);
        var opaqueRendererList = _context.CreateRendererList(ref opaqueParams);
        _buffer.DrawRendererList(opaqueRendererList);
        
        // 透明物体
        var sortingTransparent = new SortingSettings(_camera) { criteria = SortingCriteria.CommonTransparent };
        var drawingSettingsTransparent = new DrawingSettings(_unlitShaderTagId, sortingTransparent)
        {
            enableInstancing = true,
            enableDynamicBatching = true
        };
        var filteringTransparent = new FilteringSettings(RenderQueueRange.transparent);
        var transparentParams = new RendererListParams(_cullingResults,drawingSettingsTransparent, filteringTransparent);
        var transparentRendererList = _context.CreateRendererList(ref transparentParams);
        _buffer.DrawRendererList(transparentRendererList);

        /*// 不透明物体
        var opaqueRendererListDesc = new RendererListDesc(_unlitShaderTagId, _cullingResults, _camera)
        {
            // 绘制顺序
            sortingCriteria = SortingCriteria.CommonOpaque,
            // 渲染队列
            renderQueueRange = RenderQueueRange.opaque,
        };
        // 透明物体
        var transparentRendererListDesc = new RendererListDesc(_unlitShaderTagId, _cullingResults, _camera)
        {
            // 绘制顺序
            sortingCriteria = SortingCriteria.CommonTransparent,
            // 渲染队列
            renderQueueRange = RenderQueueRange.transparent,
        };
        // 1.不透明
        var opaqueRendererList = _context.CreateRendererList(opaqueRendererListDesc);
        _buffer.DrawRendererList(opaqueRendererList);
        // 2.天空盒
        var skyBoxRendererList = _context.CreateSkyboxRendererList(_camera);
        _buffer.DrawRendererList(skyBoxRendererList);
        // 3.透明物体
        var transparentRendererList = _context.CreateRendererList(transparentRendererListDesc);
        _buffer.DrawRendererList(transparentRendererList);*/
        
        ExecuteBuffer();
    }


    private void Submit()
    {
        // _buffer.EndSample(BUFFER_NAME);
        ExecuteBuffer();
        _context.Submit();
    }

    private void ExecuteBuffer()
    {
        _context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }
}