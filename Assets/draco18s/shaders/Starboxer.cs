using UnityEngine;

public class Starboxer : MonoBehaviour
{
    public enum SkyboxResolutionLOD
    {
        Low = 128,
        Medium = 512,
        High = 1024,
        Ultra = 2048,
        _4K = 4096,
    }

    [Tooltip("The resolution of the skybox.")]
    public SkyboxResolutionLOD SkyboxLOD;

    [Tooltip("Scale of the Perlin noise.")]
    public float Scale;

    [Header("Origins")]
    [Tooltip("Check to set a fixed origin, or uncheck to generate a random origin every time.")]
    public bool FixedOrigin;

    [Tooltip("The X origin of the Perlin noise.")]
    public float FixedXOrigin;

    [Tooltip("The Y origin of the Perlin noise.")]
    public float FixedYOrigin;

    int skyboxResolution;

    void Start()
    {
        skyboxResolution = (int)SkyboxLOD;

        float xOrg = FixedOrigin ? FixedXOrigin : Random.Range(0, 1000);
        float yOrg = FixedOrigin ? FixedYOrigin : Random.Range(0, 1000);

        Material mat = new Material(Shader.Find("Skybox/6 Sided"));

        for (int i = 0; i < 6; i++)
        {
            Texture2D tex = new Texture2D(skyboxResolution, skyboxResolution);
            Color[] col = new Color[skyboxResolution * skyboxResolution];

            col.GetSkyboxFace(xOrg + i * Scale, yOrg - i * Scale, skyboxResolution, Scale);
            
            tex.SetPixels(col);
            tex.Apply();

            switch (i)
            {
                case 0:
                    mat.SetTexture("_FrontTex", tex);
                    break;
                case 1:
                    mat.SetTexture("_BackTex", tex);
                    break;
                case 2:
                    mat.SetTexture("_LeftTex", tex);
                    break;
                case 3:
                    mat.SetTexture("_RightTex", tex);
                    break;
                case 4:
                    mat.SetTexture("_UpTex", tex);
                    break;
                case 5:
                    mat.SetTexture("_DownTex", tex);
                    break;
            }
        }

        if (this.GetComponent<Skybox>() == null)
        {
            this.gameObject.AddComponent<Skybox>();
        }

        this.GetComponent<Skybox>().material = mat;
    }
}

public static class ColorExtensions
{
    public static void GetSkyboxFace(this Color[] c, float xOrg, float yOrg, int resolution, float scale)
    {
        int buffer = Random.Range(10, 100);

        float y = 0.0f;
        while (y < resolution)
        {
            float x = 0.0f;
            while (x < resolution)
            {
                buffer--;

                if (buffer == 0)
                {
                    int pos = Mathf.RoundToInt(y) * resolution + Mathf.RoundToInt(x);
                    float star_dist = Random.Range(-0.4f, 2.0f);

                    /*if (star_dist < 0.66f)
                    {
                        c.DrawStar(pos, resolution, new Color(1.0f, 0.7f, 0.4f), false);
                    }
                    else if (star_dist < 0.9f)
                    {
                        c.DrawStar(pos, resolution, new Color(0.9f, 0.9f, 1.0f), false);
                    }
                    else
                    {
                        c.DrawStar(pos, resolution, new Color(0.6f, 0.7f, 1.0f), true);
                    }*/

                    c.DrawStar(pos, resolution, bv2rgb(star_dist), Random.Range(0f,1f) >= 0.9f);

                    float xCoord = xOrg + x / resolution * scale;
                    float yCoord = yOrg + y / resolution * scale;

                    float sample = Mathf.PerlinNoise(xCoord, yCoord);

                    buffer = (int)Random.Range(50, 1000 * (1 / sample));
                }

                x++;
            }

            y++;
        }
    }

    public static Color bv2rgb(float bv)    // RGB <0,1> <- BV <-0.4,+2.0> [-]
    {
        double t;
        double r=0.0;
        double g=0.0;
        double b=0.0;
        bv = Mathf.Clamp(bv, -0.4f, 2.0f);
        if ((bv>=-0.40)&&(bv<0.00)) { t=(bv+0.40)/(0.00+0.40); r=0.61+(0.11*t)+(0.1*t*t)     ; }
        else if ((bv>= 0.00)&&(bv<0.40)) { t=(bv-0.00)/(0.40-0.00); r=0.83+(0.17*t)          ; }
        else if ((bv>= 0.40)&&(bv<2.10)) { t=(bv-0.40)/(2.10-0.40); r=1.00                   ; }
        if ((bv>=-0.40)&&(bv<0.00)) { t=(bv+0.40)/(0.00+0.40); g=0.70+(0.07*t)+(0.1*t*t); }
        else if ((bv>= 0.00)&&(bv<0.40)) { t=(bv-0.00)/(0.40-0.00); g=0.87+(0.11*t)          ; }
        else if ((bv>= 0.40)&&(bv<1.60)) { t=(bv-0.40)/(1.60-0.40); g=0.98-(0.16*t)          ; }
        else if ((bv>= 1.60)&&(bv<2.00)) { t=(bv-1.60)/(2.00-1.60); g=0.82         -(0.5*t*t); }
        if ((bv>=-0.40)&&(bv<0.40)) { t=(bv+0.40)/(0.40+0.40); b=1.00                   ; }
        else if ((bv>= 0.40)&&(bv<1.50)) { t=(bv-0.40)/(1.50-0.40); b=1.00-(0.47*t)+(0.1*t*t); }
        else if ((bv>= 1.50)&&(bv<1.94)) { t=(bv-1.50)/(1.94-1.50); b=0.63         -(0.6*t*t); }

        return new Color((float)r, (float)g, (float)b);
    }

    private static void DrawStar(this Color[] c, int pos, int resolution, Color color, bool large)
    {
        c[pos] = color;

        if (!large)
        {
            return;
        }

        if (pos - 1 >= 0)
        {
            c[pos - 1] = color;
        }

        if (pos + 1 < c.Length)
        {
            c[pos + 1] = color;
        }

        if (pos - resolution >= 0)
        {
            c[pos - resolution] = color;
        }

        if (pos + resolution < c.Length)
        {
            c[pos + resolution] = color;
        }
    }
}