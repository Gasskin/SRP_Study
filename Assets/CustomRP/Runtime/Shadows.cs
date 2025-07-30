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
    }

    private void ExecuteBuffer()
    {
        _context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }
}