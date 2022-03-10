using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public static class NoiseSC {

	static List<Color[]> noiseGen2DSplit(float amp, int freq, int iters, float factor, int size){
		
		List<Color[]> outp = new List<Color[]>();
		Color[] o;
		
		for(int u=0;u<iters;u++){
			
			o=noiseGen2D(amp,freq,factor,size,1);
			
			outp.Add(o);
			freq=2*freq;
			amp=amp/factor;
		}
		
		return outp;

	}

	static Color[] noiseGen2DSum(float amp, int freq, int iters, float factor, int size){
		
		Color[] outp=new Color[size*size];
		Color[] o;
		

		o=noiseGen2D(amp,freq,factor,size,iters);
		
		return outp;

	}

	static List<Color[]> noiseGen2DSplitA(float amp, int freq, int iters, float factor, int size){
		
		List<Color[]> outp = new List<Color[]>();
		Color[] o;
		
		for(int u=0;u<iters;u++){
			
			o=noiseGen2DA(amp,freq,factor,size,1);
			
			outp.Add(o);
			freq=2*freq;
			amp=amp/factor;
		}
		
		return outp;

	}

	static Color[] noiseGen2DSumA(float amp, int freq, int iters, float factor, int size){
		
		Color[] outp=new Color[size*size];
		Color[] o;
		

		o=noiseGen2DA(amp,freq,factor,size,iters);
		
		return outp;

	}

	static Color[] noiseGen2DA(float amp, int freq, float factor,int size, int iters){
		Color[] o = new Color[size*size];
		int step=Mathf.FloorToInt( size/freq );
		float oamp=amp;
		int ofreq=freq;
		if(step<=0) step=1;
		
		Vector2 offset =new Vector2(Random.value*11111-5123,Random.value*11111-5163);
		
		for(int i=0;i<size*size;i++){
			o[i]=new Color(0,0,0,0);
			amp=oamp;
			freq=ofreq;
			step=Mathf.FloorToInt( size/freq );
			
			for(int j=0;j<iters;j++){
				int row = Mathf.FloorToInt(i/size);
				int col = i%size;
				
				int lastcol = Mathf.FloorToInt(col/(float)step)*step;
				int nextcol = lastcol + step;
				int lastrow = Mathf.FloorToInt(row/(float)step)*step;
				int nextrow = lastrow+step;
				Vector2 dif=new Vector2((col-lastcol)/(float)step,(row-lastrow)/(float)step);
				
				if(nextcol>=size) nextcol-=size;
				if(lastcol<0) lastcol+=size;
				if(nextrow>=size) nextrow-=size;
				if(lastrow<0) lastrow+=size;
				
				o[i] +=new Color(1,1,1,1)*bicubicNoise(new Vector2(lastcol,lastrow)+offset,dif,step)*amp;
				//o[i] +=new Color(0,0,0,Mathf.Pow(InterpolatedNoise(new Vector2(lastcol,lastrow)+offset,dif,step,Vector2(nextcol,nextrow)+offset)*amp,2));
				
				freq=2*freq;
				amp=amp/factor;
				step=Mathf.FloorToInt( size/freq );
				if(step<=0) step=1;
			}
		}
		
		return o;
	}

	static Color[] noiseGen2D(float amp, int freq, float factor, int size, int iters){
		Color[] o = new Color[size*size];
		int step=Mathf.FloorToInt( size/freq );
		float oamp=amp;
		int ofreq=freq;
		if(step<=0) step=1;
		
		Vector2 offset =new Vector2(Random.value*11111-5123,Random.value*11111-5163);
		Vector2 offset1 =new Vector2(Random.value*11111-5123,Random.value*11111-5163);
		Vector2 offset2 =new Vector2(Random.value*11111-5123,Random.value*11111-5163);
		Vector2 offset3 =new Vector2(Random.value*11111-5123,Random.value*11111-5163);
		
		for(int i=0;i<size*size;i++){
			o[i]=new Color(0,0,0,0);
			amp=oamp;
			freq=ofreq;
			step=Mathf.FloorToInt( size/freq );
			
			for(int j=0;j<iters;j++){
				int row = Mathf.FloorToInt(i/size);
				int col = i%size;
				
				int lastcol = Mathf.FloorToInt(col/(float)step)*step;
				//int nextcol = lastcol + step;
				int lastrow = Mathf.FloorToInt(row/(float)step)*step;
				//int nextrow = lastrow+step;
				Vector2 dif=new Vector2((col-lastcol)/(float)step,(row-lastrow)/(float)step);
				
				//if(nextcol>=size) nextcol-=size;
				if(lastcol<0) lastcol+=size;
				//if(nextrow>=size) nextrow-=size;
				if(lastrow<0) lastrow+=size;
				
				o[i] +=new Color(bicubicNoise(new Vector2(lastcol,lastrow)+offset,dif,step)*amp,
					bicubicNoise(new Vector2(lastcol,lastrow)+offset1,dif,step)*amp,
					bicubicNoise(new Vector2(lastcol,lastrow)+offset2,dif,step)*amp,
					bicubicNoise(new Vector2(lastcol,lastrow)+offset3,dif,step)*amp);
				
				freq=2*freq;
				amp=amp/factor;
				step=Mathf.FloorToInt( size/freq );
				if(step<=0) step=1;
			}
		}
		
		return o;
	}

	public static IEnumerator setNoiseGen2D(int freq,int size,Color[] m){

		float amp=1;
		float factor=2;
		int iters=3;
		
		int breakCount = 50 + 50*QualitySettings.GetQualityLevel();
		
	//	Color[] o = new Color[size*size];
		int step=Mathf.FloorToInt( size/freq );
		float oamp=amp;
		int ofreq=freq;
		if(step<=0) step=1;
		
		Vector2 offset =new Vector2(Random.value*11111-5123,Random.value*11111-5163);
		Vector2 offset1 =new Vector2(Random.value*11111-5123,Random.value*11111-5163);
		Vector2 offset2 =new Vector2(Random.value*11111-5123,Random.value*11111-5163);
		Vector2 offset3 =new Vector2(Random.value*11111-5123,Random.value*11111-5163);
		
		for(int i=0;i<size*size;i++){
			m[i]=new Color(0,0,0,0);
			amp=oamp;
			freq=ofreq;
			step=Mathf.FloorToInt(size/freq);

			for(int j=0;j<iters;j++){
				int row = Mathf.FloorToInt(i/size);
				int col = i%size;
				
				int lastcol = Mathf.FloorToInt(col/(float)step)*step;
				//int nextcol = lastcol + step;
				int lastrow = Mathf.FloorToInt(row/(float)step)*step;
				//int nextrow = lastrow+step;
				Vector2 dif=new Vector2((col-lastcol)/(float)step,(row-lastrow)/(float)step);
				
				//if(nextcol>=size) nextcol-=size;
				if(lastcol<0) lastcol+=size;
				//if(nextrow>=size) nextrow-=size;
				if(lastrow<0) lastrow+=size;
				
				Color tempC=new Color();
				tempC.r=bicubicNoise(new Vector2(lastcol,lastrow)+offset,dif,step)*amp;
				//yield;
				tempC.g=bicubicNoise(new Vector2(lastcol,lastrow)+offset1,dif,step)*amp;
				//yield;
				tempC.b=bicubicNoise(new Vector2(lastcol,lastrow)+offset2,dif,step)*amp;
				//yield;
				tempC.a=bicubicNoise(new Vector2(lastcol,lastrow)+offset3,dif,step)*amp;
				
				//yield;
				//m[i]=new Color.red;
				m[i]+=tempC;
				
				freq=2*freq;
				amp=amp/factor;
				step=Mathf.FloorToInt( size/freq );
				if(step<=0) step=1;
			}
			if(i%breakCount==0){
				yield return null;
			}
		}
		//Debug.Log("done with one");
	}

	public static Color[] smooth(Color[] o,int size,int step){
		for(int i=0;i<size*size;i++){
			int row = Mathf.FloorToInt(i/size);
			int col = i%size;
			
			int lastcol = Mathf.FloorToInt(col/(float)step)*step;
			int nextcol = lastcol + step;
			int lastrow = Mathf.FloorToInt(row/(float)step)*step;
			int nextrow = lastrow+step;
			
			if(nextcol>=size) nextcol-=size;
			if(lastcol<0) lastcol+=size;
			if(nextrow>=size) nextrow-=size;
			if(lastrow<0) lastrow+=size;
			
			Color corners = (o[lastcol+lastrow*size] + o[lastcol+nextrow*size] + o[nextcol+nextrow*size] + o[nextcol+lastrow*size])/16.0f;
			Color sides = (o[col+lastrow*size] + o[col+nextrow*size] + o[lastcol+row*size] + o[nextcol+row*size])/8.0f;
			
			o[i] = o[i]/4.0f + corners + sides;
		}
		
		return o;
	}

	public static float bicubicNoise(Vector2 cur, Vector2 dif, int step){
		float x=dif.x;
		return bicubicInterpolation(new float[]{
			bicubicInterpolation(new float[]{Mathf.PerlinNoise(cur.x-step, cur.y-step),Mathf.PerlinNoise(cur.x, cur.y-step),
				Mathf.PerlinNoise(cur.x+step, cur.y-step),Mathf.PerlinNoise(cur.x+2*step, cur.y-step)},x),
			bicubicInterpolation(new float[]{Mathf.PerlinNoise(cur.x-step, cur.y),Mathf.PerlinNoise(cur.x, cur.y),
				Mathf.PerlinNoise(cur.x+step, cur.y),Mathf.PerlinNoise(cur.x+2*step, cur.y)},x),
			bicubicInterpolation(new float[]{Mathf.PerlinNoise(cur.x-step, cur.y+step),Mathf.PerlinNoise(cur.x, cur.y+step),
				Mathf.PerlinNoise(cur.x+step, cur.y+step),Mathf.PerlinNoise(cur.x+2*step, cur.y+step)},x),
			bicubicInterpolation(new float[]{Mathf.PerlinNoise(cur.x-step, cur.y+2*step),Mathf.PerlinNoise(cur.x, cur.y+2*step),
				Mathf.PerlinNoise(cur.x+step, cur.y+2*step),Mathf.PerlinNoise(cur.x+2*step, cur.y+2*step)},x)
			}, dif.y);
	}

	static float bicubicInterpolation(float[] p, float x){
		return p[1] + 0.5f * x*(p[2] - p[0] + x*(2.0f*p[0] - 5.0f*p[1] + 4.0f*p[2] - p[3] + x*(3.0f*(p[1] - p[2]) + p[3] - p[0])));
	}

	static float SmoothedNoise(float x, float y,int step){
		float corners = ( Mathf.PerlinNoise(x-step, y-step)+Mathf.PerlinNoise(x+step, y-step)+Mathf.PerlinNoise(x-step, y+step)+Mathf.PerlinNoise(x+step, y+step) ) / 16.0f;
		float sides = ( Mathf.PerlinNoise(x-step, y) + Mathf.PerlinNoise(x+step, y) + Mathf.PerlinNoise(x, y-step) + Mathf.PerlinNoise(x, y+step) ) / 8.0f;
		float center = Mathf.PerlinNoise(x, y) / 4.0f;
		return corners + sides + center;
	}

	static float InterpolatedNoise(Vector2 cur, Vector2 dif,int step,Vector2 next){
		
		float v1 = Mathf.PerlinNoise(cur.x, cur.y);//SmoothedNoise(cur.x, cur.y,step);
		float v2 = Mathf.PerlinNoise(next.x, cur.y);//SmoothedNoise(next.x, cur.y,step);
		float v3 = Mathf.PerlinNoise(cur.x, next.y);//SmoothedNoise(cur.x, next.y,step);
		float v4 = Mathf.PerlinNoise(next.x, next.y);//SmoothedNoise(next.x, next.y,step);
		
		/*float i1 = Interpolate(v1 , v2 , dif.x); 
		float i2 = Interpolate(v3 , v4 , dif.x);
		
		return Interpolate(i1 , i2 , dif.y);*/
		
		return v1*(1-dif.x)*(1-dif.y) + v2*dif.x*(1-dif.y) + v3*(1-dif.x)*dif.y + v4*dif.x*dif.y;
	}

	static float Interpolate(float a,float b,float x){
		float f = (1 - Mathf.Cos( x*Mathf.PI) ) * 0.5f;
			
		return a*(1-f) + b*f;
	}
}