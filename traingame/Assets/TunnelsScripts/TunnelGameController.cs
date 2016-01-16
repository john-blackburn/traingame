using UnityEngine;
using System.Collections;

// Note: anchor point for train is front of first carriage (direction of travel = +x)
// anchor point for for tunnels is rear of tunnels (made of tunnel sections)
// anchor point for station is at centre of station (but note offset in y,z)

public class TunnelGameController : MonoBehaviour {

	public GameObject TrainCabinPrefab,TunnelSectionPrefab,StationPrefab;

	private GameObject mainCamera, train, tunnel, station;
	private BoxCollider tunnelCollider, stationCollider;
	private float tunnelLength, tunnelWidth, stationLength, stationWidth;

	// Use this for initialization
	void Start () 
	{
		mainCamera = GameObject.FindWithTag ("MainCamera");

		GameObject tunnelSection;

		tunnelSection = Instantiate (TunnelSectionPrefab) as GameObject;
		tunnelCollider = tunnelSection.GetComponent<BoxCollider> () as BoxCollider;
		tunnelLength = tunnelCollider.bounds.size.x;
		tunnelWidth = tunnelCollider.bounds.size.z;
		Destroy (tunnelSection);

		station = Instantiate (StationPrefab) as GameObject;
		station.transform.position = new Vector3 (0, -3, -1.6f);
		stationCollider = station.GetComponent<BoxCollider> () as BoxCollider;
		stationLength = stationCollider.bounds.size.x;
		stationWidth = stationCollider.bounds.size.z;

		print ("Tunnel length="+tunnelLength);
		print ("Tunnel width="+tunnelWidth);

		print ("Station length=" + stationLength);
		print ("Station width=" + stationWidth);

		train = new GameObject ();
		train.transform.position = Vector3.zero;

		for (int i=0; i<3; i++) {
			GameObject trainCabin = Instantiate (TrainCabinPrefab) as GameObject;
			trainCabin.transform.localPosition = new Vector3(-tunnelLength/4-tunnelLength*i/2,0,0);
			trainCabin.transform.parent = train.transform;
		}

		mainCamera.transform.position = new Vector3 (0, 0, 10);
		mainCamera.transform.LookAt (new Vector3 (10, 0, 0));

		print (train.transform.childCount);

		tunnel = new GameObject();
		tunnel.transform.position = new Vector3 (stationLength/2, 0, 0);

		for (int i=0; i<3; i++) {
			GameObject t=Instantiate(TunnelSectionPrefab) as GameObject;
			t.transform.parent=tunnel.transform;
			t.transform.localPosition=new Vector3(tunnelLength*(i+0.5f),0,0);
		}


//		foreach (Transform child in train.transform) {
//			print (child);
//		}
		StartCoroutine(playGame());
	}

	// ###########################################################

	IEnumerator playGame()
	{
		yield return StartCoroutine (trainArrives ());

		yield return StartCoroutine (rotateCameraTo (0, 180, 0, 20));
		yield return StartCoroutine (moveCameraTo (0, 0, 0, 60));
		yield return StartCoroutine (rotateCameraTo (0, -90, 0, 20));
		yield return StartCoroutine (moveCameraTo (-2, 0, 0, 40));
		yield return StartCoroutine (rotateCameraTo (0, 180, 0, 20));

		mainCamera.transform.parent = train.transform;

		int i, nacc, n, nmore;
		float x, astart, astop, x0, x1, u, xst;
		
		astart = 0.01f;       // acceleration
		u = 1f;          // top speed  (m/frame)
		n = 143;         // no of const speed frames
		nmore = 6;       // additional tunnel sections for slow down
		xst = 0;

		while (true) {
			float xtun0 = tunnel.transform.position.x;   // starting position of tunnel back-end

			nacc = (int)(u / astart);   // no time steps to get to max speed

			print ("accelerate");
			for (i=0; i<nacc; i++) {
				x = xst + 0.5f * astart * i * i;
				train.transform.position = new Vector3 (x, 0, 0);
				cycleTunnel (x);
				yield return null;
			}

			x0 = xst + 0.5f * astart * nacc * nacc;   // distance covered in acc'n phase
			print ("const speed");
			for (i=0; i<n; i++) {
				x = x0 + i * u;
				train.transform.position = new Vector3 (x, 0, 0);
				cycleTunnel (x);
				yield return null;
			}

			int sectionsDone;
			float totLength, distRemain;

			x1 = x0 + n * u;
			sectionsDone = (int)((x1 - xtun0) / tunnelLength);
			totLength = (sectionsDone + nmore) * tunnelLength;
			distRemain = totLength - (x1 - xtun0) + stationLength / 2;
			astop = -u * u / (2 * distRemain);

			print ("sectionsDone" + sectionsDone);

			station.transform.Translate (new Vector3 (stationLength + totLength, 0, 0));

			print ("slow down" + astop);
			for (i=0; i<=(int)(-u/astop); i++) {
				x = x1 + u * i + 0.5f * astop * i * i;
				train.transform.position = new Vector3 (x, 0, 0);
				cycleTunnel (x);
				yield return null;
			}

			tunnel.transform.Translate (new Vector3 (3 * tunnelLength + stationLength, 0, 0));
			xst=station.transform.position.x;
		}

//		yield return StartCoroutine (moveTrainTo (stationLength + 3 * tunnelLength, 0, 0, 200, 0, 0.5f));
//		station.transform.position = new Vector3 (stationLength + 3 * tunnelLength, -3, -1.6f);
//		yield return StartCoroutine (moveTrainTo (stationLength + 3 * tunnelLength, 0, 0, 200, 0.5f, 1));

//		tunnel.transform.position = new Vector3 (1.5f * stationLength + 3 * tunnelLength, 0, 0);
	}

