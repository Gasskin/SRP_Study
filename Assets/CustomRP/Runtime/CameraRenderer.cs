using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

public partial class CameraRenderer
{
    private ScriptableRenderContext _context;
    private Camera _camera;
    private CommandBuffer _buffer = new();
    private ShadowSettings _shadows;

    private CullingResults _cullingResults;

    private static ShaderTagId _unlitShaderTagId = new("SRPDefaultUnlit");
    private static ShaderTagId _litShaderTagId = new("CustomLit");

    private bool _useDynamicBatching;
    private bool _useGPUInstancing;

    private Lighting _lighting = new();

    public void Render(ScriptableRenderContext context, Camera camera, bool useDynamicBatching, bool useGPUInstancing, bool useSrpBatcher, ShadowSettings shadows)
    {
        _context = context;
        _camera = camera;
        _useDynamicBatching = useDynamicBatching;
        _useGPUInstancing = useGPUInstancing;
        _shadows = shadows;

        GraphicsSettings.useScriptableRenderPipelineBatching = useSrpBatcher;

        PrepareBuff();
        PrepareForSceneWindow();

        if (!Cull(_shadows.MaxDistance))
        {
            return;
        }
        _buffer.BeginSample(_buffer.name);
        ExecuteBuffer();
        _lighting.Setup(context, _cullingResults, _shadows);
        _buffer.EndSample(_buffer.name);
        Setup();
        DrawVisibleGeometry();
        DrawUnsupportedShaders();
        DrawGizoms();
        _lighting.Cleanup();
        Submit();
    }


    private bool Cull(float shadowsMaxDistance)
    {
        if (_camera.TryGetCullingParameters(out var p))
        {
            p.shadowDistance = Mathf.Min(shadowsMaxDistance, _camera.farClipPlane);
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
        var drawingSettingsOpaque = new DrawingSettings();
        drawingSettingsOpaque.sortingSettings = new SortingSettings(_camera) { criteria = SortingCriteria.CommonOpaque };
        drawingSettingsOpaque.enableInstancing = _useGPUInstancing;
        drawingSettingsOpaque.enableDynamicBatching = _useDynamicBatching;
        drawingSettingsOpaque.SetShaderPassName(0, _litShaderTagId);
        drawingSettingsOpaque.SetShaderPassName(1, _unlitShaderTagId);

        var opaqueParams = new RendererListParams(_cullingResults, drawingSettingsOpaque, new FilteringSettings(RenderQueueRange.opaque));
        var opaqueRendererList = _context.CreateRendererList(ref opaqueParams);
        _buffer.DrawRendererList(opaqueRendererList);

        // 透明物体
        var drawingSettingsTransparent = new DrawingSettings();
        drawingSettingsTransparent.sortingSettings = new SortingSettings(_camera) { criteria = SortingCriteria.CommonTransparent };
        drawingSettingsTransparent.enableInstancing = _useGPUInstancing;
        drawingSettingsTransparent.enableDynamicBatching = _useDynamicBatching;
        drawingSettingsTransparent.SetShaderPassName(0, _litShaderTagId);
        drawingSettingsTransparent.SetShaderPassName(1, _unlitShaderTagId);

        var transparentParams = new RendererListParams(_cullingResults, drawingSettingsTransparent, new FilteringSettings(RenderQueueRange.transparent));
        var transparentRendererList = _context.CreateRendererList(ref transparentParams);
        _buffer.DrawRendererList(transparentRendererList);

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