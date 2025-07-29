using System;
using UnityEngine;

public class PerObjectMaterialProperties : MonoBehaviour
{
    private static int _baseColorId = Shader.PropertyToID("_BaseColor");
    private static int _cutoffId = Shader.PropertyToID("_Cutoff");

    private static MaterialPropertyBlock _block;

    [SerializeField]
    private Color _baseColor = Color.white;
    
    [SerializeField, Range(0f, 1f)]
    private float _cutoff = 0.5f;

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
        GetComponent<Renderer>().SetPropertyBlock(_block);
    }
}