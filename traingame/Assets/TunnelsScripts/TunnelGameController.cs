using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// Note: anchor point for train is front of first carriage (direction of travel = +x)
// anchor point for for tunnels is rear of tunnels (made of tunnel sections)
// anchor point for station is at centre of station (but note offset in y,z)

public class TunnelGameController : MonoBehaviour
{

	public GameObject TrainCabinPrefab, TunnelSectionPrefab, StationPrefab, OutCharPrefab, fireballPrefab;
	public GameObject monsterPrefab;
	public GameObject inputMenu, messageBox, controls;
	public Text countDown, mbText, textEntry;
	public Vector3 camStartPos, camTrainDisp;
	public float trainInitX, camStartRot;
	public float astart, umax;   // eg 0.005, 1
	public int nmore, nInputTime;            // eg 6
	public GameObject mainMenu;
	public Text trackText;

	private float[][] trainStationDispX,umin;    // eg 20,0.03
	private int[][] nLowSpeed;                   // eg 100

	private GameObject mainCamera, train, tunnel, station;
	private GameObject[] carriages;
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
	private bool finishedMoving=false, survivedFB;
	private int itrack,ntracks,maxOutChars,nCarriages;

	//################################################################

	// Use this for initialization
	void Start ()
	{

		// parameters not exposed in the editor
		ntracks = 4;      // number of tracks (levels)
		maxOutChars = 5;  // max number of sequence characters (max preview characters is 3)
		nCarriages=4;

		sequence=new string[100];
		typed_sequence=new string[100];

		track=new string[ntracks][];
		trainStationDispX=new float[ntracks][];
		umin=new float[ntracks][];
		nLowSpeed=new int[ntracks][];
		
		// track 0 details
		// R = right side sequence, L=left side, E=empty, P=preview
        //		track[0]            =new string[5]{"","FP1",".R2","IL3","IE"};

		track[0]            =new string[]{   "",".R2", "IR3", "IR2", ".R2", "IE"};
		trainStationDispX[0]=new float []{33.3f,   32,     0,     0,     0,    5};   // first is start station
		umin[0]             =new float []{    0,    0, 0.07f, 0.07f, 0.07f,    0};      // first and last not used
		nLowSpeed[0]        =new int   []{    0,  100,   250,   250,   250,    0};          // first and last not used

		track[1]            =new string[]{   "",".R3","IP1",".L3","IP2",".R2",".L2","IE"};
		trainStationDispX[1]=new float []{33.3f,    0,    0,    0,    0,    0,    0,   5};   // first is start station
		umin[1]             =new float []{    0,    0, 0.1f, 0.1f, 0.1f,    0,    0,   0};      // first and last not used
		nLowSpeed[1]        =new int   []{    0,  100,  300,  300,  200,    0,    0,   0};          // first and last not used

		track[2]            =new string[]{"",  ".R3", ".E",".R2", "IE",".R2",".P2",".L2","IE",".R3","IE"};
		trainStationDispX[2]=new float []{33.3f,   0,    0,    0,    0,    0,    0,    0,   0,    0,   5};   // first is start station
		umin[2]             =new float []{0,    0.2f, 0.8f, 0.2f, 0.6f, 0.2f, 0.2f, 0.2f,0.5f, 0.3f,   0};      // first and last not used
		nLowSpeed[2]        =new int   []{0,     100,   50,  100,   50,  100,  100,  100,  50,   70,   0};          // first and last not used

		track[3]            =new string[]{"",  ".P3","FL2","FE","IL3", "FL2","IL4","IE","FE"};
		trainStationDispX[3]=new float []{33.3f,   0,    0,    0,    0,     0,    0,   0,   5};   // first is start station
		umin[3]             =new float []{0,       0, 0.3f, 0.3f, 0.3f,  0.3f, 0.3f,0.8f,   0};      // first and last not used
		nLowSpeed[3]        =new int   []{0,     100,   70,   70,   70,    70,   50,  20,   0};          // first and last not used

		//--------------------------------------------------------------------------------------

		inputMenu.SetActive (false);
		messageBox.SetActive (false);
		mainMenu.SetActive (false);
		controls.SetActive (false);

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
		carriages = new GameObject[nCarriages];

		for (int i=0; i<nCarriages; i++) {
			GameObject trainCabin = Instantiate (TrainCabinPrefab) as GameObject;
//			Renderer rend = trainCabin.GetComponent<Renderer> ();
//			rend.material.color = Color.red;
			carriages [i] = trainCabin;
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
		string temp="";
		Vector3 pos;
		int i;

		bool tutorial = true;    // if true, show the tutorial text
		bool cheat = false;       // if true, monster does not kill you, progress to next carriage
		bool calcDispx = true;   // if true, calculate values in trainStationDispX to give best view of characters

		while (true) {  // main game loop

			controls.SetActive (false);
			trackText.text = "";
			mainCamera.transform.parent = null;     // camera gets off train

			pos = station.transform.position;
			pos.x = 0;
			station.transform.position = pos;

			tunnel.transform.position = new Vector3 (stationLength / 2, 0, 0);

			for (i = 0; i < nCarriages; i++)
				carriages [i].SetActive (true);			

			mainCamera.transform.position = camStartPos;   // -4,0,10
			mainCamera.transform.rotation = Quaternion.Euler (0, camStartRot, 0);

			train.transform.position = Vector3.zero;

			mainMenu.SetActive (true);

			int iframe = 0;
			finishedMoving = false;
			while (!finishedMoving) {
				mainCamera.transform.position = new Vector3 (10*Mathf.Cos (iframe * 0.01f), 0, 10*Mathf.Sin (iframe * 0.01f));
				mainCamera.transform.LookAt (train.transform);
				iframe++;
				yield return null;
			}

			mainMenu.SetActive (false);
				
			// Loop over tracks
			for (itrack = 1; itrack < ntracks; itrack++) {
				trackText.text = "Track: " + itrack.ToString ();
				int nsequence = 0;

				mainCamera.transform.position = camStartPos;   // -4,0,10
				mainCamera.transform.rotation = Quaternion.Euler (0, camStartRot, 0);

				controls.SetActive (false);

				yield return StartCoroutine (trainArrives ());
				
				yield return StartCoroutine (mainCameraScript.rotateTo (0, 180, 0, 20));
				yield return StartCoroutine (mainCameraScript.moveTo (camStartPos.x, 0, 0, 60));
				yield return StartCoroutine (mainCameraScript.rotateTo (0, -90, 0, 20));
				yield return StartCoroutine (mainCameraScript.moveTo (camStartPos.x + camTrainDisp.x, 0, 0, 60));
				yield return StartCoroutine (mainCameraScript.rotateTo (0, 180, 0, 20));
				yield return StartCoroutine (mainCameraScript.moveTo (camStartPos.x + camTrainDisp.x, 0, camTrainDisp.z, 60));

				if (itrack > 1)
					controls.SetActive (true);
				else
					controls.SetActive (false);

				trackText.text = "";

				mainCamera.transform.parent = train.transform;

				activateCarriages (0);   // 0 means we are in the rear carriage

				int nacc;
				float x, astop, xst;

				xst = trainStationDispX [itrack] [0];         // starting position of front of train
				float camDistFromFront = trainStationDispX [itrack] [0] - (camStartPos.x + camTrainDisp.x);
				int carriagesBehind = 0;

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
						yield return new WaitForFixedUpdate ();
					}
					xst = xst + ust * nacc + 0.5f * astart * nacc * nacc;   // distance covered in acc'n phase
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
						yield return new WaitForFixedUpdate ();
					}
					xst = xst + n * umax;

					//--------------------------------------------------------------------
					// ask for sequence in tunnel
					//--------------------------------------------------------------------

					if (track [itrack] [istation] [0] == 'I') {

						print ("wait for input");
						inputMenu.SetActive (true);
						textEntry.text = "---";

						nentry = 0;                   // set by buttoms in inputMenu
						for (i = 0; i < nsequence * nInputTime; i++) {
							if (nentry >= nsequence)
								break;
							x = xst + i * umax;
							train.transform.position = new Vector3 (x, 0, 0);
							cycleTunnel (x - camDistFromFront);
							countDown.text = (nsequence * nInputTime - i).ToString ();
							yield return new WaitForFixedUpdate ();
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
								yield return new WaitForFixedUpdate ();
							}
							xst = xst + i * umax;
							camDistFromFront -= tunnelLength / 2;
							carriagesBehind++;

							activateCarriages (carriagesBehind);
						} else {
							finishedMoving = false;
							StartCoroutine (jumpScare (camDistFromFront));

							i = 0;
							while (!finishedMoving) {
								x = xst + i * umax;
								train.transform.position = new Vector3 (x, 0, 0);
								cycleTunnel (x - camDistFromFront);
								i++;
								yield return new WaitForFixedUpdate ();
							}
							xst = xst + i * umax;
							if (!cheat) goto jumpout; // exit to main game loop
						}

					// -------------------------------------------------------
					// Fireball attack in tunnel
					// -------------------------------------------------------

					} else if (track [itrack] [istation] [0] == 'F') {

						StartCoroutine (fireballAttack (itrack==3 && istation==2));

						i = 0;
						survivedFB = false;
						while (mainCameraScript.getFireball ()) {
							x = xst + i * umax;
							train.transform.position = new Vector3 (x, 0, 0);
							cycleTunnel (x - camDistFromFront);
							i++;
							yield return new WaitForFixedUpdate ();
						}
						xst = xst + i * umax;
						if (!cheat && !survivedFB)
							goto jumpout;
					}

					//--------------------------------------------------------------------
					// Slow down phase calculation
					//--------------------------------------------------------------------

					int sectionsDone;
					float totLength, distRemain;
					float dispx;

					if (calcDispx && istation != nstation - 1) {
						dispx = camDistFromFront - umin [itrack] [istation] * nLowSpeed [itrack] [istation] / 2;
					} else {
						dispx = trainStationDispX [itrack] [istation];
					}
			
					sectionsDone = (int)((xst - xtun0) / tunnelLength);
					totLength = (sectionsDone + nmore) * tunnelLength;
					distRemain = totLength - (xst - xtun0) + stationLength / 2 + dispx;
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
							outChars [i].transform.localPosition = new Vector3 (-(nOutChars - 1) * 2 + i * 4, 1, z);
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
						print ("nPreviewChars"+nPreviewChars);
						previewCharGroup.SetActive (true);

						char[] previewList;
						previewList = new char[nPreviewChars];

						int ind = 0;
						i = 0;
						while (ind < nPreviewChars) {
							if (track [itrack] [istation + i + 1] [1] == 'R') {
								previewList [ind] = 'R';
								ind++;
							} else if (track [itrack] [istation + i + 1] [1] == 'L') {
								previewList [ind] = 'L';
								ind++;
							}
							i++;
						}

						for (i = 0; i < 3; i++) {
							previewChars [0, i].SetActive (false);
							previewChars [1, i].SetActive (false);
						}

						for (i = 0; i < nPreviewChars; i++) {
							previewChars [0, i].SetActive (true);
							previewChars [1, i].SetActive (true);

							string text;
							if (previewList[i] == 'R')
								text = "R";
							else
								text = "L";

							Text number = (previewChars [0, i]).GetComponentInChildren<Text> ();
							number.text = text;

							number = previewChars [1, i].GetComponentInChildren<Text> ();
							number.text = text;
						}

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
						yield return new WaitForFixedUpdate ();
					}
					xst = station.transform.position.x + dispx;
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
							yield return new WaitForFixedUpdate ();
						}

						// -----------------------------------------------
						// Put tutorial dialogs here
						// -----------------------------------------------

						if (tutorial && itrack == 0 && istation == 1) {
							float lookCharsOffsetZ = -2.65f;
							float lookCharsRotateTo = 160;

							yield return StartCoroutine (moveCameraTo (0, 0, lookCharsOffsetZ, 60, true));
							yield return StartCoroutine (rotateCameraTo (0, lookCharsRotateTo, 0, 30));
				
							mbText.text = "What were those strange figures? " +
							"They didn't look human... The sequence seemed important: '" + temp + "'";
				
							messageBox.SetActive (true);
							pressedOK = false;
							while (!pressedOK) {
								yield return null;
							}
							messageBox.SetActive (false);
				
							yield return StartCoroutine (rotateCameraTo (0, -180, 0, 30));
							yield return StartCoroutine (moveCameraTo (0, 0, -lookCharsOffsetZ, 40, true));
						} else if (tutorial && itrack == 0 && istation == 4) {
							mbText.text = "I wasn't asked for the sequence in that tunnel " +
							"So I need to combine the two sequences to give: '" + temp + "'";

							messageBox.SetActive (true);
							pressedOK = false;
							while (!pressedOK) {
								yield return null;
							}
							messageBox.SetActive (false);												
						} else if (tutorial && ((itrack == 1 && (istation == 2 || istation==4 || istation==6)) || (itrack==2 && istation==2))) {
							if (itrack == 1) {
								if (istation == 2)
									mbText.text = "The little guy was telling me the next sequence would be shown on the LEFT platform. " +
									"Press 'SWITCH' to look out the other window";
								else if (istation == 4)
									mbText.text = "So I should look RIGHT in the next station, then LEFT in the station after that";
								else
									mbText.text = "Remember the sequence builds up (if not input) so it is now: " + temp;
							} else
								mbText.text = "Some stations were empty...";
							
							controls.SetActive (true);
							
							messageBox.SetActive (true);
							pressedOK = false;
							while (!pressedOK) {
								yield return null;
							}
							messageBox.SetActive (false);
						}

						for (i = nLowSpeed [itrack] [istation] / 2; i < nLowSpeed [itrack] [istation]; i++) {
							x = xst + umin [itrack] [istation] * i;
							train.transform.position = new Vector3 (x, 0, 0);
							cycleTunnel (x - camDistFromFront);
							yield return new WaitForFixedUpdate ();
						}
						xst = xst + umin [itrack] [istation] * nLowSpeed [itrack] [istation];
					}

					tunnel.transform.Translate (new Vector3 (3 * tunnelLength + stationLength, 0, 0));
				}  // istation loop

				print ("Level complete");

				// -----------------------------------------------------------------------------
				// Leave train. 
				// -----------------------------------------------------------------------------

				controls.SetActive (false);

				for (i = 0; i < nCarriages; i++)
					carriages [i].SetActive (true);

				if (mainCamera.transform.position.z > 0) {
					yield return StartCoroutine (mainCameraScript.rotateTo (0, 180, 0, 30));
				} else {
					yield return StartCoroutine (mainCameraScript.rotateTo (0, 0, 0, 30));
				}

				pos = mainCamera.transform.localPosition;
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

			}  // loop over tracks

			jumpout: print ("jumped out");

		}  // end of main game loop

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
	void Update ()
	{
		if (inputMenu.activeSelf) {
			if (Input.GetKeyDown ("1"))
				SendMessage ("button1Pressed");
			else if (Input.GetKeyDown ("2"))
				SendMessage ("button2Pressed");
			else if (Input.GetKeyDown ("3"))
				SendMessage ("button3Pressed");
		}		
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
        Debug.Log("btn 1");
		typed_sequence [nentry] = "1";
		nentry++;

		string temp = "";
		for (int i = 0; i < nentry; i++)
			temp = temp + typed_sequence [i];
		
		textEntry.text = temp;
	}
	
	public void button2Pressed ()
	{
        Debug.Log("btn 2");
        typed_sequence [nentry] = "2";
		nentry++;

		string temp = "";
		for (int i = 0; i < nentry; i++)
			temp = temp + typed_sequence [i];

		textEntry.text = temp;
	}
	
	public void button3Pressed ()
	{
        Debug.Log("btn 3");
        typed_sequence [nentry] = "3";
		nentry++;

		string temp = "";
		for (int i = 0; i < nentry; i++)
			temp = temp + typed_sequence [i];

		textEntry.text = temp;
	}

	public void buttonOKPressed ()
	{
		pressedOK = true;
	}

	public void buttonStartPressed()
	{
		finishedMoving = true;
	}

	// ############################################################

	IEnumerator jumpScare(float camDistFromFront)
	{

		float r = Random.value;

		if (r < 0.5f) {

			GameObject monster = Instantiate (monsterPrefab) as GameObject;
			monster.transform.parent = train.transform;
			monster.transform.localPosition = new Vector3 (-camDistFromFront + 2, 0, 0);
			monster.transform.rotation = Quaternion.Euler (270, 270, 0);
			monster.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);

			Vector3 pos = mainCamera.transform.localPosition;
			yield return StartCoroutine (mainCameraScript.moveTo (pos.x, pos.y, 0, 60));

			yield return StartCoroutine (mainCameraScript.rotateTo (0, 90, 0, 20));

			trackText.text = "AAARRGHHHH!";
			yield return StartCoroutine (mainCameraScript.moveTo (0, -1, 0, 60, true));

			Destroy (monster);
		} else {
			GameObject monster = Instantiate (monsterPrefab) as GameObject;
			float x0 = -camDistFromFront - 4;

			monster.transform.parent = train.transform;
			monster.transform.localPosition = new Vector3 (x0, 0, 0);
			monster.transform.rotation = Quaternion.Euler (270, 90, 0);
			monster.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);

			Vector3 pos=mainCamera.transform.localPosition;

			yield return StartCoroutine (rotateCameraTo (0, 90, 0, 60));
			yield return StartCoroutine (moveCameraTo(pos.x,pos.y,0,20));
			yield return StartCoroutine (moveCameraTo (tunnelLength/5, 0, 0, 200, true));
			yield return StartCoroutine (rotateCameraTo (0, -90, 0, 10));

			for (int i = 0; i < 30; i++) {
				monster.transform.localPosition = new Vector3 (x0 + i * 0.28f, 0, 0);
				yield return null;
			}

			trackText.text = "WHAT IS THAT THING?!";
			yield return StartCoroutine (mainCameraScript.moveTo (0, -1, 0, 60, true));

			Destroy (monster);
		}

		finishedMoving = true;
	}

	// ############################################################

	IEnumerator gotoNextCarriage ()
	{
		bool controlActive = controls.activeSelf;

		controls.SetActive (false);
		Vector3 pos=mainCamera.transform.localPosition;

		float z0 = mainCamera.transform.position.z;

		yield return StartCoroutine (rotateCameraTo (0, 90, 0, 60));
		yield return StartCoroutine (moveCameraTo(pos.x,pos.y,0,20));
		yield return StartCoroutine (moveCameraTo (tunnelLength/2, 0, 0, 200, true));

		if (z0 < 0) {
			yield return StartCoroutine (moveCameraTo (0, 0, camTrainDisp.z, 20, true));
			yield return StartCoroutine (rotateCameraTo (0, 180, 0, 60));
		} else {
			yield return StartCoroutine (moveCameraTo (0, 0, -camTrainDisp.z, 20, true));
			yield return StartCoroutine (rotateCameraTo (0, 0, 0, 60));
		}

		finishedMoving=true;
		if (controlActive) controls.SetActive (true);
	}

	// ############################################################

	public int nWavesFB, nPerWaveFB;
	public float delayFB, delayWavesFB;

	IEnumerator fireballAttack(bool hint)
	{
		mainCameraScript.setFireball (true);
		yield return StartCoroutine (mainCameraScript.rotateTo (0, 90, 0, 30));

		if (hint) {
			mbText.text = "Use SWITCH to dodge fireballs, or burn!";
			messageBox.SetActive (true);
			pressedOK = false;
			while (!pressedOK) {
				yield return null;
			}
			messageBox.SetActive (false);
		}

		controls.SetActive (true);

		for (int j = 0; j < nWavesFB; j++) {
			for (int i = 0; i < nPerWaveFB; i++) {
				if (!mainCameraScript.getFireball ())
					yield break;

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
		survivedFB = true;
	}

	// ############################################################

	void activateCarriages(int carriagesBehind)
	{

		for (int i = 0; i < nCarriages; i++) {
			carriages [i].SetActive (false);
		}

		int myCarriage = nCarriages - 1 - carriagesBehind;

		if (myCarriage>=0)
			carriages [myCarriage].SetActive (true);

		if (myCarriage-1 >= 0)
			carriages [myCarriage-1].SetActive (true);
	}

}
