﻿using UnityEngine;
using System.Collections;

public class TunnelCameraController : MonoBehaviour {

	private bool moving;

	// Use this for initialization
	void Start () {
		moving=false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	IEnumerator switchSide()
	{
		moving=true;
		Vector3 pos=transform.localPosition;

		if (pos.z>0){
			yield return StartCoroutine(rotateTo(0,180,0,60));
			yield return StartCoroutine(moveTo(pos.x,pos.y,-pos.z,100));
		}
		else {
			yield return StartCoroutine(rotateTo(0,0,0,60));
			yield return StartCoroutine(moveTo(pos.x,pos.y,-pos.z,100));
		}

		moving=false;
	}

	public IEnumerator moveTo (float x, float y, float z, int nframes, bool relative=false)
	{
		Vector3 r1 = transform.localPosition;
		Vector3 r2; 
		
		if (relative)
			r2 = r1 + new Vector3 (x, y, z);
		else
			r2 = new Vector3 (x, y, z);
		
		float ds = 1.0f / nframes;
		for (int i=0; i<=nframes; i++) {
			transform.localPosition = r1 + (r2 - r1) * i * ds;
			yield return null;
		}
	}

	public IEnumerator rotateTo (float x, float y, float z, int nframes)
	{
		Quaternion q1 = transform.rotation;
		Quaternion q2 = Quaternion.Euler (x, y, z);
		
		float ds = 1.0f / nframes;
		for (int i=0; i<=nframes; i++) {
			transform.rotation = Quaternion.Slerp (q1, q2, i * ds);
			yield return null;
		}
	}
		
	public void switchPressed()
	{
		if (! moving)
			StartCoroutine(switchSide());
	}
}