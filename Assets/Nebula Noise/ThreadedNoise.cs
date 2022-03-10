using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using Random = UnityEngine.Random;

public class ThreadedNoise : MonoBehaviour
{
    public List<Color[]> mat16;
    public List<Color[]> mat8;
    public List<Color[]> masks;

    public Texture2D[] m16Pre;
    public Texture2D[] m8Pre;
    public Texture2D[] maskPre;

    public List<int> queue;
    public bool qmRunning;

    public int breakCount;

    public static ThreadedNoise instance;

    public void Start () {
        instance = this;
        breakCount = 50 + 50*QualitySettings.GetQualityLevel();
        DontDestroyOnLoad(this.gameObject);
        
        mat8=new List<Color[]>();
        mat16=new List<Color[]>();
        masks=new List<Color[]>();
        //queue=new List<int>();
        
        loadPre();
        
        /*if(QualitySettings.GetQualityLevel()>1){
            queue.Add(8);
            for(int i=0;i<4;i++){
                queue.Add(16);
                queue.Add(0);
            }
            queue.Add(8);
        }*/
    }

    public void loadPre() {
        Color[] c;
        int i;
        for(i=0;i<m16Pre.Length;i++){
            c=m16Pre[i].GetPixels();
            mat16.Add(c);
        }
        
        for(i=0;i<m8Pre.Length;i++){
            c=m8Pre[i].GetPixels();
            mat8.Add(c);
        }
        
        for(i=0;i<maskPre.Length;i++){
            c=maskPre[i].GetPixels();
            masks.Add(c);
        }
    }

    public void Update() {
        /*if(Input.GetKeyDown(KeyCode.S)){
            saveTexs();
        }*/
        
        if(queue.Count>0 && !qmRunning){
            StartCoroutine(queueManager());
            //Debug.Log("queue started");
        }
    }

    public void saveTexs() {
        byte[] b;
        Texture2D t;
        t = new Texture2D(256,256,TextureFormat.ARGB32,false);
        int i;
        for(i=0;i<mat16.Count;i++){
            t.SetPixels(mat16[i]);
            t.Apply();
            b = t.EncodeToPNG();
            
            File.WriteAllBytes(Application.dataPath+"/c16_"+i.ToString()+".png",b);
        }
        
        for(i=0;i<mat8.Count;i++){
            t.SetPixels(mat8[i]);
            t.Apply();
            b = t.EncodeToPNG();
            
            File.WriteAllBytes(Application.dataPath+"/c8_"+i.ToString()+".png",b);
        }
        
        for(i=0;i<masks.Count;i++){
            t.SetPixels(masks[i]);
            t.Apply();
            b = t.EncodeToPNG();
            
            File.WriteAllBytes(Application.dataPath+"/m_"+i.ToString()+".png",b);
        }
    }

    public Color[] getNoise16() {
        Color[] mat=mat16[0];
        
        if(mat16.Count>1) mat16.RemoveAt(0);
        if(mat16.Count<=4) queue.Add(16);
        
        return mat;
    }

    public Color[] getNoise8() {
        Color[] mat=mat8[0];
        
        if(mat8.Count>1) mat8.RemoveAt(0);
        if(mat8.Count<=1) queue.Add(8);
        
        return mat;
    }

    public Color[] getMask() {
        Color[] mat=masks[0];
        
        if(masks.Count>1) masks.RemoveAt(0);
        if(masks.Count<=4) queue.Add(0);
        
        return mat;
    }

    public IEnumerator makeNoise(int freq) {
        yield return null;
        Color[] tempC=new Color[256*256];
        IEnumerator noise = NoiseSC.setNoiseGen2D(freq,256,tempC);
        while(noise.MoveNext()){
            yield return null;
        }
        if(freq==8){
            mat8.Add(tempC);
        }else{
            mat16.Add(tempC);
        }
        var tex=new Texture2D(256,256,TextureFormat.ARGB32,false);
        
        tex.SetPixels(tempC);
        tex.Apply();
        this.transform.Find("Plane").GetComponent<Renderer>().material.SetTexture("_MainTex",tex);
    }

