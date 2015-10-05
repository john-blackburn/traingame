using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	public GameObject TrainPrefab;
	public GameObject cylinderPrefab;

	private GameObject mycamera;
	
	// Use this for initialization
	void Start () {
	
		GameObject train = (GameObject)Instantiate (TrainPrefab);
		train.transform.position = new Vector3 (0f, 0f, 0f);

		mycamera = GameObject.FindWithTag ("MainCamera");
		mycamera.transform.position = new Vector3 (0f, 0f, 0f);

		StartCoroutine (boardTrain ());
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	IEnumerator boardTrain()
	{
		Vector3 r1 = new Vector3 (5f, 0f, -1f);
		Vector3 r2 = new Vector3 (0f, 0f, 0f);
		
		for (float s=0f; s<1f; s+=0.01f) {
			mycamera.transform.position = r1 + (r2 - r1) * s;
			yield return null;
		}
		
		r1 = r2;
		r2 = new Vector3 (0f, 0f, 2f);
		
		for (float s=0f; s<1f; s+=0.01f) {
			mycamera.transform.position = r1 + (r2 - r1) * s;
			yield return null;
		}
		
		Quaternion q1 = Quaternion.identity;
		Quaternion q2 = Quaternion.Euler (0, 180, 0);
		
		GameObject cyl=(GameObject)Instantiate (cylinderPrefab);
		cyl.transform.position = new Vector3 (0, 0, -5);
		
		int ns = 10;
		float ds = 1.0f / ns;
		
		for (int i=0; i<=ns; i++) {
			float s=i*ds;
			mycamera.transform.rotation = Quaternion.Slerp(q1,q2,s);
			yield return null;
		}
		
		yield return new WaitForSeconds (1f);
		
		r1 = new Vector3 (0f, 0f, -5f);
		r2 = new Vector3 (0f, 0f, 1f);
		
		q1 = Quaternion.identity;
		q2 = Quaternion.Euler (40, 0, 0);
		
		for (float s=0f; s<=1f; s+=0.05f) {
			cyl.transform.position = r1 + (r2 - r1) * s;
			cyl.transform.rotation = Quaternion.Slerp(q1,q2,s);
			yield return null;
		}
	}

}
