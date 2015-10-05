using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	public GameObject TrainPrefab;

	// Use this for initialization
	void Start () {
	
		GameObject train = (GameObject)Instantiate (TrainPrefab);
		train.transform.position = new Vector3 (0f, 0f, 0f);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
