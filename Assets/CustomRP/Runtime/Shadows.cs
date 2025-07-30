using UnityEngine;
using UnityEngine.Rendering;

public struct ShadowedDirectionalLight
{
    public int VisibleLightIndex;
}

public class Shadows
{
    private static int _dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");

    // 阴影光源数量
    private const int MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT = 1;
    private const string BUFFER_NAME = "Shadows";

    private CommandBuffer _buffer = new CommandBuffer
    {
        name = BUFFER_NAME
    };

    private ScriptableRenderContext _context;

    private CullingResults _cullingResults;

    private ShadowSettings _settings;

    private ShadowedDirectionalLight[] _shadowedDirectionalLights = new ShadowedDirectionalLight[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT];

    private int _shadowedDirectionalLightCount;

    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings settings)
    {
        _context = context;
        _cullingResults = cullingResults;
        _settings = settings;
        _shadowedDirectionalLightCount = 0;
    }

    public void ReserveDirectionalShadows(Light lighting, int visibleLightIndex)
    {
        if (lighting.shadows == LightShadows.None && lighting.shadowStrength <= 0f)
        {
            return;
        }
        if (!_cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
        {
            return;
        }
        if (_shadowedDirectionalLightCount < MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT)
        {
            _shadowedDirectionalLights[_shadowedDirectionalLightCount++] = new ShadowedDirectionalLight { VisibleLightIndex = visibleLightIndex };
        }
    }

    public void Render()
    {
        if (_shadowedDirectionalLightCount > 0)
        {
            RenderDirectionalShadows();
        }
        //对于WebGL 2.0来说，不领取纹理会出问题，因为它把纹理和采样器绑定在了一起
        //为了避免这种情况，我们可以引入一个着色器关键字，生成着色器变体跳过阴影采样代码
        //另一个替代的办法是当不需要阴影时，获取一个1×1的空纹理来避免额外的着色器变体。我们就用这种方法
        else
        {
            _buffer.GetTemporaryRT(
                _dirShadowAtlasId, 1, 1,
                32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap
            );
        }
    }

    public void Cleanup()
    {
        _buffer.ReleaseTemporaryRT(_dirShadowAtlasId);
        ExecuteBuffer();
    }

    private void RenderDirectionalShadows()
    {
        int atlasSize = (int)_settings.Directional.AtlasSize;
        _buffer.GetTemporaryRT(_dirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        _buffer.SetRenderTarget(_dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        _buffer.ClearRenderTarget(true, false, Color.clear);
        ExecuteBuffer();
    }

    private void ExecuteBuffer()
    {
        _context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }
}