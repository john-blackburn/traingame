using UnityEngine;
using System.Collections;

public class TunnelGameController : MonoBehaviour {

	public GameObject TrainCabinPrefab,TunnelSectionPrefab;

	private GameObject mainCamera, trainCabin, tunnelSection;
	private BoxCollider tunnelCollider;
	private float tunnelLength, tunnelWidth;

	// Use this for initialization
	void Start () {
		mainCamera = GameObject.FindWithTag ("MainCamera");

		trainCabin = Instantiate (TrainCabinPrefab) as GameObject;
		trainCabin.transform.position = Vector3.zero;

		tunnelSection = Instantiate (TunnelSectionPrefab) as GameObject;
		tunnelSection.transform.position = Vector3.zero;

		tunnelCollider = tunnelSection.GetComponent<BoxCollider> () as BoxCollider;
		print ("length="+tunnelCollider.bounds.size.x);
		print ("width="+tunnelCollider.bounds.size.z);

		mainCamera.transform.parent = trainCabin.transform;
		mainCamera.transform.position = Vector3.zero;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
