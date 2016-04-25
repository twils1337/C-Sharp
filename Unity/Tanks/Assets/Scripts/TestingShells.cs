using UnityEngine;
using System.Collections;

public class TestingShells : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        GetComponent<Rigidbody>().velocity = 10f*transform.forward;	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
