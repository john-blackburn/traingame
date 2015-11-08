using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{

	public GameObject TrainPrefab;
	public GameObject monsterPrefab;
	public GameObject pillarPrefab;
	public GameObject redPillarPrefab;
	public GameObject greenPillarPrefab;
	public GameObject bluePillarPrefab;
	public GameObject doorPrefab;
	public GameObject mainMenu;
	public GameObject rgbMenu;
	public GameObject optionsMenu;
	public UILabel countDown;
	private GameObject mycamera;
	private GameObject train, door;
	private int level;
	private bool showMenu;
	private int nentry = 0;
	private char[] sequence, typed_sequence;

	// ###########################################################

	void Start ()
	{
			
		level = 1;
		train = (GameObject)Instantiate (TrainPrefab);
		train.transform.position = new Vector3 (0f, 0f, 0f);

		mycamera = GameObject.FindWithTag ("MainCamera");
		mycamera.transform.position = new Vector3 (0f, 0f, 0f);

		mainMenu.SetActive (false);
		rgbMenu.SetActive (false);
		optionsMenu.SetActive (false);

		StartCoroutine (playGame ());
	}

	// ###########################################################
	
	IEnumerator playGame ()
	{
		bool correct;

		while (true) {

			//-------------------------------------------------------
			// Main menu
			//-------------------------------------------------------

			level = 1;
			train.transform.position = new Vector3 (0, 0, 0);

			mycamera.transform.position = new Vector3 (6, 0, -5);
			mycamera.transform.LookAt (train.transform, new Vector3 (0, 1, 0));

			// This will eventually be the main menu
			// For now just run for 3 waves of pillars
			mainMenu.SetActive (true);

			GameObject train1 = Instantiate (TrainPrefab) as GameObject;
			GameObject train2 = Instantiate (TrainPrefab) as GameObject;

			train1.transform.position = (new Vector3 (0, 0, 11));
			train2.transform.position = (new Vector3 (0, 0, -11));

			GameObject monster = Instantiate (monsterPrefab) as GameObject;
			yield return null;   // make sure Start gets called

			monster.transform.position = new Vector3 (0, 0, 0);
			MonsterController monsterScript = monster.GetComponent<MonsterController> ();
			monsterScript.prowl (true);

			showMenu = true;
			while (true) {

				for (int i=0; i<5; i++) {

					if (!showMenu)
						goto jumpout;
					
					GameObject pillar;
					float r = Random.value;

					if (r < 0.25)
						pillar = (GameObject)Instantiate (redPillarPrefab);
					else if (r < 0.5)
						pillar = (GameObject)Instantiate (greenPillarPrefab);
					else if (r < 0.75)
						pillar = (GameObject)Instantiate (bluePillarPrefab);
					else
						pillar = (GameObject)Instantiate (pillarPrefab);

					pillar.transform.position = new Vector3 (Random.Range (3, 6), 0, 10);

					yield return new WaitForSeconds (0.5f);
				}
				yield return new WaitForSeconds (2f);
			}

			// user exits from menu and starts game
			jumpout:
			mainMenu.SetActive (false);
			Destroy (monster);

			Destroy (train1);
			Destroy (train2);

			door = (GameObject)Instantiate (doorPrefab);
			door.transform.position = new Vector3 (0, 0, 5.28f);

			// ---------------------------------------------------
			// Play game
			// ---------------------------------------------------

			mycamera.transform.rotation = Quaternion.identity;

			yield return StartCoroutine (boardTrain ());

			while (true) {
				yield return StartCoroutine (showPillars (3));

				typed_sequence = new char[3];
				rgbMenu.SetActive (true);

				nentry = 0;
				Vector3 pos = mycamera.transform.position;

				for (int i=0; i<=200; i++) {
					if (nentry >= 3)
						break;

					float y = 0.2f * Mathf.Sin (i * 0.1f);
					pos.y = y;
					mycamera.transform.position = pos;

					countDown.text = "Time: " + (200 - i);

					yield return null;
				}				

				rgbMenu.SetActive (false);

				string temp = "";
				for (int i=0; i < nentry; i++)
					temp += typed_sequence [i];
				print ("typed sequence=" + temp);

				correct = true;
				for (int i=0; i<3; i++)
					if (sequence [i] != typed_sequence [i])
						correct = false;

				print ("got it right? " + correct);

				if (correct) {
					level++;
					yield return StartCoroutine (nextCarriage ());
				} else {
					float r = Random.value;
					if (r < 0.5)
						yield return StartCoroutine (jumpScare ());
					else
						yield return StartCoroutine (jumpScare2 ());

					Destroy (door);
					break;
				}
			}

		}
	}

	// ###########################################################
	
	IEnumerator boardTrain ()
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

	IEnumerator showPillars (int nWaves)
	{
		sequence = new char[nWaves];

		for (int i=0; i<sequence.Length; i++) {
			float r = Random.value;
			if (r < 0.333)
				sequence [i] = 'R';
			else if (r < 0.666)
				sequence [i] = 'G';
			else
				sequence [i] = 'B';
		}

		string temp = "";
		for (int i=0; i<sequence.Length; i++)
			temp += sequence [i];
		print ("sequence=" + temp);


		for (int j=0; j<nWaves; j++) {

			int p = Random.Range (0, 5);

			for (int i=0; i<5; i++) {

				GameObject pillar;

				if (i == p) {
					if (sequence [j] == 'R')
						pillar = (GameObject)Instantiate (redPillarPrefab);
					else if (sequence [j] == 'G')
						pillar = (GameObject)Instantiate (greenPillarPrefab);
					else
						pillar = (GameObject)Instantiate (bluePillarPrefab);
				} else
					pillar = (GameObject)Instantiate (pillarPrefab);

				if (Random.value < 0.5)
					pillar.transform.position = new Vector3 (Random.Range (-7, -3), 0, level * 10);
				else
					pillar.transform.position = new Vector3 (Random.Range (3, 7), 0, level * 10);

				yield return new WaitForSeconds (0.5f);
			}
			yield return new WaitForSeconds (2f);
		}
	}

	// ###########################################################

	IEnumerator jumpScare ()
	{
		Quaternion q1 = mycamera.transform.rotation;
		Quaternion q2 = Quaternion.Euler (0, 180, 0);

		Vector3 r2 = mycamera.transform.position;
		r2.x = 0;
		r2.z -= 0.5f;

		Vector3 r1 = r2 + new Vector3 (0f, 0f, -5f);

		GameObject monster = (GameObject)Instantiate (monsterPrefab);
		monster.transform.position = r1;
		
		int ns = 10;
		float ds = 1.0f / ns;
		
		for (int i=0; i<=ns; i++) {
			float s = i * ds;
			mycamera.transform.rotation = Quaternion.Slerp (q1, q2, s);
			yield return null;
		}
		
		yield return new WaitForSeconds (1f);

		q1 = Quaternion.identity;
		q2 = Quaternion.Euler (40, 0, 0);
		
		for (float s=0f; s<=1f; s+=0.05f) {
			monster.transform.position = r1 + (r2 - r1) * s;
			monster.transform.rotation = Quaternion.Slerp (q1, q2, s);
			yield return null;
		}

		yield return StartCoroutine (moveCameraTo (0, -0.8f, 0, 120, true));
		yield return new WaitForSeconds (2);
		Destroy (monster);
	}

	// ###########################################################

	IEnumerator jumpScare2 ()
	{
		GameObject monster = Instantiate (monsterPrefab) as GameObject;
		monster.transform.position = new Vector3 (0, 0, (level - 1) * 10 + 6);

		Vector3 pos = mycamera.transform.position;
		pos.x = 0;
		yield return StartCoroutine (moveCameraTo (pos.x, pos.y, pos.z, 30));

		pos.z = (level - 1) * 10 + 4;
		yield return StartCoroutine (moveCameraTo (pos.x, pos.y, pos.z, 60));

		DoorController doorScript = door.GetComponent<DoorController> ();
		yield return StartCoroutine (doorScript.open ());

		yield return StartCoroutine (moveCameraTo (0, -0.8f, 0, 120, true));
		yield return new WaitForSeconds (2);
		Destroy (monster);
	}

	// ###########################################################

	IEnumerator nextCarriage ()
	{
		GameObject newTrain = Instantiate (TrainPrefab) as GameObject;
		newTrain.transform.position = new Vector3 (0f, 0f, (level - 1) * 10f);

		GameObject newDoor = Instantiate (doorPrefab) as GameObject;
		newDoor.transform.position = new Vector3 (0, 0, (level - 1) * 10f + 5.28f);

		DoorController doorScript = door.GetComponent<DoorController> ();
		StartCoroutine (doorScript.open ());

		Vector3 pos = mycamera.transform.position;

		float zdisp, xdisp;

		float r = Random.value;
		if (r < 0.5f)
			zdisp = 2f;
		else
			zdisp = -4.7f;

		r = Random.value;
		if (r < 0.5f)
			xdisp = 1f;
		else
			xdisp = -1f;

		print ("xdisp, zdisp=" + xdisp + " " + zdisp);

		yield return StartCoroutine (moveCameraTo (0f, pos.y, pos.z, 50));
		yield return StartCoroutine (moveCameraTo (0f, 0f, (level - 1) * 10f + zdisp, 200));
		yield return StartCoroutine (moveCameraTo (xdisp, 0f, 0f, 50, true));

		Destroy (train);
		train = newTrain;

		Destroy (door);
		door = newDoor;
	}

	// ###########################################################

	IEnumerator moveCameraTo (float x, float y, float z, int nframes, bool relative=false)
	{
		Vector3 r1 = mycamera.transform.position;
		Vector3 r2; 

		if (relative)
			r2 = r1 + new Vector3 (x, y, z);
		else
			r2 = new Vector3 (x, y, z);
		
		float ds = 1.0f / nframes;
		for (int i=0; i<=nframes; i++) {
			mycamera.transform.position = r1 + (r2 - r1) * i * ds;
			yield return null;
		}

	}

	// ###########################################################

	public void startGame ()
	{
		showMenu = false;
	}

	// ###########################################################

	public void redPressed ()
	{
		typed_sequence [nentry] = 'R';
		nentry++;
	}
	
	public void greenPressed ()
	{
		typed_sequence [nentry] = 'G';
		nentry++;
	}

	public void bluePressed ()
	{
		typed_sequence [nentry] = 'B';
		nentry++;
	}

	// ###########################################################

	public void main_options ()
	{
		optionsMenu.SetActive (true);
		mainMenu.SetActive (false);
	}
	
	public void options_back ()
	{
		optionsMenu.SetActive (false);
		mainMenu.SetActive (true);
	}

}
