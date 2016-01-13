using UnityEngine;
using System.Collections;

public class TunnelGameController : MonoBehaviour {

	public GameObject TrainCabinPrefab,TunnelSectionPrefab,StationPrefab;

	private GameObject mainCamera, train, tunnel, station;
	private BoxCollider tunnelCollider, stationCollider;
	private float tunnelLength, tunnelWidth, stationLength, stationWidth;

	// Use this for initialization
	void Start () {
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

		StartCoroutine (trainArrives());


	}
	
	// Update is called once per frame
	void Update () {
	}

	IEnumerator trainArrives()
	{
		for (int i=1000; i>=0; i--) {
			Vector3 pos=new Vector3(i,0,0);
			train.transform.position=pos;
			yield return null;
		}
	}

}
