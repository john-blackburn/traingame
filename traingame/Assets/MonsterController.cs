using UnityEngine;
using System.Collections;

public class MonsterController : MonoBehaviour
{

	private int i;
	private bool prowling;

	// Use this for initialization
	void Start ()
	{
		print ("monster start");
		i = 0;
		prowling = false;
	}
	
	// Update is called once per frame
	void Update ()
	{
//		print ("prowling="+prowling);
		if (prowling) {
//			print (i);
			Vector3 pos = new Vector3 (0, 0, 5 * Mathf.Sin (i * 0.01f));
			transform.position = pos;
			i++;
		}
	}

	public void prowl (bool p)
	{
		print ("prowl called " + p);
		prowling = p;
		i = 0;
	}
}
