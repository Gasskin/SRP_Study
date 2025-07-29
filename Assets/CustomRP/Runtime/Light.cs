using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Light
{
    private const int MAX_DIR_LIGHT_COUNT = 4;

    private static int _dirLightCountId = Shader.PropertyToID("_DirectionalLightCount"),
        _dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors"),
        _dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");

    private static Vector4[]
        _dirLightColors = new Vector4[MAX_DIR_LIGHT_COUNT],
        _dirLightDirections = new Vector4[MAX_DIR_LIGHT_COUNT];

    private const string BUFFER_NAME = "Lighting";

    private CullingResults _cullingResults;

    private CommandBuffer _buffer = new CommandBuffer
    {
        name = BUFFER_NAME
    };

    public void Setup(ScriptableRenderContext context, CullingResults cullingResults)
    {
        _cullingResults = cullingResults;
        _buffer.BeginSample(BUFFER_NAME);
        SetupLights();
        _buffer.EndSample(BUFFER_NAME);
        context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }

    private void SetupLights()
    {
        // var light = RenderSettings.sun;
        // _buffer.SetGlobalVector(_dirLightColorId, light.color.linear * light.intensity);
        // _buffer.SetGlobalVector(_dirLightDirectionId, -light.transform.forward);

        NativeArray<VisibleLight> visibleLights = _cullingResults.visibleLights;
        int dirLightCount = 0;
        for (int i = 0; i < visibleLights.Length; i++)
        {
            VisibleLight visibleLight = visibleLights[i];
            if (visibleLight.lightType == LightType.Directional)
            {
                SetupDirectionalLight(dirLightCount++, ref visibleLight);
                if (dirLightCount >= MAX_DIR_LIGHT_COUNT)
                {
                    break;
                }
            }
        }

        _buffer.SetGlobalInt(_dirLightCountId, visibleLights.Length);
        _buffer.SetGlobalVectorArray(_dirLightColorsId, _dirLightColors);
        _buffer.SetGlobalVectorArray(_dirLightDirectionsId, _dirLightDirections);
    }

    private void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
    {
        _dirLightColors[index] = visibleLight.finalColor;
        _dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);
    }
}