    public IEnumerator makeMask() {
        Debug.Log("makeMask");
        Color[] tempC=new Color[256*256];
        yield return (makeCamMask(256,tempC));
        
        masks.Add(tempC);
        var tex=new Texture2D(256,256,TextureFormat.ARGB32,false);
        
        tex.SetPixels(tempC);
        tex.Apply();
        this.transform.Find("Plane").GetComponent<Renderer>().material.SetTexture("_MainTex",tex);
    }

    public IEnumerator makeCamMask(int s,Color[] mat) {
        //mat = new Color[s*s];
        for(int i=0;i<s*s;i++){
            mat[i]=new Color(Mathf.Floor(Random.Range(0.0f, 2.0f)),
                Mathf.Floor(Random.Range(0.0f, 2.0f)),
                Mathf.Floor(Random.Range(0.0f, 2.0f)),
                Mathf.Floor(Random.Range(0.0f, 2.0f)));
        }
        yield return (CAM(mat,s,breakCount));
        yield return (blur(mat,s,breakCount));
    }
    
    static public IEnumerator CAM(Color[]mat,int size,int bc) {
        Color[] tmpMat =new Color[size*size];
        for(int u=0;u<4;u++){
            //Color tmpMat[] =new Color[size*size];
            for(int j=0; j<size; ++j){
                for(int i=0; i<size; ++i){
                    Color sum = new Color();
                    sum=getM(mat,j-1,i-1,256) + getM(mat,j-1,i,256) + getM(mat,j-1,i+1,256)+
                        getM(mat,j,i-1,256) + getM(mat,j,i,256) + getM(mat,j,i+1,256)+
                        getM(mat,j+1,i-1,256) + getM(mat,j+1,i,256) + getM(mat,j+1,i+1,256);
                    
                    sum.r=sum.r<5?0:1;
                    sum.g=sum.g<5?0:1;
                    sum.b=sum.b<5?0:1;
                    sum.a=sum.a<5?0:1;
                    
                    setItem(ref tmpMat,i, j, 256,sum);
                }
                if(j%(bc/2)==0){
                    yield return null;
                }
            }
            yield return null;
            for(int u2=0;u2<mat.Length;u2++){
                mat[u2]=tmpMat[u2];
            }
        }
    }

    static public IEnumerator blur(Color[] mat,int size,int bc) {
        for(int u=0;u<4;u++){
            Color[] tmpMat =new Color[size*size];
            
            for(int j=0; j<size; ++j){
                for(int i=0; i<size; ++i){
                    Color sum = getM(mat,j-1,i-1,256) + getM(mat,j-1,i,256) + getM(mat,j-1,i+1,256)+
                        getM(mat,j,i-1,256) + getM(mat,j,i,256) + getM(mat,j,i+1,256)+
                        getM(mat,j+1,i+1,256) + getM(mat,j+1,i+1,256) + getM(mat,j+1,i+1,256);
                    
                    setItem(ref tmpMat,i, j, 256,sum*(1/9.0f));
                }
                if(j%bc==0){
                    yield return null;
                }
            }
            yield return null;
            for(int u2=0;u2<mat.Length;u2++){
                mat[u2]=tmpMat[u2];
            }
        }
    }

    public IEnumerator queueManager() {
        qmRunning=true;
        Debug.Log("starting manager");
        while(queue.Count>0){
            switch(queue[0]){
                case 0:
                    yield return (makeMask());
                    break;
                case 8:
                    yield return (makeNoise(8));
                    break;
                case 16:
                    yield return (makeNoise(16));
                    break;
                default:
                    Debug.Log("waaat");
                    break;
            }
            queue.RemoveAt(0);
            //Debug.Log("Nebula queue: "+queue.Count.ToString());
        }
        Debug.Log("finished queue");
        qmRunning=false;
    }

    static public Color getM(Color[] m,int x,int y,int size) {
        if(x<0){
            x += size;
        }
        if(y<0){
            y += size;
        }
        return m[size*(y%size)+(x%size)];
    }

    static public void setItem(ref Color[] m,int x,int y,int size,Color val) {
        if(x<0){
            x += size;
        }
        if(y<0){
            y += size;
        }
        m[size*(y%size)+(x%size)] = val;
    }
}
