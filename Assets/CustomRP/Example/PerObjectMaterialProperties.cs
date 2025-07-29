using System;
using UnityEngine;

public class PerObjectMaterialProperties : MonoBehaviour
{
    private static int
        _baseColorId = Shader.PropertyToID("_BaseColor"),
        _cutoffId = Shader.PropertyToID("_Cutoff"),
        _metallicId = Shader.PropertyToID("_Metallic"),
        _smoothnessId = Shader.PropertyToID("_Smoothness");

    private static MaterialPropertyBlock _block;

    [SerializeField]
    private Color _baseColor = Color.white;

    [SerializeField, Range(0f, 1f)]
    private float
        _cutoff = 0.5f,
        _metallic = 0f,
        _smoothness = 0.5f;

    private void Awake()
    {
        OnValidate();
    }

    void OnValidate()
    {
        if (_block == null)
        {
            _block = new MaterialPropertyBlock();
        }
        _block.SetColor(_baseColorId, _baseColor);
        _block.SetFloat(_cutoffId, _cutoff);
        _block.SetFloat(_metallicId, _metallic);
        _block.SetFloat(_smoothnessId, _smoothness);
        GetComponent<Renderer>().SetPropertyBlock(_block);
    }
}