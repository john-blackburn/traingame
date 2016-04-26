using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// Note: anchor point for train is front of first carriage (direction of travel = +x)
// anchor point for for tunnels is rear of tunnels (made of tunnel sections)
// anchor point for station is at centre of station (but note offset in y,z)

public class TunnelGameController : MonoBehaviour
{

	public GameObject TrainCabinPrefab, TunnelSectionPrefab, StationPrefab, OutCharPrefab;
	public GameObject inputMenu, messageBox;
	public Text countDown, mbText;
	public Vector3 camStartPos, camStartLookAt, camTrainDisp;
	public float trainInitX;
	public float lookCharsOffsetZ, lookCharsRotateTo;
	public float astart, umax;   // eg 0.005, 1
	public int nmore;            // eg 6

	private float[][] trainStationDispX,umin;    // eg 20,0.03
	private int[][] nLowSpeed;                   // eg 100

	private GameObject mainCamera, train, tunnel, station;
	private BoxCollider tunnelCollider, stationCollider;
	private float tunnelLength, tunnelWidth, stationLength, stationWidth;
	private int nentry = 0;
	private string[] sequence, typed_sequence;
	private GameObject outCharGroup;
	private GameObject[] outChars;
	private bool pressedOK;
	private string[][] track;
	private TunnelCameraController mainCameraScript;
	private bool finishedMoving=false;
	private int itrack;

