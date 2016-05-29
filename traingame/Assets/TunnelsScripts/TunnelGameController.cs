using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// Note: anchor point for train is front of first carriage (direction of travel = +x)
// anchor point for for tunnels is rear of tunnels (made of tunnel sections)
// anchor point for station is at centre of station (but note offset in y,z)

public class TunnelGameController : MonoBehaviour
{

	public GameObject TrainCabinPrefab, TunnelSectionPrefab, StationPrefab, OutCharPrefab, fireballPrefab;
	public GameObject inputMenu, messageBox;
	public Text countDown, mbText;
	public Vector3 camStartPos, camTrainDisp;
	public float trainInitX, camStartRot;
	public float astart, umax;   // eg 0.005, 1
	public int nmore, nInputTime;            // eg 6

	private float[][] trainStationDispX,umin;    // eg 20,0.03
	private int[][] nLowSpeed;                   // eg 100

	private GameObject mainCamera, train, tunnel, station;
	private BoxCollider tunnelCollider, stationCollider;
	private float tunnelLength, tunnelWidth, stationLength, stationWidth;
	private int nentry = 0;
	private string[] sequence, typed_sequence;
	private GameObject outCharGroup, previewCharGroup;
	private GameObject[] outChars;
	private GameObject[,] previewChars;
	private bool pressedOK;
	private string[][] track;
	private TunnelCameraController mainCameraScript;
	private bool finishedMoving=false;
	private int itrack,ntracks,maxOutChars;

	//################################################################

	// Use this for initialization
	void Start ()
	{

		// parameters not exposed in the editor
		ntracks = 3;
		maxOutChars = 5;

		sequence=new string[100];
		typed_sequence=new string[100];

		track=new string[ntracks][];
		trainStationDispX=new float[ntracks][];
		umin=new float[ntracks][];
		nLowSpeed=new int[ntracks][];
		
		// track 0 details
		// R = right side sequence, L=left side, E=empty, P=preview
        //		track[0]            =new string[5]{"","FP1",".R2","IL3","IE"};

		track[0]            =new string[]{"",".R2","IR3","IL3","IE"};
		trainStationDispX[0]=new float []{20,20,10,10,20};   // first is start station
		umin[0]             =new float []{0, 0, 0.1f, 0.1f, 0};      // first and last not used
		nLowSpeed[0]        =new int   []{0,100,300,300,0};          // first and last not used

		track[1]            =new string[]{"",".R3","IR3","IP1",".L3","IE"};
		trainStationDispX[1]=new float []{20,20,10,10,20,20};   // first is start station
		umin[1]             =new float []{0, 0, 0.1f, 0.1f, 0.1f, 0};      // first and last not used
		nLowSpeed[1]        =new int   []{0,100,300,300,200,0};          // first and last not used

		track[2]            =new string[]{"","FR3",".E","IR3",".R3","IE"};
		trainStationDispX[2]=new float []{20,20,10,10,20,20};   // first is start station
		umin[2]             =new float []{0, 0, 0.8f, 0.1f, 0.1f, 0};      // first and last not used
		nLowSpeed[2]        =new int   []{0,100,30,300,200,0};          // first and last not used

		//--------------------------------------------------------------------------------------

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

		outChars = new GameObject[maxOutChars];

		outCharGroup = new GameObject ();
		for (int i=0; i<maxOutChars; i++) {
			GameObject outChar = Instantiate (OutCharPrefab) as GameObject;
			outChars [i] = outChar;
			outChar.transform.parent = outCharGroup.transform;
			outChar.transform.localPosition = new Vector3 (-4 + i * 4, 1, -6);
		}
		outCharGroup.SetActive (false);

		previewChars = new GameObject[2,3];     // [0=left, 1=right; 1,2,3...], maximum 3 preview characters

		previewCharGroup = new GameObject ();
		for (int j = 0; j < 2; j++) {
			for (int i = 0; i < 3; i++) {
				GameObject previewChar = Instantiate (OutCharPrefab) as GameObject;
				previewChars [j, i] = previewChar;
				previewChar.transform.parent = previewCharGroup.transform;

				previewChar.transform.localPosition = new Vector3 (-4 + i * 4, 1, -8 + j * 16);

				if (j == 1)
					previewChar.transform.rotation = Quaternion.Euler (0, 180, 0);
			}
		}
		previewCharGroup.SetActive (false);

		StartCoroutine (playGame ());
	}

	// ###########################################################
	
