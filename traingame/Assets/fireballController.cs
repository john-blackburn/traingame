using UnityEngine;
using System.Collections;

public class fireballController : MonoBehaviour {

	public float speed;
	private float distMoved;
	private GameObject mainCamera, gameController;
	private TunnelCameraController mainCameraScript;
	private TunnelGameController gameControllerScript;

	// Use this for initialization
	void Start () {
		distMoved = 0;
		mainCamera = GameObject.FindWithTag ("MainCamera");
		mainCameraScript = mainCamera.GetComponent<TunnelCameraController> ();

		gameController = GameObject.FindWithTag ("GameController");
		gameControllerScript = gameController.GetComponent<TunnelGameController> ();
	}

	// Update is called once per frame
	void Update () {
		Vector3 pos = transform.localPosition;
		Vector3 camPos = mainCamera.transform.localPosition + new Vector3 (0.5f, 0, 0);

		float dist = Vector3.Distance (pos, camPos);

//		print ("FB dist="+dist);
		if (dist < 0.1f)
			mainCameraScript.setFireball (false);

		pos.x -= speed;
		transform.localPosition = pos;

		distMoved += speed;

		if (distMoved>30)
			Destroy (gameObject);
	}
}
