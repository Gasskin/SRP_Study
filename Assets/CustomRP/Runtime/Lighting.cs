using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Lighting
{
    private const int MAX_DIR_LIGHT_COUNT = 4;

    private static int _dirLightCountId = Shader.PropertyToID("_DirectionalLightCount"),
        _dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors"),
        _dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");

    private static Vector4[]
        _dirLightColors = new Vector4[MAX_DIR_LIGHT_COUNT],
        _dirLightDirections = new Vector4[MAX_DIR_LIGHT_COUNT];

    private const string BUFFER_NAME = "Lighting";


    private CommandBuffer _buffer = new CommandBuffer
    {
        name = BUFFER_NAME
    };
    
    private Shadows _shadows = new Shadows();
    
    private CullingResults _cullingResults;
    

    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        _cullingResults = cullingResults;
        _buffer.BeginSample(BUFFER_NAME);
        _shadows.Setup(context, cullingResults, shadowSettings);
        SetupLights();
        _shadows.Render();
        _buffer.EndSample(BUFFER_NAME);
        context.ExecuteCommandBuffer(_buffer);
        _buffer.Clear();
    }

    public void Cleanup ()
    {
        _shadows.Cleanup();
    }
    private void PrepareBuff()
    {
        _buffer.BeginSample(BUFFER_NAME);
        _buffer.SetGlobalVectorArray(_dirLightColorsId, _dirLightColors);
        _buffer.SetGlobalVectorArray(_dirLightDirectionsId, _dirLightDirections);
        _buffer.EndSample(BUFFER_NAME);
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
        _shadows.ReserveDirectionalShadows(visibleLight.light, index);
    }
}