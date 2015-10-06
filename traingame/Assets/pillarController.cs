using UnityEngine;
using System.Collections;

public class pillarController : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 pos = transform.position;
		pos.z -= 0.2f;
		transform.position = pos;

		if (pos.z < -10)
			Destroy (gameObject);
	}
}
