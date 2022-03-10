using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public class RunCellular : MonoBehaviour
{
    public Material blurMaterial; // Wraps the shader
    public Material finalMaterial; // Wraps the shader
    public Gradient oxygenEmission;

    private MaterialPropertyBlock mp;
    private RenderTexture outputTex;
    private RenderTexture middle;
    private Texture2D testTexture;
    private int max=30;

    private static float dustVal = -1;
    private static float oxyVal = -1;

    static int[] rules =
    {
        (0),
        (0),
        (0),
        (0),
        (1),
        (0),
        (1),
        (1),
        (1),
        (1),
    };

    static Vector3Int CubicPolate(Vector3Int v0, Vector3Int v1, Vector3Int v2, Vector3Int v3, float frac) {
        Vector3 A = (v3-v2)-(v0-v1);
        Vector3 B = (v0-v1)-A;
        Vector3 C = v2-v0;
        Vector3 D = v1;

        return clamp(A*Mathf.Pow(frac,3)+B*Mathf.Pow(frac,2)+C*frac+D, 0, 255);
    }

    static Vector3Int clamp(Vector3 v, float a, float b) {
        return new Vector3Int(
            Mathf.RoundToInt(Mathf.Clamp(v.x, a, b)),
            Mathf.RoundToInt(Mathf.Clamp(v.y, a, b)),
            Mathf.RoundToInt(Mathf.Clamp(v.z, a, b)));
    }

    static Vector3Int vote(Vector3Int oldstate, Vector3Int NineSum) {
        return bitor(bitand(bitshift((oldstate + NineSum), 1), 0xFE), new Vector3Int(rules[NineSum.x],rules[NineSum.y],rules[NineSum.z]));
    }

    private static Vector3Int bitshift(Vector3Int v, int b) {
        v.x = v.x << b;
        v.y = v.y << b;
        v.z = v.z << b;
        return v;
    }

    private static Vector3Int bitand(Vector3Int v, int b) {
        v.x = v.x & b;
        v.y = v.y & b;
        v.z = v.z & b;
        return v;
    }

    private static Vector3Int bitor(Vector3Int v, Vector3Int b) {
        v.x = v.x | b.x;
        v.y = v.y | b.y;
        v.z = v.z | b.z;
        return v;
    }
    
    static Vector3Int when_eq(Vector3Int x, int y) {
        return Vector3Int.one - when_neq(x, y);
    }
    
    static Vector3Int when_neq(Vector3Int x, int y) {
        return new Vector3Int(Math.Abs(Math.Sign(x.x - y)),Math.Abs(Math.Sign(x.y - y)),Math.Abs(Math.Sign(x.z - y)));
    }
    
    static Vector3Int Max(Vector3Int x, Vector3Int y) {
        return new Vector3Int(Math.Max(x.x,y.x),Math.Max(x.y,y.y),Math.Max(x.z,y.z));
    }

    static Vector3Int when_ge(Vector3Int x, int y) {
        return new Vector3Int(
            Math.Max(Math.Sign(x.x - y), 0),
            Math.Max(Math.Sign(x.y - y), 0),
            Math.Max(Math.Sign(x.z - y), 0));
    }

    static Vector3Int when_lt(Vector3Int x, int y) {
        return Vector3Int.one - when_ge(x, y);
    }

    private List<Vector4> list = new List<Vector4>();

    void Start() {
        list.Add(Vector4.zero);
        testTexture = new Texture2D(128, 128);
        testTexture.filterMode = FilterMode.Point;
        middle = new RenderTexture(128, 128, 0, RenderTextureFormat.ARGB32);
        middle.filterMode = FilterMode.Point;
        outputTex = new RenderTexture(128, 128, 0, RenderTextureFormat.ARGB32);
        outputTex.filterMode = FilterMode.Bilinear;
        mp = new MaterialPropertyBlock();
        mp.SetTexture("_MaskTex",outputTex);
            StartCoroutine(Generate());
    }

    IEnumerator Generate() {
        if(oxyVal < 0 || dustVal < 0) {
            dustVal = Random.Range(.1f, .2f);
            oxyVal = Random.Range(0f, 1f);
        }
        Vector3Int[,] cellGrid = new Vector3Int[64,64];
        Vector3Int[,] buffer = new Vector3Int[64,64];
        UnityEngine.Profiling.Profiler.BeginSample("Setup");
        for(int y=0;y<64;y++) {
            for(int x=0;x<64;x++) {
                cellGrid[x,y] = new Vector3Int(Mathf.RoundToInt(Random.Range(0,128)),Mathf.RoundToInt(Random.Range(0,128)),Mathf.RoundToInt(Random.Range(0,128)));
            }
        }
        UnityEngine.Profiling.Profiler.EndSample();
        UnityEngine.Profiling.Profiler.BeginSample("Sim");
        const int s = 1;
        for(int i = 0; i < max; i++) {
            //buffer = new Vector3Int[64,64];
            for(int y=0;y<64;y++) {
                for(int x=0;x<64;x++) {
                    Vector3Int tl = cellGrid[(x-s+64)%64, (y-s+64)%64];    // Top Left
                    Vector3Int cl = cellGrid[(x-s+64)%64, (y+0+64)%64];    // Centre Left
                    Vector3Int bl = cellGrid[(x-s+64)%64, (y+s+64)%64];    // Bottom Left
                    Vector3Int tc = cellGrid[(x-0+64)%64, (y-s+64)%64];    // Top Centre
                    Vector3Int cc = cellGrid[(x+0+64)%64, (y+0+64)%64];    // Centre Centre
                    Vector3Int bc = cellGrid[(x+0+64)%64, (y+s+64)%64];    // Bottom Centre
                    Vector3Int tr = cellGrid[(x+s+64)%64, (y-s+64)%64];    // Top Right
                    Vector3Int cr = cellGrid[(x+s+64)%64, (y+0+64)%64];    // Centre Right
                    Vector3Int br = cellGrid[(x+s+64)%64, (y+s+64)%64];    // Bottom Right

                    Vector3Int count = 
                        bitand(tl,0x01) + bitand(tc,0x01) + bitand(tr,0x01) +
                        bitand(cl,0x01) + bitand(cc,0x01) + bitand(cr,0x01) +
                        bitand(bl,0x01) + bitand(bc,0x01) + bitand(br,0x01);

                    buffer[x,y] = vote(cc, count);
                }
            }
            cellGrid = buffer;
            UnityEngine.Profiling.Profiler.EndSample();
            yield return null;
            UnityEngine.Profiling.Profiler.BeginSample("Sim");
        }
        buffer = new Vector3Int[64,64];
        UnityEngine.Profiling.Profiler.EndSample();
        UnityEngine.Profiling.Profiler.BeginSample("Clamp");
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                Vector3Int tc = cellGrid[(x-0+64)%64, (y-s+64)%64];
                Vector3Int bc = cellGrid[(x+0+64)%64, (y+s+64)%64];
                Vector3Int cl = cellGrid[(x-s+64)%64, (y+0+64)%64];
                Vector3Int cr = cellGrid[(x+s+64)%64, (y+0+64)%64];
                Vector3Int cc = cellGrid[x,y];

                Vector3Int n = 
                    when_neq(tc, 237)*when_neq(tc, 0) + when_neq(cr, 237)*when_neq(cr, 0) + 
                    when_neq(bc, 237)*when_neq(bc, 0) + when_neq(cl, 237)*when_neq(cl, 0);
                Vector3Int c = Max(when_neq(cc, 237) * when_neq(cc, 0), when_neq(n, 0));

                buffer[x,y] = new Vector3Int((c.x*255), (c.y*255), (c.z*255));
            }
        }
        cellGrid = buffer;

        UnityEngine.Profiling.Profiler.EndSample();
        UnityEngine.Profiling.Profiler.BeginSample("Expand");
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                Vector3Int tl = cellGrid[(x-s+64)%64, (y-s+64)%64];    // Top Left
                Vector3Int cl = cellGrid[(x-s+64)%64, (y+0+64)%64];    // Centre Left
                Vector3Int bl = cellGrid[(x-s+64)%64, (y+s+64)%64];    // Bottom Left
                Vector3Int tc = cellGrid[(x+0+64)%64, (y-s+64)%64];    // Top Centre
                Vector3Int cc = cellGrid[(x+0+64)%64, (y+0+64)%64];    // Centre Centre
                Vector3Int bc = cellGrid[(x+0+64)%64, (y+s+64)%64];    // Bottom Centre
                Vector3Int tr = cellGrid[(x+s+64)%64, (y-s+64)%64];    // Top Right
                Vector3Int cr = cellGrid[(x+s+64)%64, (y+0+64)%64];    // Centre Right
                Vector3Int br = cellGrid[(x+s+64)%64, (y+s+64)%64];    // Bottom Right

                Vector3Int sum = when_eq(tl,255)+when_eq(cl,255)+when_eq(bl,255)+when_eq(tc,255)+when_eq(cc,255)+when_eq(bc,255)+when_eq(tr,255)+when_eq(cr,255)+when_eq(br,255);
                Vector3Int c = when_ge(sum,4);

                buffer[x,y] = new Vector3Int((c.x*255), (c.y*255), (c.z*255));
            }
        }
        cellGrid = buffer;

        var data = testTexture.GetRawTextureData<Color32>();
        int index = 0;

        UnityEngine.Profiling.Profiler.EndSample();
        UnityEngine.Profiling.Profiler.BeginSample("Blur");
        data = testTexture.GetRawTextureData<Color32>();
        index = 0;
        for(int y=0;y<128;y++) {
            for(int x=0;x<128;x++) {
                int px = x>>1;
                int py = y>>1;
                Vector3Int c = cellGrid[(px+0)%64,(py+0)%64];
                data[index++] = new Color32((byte)(c.x), (byte)(c.y), (byte)(c.z), 255);
            }
        }
        testTexture.Apply();
        UnityEngine.Profiling.Profiler.EndSample();

        blurMaterial.SetFloat("_Pixels", 128f);
        Graphics.Blit(testTexture, middle, blurMaterial);
        Graphics.Blit(middle, outputTex, blurMaterial);

        byte[] bytes = toTexture2D(outputTex).EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.persistentDataPath + "/SavedScreen.png", bytes);
        Debug.Log("Exported to: " + Application.persistentDataPath + "/SavedScreen.png");

        UnityEngine.Profiling.Profiler.EndSample();
        list[0] = new Vector4(Random.value,Random.value,Random.value,5);
        mp.SetVectorArray("_PerlinInputs", list);
        float dust = dustVal;//Random.Range(.1f, .2f);
        float oxy = oxyVal;//Random.Range(0f, 1f);
		float fin = (oxy+dust+(Random.value>0.5f?0:0.5f))%1;
Debug.Log(dust + "," + oxy + "=" + fin);
        Color color1 = oxygenEmission.Evaluate(oxy);
        Color color2 = oxygenEmission.Evaluate(fin);
        color1.a = 1;
        color2.a = 1;
        mp.SetColor("_Color", color1);
        mp.SetColor("_Highlight", color2);
        GetComponent<Renderer>().SetPropertyBlock(mp);
    }

    public static Texture2D toTexture2D(RenderTexture rTex)
    {
        Texture2D tex = new Texture2D(rTex.width, rTex.height, TextureFormat.RGB24, false);
        RenderTexture.active = rTex;
        tex.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        tex.Apply();
        return tex;
    }
}