	// Use this for initialization
	void Start ()
	{

		sequence=new string[100];
		typed_sequence=new string[100];

		//		track [0] = ".R3 IR3 IL3 IE. ";  // 4 stations (excluding first station)
		//		track [1] = ".R3 IR3 IP1 .L3 IE. ";
		
		track=new string[4][];
		trainStationDispX=new float[4][];
		umin=new float[4][];
		nLowSpeed=new int[4][];
		
		// track 0 details
		// R = right side sequence, L=left side, E=empty, P=preview

		track[0]            =new string[5]{"",".R3",".R3","IL3","IE."};
		trainStationDispX[0]=new float [5]{20,20,10,10,20};   // first is start station
		umin[0]             =new float [5]{0, 0, 0.1f, 0.1f, 0};      // first and last not used
		nLowSpeed[0]        =new int   [5]{0,100,300,300,0};          // first and last not used

		inputMenu.SetActive (false);
		messageBox.SetActive (false);

		mainCamera = GameObject.FindWithTag ("MainCamera");
		mainCameraScript = mainCamera.GetComponent<TunnelCameraController> ();

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

		for (int i=0; i<3; i++) {
			GameObject trainCabin = Instantiate (TrainCabinPrefab) as GameObject;
			trainCabin.transform.localPosition = new Vector3 (-tunnelLength / 4 - tunnelLength * i / 2, 0, 0);
			trainCabin.transform.parent = train.transform;
		}

		print (train.transform.childCount);

		tunnel = new GameObject ();
		tunnel.transform.position = new Vector3 (stationLength / 2, 0, 0);

		for (int i=0; i<3; i++) {
			GameObject t = Instantiate (TunnelSectionPrefab) as GameObject;
			t.transform.parent = tunnel.transform;
			t.transform.localPosition = new Vector3 (tunnelLength * (i + 0.5f), 0, 0);
		}

		outChars = new GameObject[3];

		outCharGroup = new GameObject ();
		for (int i=0; i<3; i++) {
			GameObject outChar = Instantiate (OutCharPrefab) as GameObject;
			outChars [i] = outChar;
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

		itrack=0;
		int nsequence=0;

		mainCamera.transform.position = camStartPos;   // -4,0,10
		mainCamera.transform.LookAt (camStartLookAt);  // 10,0,0

		yield return StartCoroutine (trainArrives ());
				
		yield return StartCoroutine (mainCameraScript.rotateTo (0, 180, 0, 60));
		yield return StartCoroutine (mainCameraScript.moveTo (camStartPos.x, 0, 0, 60));
		yield return StartCoroutine (mainCameraScript.rotateTo (0, -90, 0, 30));
		yield return StartCoroutine (mainCameraScript.moveTo (camStartPos.x + camTrainDisp.x, 0, 0, 60));
		yield return StartCoroutine (mainCameraScript.rotateTo (0, 180, 0, 30));
		yield return StartCoroutine (mainCameraScript.moveTo (camStartPos.x + camTrainDisp.x, 0, camTrainDisp.z, 60));

		mainCamera.transform.parent = train.transform;

		int i, nacc;
		float x, astop, xst;

		xst = trainStationDispX[itrack][0];         // starting position of front of train
		float camDistFromFront = trainStationDispX[itrack][0] - (camStartPos.x + camTrainDisp.x);
		float ust = 0;

		// Process the track
		//            0   1   2   3   4
		//            000 .R3 IL3 IR3 IE.;
		int nstation = track [itrack].Length;   // 5 stations (incl beginning and end)

		for (int istation=1; istation < nstation; istation++) {
			print ("station text" + track [itrack][istation]);

			float xtun0 = tunnel.transform.position.x;   // starting position of tunnel back-end
			nacc = (int)((umax - ust) / astart);   // no time steps to get to max speed

			//--------------------------------------------------------------------
			// Acceleration phase
			//--------------------------------------------------------------------

			print ("accelerate");
			for (i=0; i<nacc; i++) {
				x = xst + ust * i + 0.5f * astart * i * i;
				train.transform.position = new Vector3 (x, 0, 0);
				cycleTunnel (x - camDistFromFront);
				yield return null;
			}
			xst = xst + 0.5f * astart * nacc * nacc;   // distance covered in acc'n phase
			ust = umax;

			outCharGroup.SetActive (false);

			//--------------------------------------------------------------------
			// Constant speed phase for n frames
			//--------------------------------------------------------------------

			int n = Random.Range (100, 500);
			print (n + " frames at constant speed");

			for (i=0; i<n; i++) {
				x = xst + i * umax;
				train.transform.position = new Vector3 (x, 0, 0);
				cycleTunnel (x - camDistFromFront);
				yield return null;
			}
			xst = xst + n * umax;

			//--------------------------------------------------------------------
			// continue moving while user enters code (if needed)
			//--------------------------------------------------------------------

			if (track [itrack][istation][0] == 'I') {

				print ("wait for input");
				inputMenu.SetActive (true);

				nentry = 0;                   // set by buttoms in inputMenu
				for (i=0; i<500; i++) {
					if (nentry >= nsequence)
						break;
					x = xst + i * umax;
					train.transform.position = new Vector3 (x, 0, 0);
					cycleTunnel (x - camDistFromFront);
					countDown.text = (500 - i).ToString ();
					yield return null;
				}
				xst = xst + i * umax;

				inputMenu.SetActive (false);

				temp = "";
				for (i=0; i < nentry; i++)
					temp += typed_sequence [i];
				print ("typed sequence=" + temp);
				
				bool correct = true;
				for (i=0; i < nentry; i++) {
					if (sequence [i] != typed_sequence [i]) {
						correct = false;
						break;
					}
				}
				print ("got it right? " + correct);

				nsequence=0;

				if (correct) {
					finishedMoving=false;
					StartCoroutine (gotoNextCarriage ());

					i=0;
					while(!finishedMoving){
						x = xst + i * umax;
						train.transform.position = new Vector3 (x, 0, 0);
						cycleTunnel (x - camDistFromFront);
						i++;
						yield return null;
					}
					xst = xst + i * umax;
					camDistFromFront -= tunnelLength/2;
				}

			}

			//--------------------------------------------------------------------
			// Slow down phase calculation
			//--------------------------------------------------------------------

			int sectionsDone;
			float totLength, distRemain;
			
			sectionsDone = (int)((xst - xtun0) / tunnelLength);
			totLength = (sectionsDone + nmore) * tunnelLength;
			distRemain = totLength - (xst - xtun0) + stationLength / 2 + trainStationDispX[itrack][istation];
			astop = (umin[itrack][istation] * umin[itrack][istation] - umax * umax) / (2 * distRemain);    
			// v^2=u^2+2as (will be negative)

			//--------------------------------------------------------------------
			// Move Station. Set numbers on outside characters' signs (if needed)
			//--------------------------------------------------------------------

			station.transform.Translate (new Vector3 (stationLength + totLength, 0, 0));

			//                "000 .R3 IL3 IR3 IE. ";
			if (track [itrack][istation][1] == 'E') {
				outCharGroup.SetActive (false);
			} else if (track [itrack][istation][1] == 'R' || track [itrack][istation][1] == 'L') {

				float z, rotY;
				if (track [itrack][istation][1] == 'R') {
					z = -8;
					rotY = 0;
				} else {
					z = 8;
					rotY = 180;
				}

				int nOutChars=track[itrack][istation][2]-'0';

				for (i=0; i<nOutChars; i++) {
					outChars [i].transform.localPosition = new Vector3 (-4 + i * 4, 1, z);
					outChars [i].transform.rotation = Quaternion.Euler (0, rotY, 0);
				}

				outCharGroup.SetActive (true);
				outCharGroup.transform.position = new Vector3 (station.transform.position.x,
  				                                               station.transform.position.y, 0);
							
				for (i=0; i<nOutChars; i++) {
				
					float r = Random.value;
					if (r < 0.333)
						temp = "1";
					else if (r < 0.666)
						temp = "2";
					else
						temp = "3";

					sequence [nsequence] = temp;
					nsequence++;

					Text number = outChars [i].GetComponentInChildren<Text> ();
					number.text = temp;
				}
			
				temp = "";
				for (i=0; i<nsequence; i++)
					temp += sequence [i];
				print ("sequence=" + temp);

			} else if (track [itrack][istation][1] == 'P') {
				outCharGroup.SetActive (true);
				for (i=0; i<3; i++) {
					Text number = outChars [i].GetComponentInChildren<Text> ();
					number.text = "RIGHT";
				}
			}

			//--------------------------------------------------------------------
			// Slow down phase
			//--------------------------------------------------------------------
			
			print ("sectionsDone" + sectionsDone);
			print ("slow down" + astop);
			for (i=0; i<=(int)((umin[itrack][istation]-umax)/astop); i++) {
				x = xst + umax * i + 0.5f * astop * i * i;
				train.transform.position = new Vector3 (x, 0, 0);
				cycleTunnel (x - camDistFromFront);
				yield return null;
			}
			xst = station.transform.position.x + trainStationDispX[itrack][istation];
			ust = umin[itrack][istation];

			//--------------------------------------------------
			// Move slowly through the station 
			// (divided into 2 parts with tutorial text between)
			//--------------------------------------------------

			if (istation != nstation) {
				for (i=0; i<nLowSpeed[itrack][istation]/2; i++) {
					x = xst + umin[itrack][istation] * i;
					train.transform.position = new Vector3 (x, 0, 0);
					cycleTunnel (x - camDistFromFront);
					yield return null;
				}

				if (istation == 1) {
					yield return StartCoroutine (moveCameraTo (0, 0, lookCharsOffsetZ, 60, true));
					yield return StartCoroutine (rotateCameraTo (0, lookCharsRotateTo, 0, 60));
				
					mbText.text = "What were those strange figures? " +
								  "They didn't look human...";
				
					messageBox.SetActive (true);
					pressedOK = false;
					while (!pressedOK) {
						yield return null;
					}
					messageBox.SetActive (false);
				
					yield return StartCoroutine (rotateCameraTo (0, -180, 0, 40));
					yield return StartCoroutine (moveCameraTo (0, 0, -lookCharsOffsetZ, 40, true));
				}

				for (i=nLowSpeed[itrack][istation]/2; i<nLowSpeed[itrack][istation]; i++) {
					x = xst + umin[itrack][istation] * i;
					train.transform.position = new Vector3 (x, 0, 0);
					cycleTunnel (x - camDistFromFront);
					yield return null;
				}
				xst = xst + umin[itrack][istation] * nLowSpeed[itrack][istation];
			}

			tunnel.transform.Translate (new Vector3 (3 * tunnelLength + stationLength, 0, 0));
		}  // istation loop

		print ("Level complete");
	}

	// ###########################################################

	void cycleTunnel (float x)   // x: camera position
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
//	void Update ()
//	{
//	}

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
			Vector3 pos = new Vector3 (trainInitX - (trainInitX - trainStationDispX[itrack][0]) * inOutExponential (s), 0, 0);
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
		typed_sequence [nentry] = "1";
		nentry++;
	}
	
	public void button2Pressed ()
	{
		typed_sequence [nentry] = "2";
		nentry++;
	}
	
	public void button3Pressed ()
	{
		typed_sequence [nentry] = "3";
		nentry++;
	}

	public void buttonOKPressed ()
	{
		pressedOK = true;
	}

	// ############################################################

	IEnumerator gotoNextCarriage ()
	{
		Vector3 pos=mainCamera.transform.localPosition;

		yield return StartCoroutine (rotateCameraTo (0, 90, 0, 60));
		yield return StartCoroutine (moveCameraTo(pos.x,pos.y,0,20));
		yield return StartCoroutine (moveCameraTo (tunnelLength/2, 0, 0, 200, true));
		yield return StartCoroutine (moveCameraTo(0,0,camTrainDisp.z,20,true));
		yield return StartCoroutine (rotateCameraTo (0, 180, 0, 60));
		finishedMoving=true;
	}

}
