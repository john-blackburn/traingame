using UnityEngine;
using System.Collections;

public class GameController : MonoBehaviour {

	public GameObject TrainPrefab;
	public GameObject monsterPrefab;
	public GameObject pillarPrefab;
	public GameObject redPillarPrefab;
	public GameObject greenPillarPrefab;
	public GameObject bluePillarPrefab;
	public GameObject doorPrefab;

	private GameObject mycamera;
	private GameObject train, door;
	private int level;
	private DoorController doorScript;

	// ###########################################################

	void Start () {
	
		level = 1;
		train = (GameObject)Instantiate (TrainPrefab);
		train.transform.position = new Vector3 (0f, 0f, 0f);

		door = (GameObject)Instantiate (doorPrefab);
		door.transform.position = new Vector3 (0,0,5.28f);
		doorScript=door.GetComponent<DoorController> ();

		mycamera = GameObject.FindWithTag ("MainCamera");
		mycamera.transform.position = new Vector3 (0f, 0f, 0f);

		StartCoroutine (playGame ());
	}

	// ###########################################################
	
	IEnumerator playGame()
	{
		bool correct;

		while (true) {

			train.transform.position=new Vector3(0,0,0);

			mycamera.transform.position=new Vector3(10,0,-3);
			mycamera.transform.LookAt(train.transform,new Vector3(0,1,0));

			// This will eventually be the main menu
			// For now just run for 3 waves of pillars
			yield return StartCoroutine(showPillars(3));
			mycamera.transform.rotation=Quaternion.identity;

			// user exits from menu and starts game
			yield return StartCoroutine (boardTrain ());

			while (true) {
				yield return StartCoroutine (showPillars (3));

				correct = Random.value < 0.7f;
				print ("got it right? " + correct);

				if (correct) {
					level++;
					yield return StartCoroutine (nextCarriage ());
				} else {
					yield return StartCoroutine (jumpScare ());
					break;
				}
			}

		}
	}

	// ###########################################################
	
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
	}

	// ###########################################################

	IEnumerator showPillars(int nWaves)
	{

		for (int j=0; j<nWaves; j++) {
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
					pillar.transform.position = new Vector3 (Random.Range (-7, -3), 0, level*10);
				else
					pillar.transform.position = new Vector3 (Random.Range (3, 7), 0, level*10);

				yield return new WaitForSeconds (0.5f);
			}
			yield return new WaitForSeconds (2f);
		}
	}

	// ###########################################################

	IEnumerator jumpScare()
	{
		Quaternion q1 = mycamera.transform.rotation;
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

	// ###########################################################

	IEnumerator nextCarriage()
	{
		GameObject newTrain = (GameObject)Instantiate (TrainPrefab);
		newTrain.transform.position = new Vector3 (0f, 0f, (level-1)*10f);

		StartCoroutine(doorScript.open());

		Vector3 pos = mycamera.transform.position;

		yield return StartCoroutine(moveCameraTo (0f, pos.y, pos.z, 50));
		yield return StartCoroutine(moveCameraTo (0f, 0f, (level - 1) * 10f + 2f, 200));
		yield return StartCoroutine(moveCameraTo (1f, 0f, (level - 1) * 10f + 2f,  50));

//		Vector3 r1 = mycamera.transform.position;
//		Vector3 r2 = new Vector3 (1f, 0f, (level-1)*10f+2f);
		
//		for (float s=0f; s<1f; s+=0.005f) {
//			mycamera.transform.position = r1 + (r2 - r1) * s;
//			yield return null;
//		}

		Destroy (train);
		train = newTrain;

	}

	// ###########################################################

	IEnumerator moveCameraTo(float x, float y, float z, int nframes)
	{
		Vector3 r1 = mycamera.transform.position;
		Vector3 r2 = new Vector3 (x,y,z);
		
		float ds = 1.0f / nframes;
		for (int i=0; i<=nframes; i++) {
			mycamera.transform.position = r1 + (r2 - r1) * i * ds;
			yield return null;
		}

	}
}
