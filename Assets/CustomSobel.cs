using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[Serializable, VolumeComponentMenuForRenderPipeline("Assets/CustomSobel", typeof(UniversalRenderPipeline))]
public class CustomSobel : VolumeComponent, IPostProcessComponent
{
    public BoolParameter isEnabled = new BoolParameter(true);  // To toggle the effect on/off
    public FloatParameter threshold = new FloatParameter(1f);  // Example parameter for the Sobel filter

    public bool IsActive() => isEnabled.value;  // Returns true if the effect is enabled
    public bool IsTileCompatible() => true;  // Compatible with tiled rendering
}
