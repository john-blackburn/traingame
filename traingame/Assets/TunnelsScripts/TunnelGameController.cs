using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// Note: anchor point for train is front of first carriage (direction of travel = +x)
// anchor point for for tunnels is rear of tunnels (made of tunnel sections)
// anchor point for station is at centre of station (but note offset in y,z)

public class TunnelGameController : MonoBehaviour
{

	public GameObject TrainCabinPrefab, TunnelSectionPrefab, StationPrefab, OutCharPrefab;
	public GameObject inputMenu;
	public Text countDown;
	private GameObject mainCamera, train, tunnel, station;
	private BoxCollider tunnelCollider, stationCollider;
	private float tunnelLength, tunnelWidth, stationLength, stationWidth;
	private int nentry = 0;
	private char[] sequence, typed_sequence;
	private GameObject outCharGroup;
	private GameObject[] outChars;

	// Use this for initialization
	void Start ()
	{
		inputMenu.SetActive (false);

		mainCamera = GameObject.FindWithTag ("MainCamera");

		GameObject tunnelSection;

		tunnelSection = Instantiate (TunnelSectionPrefab) as GameObject;
		tunnelCollider = tunnelSection.GetComponent<BoxCollider> () as BoxCollider;
		tunnelLength = tunnelCollider.bounds.size.x;
		tunnelWidth = tunnelCollider.bounds.size.z;
		Destroy (tunnelSection);

		station = Instantiate (StationPrefab) as GameObject;
		station.transform.position = new Vector3 (0, -3, -1.6f);
		stationCollider = station.GetComponent<BoxCollider> () as BoxCollider;
		stationLength = stationCollider.bounds.size.x;
		stationWidth = stationCollider.bounds.size.z;

		print ("Tunnel length=" + tunnelLength);
		print ("Tunnel width=" + tunnelWidth);

		print ("Station length=" + stationLength);
		print ("Station width=" + stationWidth);

		train = new GameObject ();
		train.transform.position = Vector3.zero;

		for (int i=0; i<3; i++) {
			GameObject trainCabin = Instantiate (TrainCabinPrefab) as GameObject;
			trainCabin.transform.localPosition = new Vector3 (-tunnelLength / 4 - tunnelLength * i / 2, 0, 0);
			trainCabin.transform.parent = train.transform;
		}

		mainCamera.transform.position = new Vector3 (-4, 0, 10);
		mainCamera.transform.LookAt (new Vector3 (10, 0, 0));

		print (train.transform.childCount);

		tunnel = new GameObject ();
		tunnel.transform.position = new Vector3 (stationLength / 2, 0, 0);

		for (int i=0; i<3; i++) {
			GameObject t = Instantiate (TunnelSectionPrefab) as GameObject;
			t.transform.parent = tunnel.transform;
			t.transform.localPosition = new Vector3 (tunnelLength * (i + 0.5f), 0, 0);
		}

		outChars=new GameObject[3];

		outCharGroup = new GameObject ();
		for (int i=0; i<3; i++) {
			GameObject outChar = Instantiate (OutCharPrefab) as GameObject;
			outChars[i]=outChar;
			outChar.transform.parent = outCharGroup.transform;
			outChar.transform.localPosition = new Vector3 (-4 + i * 4, 1, -6);
		}
		outCharGroup.SetActive (false);

		StartCoroutine (playGame ());
	}

	// ###########################################################