	// ###########################################################

	void cycleTunnel(float x)   // x: train front position
	{
		float xtunnel = tunnel.transform.position.x;

		if (x > xtunnel + 2 * tunnelLength) {
			float xend = xtunnel + 4 * tunnelLength;   // end point of tunnel if we move it
			float xstation = station.transform.position.x;
//			print (xend+":"+xstation+":"+stationLength);
			if (!(xend > xstation - stationLength / 2.1 && xend < xstation + stationLength / 2.1))
				tunnel.transform.Translate (new Vector3 (tunnelLength, 0, 0));
		}

	}

	// ###########################################################
	
	// Update is called once per frame
	void Update () {
	}

	// ###########################################################
	
	float inOutExponential(float ratio)
	{
		if (ratio == 0 || ratio == 1) {
			return ratio;
		}
		ratio = ratio*2-1;
		if (ratio<0) {
			return 0.5f*Mathf.Pow(2, 10*ratio);
		}
		return 1-0.5f*Mathf.Pow(2, -10*ratio);
	}

	// ###########################################################
	
	IEnumerator moveCameraTo (float x, float y, float z, int nframes, bool relative=false)
	{
		Vector3 r1 = mainCamera.transform.position;
		Vector3 r2; 
		
		if (relative)
			r2 = r1 + new Vector3 (x, y, z);
		else
			r2 = new Vector3 (x, y, z);
		
		float ds = 1.0f / nframes;
		for (int i=0; i<=nframes; i++) {
			mainCamera.transform.position = r1 + (r2 - r1) * i * ds;
			yield return null;
		}
		
	}

	// ###########################################################

	IEnumerator rotateCameraTo(float x, float y, float z, int nframes)
	{
		Quaternion q1 = mainCamera.transform.rotation;
		Quaternion q2 = Quaternion.Euler (x,y,z);
		float s;

		float ds = 1.0f / nframes;
		for (int i=0;i<=nframes;i++) {
			mainCamera.transform.rotation = Quaternion.Slerp (q1, q2, i*ds);
			yield return null;
		}
	}

	// ###########################################################

	IEnumerator moveTrainTo(float x, float y, float z, int nframes, float s1=0f, float s2=1f)
	{
		// Move train from current point to (x,y,z) in nframes.
		// if s1=0 and s2=1, do whole animation
		// else do partial animation [s1,s2]

		Vector3 r1 = mainCamera.transform.position;
		Vector3 r2 = new Vector3 (x, y, z);
		
		float ds = (s2-s1) / nframes;

		for (int i=0; i<nframes; i++) {
			Vector3 pos = r1 + (r2 - r1) * inOutExponential (s1 + i * ds);
			train.transform.position = pos;
			yield return null;
		}
	}

	// ###########################################################

	IEnumerator trainArrives()
	{
		float s;

		for (s=0; s<1; s+=0.01f) {
			Vector3 pos=new Vector3(520-500*inOutExponential(s),0,0);
			train.transform.position=pos;
			yield return null;
		}
	}

	// ###########################################################

	IEnumerator trainLeaves()
	{
		float s;
		
		for (s=0; s<1; s+=0.002f) {
			Vector3 pos=new Vector3((stationLength+3*tunnelLength)*inOutExponential(s),0,0);
			train.transform.position=pos;
			yield return null;
		}
	}

}
