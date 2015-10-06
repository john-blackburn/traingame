using UnityEngine;
using System.Collections;

public class cameraController : MonoBehaviour {

	public float tilt;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		float horiz = Input.GetAxis ("Horizontal");
//		Quaternion q = Quaternion.Euler (0, horiz * tilt, 0);
//		transform.rotation = q;
		transform.Rotate (new Vector3 (0, horiz * tilt, 0));
	}
	
}
