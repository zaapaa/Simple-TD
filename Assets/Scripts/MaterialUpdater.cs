using UnityEngine;

public class MaterialUpdater : MonoBehaviour
{
    public bool isFlashing = false;
    public AnimationCurve animationCurve;
    public Color testColor;
    public float flashingPeriod = 1f;
    private float flashingTime = 0f;
    private Color flashingColor;
    private Color flashingEmissive;
    private Color normalColor;
    private Color normalEmissive;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var newMaterial = new Material(GetComponent<Renderer>().material);
        GetComponent<Renderer>().material = newMaterial;
        normalColor = GetComponent<Renderer>().material.color;
        normalEmissive = GetComponent<Renderer>().material.GetColor("_EmissionColor");
        flashingColor = testColor;
        flashingEmissive = testColor;
    }

    // Update is called once per frame
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
        GetComponent<Renderer>().material.color = color;
    }
    public void SetMaterialEmissive(Color color)
    {
        GetComponent<Renderer>().material.SetColor("_EmissionColor", color);
    }

    private void Flash()
    {
        Color newColor = Color.Lerp(normalColor, flashingColor, animationCurve.Evaluate(flashingTime / flashingPeriod));
        Color newEmissive = Color.Lerp(normalEmissive, flashingEmissive, animationCurve.Evaluate(flashingTime / flashingPeriod));
        SetMaterialColor(newColor);
        SetMaterialEmissive(newEmissive);
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
        SetMaterialColor(normalColor);
        SetMaterialEmissive(normalEmissive);
    }
}
