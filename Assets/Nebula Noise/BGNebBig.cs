using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class BGNebBig : MonoBehaviour {
	public Transform[] ts;

	public Color[] mat;
	public Color[] mask;

	public Color[] cols;
	public Color[] highs;

	public Texture2D[] texs;

	public int freq;


	public void fullSetup(int c,Color[] mat,Color[] mask) {
		setupTransforms();
		setupTexs(mat,mask);
		setCols(c);
	}

	public bool setupTransforms() {
		this.transform.Rotate(new Vector3(0,0,Random.value*360),Space.World);
		//this.transform.position= Vector3(randRange(-4,4),randRange(-2,2),0);
		this.transform.localScale=Vector3.one*(Random.value*1.6f + .4f);

		ts[0].Rotate(new Vector3(0,0,Random.value*360),Space.World);
		ts[1].Rotate(new Vector3(0,0,Random.value*360),Space.World);
		ts[2].Rotate(new Vector3(0,0,Random.value*360),Space.World);
		ts[3].Rotate(new Vector3(0,0,Random.value*360),Space.World);
		
		ts[0].localPosition=new Vector3(randRange(-20,20),randRange(-20,20),0);
		ts[1].localPosition=new Vector3(randRange(-20,20),randRange(-20,20),0);
		ts[2].localPosition=new Vector3(randRange(-20,20),randRange(-20,20),0);
		ts[3].localPosition=new Vector3(randRange(-20,20),randRange(-20,20),0);
		
		ts[0].localScale=new Vector3(Random.value*4+2,Random.value*4+2,1);
		ts[1].localScale=new Vector3(Random.value*4+2,Random.value*4+2,1);
		ts[2].localScale=new Vector3(Random.value*4+2,Random.value*4+2,1);
		ts[3].localScale=new Vector3(Random.value*4+2,Random.value*4+2,1);
		
		return true;
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
		
		Color off=new Color(randPM(.35f),randPM(.35f),randPM(.35f),0);
		ts[0].GetComponent<Renderer>().material.SetColor("_Color",col+off);
		ts[0].GetComponent<Renderer>().material.SetColor("_HighLight",high-off);
		
		off=new Color(randPM(.35f),randPM(.35f),randPM(.35f),0);
		ts[1].GetComponent<Renderer>().material.SetColor("_Color",col+off);
		ts[1].GetComponent<Renderer>().material.SetColor("_HighLight",high-off);
		
		off=new Color(randPM(.35f),randPM(.35f),randPM(.35f),0);
		ts[2].GetComponent<Renderer>().material.SetColor("_Color",col-off);
		ts[2].GetComponent<Renderer>().material.SetColor("_HighLight",high+off);
		
		off=new Color(randPM(.35f),randPM(.35f),randPM(.35f),0);
		ts[3].GetComponent<Renderer>().material.SetColor("_Color",col-off);
		ts[3].GetComponent<Renderer>().material.SetColor("_HighLight",high+off);
	}

	public void setupTexs(Color[] mat,Color[] mask) {
		Texture2D[] texs=new Texture2D[8];
		for(int i=0;i<8;i++){
			texs[i]=new Texture2D(256,256,TextureFormat.ARGB32,false);
		}
		
		texs[0].SetPixels(mat);
		texs[0].Apply();
		ts[0].GetComponent<Renderer>().material.SetTexture("_MainTex",texs[0]);
		
		mat=BGNebSc.rotateColors(mat);
		texs[1].SetPixels(mat);
		texs[1].Apply();
		ts[1].GetComponent<Renderer>().material.SetTexture("_MainTex",texs[1]);
		//yield 0;
		
		mat=BGNebSc.rotateColors(mat);
		texs[2].SetPixels(mat);
		texs[2].Apply();
		ts[2].GetComponent<Renderer>().material.SetTexture("_MainTex",texs[2]);
		
		mat=BGNebSc.rotateColors(mat);
		texs[3].SetPixels(mat);
		texs[3].Apply();
		ts[3].GetComponent<Renderer>().material.SetTexture("_MainTex",texs[3]);
		//yield 0;
		
		texs[4].SetPixels(mask);
		texs[4].Apply();
		ts[0].GetComponent<Renderer>().material.SetTexture("_MaskTex",texs[4]);
		
		mask=BGNebSc.rotateColors(mask);
		texs[5].SetPixels(mask);
		texs[5].Apply();
		ts[1].GetComponent<Renderer>().material.SetTexture("_MaskTex",texs[5]);
		//yield 0;
		
		mask=BGNebSc.rotateColors(mask);
		texs[6].SetPixels(mask);
		texs[6].Apply();
		ts[2].GetComponent<Renderer>().material.SetTexture("_MaskTex",texs[6]);
		
		mask=BGNebSc.rotateColors(mask);
		texs[7].SetPixels(mask);
		texs[7].Apply();
		ts[3].GetComponent<Renderer>().material.SetTexture("_MaskTex",texs[7]);
	}
}