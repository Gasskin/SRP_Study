using System;
using UnityEngine;

[Serializable]
public struct Directional
{
    public ShadowSettings.TextureSize AtlasSize;
}


[Serializable]
public class ShadowSettings
{
    public enum TextureSize
    {
        _256 = 256,
        _512 = 512,
        _1024 = 1024,
        _2048 = 2048,
        _4096 = 4096,
        _8192 = 8192
    }


    public Directional Directional = new Directional
    {
        AtlasSize = TextureSize._1024
    };

    [Min(0f)]
    public float MaxDistance = 100f;
}