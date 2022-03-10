using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class BGNebDark : MonoBehaviour {
	public Transform t1;
	public Transform t2;
	public Transform t3;
	public Transform t4;

	public Color[] mat;
	public Color[] mask;

	public Color[] cols;
	public Color[] highs;

	public Texture2D[] texs;

	public int freq;

	public void fullSetup(int c,Color[] mat, Color[] mask) {
		setupTransforms();
		setupTexs(mat,mask);
		setCols(c);
	}

	public void setupTransforms() {
		this.transform.Rotate(new Vector3(0,0,Random.value*360),Space.World);
		this.transform.position+= new Vector3(randRange(-3f,3f),randRange(-2f,2f),0);
		this.transform.localScale=Vector3.one*(Random.value*1.6f + .7f);

		t1.Rotate(new Vector3(0,0,Random.value*360),Space.World);
		t2.Rotate(new Vector3(0,0,Random.value*360),Space.World);
		t3.Rotate(new Vector3(0,0,Random.value*360),Space.World);
		t4.Rotate(new Vector3(0,0,Random.value*360),Space.World);
		
		t1.localPosition=new Vector3(randRange(-1f,1f),randRange(-1f,1f),0);
		t2.localPosition=new Vector3(randRange(-3f,3f),randRange(-2f,2f),0);
		t3.localPosition=new Vector3(randRange(-6f,6f),randRange(-4f,4f),0);
		t4.localPosition=new Vector3(randRange(-10f,10f),randRange(-4f,4f),0);
		
		float sc= Random.value*2+.5f;
		t1.localScale=new Vector3(sc,sc*(1+randPM(.35f)),1);
		sc = Random.value*2+.5f;
		t2.localScale=new Vector3(sc,sc*(1+randPM(.35f)),1);
		sc = Random.value*2+.5f;
		t3.localScale=new Vector3(sc,sc*(1+randPM(.35f)),1);
		sc = Random.value*2+.5f;
		t4.localScale=new Vector3(sc,sc*(1+randPM(.35f)),1);
	}

	public float randRange(float min,float max) {
		return Random.value*(max-min)+min;
	}
	public float randPM(float i) {
		return Random.value*(i*2)-i;
	}

	public void setCols(int i) {
		Color col=cols[i];
		Color high=highs[i];
		
		Color off= new Color(Random.value*.1f,Random.value*.1f,Random.value*.1f,0);
		t1.GetComponent<Renderer>().material.SetColor("_Color",col-off);
		t1.GetComponent<Renderer>().material.SetColor("_HighLight",high-off);
		
		off=new Color(Random.value*.1f,Random.value*.1f,Random.value*.1f,0);
		t2.GetComponent<Renderer>().material.SetColor("_Color",col-off);
		t2.GetComponent<Renderer>().material.SetColor("_HighLight",high-off);
		
		off=new Color(Random.value*.1f,Random.value*.25f,Random.value*.1f,0);
		t3.GetComponent<Renderer>().material.SetColor("_Color",col-off);
		t3.GetComponent<Renderer>().material.SetColor("_HighLight",high-off);
		
		off=new Color(Random.value*.1f,Random.value*.25f,Random.value*.1f,0);
		t4.GetComponent<Renderer>().material.SetColor("_Color",col-off);
		t4.GetComponent<Renderer>().material.SetColor("_HighLight",high-off);
	}

	public void setupTexs(Color[] mat,Color[] mask) {
		Texture2D[]  texs= new Texture2D[8];
		for(int i=0;i<8;i++){
			texs[i]=new Texture2D(256,256,TextureFormat.ARGB32,false);
		}
		
		
		texs[0].SetPixels(mat);
		texs[0].Apply();
		t1.GetComponent<Renderer>().material.SetTexture("_MainTex",texs[0]);
		
		mat=BGNebSc.rotateColors(mat);
		texs[1].SetPixels(mat);
		texs[1].Apply();
		t2.GetComponent<Renderer>().material.SetTexture("_MainTex",texs[1]);
		
		mat=BGNebSc.rotateColors(mat);
		texs[2].SetPixels(mat);
		texs[2].Apply();
		t3.GetComponent<Renderer>().material.SetTexture("_MainTex",texs[2]);
		
		mat=BGNebSc.rotateColors(mat);
		texs[3].SetPixels(mat);
		texs[3].Apply();
		t4.GetComponent<Renderer>().material.SetTexture("_MainTex",texs[3]);
		
		texs[4].SetPixels(mask);
		texs[4].Apply();
		t1.GetComponent<Renderer>().material.SetTexture("_MaskTex",texs[4]);
		
		mask=BGNebSc.rotateColors(mask);
		texs[5].SetPixels(mask);
		texs[5].Apply();
		t2.GetComponent<Renderer>().material.SetTexture("_MaskTex",texs[5]);
		
		mask=BGNebSc.rotateColors(mask);
		texs[6].SetPixels(mask);
		texs[6].Apply();
		t3.GetComponent<Renderer>().material.SetTexture("_MaskTex",texs[6]);
		
		mask=BGNebSc.rotateColors(mask);
		texs[7].SetPixels(mask);
		texs[7].Apply();
		t4.GetComponent<Renderer>().material.SetTexture("_MaskTex",texs[7]);
	}
}