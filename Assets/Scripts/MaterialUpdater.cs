using UnityEngine;
using System.Collections.Generic;

public class MaterialUpdater : MonoBehaviour
{
    public bool isFlashing = false;
    public AnimationCurve animationCurve;
    public Color testColor;
    public float flashingPeriod = 1f;
    private float flashingTime = 0f;
    private Color flashingColor;
    private Color flashingEmissive;
    
    private List<Renderer> placeablePartRenderers = new List<Renderer>();
    private List<Color> normalColors = new List<Color>();
    private List<Color> normalEmissives = new List<Color>();
    
    void Start()
    {
        // Add the renderer of this object if it has one
        var mainRenderer = GetComponent<Renderer>();
        if (mainRenderer != null)
        {
            placeablePartRenderers.Add(mainRenderer);
        }
        
        // Find all child objects with "PlaceablePart" tag
        FindPlaceableParts(transform);
        
        // Create material instances and store original colors
        foreach (var renderer in placeablePartRenderers)
        {
            var newMaterial = new Material(renderer.material);
            renderer.material = newMaterial;
            normalColors.Add(newMaterial.color);
            normalEmissives.Add(newMaterial.GetColor("_EmissionColor"));
        }
        
        flashingColor = testColor;
        flashingEmissive = testColor;
    }
    
    private void FindPlaceableParts(Transform parent)
    {
        // Recursively check children (skip the parent since we already handled it)
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            
            // Check if child has "PlaceablePart" tag
            if (child.CompareTag("PlaceablePart"))
            {
                var renderer = child.GetComponent<Renderer>();
                if (renderer != null)
                {
                    placeablePartRenderers.Add(renderer);
                }
            }
            
            // Recursively check grandchildren
            FindPlaceableParts(child);
        }
    }

    void Update()
    {
        if (isFlashing)
        {
            flashingTime += Time.deltaTime;
            Flash();
            if (flashingTime >= flashingPeriod)
            {
                flashingTime = 0f;
            }
        }
    }

    public void SetMaterialColor(Color color)
    {
        foreach (var renderer in placeablePartRenderers)
        {
            renderer.material.color = color;
        }
    }
    
    public void SetMaterialEmissive(Color color)
    {
        foreach (var renderer in placeablePartRenderers)
        {
            renderer.material.SetColor("_EmissionColor", color);
        }
    }

    private void Flash()
    {
        float curveValue = animationCurve.Evaluate(flashingTime / flashingPeriod);
        
        for (int i = 0; i < placeablePartRenderers.Count; i++)
        {
            Color newColor = Color.Lerp(normalColors[i], flashingColor, curveValue);
            Color newEmissive = Color.Lerp(normalEmissives[i], flashingEmissive, curveValue);
            
            placeablePartRenderers[i].material.color = newColor;
            placeablePartRenderers[i].material.SetColor("_EmissionColor", newEmissive);
        }
    }

    public void StartFlash(Color color, Color emissive)
    {
        flashingColor = color;
        flashingEmissive = emissive;
        isFlashing = true;
    }
    
    public void StartFlash(float period, Color color, Color emissive)
    {
        flashingPeriod = period;
        flashingColor = color;
        flashingEmissive = emissive;
        isFlashing = true;
        flashingTime = 0f;
    }
    
    public void StopFlash()
    {
        isFlashing = false;
        flashingTime = 0f;
        
        // Restore original colors
        for (int i = 0; i < placeablePartRenderers.Count; i++)
        {
            placeablePartRenderers[i].material.color = normalColors[i];
            placeablePartRenderers[i].material.SetColor("_EmissionColor", normalEmissives[i]);
        }
    }
}
