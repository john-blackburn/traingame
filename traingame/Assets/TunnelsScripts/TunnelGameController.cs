using UnityEngine;
using System.Collections;

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
			trainCabin.transform.localPosition = new Vector3(tunnelLength*i/2,0,0);
			trainCabin.transform.parent = train.transform;
		}

		mainCamera.transform.position = new Vector3 (0, 0, 10);
		mainCamera.transform.LookAt (new Vector3 (10, 0, 0));

		print (train.transform.childCount);

		tunnel = new GameObject();
		tunnel.transform.position = new Vector3 (stationLength, 0, 0);

		for (int i=0; i<3; i++) {
			GameObject t=Instantiate(TunnelSectionPrefab) as GameObject;
			t.transform.parent=tunnel.transform;
			t.transform.localPosition=new Vector3(tunnelLength*(i-0.5f),0,0);
		}


//		foreach (Transform child in train.transform) {
//			print (child);
//		}
		StartCoroutine(playGame());
	}

	IEnumerator playGame()
	{
		yield return StartCoroutine (trainArrives());

		yield return StartCoroutine(rotateCameraTo (0, 180, 0, 20));
		yield return StartCoroutine(moveCameraTo (0, 0, 0, 60));
		yield return StartCoroutine(rotateCameraTo (0, -90, 0, 20));
		yield return StartCoroutine(moveCameraTo (-2, 0, 0, 40));
		yield return StartCoroutine(rotateCameraTo (0, 0, 0, 20));

		mainCamera.transform.parent = train.transform;
		yield return StartCoroutine (trainLeaves ());
	}
	
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


	IEnumerator trainArrives()
	{
		float s;

		for (s=0; s<1; s+=0.01f) {
			Vector3 pos=new Vector3(500-500*inOutExponential(s),0,0);
			train.transform.position=pos;
			yield return null;
		}
	}

	IEnumerator trainLeaves()
	{
		float s;
		
		for (s=0; s<1; s+=0.002f) {
			Vector3 pos=new Vector3(200*inOutExponential(s),0,0);
			train.transform.position=pos;
			yield return null;
		}
	}

}
