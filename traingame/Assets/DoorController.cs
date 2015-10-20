using UnityEngine;
using System.Collections;

public class DoorController : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public IEnumerator open()
	{
		const float backS = 1.70158f;

		for (float s=0; s<1; s+=0.02f) {
			float ratio = s - 1;
			float sp=ratio*ratio*((backS+1)*ratio+backS)+1;

			Vector3 pos = transform.position;
			pos.y = sp * 2.62f;
			transform.position=pos;
			yield return null;
		}

	}

}
