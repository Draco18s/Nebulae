using UnityEngine;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class BGNebSc : MonoBehaviour {

	public BGNebBig big;

	public List<BGNebDark> darks;
	public List<BGNebDetail> details;

	public ThreadedNoise nh;//noiseHandler nh;

	public int curColor;

	public void  Start () {
		nh=GameObject.FindWithTag("noiseHandler").GetComponent<ThreadedNoise>();

		fullReset();
	}

	public void setupT() {
		this.transform.position=new Vector3(randRange(-4,4),randRange(-2,2),this.transform.position.z);
	}

	public void setTransforms() {
		setupT();
		big.setupTransforms();
		
		for(int i=0;i<darks.Count;i++){
			darks[i].transform.position=big.ts[i].position;
			darks[i].setupTransforms();
		}
		for(int i=0;i<details.Count;i++){
			details[i].transform.position=big.ts[i].position;
			details[i].setupTransforms();
		}
	}

	public void switchCols(int c) {
		Debug.Log("Current color: " + c);
		if(c>=0){
			curColor=c;
		}else if(c==-1){
			curColor++;
			if(curColor>8){
				curColor=0;
			}
		}else{
			randCol();
		}
		big.setCols(curColor);
		for(int i=0;i<darks.Count;i++){
			darks[i].setCols(curColor);
		}
		for(int i=0;i<details.Count;i++){
			details[i].setCols(curColor);
		}
	}

	public void randCol() {
		curColor=Mathf.RoundToInt(Random.value*8);
	}

	public void fullReset() {
		randCol();
		Color[] mat= nh.getNoise8();
		Color[] mask= nh.getMask();
		
		big.fullSetup(curColor,mat,mask);
		
		mat=nh.getNoise16();
		for(int i=0;i<darks.Count;i++){
			rotateColors(mask);
			if(i==1) mat=nh.getNoise16();
			darks[i].fullSetup(curColor,mat,mask);
		}
		mask=nh.getMask();
		for(int i=0;i<details.Count;i++){
			details[i].fullSetup(curColor,mat,mask);
			rotateColors(mask);
			if(i==0) mat=nh.getNoise16();
		}
	}

	static public float randRange(float min, float max) {
		return Random.value*(max-min)+min;
	}


	static public Color[] rotateColors(Color[] mat) {
		for(int u=0;u<mat.Length;u++){
			Color tmp=mat[u];
			mat[u]=new Color(tmp.g,tmp.b,tmp.a,tmp.r);
		}
		return mat;
	}
}