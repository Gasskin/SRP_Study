using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

public partial class CameraRenderer
{
#if UNITY_EDITOR
    private static ShaderTagId[] _legacyShaderTagId =
    {
        new("Always"),
        new("ForwardBase"),
        new("PrepassBase"),
        new("Vertex"),
        new("VertexLMRGBM"),
        new("VertexLM"),
    };

    private static Material _errorMaterial;
#endif

    private void PrepareForSceneWindow()
    {
#if UNITY_EDITOR
        if (_camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(_camera);
        }
#endif
    }

    private void DrawUnsupportedShaders()
    {
#if UNITY_EDITOR
        if (_errorMaterial == null)
        {
            _errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader"));
        }

        var renderListDesc = new RendererListDesc(_legacyShaderTagId, _cullingResults, _camera)
        {
            sortingCriteria = SortingCriteria.None,
            renderQueueRange = RenderQueueRange.all,
            overrideMaterial = _errorMaterial,
        };
        var renderList = _context.CreateRendererList(renderListDesc);
        _buffer.DrawRendererList(renderList);
#endif
    }

    private void DrawGizoms()
    {
#if UNITY_EDITOR
        if (Handles.ShouldRenderGizmos())
        {
            _context.DrawGizmos(_camera, GizmoSubset.PreImageEffects);
            _context.DrawWireOverlay(_camera);
            _context.DrawGizmos(_camera, GizmoSubset.PostImageEffects);
        }
#endif
    }

    private void PrepareBuff()
    {
#if UNITY_EDITOR
        _buffer.name = _camera.name;
#endif
    }
}