	IEnumerator playGame ()
	{
		string temp;

		yield return StartCoroutine (trainArrives ());

		yield return StartCoroutine (rotateCameraTo (0, 180, 0, 20));
		yield return StartCoroutine (moveCameraTo (-4, 0, 0, 60));
		yield return StartCoroutine (rotateCameraTo (0, -90, 0, 20));
		yield return StartCoroutine (moveCameraTo (-6, 0, 0, 40));
		yield return StartCoroutine (rotateCameraTo (0, 180, 0, 20));
		yield return StartCoroutine (moveCameraTo (-6, 0, -1, 40));

		mainCamera.transform.parent = train.transform;

		int i, nacc, n, nmore;
		float x, astart, astop, u, xst;
		
		astart = 0.005f;       // acceleration
		u = 1f;          // top speed  (m/frame)
		n = 143;         // no of const speed frames
		nmore = 6;       // additional tunnel sections for slow down
		xst = 0;         // starting position of front of train

		bool first = true;

		while (true) {
			float xtun0 = tunnel.transform.position.x;   // starting position of tunnel back-end
			nacc = (int)(u / astart);   // no time steps to get to max speed

			//--------------------------------------------------------------------
			// Acceleration phase
			//--------------------------------------------------------------------

			print ("accelerate");
			for (i=0; i<nacc; i++) {
				x = xst + 0.5f * astart * i * i;
				train.transform.position = new Vector3 (x, 0, 0);
				cycleTunnel (x);
				yield return null;
			}
			xst = xst + 0.5f * astart * nacc * nacc;   // distance covered in acc'n phase

			n = Random.Range (100, 500);
			print (n + " frames at constant speed");

			//--------------------------------------------------------------------
			// Constant speed phase for n frames
			//--------------------------------------------------------------------

			for (i=0; i<n; i++) {
				x = xst + i * u;
				train.transform.position = new Vector3 (x, 0, 0);
				cycleTunnel (x);
				yield return null;
			}
			xst = xst + n * u;

			//--------------------------------------------------------------------
			// continue moving while user enters code
			//--------------------------------------------------------------------

			if (!first) {

				print ("wait for input");
				inputMenu.SetActive (true);

				typed_sequence=new char[3];
				nentry = 0;
				for (i=0; i<200; i++) {
					if (nentry >= 3)
						break;
					x = xst + i * u;
					train.transform.position = new Vector3 (x, 0, 0);
					cycleTunnel (x);
					countDown.text = (200 - i).ToString ();
					yield return null;
				}
				xst = xst + i * u;

				inputMenu.SetActive (false);

				temp = "";
				for (i=0; i < nentry; i++)
					temp += typed_sequence [i];
				print ("typed sequence=" + temp);
				
				bool correct = true;
				for (i=0; i<3; i++){
					if (sequence [i] != typed_sequence [i]){
						correct = false;
					    break;
					}
				}
				print ("got it right? " + correct);

				if (correct) {
					StartCoroutine(gotoNextCarriage());
				}

			}

			//--------------------------------------------------------------------
			// Slow down phase calculation
			//--------------------------------------------------------------------

			int sectionsDone;
			float totLength, distRemain;
			
			sectionsDone = (int)((xst - xtun0) / tunnelLength);
			totLength = (sectionsDone + nmore) * tunnelLength;
			distRemain = totLength - (xst - xtun0) + stationLength / 2;
			astop = -u * u / (2 * distRemain);

			//--------------------------------------------------------------------
			// Move Station. Set numbers on outside characters' signs
			//--------------------------------------------------------------------

			station.transform.Translate (new Vector3 (stationLength + totLength, 0, 0));

			outCharGroup.SetActive(true);
			outCharGroup.transform.position = station.transform.position;
			sequence = new char[3];
			
			for (i=0; i<sequence.Length; i++) {
				
				float r = Random.value;
				if (r < 0.333)
					sequence [i] = '1';
				else if (r < 0.666)
					sequence [i] = '2';
				else
					sequence [i] = '3';
			}
			
			temp = "";
			for (i=0; i<sequence.Length; i++)
				temp += sequence [i];
			print ("sequence=" + temp);

			for (i=0;i<3;i++){
				Text number=outChars[i].GetComponentInChildren<Text>();
				number.text = sequence [i].ToString ();
			}

			//--------------------------------------------------------------------
			// Slow down phase
			//--------------------------------------------------------------------
			
			print ("sectionsDone" + sectionsDone);
			print ("slow down" + astop);
			for (i=0; i<=(int)(-u/astop); i++) {
				x = xst + u * i + 0.5f * astop * i * i;
				train.transform.position = new Vector3 (x, 0, 0);
				cycleTunnel (x);
				yield return null;
			}

			tunnel.transform.Translate (new Vector3 (3 * tunnelLength + stationLength, 0, 0));
			xst = station.transform.position.x;
			first=false;
		}

	}

