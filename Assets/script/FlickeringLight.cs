using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlickeringLight : MonoBehaviour
{
    private Light2D light2D;

    [Header("pulzovani")]
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.2f;
    public float pulseSpeed = 1f;

    [Header("zmena barvy")]
    public bool useColors = false;
    public Gradient colorGradient;
    public float colorSpeed = 0.5f;

    private float randomOffset; // at neblikaji vsechna svetla naráz stejně

    void Start()
    {
        light2D = GetComponent<Light2D>();
        randomOffset = Random.Range(0f, 100f); 
    }

    void Update()
    {
        if (light2D != null)
        {
            // perlin noise na hezci blikani
            float noise = Mathf.PerlinNoise(Time.time * pulseSpeed + randomOffset, 0f);
            light2D.intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);

            if (useColors)
            {
                // barvicky 
                float colorT = Mathf.PingPong(Time.time * colorSpeed + randomOffset, 1f);
                light2D.color = colorGradient.Evaluate(colorT);
            }
        }
    }
}