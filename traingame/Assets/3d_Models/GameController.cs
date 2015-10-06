using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	public GameObject TrainPrefab;
	public GameObject monsterPrefab;
	public GameObject pillarPrefab;
	public GameObject redPillarPrefab;
	public GameObject greenPillarPrefab;
	public GameObject bluePillarPrefab;

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

		r1 = r2;
		r2 = new Vector3 (1f, 0f, 2f);
		
		for (float s=0f; s<1f; s+=0.1f) {
			mycamera.transform.position = r1 + (r2 - r1) * s;
			yield return null;
		}

		for (int j=0; j<3; j++) {
			for (int i=0; i<5; i++) {

				GameObject pillar;

				if (Random.value < 0.1f)
					pillar = (GameObject)Instantiate (redPillarPrefab);
				else if (Random.value < 0.2f)
					pillar = (GameObject)Instantiate (greenPillarPrefab);
				else if (Random.value < 0.3f)
					pillar = (GameObject)Instantiate (bluePillarPrefab);
				else
					pillar = (GameObject)Instantiate (pillarPrefab);

				if (Random.value < 0.5)
					pillar.transform.position = new Vector3 (Random.Range (-7, -3), 0, 10);
				else
					pillar.transform.position = new Vector3 (Random.Range (3, 7), 0, 10);

				yield return new WaitForSeconds (0.5f);
			}
			yield return new WaitForSeconds (5f);
		}

		StartCoroutine (jumpScare());

	}

	IEnumerator jumpScare()
	{
		Quaternion q1 = Quaternion.identity;
		Quaternion q2 = Quaternion.Euler (0, 180, 0);
		
		GameObject monster=(GameObject)Instantiate (monsterPrefab);
		monster.transform.position = new Vector3 (0, 0, -5);
		
		int ns = 10;
		float ds = 1.0f / ns;
		
		for (int i=0; i<=ns; i++) {
			float s=i*ds;
			mycamera.transform.rotation = Quaternion.Slerp(q1,q2,s);
			yield return null;
		}
		
		yield return new WaitForSeconds (1f);
		
		Vector3 r1 = new Vector3 (0f, 0f, -5f);
		Vector3 r2 = new Vector3 (0f, 0f, 1f);
		
		q1 = Quaternion.identity;
		q2 = Quaternion.Euler (40, 0, 0);
		
		for (float s=0f; s<=1f; s+=0.05f) {
			monster.transform.position = r1 + (r2 - r1) * s;
			monster.transform.rotation = Quaternion.Slerp(q1,q2,s);
			yield return null;
		}
	}

}