	// ###########################################################

	void cycleTunnel (float x)   // x: train front position
	{
		float xtunnel = tunnel.transform.position.x;

		if (x > xtunnel + 2 * tunnelLength) {
			float xend = xtunnel + 4 * tunnelLength;   // end point of tunnel if we move it
			float xstation = station.transform.position.x;
//			print (xend+":"+xstation+":"+stationLength);
			if (!(xend > xstation - stationLength / 2.1 && xend < xstation + stationLength / 2.1))
				tunnel.transform.Translate (new Vector3 (tunnelLength, 0, 0));
		}

	}

	// ###########################################################
	
	// Update is called once per frame
	void Update ()
	{
	}

	// ###########################################################
	
	float inOutExponential (float ratio)
	{
		if (ratio == 0 || ratio == 1) {
			return ratio;
		}
		ratio = ratio * 2 - 1;
		if (ratio < 0) {
			return 0.5f * Mathf.Pow (2, 10 * ratio);
		}
		return 1 - 0.5f * Mathf.Pow (2, -10 * ratio);
	}

	// ###########################################################
	
	IEnumerator moveCameraTo (float x, float y, float z, int nframes, bool relative=false)
	{
		Vector3 r1 = mainCamera.transform.localPosition;
		Vector3 r2; 
		
		if (relative)
			r2 = r1 + new Vector3 (x, y, z);
		else
			r2 = new Vector3 (x, y, z);
		
		float ds = 1.0f / nframes;
		for (int i=0; i<=nframes; i++) {
			mainCamera.transform.localPosition = r1 + (r2 - r1) * i * ds;
			yield return null;
		}
		
	}

	// ###########################################################

	IEnumerator rotateCameraTo (float x, float y, float z, int nframes)
	{
		Quaternion q1 = mainCamera.transform.rotation;
		Quaternion q2 = Quaternion.Euler (x, y, z);
		float s;

		float ds = 1.0f / nframes;
		for (int i=0; i<=nframes; i++) {
			mainCamera.transform.rotation = Quaternion.Slerp (q1, q2, i * ds);
			yield return null;
		}
	}

	// ###########################################################

	IEnumerator moveTrainTo (float x, float y, float z, int nframes, float s1=0f, float s2=1f)
	{
		// Move train from current point to (x,y,z) in nframes.
		// if s1=0 and s2=1, do whole animation
		// else do partial animation [s1,s2]

		Vector3 r1 = mainCamera.transform.position;
		Vector3 r2 = new Vector3 (x, y, z);
		
		float ds = (s2 - s1) / nframes;

		for (int i=0; i<nframes; i++) {
			Vector3 pos = r1 + (r2 - r1) * inOutExponential (s1 + i * ds);
			train.transform.position = pos;
			yield return null;
		}
	}

	// ###########################################################

	IEnumerator trainArrives ()
	{
		float s;

		for (s=0; s<1; s+=0.01f) {
			Vector3 pos = new Vector3 (520 - 520 * inOutExponential (s), 0, 0);
			train.transform.position = pos;
			yield return null;
		}
	}

	// ###########################################################

	IEnumerator trainLeaves ()
	{
		float s;
		
		for (s=0; s<1; s+=0.002f) {
			Vector3 pos = new Vector3 ((stationLength + 3 * tunnelLength) * inOutExponential (s), 0, 0);
			train.transform.position = pos;
			yield return null;
		}
	}

	// ###########################################################
	
	public void button1Pressed ()
	{
		typed_sequence [nentry] = '1';
		nentry++;
	}
	
	public void button2Pressed ()
	{
		typed_sequence [nentry] = '2';
		nentry++;
	}
	
	public void button3Pressed ()
	{
		typed_sequence [nentry] = '3';
		nentry++;
	}

	// ############################################################

	IEnumerator gotoNextCarriage()
	{
		yield return StartCoroutine (rotateCameraTo (0, 90, 0, 20));
		yield return StartCoroutine (moveCameraTo (10, 0, 0, 40, true));
	}

}
