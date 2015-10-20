using UnityEngine;
using System.Collections;

public class pillarController : MonoBehaviour {

	private float distMoved;
	
	// Use this for initialization
	void Start () {
		distMoved = 0;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 pos = transform.position;
		pos.z -= 0.2f;
		transform.position = pos;

		distMoved += 0.2f;
		
		if (distMoved>20)
			Destroy (gameObject);
	}
}