	IEnumerator playGame ()
	{
		string temp;
		bool tutorial = false;

		for (itrack = 0; itrack < ntracks; itrack++) {
			int nsequence = 0;

			mainCamera.transform.position = camStartPos;   // -4,0,10
//			mainCamera.transform.LookAt (camStartLookAt);  // 10,0,0
			mainCamera.transform.rotation = Quaternion.Euler (0, camStartRot, 0);

			yield return StartCoroutine (trainArrives ());
				
			yield return StartCoroutine (mainCameraScript.rotateTo (0, 180, 0, 20));
			yield return StartCoroutine (mainCameraScript.moveTo (camStartPos.x, 0, 0, 60));
			yield return StartCoroutine (mainCameraScript.rotateTo (0, -90, 0, 20));
			yield return StartCoroutine (mainCameraScript.moveTo (camStartPos.x + camTrainDisp.x, 0, 0, 60));
			yield return StartCoroutine (mainCameraScript.rotateTo (0, 180, 0, 20));
			yield return StartCoroutine (mainCameraScript.moveTo (camStartPos.x + camTrainDisp.x, 0, camTrainDisp.z, 60));

			mainCamera.transform.parent = train.transform;

			int i, nacc;
			float x, astop, xst;

			xst = trainStationDispX [itrack] [0];         // starting position of front of train
			float camDistFromFront = trainStationDispX [itrack] [0] - (camStartPos.x + camTrainDisp.x);
			float ust = 0;

			// Process the track
			//            0   1   2   3   4
			//            000 .R3 IL3 IR3 IE.;
			int nstation = track [itrack].Length;   // 5 stations (incl beginning and end)

			for (int istation = 1; istation < nstation; istation++) {
				print ("station text" + track [itrack] [istation]);

				float xtun0 = tunnel.transform.position.x;   // starting position of tunnel back-end
				nacc = (int)((umax - ust) / astart);   // no time steps to get to max speed

				//--------------------------------------------------------------------
				// Acceleration phase
				//--------------------------------------------------------------------

				print ("accelerate");
				for (i = 0; i < nacc; i++) {
					x = xst + ust * i + 0.5f * astart * i * i;
					train.transform.position = new Vector3 (x, 0, 0);
					cycleTunnel (x - camDistFromFront);
					yield return null;
				}
				xst = xst + ust*nacc + 0.5f * astart * nacc * nacc;   // distance covered in acc'n phase
				ust = umax;

				outCharGroup.SetActive (false);
				previewCharGroup.SetActive (false);

				//--------------------------------------------------------------------
				// Constant speed phase for n frames
				//--------------------------------------------------------------------

				int n = Random.Range (100, 500);
				print (n + " frames at constant speed");

				for (i = 0; i < n; i++) {
					x = xst + i * umax;
					train.transform.position = new Vector3 (x, 0, 0);
					cycleTunnel (x - camDistFromFront);
					yield return null;
				}
				xst = xst + n * umax;

				//--------------------------------------------------------------------
				// continue moving while user enters code (if needed)
				//--------------------------------------------------------------------

				if (track [itrack] [istation] [0] == 'I') {

					print ("wait for input");
					inputMenu.SetActive (true);

					nentry = 0;                   // set by buttoms in inputMenu
					for (i = 0; i < nsequence*nInputTime; i++) {
						if (nentry >= nsequence)
							break;
						x = xst + i * umax;
						train.transform.position = new Vector3 (x, 0, 0);
						cycleTunnel (x - camDistFromFront);
						countDown.text = (nsequence*nInputTime - i).ToString ();
						yield return null;
					}
					xst = xst + i * umax;

					inputMenu.SetActive (false);

					temp = "";
					for (i = 0; i < nentry; i++)
						temp += typed_sequence [i];
					print ("typed sequence=" + temp);

					bool correct;

					if (nentry != nsequence)
						correct = false;
					else {
						correct = true;
						for (i = 0; i < nsequence; i++) {
							if (sequence [i] != typed_sequence [i]) {
								correct = false;
								break;
							}
						}
					}
					print ("got it right? " + correct);

					nsequence = 0;    // reset sequence

					if (correct) {
						finishedMoving = false;
						StartCoroutine (gotoNextCarriage ());

						i = 0;
						while (!finishedMoving) {
							x = xst + i * umax;
							train.transform.position = new Vector3 (x, 0, 0);
							cycleTunnel (x - camDistFromFront);
							i++;
							yield return null;
						}
						xst = xst + i * umax;
						camDistFromFront -= tunnelLength / 2;
					}

				} else if (track [itrack] [istation] [0] == 'F') {
					StartCoroutine (fireballAttack ());

					i = 0;
					while (mainCameraScript.getFireball ()) {
						x = xst + i * umax;
						train.transform.position = new Vector3 (x, 0, 0);
						cycleTunnel (x - camDistFromFront);
						i++;
						yield return null;
					}
					xst = xst + i * umax;
				}

				//--------------------------------------------------------------------
				// Slow down phase calculation
				//--------------------------------------------------------------------

				int sectionsDone;
				float totLength, distRemain;
			
				sectionsDone = (int)((xst - xtun0) / tunnelLength);
				totLength = (sectionsDone + nmore) * tunnelLength;
				distRemain = totLength - (xst - xtun0) + stationLength / 2 + trainStationDispX [itrack] [istation];
				astop = (umin [itrack] [istation] * umin [itrack] [istation] - umax * umax) / (2 * distRemain);    
				// v^2=u^2+2as (will be negative)

				//--------------------------------------------------------------------
				// Move Station. Set numbers on outside characters' signs (if needed)
				//--------------------------------------------------------------------

				station.transform.Translate (new Vector3 (stationLength + totLength, 0, 0));

				//                "000 .R3 IL3 IR3 IE. ";
				if (track [itrack] [istation] [1] == 'E') {
					outCharGroup.SetActive (false);
					previewCharGroup.SetActive (false);
				} else if (track [itrack] [istation] [1] == 'R' || track [itrack] [istation] [1] == 'L') {

					float z, rotY;
					if (track [itrack] [istation] [1] == 'R') {
						z = -8;
						rotY = 0;
					} else {
						z = 8;
						rotY = 180;
					}

					int nOutChars = track [itrack] [istation] [2] - '0';

					for (i = 0; i < maxOutChars; i++) {
						outChars [i].SetActive (false);
					}

					for (i = 0; i < nOutChars; i++) {
						outChars [i].SetActive (true);
						outChars [i].transform.localPosition = new Vector3 (-4 + i * 4, 1, z);
						outChars [i].transform.rotation = Quaternion.Euler (0, rotY, 0);
					}

					outCharGroup.SetActive (true);
					outCharGroup.transform.position = new Vector3 (station.transform.position.x,
						station.transform.position.y, 0);
							
					for (i = 0; i < nOutChars; i++) {
				
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
					for (i = 0; i < nsequence; i++)
						temp += sequence [i];
					print ("sequence=" + temp);

				} else if (track [itrack] [istation] [1] == 'P') {

					// ----------------------------------------------------
					// set preview characters
					// ----------------------------------------------------

					int nPreviewChars = track [itrack] [istation] [2] - '0';

					for (i = 0; i < 3; i++) {
						previewChars [0, i].SetActive (false);
						previewChars [1, i].SetActive (false);
					}

					for (i = 0; i < nPreviewChars; i++) {
						previewChars [0, i].SetActive (true);
						previewChars [1, i].SetActive (true);

						string text;
						if (track [itrack] [istation + i + 1] [1] == 'R')
							text = "RIGHT";
						else
							text = "LEFT";

						Text number = previewChars [0, i].GetComponentInChildren<Text> ();
						number.text = text;

						number = previewChars [1, i].GetComponentInChildren<Text> ();
						number.text = text;
					}

					previewCharGroup.SetActive (true);
					previewCharGroup.transform.position = 
					new Vector3 (station.transform.position.x, station.transform.position.y, 0);
				}

				//--------------------------------------------------------------------
				// Slow down phase
				//--------------------------------------------------------------------
			
				print ("sectionsDone" + sectionsDone);
				print ("slow down" + astop);
				for (i = 0; i <= (int)((umin [itrack] [istation] - umax) / astop); i++) {
					x = xst + umax * i + 0.5f * astop * i * i;
					train.transform.position = new Vector3 (x, 0, 0);
					cycleTunnel (x - camDistFromFront);
					yield return null;
				}
				xst = station.transform.position.x + trainStationDispX [itrack] [istation];
				ust = umin [itrack] [istation];

				//--------------------------------------------------
				// Move slowly through the station 
				// (divided into 2 parts with tutorial text between)
				//--------------------------------------------------

				if (istation != nstation) {
					for (i = 0; i < nLowSpeed [itrack] [istation] / 2; i++) {
						x = xst + umin [itrack] [istation] * i;
						train.transform.position = new Vector3 (x, 0, 0);
						cycleTunnel (x - camDistFromFront);
						yield return null;
					}

					// -----------------------------------------------
					// Put tutorial dialogs here
					// -----------------------------------------------

					if (tutorial && itrack == 0 && istation == 1) {
						float lookCharsOffsetZ = -2;
						float lookCharsRotateTo = 160;

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

					for (i = nLowSpeed [itrack] [istation] / 2; i < nLowSpeed [itrack] [istation]; i++) {
						x = xst + umin [itrack] [istation] * i;
						train.transform.position = new Vector3 (x, 0, 0);
						cycleTunnel (x - camDistFromFront);
						yield return null;
					}
					xst = xst + umin [itrack] [istation] * nLowSpeed [itrack] [istation];
				}

				tunnel.transform.Translate (new Vector3 (3 * tunnelLength + stationLength, 0, 0));
			}  // istation loop

			print ("Level complete");

			// -----------------------------------------------------------------------------
			// Leave train. 
			// -----------------------------------------------------------------------------

			if (mainCamera.transform.position.z > 0) {
				yield return StartCoroutine (mainCameraScript.rotateTo (0, 180, 0, 30));
			} else {
				yield return StartCoroutine (mainCameraScript.rotateTo (0, 0, 0, 30));
			}

			Vector3 pos = mainCamera.transform.localPosition;
			yield return StartCoroutine (mainCameraScript.moveTo (pos.x, pos.y, 0, 60));

			yield return StartCoroutine (mainCameraScript.rotateTo (0, 90, 0, 20));
			yield return StartCoroutine (mainCameraScript.moveTo (-camTrainDisp.x, 0, 0, 60, true));
			yield return StartCoroutine (mainCameraScript.rotateTo (0, 0, 0, 20));
			yield return StartCoroutine (mainCameraScript.moveTo (0, 0, camStartPos.z, 60, true));

			yield return StartCoroutine (mainCameraScript.rotateTo (0, camStartRot, 0, 20));

			mainCamera.transform.parent = null;     // camera gets off train

			// -----------------------------------------------------------------------
			// Move station, camera, train back to x=0
			// -----------------------------------------------------------------------

			Vector3 disp = mainCamera.transform.position - station.transform.position;  // how far down the train we got
			disp.y = 0;

			pos = station.transform.position;
			pos.x = 0;
			station.transform.position = pos;

			mainCamera.transform.position = disp;

			tunnel.transform.position = new Vector3 (stationLength / 2, 0, 0);

			train.transform.position = new Vector3 (trainStationDispX [itrack] [nstation - 1], 0, 0);

			yield return StartCoroutine (trainLeaves ());
			yield return StartCoroutine (mainCameraScript.moveTo (camStartPos.x, camStartPos.y, camStartPos.z, 60));

		}

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

		for (s=0; s<1; s+=0.005f) {
			Vector3 pos = new Vector3 (trainInitX - (trainInitX - trainStationDispX[itrack][0]) * inOutExponential (s), 0, 0);
			train.transform.position = pos;
			yield return null;
		}
	}

	// ###########################################################

	IEnumerator trainLeaves ()
	{
		float s;
		int nstation = trainStationDispX [itrack].Length;

		for (s=1; s>0; s-=0.005f) {
			Vector3 pos = new Vector3 (trainInitX - (trainInitX - trainStationDispX[itrack][nstation-1]) * inOutExponential (s), 0, 0);

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

	// ############################################################

	public int nWavesFB, nPerWaveFB;
	public float delayFB, delayWavesFB;

	IEnumerator fireballAttack()
	{
		mainCameraScript.setFireball (true);
		yield return StartCoroutine (mainCameraScript.rotateTo (0, 90, 0, 30));

		for (int j = 0; j < nWavesFB; j++) {
			for (int i = 0; i < nPerWaveFB; i++) {
				GameObject fb = Instantiate (fireballPrefab) as GameObject;
				fb.transform.parent = train.transform;

				float r = Random.value;

				float z;
				if (r < 0.5f)
					z = camTrainDisp.z;
				else
					z = -camTrainDisp.z;

				fb.transform.localPosition = new Vector3 (mainCamera.transform.localPosition.x + tunnelLength / 2, 0, z);
				yield return new WaitForSeconds (delayFB);
			}
			yield return new WaitForSeconds (delayWavesFB);
		}

		if (mainCamera.transform.localPosition.z>0)
			yield return StartCoroutine (mainCameraScript.rotateTo (0, 0, 0, 30));
		else
			yield return StartCoroutine (mainCameraScript.rotateTo (0, 180, 0, 30));

		mainCameraScript.setFireball (false);
	}

}
