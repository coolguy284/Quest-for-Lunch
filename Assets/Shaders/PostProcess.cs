using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostProcess : MonoBehaviour
{
    public Material material;
    public bool vignette = true;
    public bool psychedelic = false;
    public bool bowl = false;

    void OnRenderImage(RenderTexture src, RenderTexture dest) {
        material.SetFloat("_vignette", vignette ? 1.0f : 0.0f);
        material.SetFloat("_psychedelic", psychedelic ? 1.0f : 0.0f);
        material.SetFloat("_bowl", bowl ? 1.0f : 0.0f);
        Graphics.Blit(src, dest, material);
    }
}
