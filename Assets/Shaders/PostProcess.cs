using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostProcess : MonoBehaviour
{
    void OnRenderImage(RenderTexture src, RenderTexture dest) {
        var material = new Material(Shader.Find("Custom/PostProcess"));
        Graphics.Blit(src, dest, material);
    }
}
