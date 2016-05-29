using UnityEngine;
using System.Collections;

public class fireballController : MonoBehaviour {

	public float speed;
	private float distMoved;

	// Use this for initialization
	void Start () {
		distMoved = 0;
	}

	// Update is called once per frame
	void Update () {
		Vector3 pos = transform.localPosition;
		pos.x -= speed;
		transform.localPosition = pos;

		distMoved += speed;

		if (distMoved>30)
			Destroy (gameObject);
	}
}
