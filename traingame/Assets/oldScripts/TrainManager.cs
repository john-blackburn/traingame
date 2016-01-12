using UnityEngine;
using System.Collections;

public class TrainManager : MonoBehaviour {

	// this is just a test
     [Header("Train Dimensions")]
	public float Tr_width;
	public float Tr_Length;
	BoxCollider _trCollider;
	// Use this for initialization


	void Start () {
	
		_trCollider = transform.GetComponent<BoxCollider>() as BoxCollider;
		Debug.Log("Length : "+_trCollider.bounds.size.z);
		Debug.Log("width : "+_trCollider.bounds.size.x);
	}

	void Update()
	{
	}


}
