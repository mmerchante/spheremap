using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class SMBuilder : MonoBehaviour 
{
    private const int IMAGE_LENGTH = 128;

#if UNITY_EDITOR
    public enum ShadingType
    {
        Lambert,
        HalfLambert, 
        Phong,
        Blinn
    }

    public int aaQuality = 1;

    public ShadingType shadingType;

    public Color diffuseColor = Color.gray;
    public float diffuseWeight = 1f;
    public float diffuseRoughness = 0f;
    public Gradient diffuseWrapGradient;
    public bool diffuseFresnel = false;
    public float diffuseFresnelWeight = 1f;
    public AnimationCurve diffuseFresnelCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    public Color specularColor = Color.white;
    public float specularWeight = 1f;
    public float specularRoughness = 0f;
    public bool specularFresnel = false;
    public float specularFresnelWeight = 1f;
    public AnimationCurve specularFresnelCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    public bool fresnelReflective = false;

    public Color emissiveColor = Color.white;
    public float emissiveWeight = 0f;
    public bool emissiveFresnel = false;
    public float emissiveFresnelWeight = 1f;
    public AnimationCurve emissiveFresnelCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    public Texture2D sphereMap;

    private List<SMLight> lights = new List<SMLight>();

    private Color[] texturePixels;
    private float[] sampleWeights;

    private int texturePixelsLength;

    private int updateCounter = 0;
    private bool previewMode = true;
    
    [ContextMenu ("Build")]
    public void BuildTexture()
    {
        Initialize();

        BuildTexure(GetFullImageSamples());

        this.sphereMap.SetPixels(texturePixels);
        this.sphereMap.Apply();

        DisablePreview();
    }

    public void BuildPreviewTexture()
    {
        EnablePreview();
        this.sampleWeights = new float[IMAGE_LENGTH * IMAGE_LENGTH];
       
    }

    private void Initialize()
    {
        if(!sphereMap || sphereMap.width != IMAGE_LENGTH || sphereMap.height != IMAGE_LENGTH)
            this.sphereMap = new Texture2D(IMAGE_LENGTH, IMAGE_LENGTH);

        Debug.Log("Initializing...");

        this.texturePixels = new Color[IMAGE_LENGTH * IMAGE_LENGTH];
        this.sampleWeights = new float[IMAGE_LENGTH * IMAGE_LENGTH];
        this.lights = new List<SMLight>(this.gameObject.GetComponents<SMLight>());

        for (int i = 0; i < IMAGE_LENGTH * IMAGE_LENGTH; i++)
        {
            texturePixels[i] = new Color(0f, 0f, 0f, 1f);
            sampleWeights[i] = 0f;
        }

        PrecomputeOrenNayar();
    }

    public void EnablePreview()
    {
        previewMode = true;
    }

    public void DisablePreview()
    {
        previewMode = false;
    }

    public void Update()
    {   
        if (previewMode)
        {
            int previewSamples = (int) (IMAGE_LENGTH);

            BuildTexure(Stochastic.GetStratifiedUniformGrid(previewSamples, previewSamples));

            updateCounter++;

            if (updateCounter > 1)
            {
                sphereMap.SetPixels(texturePixels);
                sphereMap.Apply();
                updateCounter = 0;
            }
        }
    }

    private List<Vector2> GetFullImageSamples()
    {
        List<Vector2> samples = new List<Vector2>();
        float pixelLength = 1f / IMAGE_LENGTH;

        for (int x = 0; x < IMAGE_LENGTH; x++)
            for (int y = 0; y < IMAGE_LENGTH; y++)
                samples.Add(new Vector2(x / (float)IMAGE_LENGTH, y / (float)IMAGE_LENGTH) + Vector2.one * pixelLength * .5f);

       return samples;
    }

    private void BuildTexure(List<Vector2> samples)
    {
        if (texturePixels == null || texturePixels.Length != IMAGE_LENGTH * IMAGE_LENGTH || lights == null || sampleWeights == null || sampleWeights.Length != IMAGE_LENGTH * IMAGE_LENGTH)
            Initialize();

        Vector2 center = new Vector2(.5f, .5f);

        int samplesAAexp = aaQuality;

        float pixelLength = 1f / IMAGE_LENGTH;

        foreach(Vector2 sample in samples)
            {
                int x = Mathf.Clamp((int)(sample.x * IMAGE_LENGTH), 0, IMAGE_LENGTH - 1);
                int y = Mathf.Clamp((int)(sample.y * IMAGE_LENGTH), 0, IMAGE_LENGTH - 1); ;

                List<Vector2> aaSamples = Stochastic.GetStratifiedUniformGrid(samplesAAexp, samplesAAexp);
                Color pixel = Color.black;

                foreach (Vector2 aa in aaSamples)
                {
                    Vector2 p = sample + aa * pixelLength;

                    if ((p - center).magnitude < .5f)
                    {
                        pixel += RenderPixel((p - center) / .5f) / aaSamples.Count;
                    }
                    else
                    {
                        // Background pixel
                        Vector2 dirToPoint = p - center;
                        Vector2 edge = dirToPoint.normalized;
                        //pixel += RenderPixel(edge *.999f) / aaSamples.Count;
                    }
                }

                Color previousColor = texturePixels[y * IMAGE_LENGTH + x];
                float previousWeight = sampleWeights[y * IMAGE_LENGTH + x];

                sampleWeights[y * IMAGE_LENGTH + x] += 1f;

                if (previousWeight > 0.0001f)
                    texturePixels[y * IMAGE_LENGTH + x] = ((previousColor * previousWeight) + pixel) / (previousWeight + 1f);
                else
                    texturePixels[y * IMAGE_LENGTH + x] = pixel;
            }
    }


    private Color Max(Color c, float v)
    {
        return new Color(Mathf.Max(c.r, v), Mathf.Max(c.g, v), Mathf.Max(c.b, v), c.a);
    }

    private class SurfaceData
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector3 tangent;
        public Vector3 binormal;
        // uvs, whatev
    }

    private SurfaceData BuildSphereData(Vector2 ssPos)
    {
        SurfaceData data = new SurfaceData();

        data.normal = new Vector3(ssPos.x, ssPos.y, Mathf.Clamp01(Mathf.Sqrt(1f - ssPos.x * ssPos.x - ssPos.y * ssPos.y))).normalized;
        data.position = data.normal;

        Vector3.OrthoNormalize(ref data.normal, ref data.tangent, ref data.binormal);

        return data;
    }

    private Color RenderPixel(Vector2 ssPos)
    {
        Color pixel = Color.black;

        SurfaceData sphereData = BuildSphereData(ssPos);

        float diffuseFresnelContribution = diffuseFresnel ? Mathf.Lerp(1f, diffuseFresnelCurve.Evaluate(sphereData.normal.z), diffuseFresnelWeight) : 1f;

        foreach (SMLight l in lights)
        {
            Vector3 lightIncidence = -l.SampleLightIncidence(sphereData.position);
            
            float cosThetaI = Mathf.Clamp01(Vector3.Dot(lightIncidence, sphereData.normal));

            Color diffuse = OrenNayar(sphereData, lightIncidence) * l.color.linear * l.intensity * cosThetaI * diffuseWeight * diffuseFresnelContribution;
            Color specular = specularWeight > 0.0001f ? TorranceSparrow(sphereData, lightIncidence) * cosThetaI * Mathf.PI * l.color.linear * l.intensity : Color.black;

            // Very, horribly fake AO
            diffuse *= Mathf.Clamp01(ssPos.y + 1f);

            pixel += specular + diffuse;
        }

        float emissiveFresnelContribution = emissiveFresnel ? Mathf.Lerp(1f, emissiveFresnelCurve.Evaluate(sphereData.normal.z), emissiveFresnelWeight) : 1f;
        pixel += emissiveColor.linear * emissiveFresnelContribution * emissiveWeight;

        // Always...
        pixel.a = 1f;

        return pixel.gamma;
    }

    private Color GammaCorrect(Color c, float gamma)
    {
        return new Color(Mathf.Pow(c.r, gamma), Mathf.Pow(c.g, gamma), Mathf.Pow(c.b, gamma), Mathf.Pow(c.a, gamma));
    }

    private float orenNayarA = 1f;
    private float orenNayarB = 0f;
    
    private void PrecomputeOrenNayar()
    {
        float sigma = diffuseRoughness;
        float sigma2 = sigma * sigma;
        orenNayarA = 1f - (sigma2 / (2f * (sigma2 + 0.33f)));
        orenNayarB = 0.45f * sigma2 / (sigma2 + 0.09f);
    }

    private Color ShadeDiffuse(Vector3 normal, Vector3 position, SMLight l)
    {
        Vector3 lightIncidence = l.SampleLightIncidence(position);
        float dot = Mathf.Clamp01(Vector3.Dot(normal, -lightIncidence));
        return Color.white * dot;
    }


    private float BeckmannD(float NdotH)
    {
        float alpha = Mathf.Acos(NdotH);
        float tan = Mathf.Tan(alpha);
        float m = specularRoughness + .00001f;

        return Mathf.Exp(-tan * tan / (m * m)) / (Mathf.PI * m * m * Mathf.Pow(NdotH, 4f));
    }

    // Reference: PBRT
    private Color TorranceSparrow(SurfaceData data, Vector3 lightIncidence)
    {
        float cosThetaO = Mathf.Abs(Vector3.Dot(data.normal, lightIncidence));
        float cosThetaI = Mathf.Abs(data.normal.z);

        if (cosThetaI + cosThetaO < 0.0001f)
            return Color.black;

        Vector3 halfVector = (lightIncidence + Vector3.forward).normalized;

        // Geometric Attenuation
        float NdotWh = Mathf.Clamp01(Vector3.Dot(data.normal, halfVector));
        float NdotWo = cosThetaI; 
        float NdotWi = cosThetaO;  
        float WOdotWh = Mathf.Abs(Vector3.Dot(lightIncidence, halfVector));

        float geomAtten = Mathf.Min(1f, Mathf.Min(2f * NdotWh * NdotWo / WOdotWh, 2f * NdotWh * NdotWi / WOdotWh));

        // While we could have used a more physically based fresnel function, this is easier for the artist :)
        float fresnel = specularFresnel ? Mathf.Lerp(1f, specularFresnelCurve.Evaluate(Mathf.Clamp01(cosThetaO)),specularFresnelWeight) : 1f;

        float finalSpecular = specularWeight * fresnel * BeckmannD(NdotWh) * geomAtten / (4f * cosThetaO * cosThetaI);

        return specularColor.linear * Mathf.Max(finalSpecular, 0f);
    }

    // Reference: PBRT p.451
    private Color OrenNayar(SurfaceData data, Vector3 lightIncidence)
    {
        float cosThetaI = Vector3.Dot(data.normal, lightIncidence);
        float cosThetaO = data.normal.z; // Vector3.Dot(normal, Vector3.forward);

        float sinThetaI = Mathf.Pow(1f - cosThetaI * cosThetaI, 2f);
        float sinThetaO = Mathf.Pow(1f - cosThetaO * cosThetaO, 2f);

        float maxCos = 0f;

        if(sinThetaI + sinThetaO > 0f)
        {
            float cosPhiI = Vector3.Dot(data.tangent, lightIncidence);
            float cosPhiO = data.tangent.z;// Vector3.Dot(tangent, Vector3.forward); 

            float sinPhiI = Mathf.Pow(1f - cosPhiI * cosPhiI, 2f);
            float sinPhiO = Mathf.Pow(1f - cosPhiO * cosPhiO, 2f);

            maxCos = Mathf.Max(0f, cosPhiI * cosPhiO + sinPhiI * sinPhiO);
        }

        float sinAlpha = 0f;
        float tanBeta = 0f;

        if (Mathf.Abs(cosThetaI) > Mathf.Abs(cosThetaO))
        {
            sinAlpha = sinThetaO;
            tanBeta = sinThetaI / Mathf.Abs(cosThetaI);
        }
        else
        {
            sinAlpha = sinThetaI;
            tanBeta = sinThetaO / Mathf.Abs(cosThetaO);
        }

        return diffuseColor.linear * (orenNayarA + orenNayarB * maxCos * sinAlpha * tanBeta) / Mathf.PI;
    }

    public Texture2D GetTexture()
    {
        return sphereMap;
    }
#endif
}
