using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PostProcess : MonoBehaviour {
    public Material material;
    Material materialInst;
    public bool vignette = true;
    public bool psychedelic = false;
    public bool bowl = false;
    
    void Awake() {
        materialInst = new Material(material);
    }
    
    void OnRenderImage(RenderTexture src, RenderTexture dest) {
        materialInst.SetFloat("_vignette", vignette ? 1.0f : 0.0f);
        materialInst.SetFloat("_psychedelic", psychedelic ? 1.0f : 0.0f);
        materialInst.SetFloat("_bowl", bowl ? 1.0f : 0.0f);
        Graphics.Blit(src, dest, materialInst);
    }
}
