using UnityEngine;
using System.Collections;

public class handUp : MonoBehaviour {

public Transform handBoundary;
	public bool isUp=false;
	public float handData=0;
	public float headData=0;
		
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		handData=transform.position.y;
		
		if(transform.position.y > handBoundary.position.y) {
			 isUp=true;
		}

		if(transform.position.y < handBoundary.position.y) {
			 isUp=false;
		}
	}
}