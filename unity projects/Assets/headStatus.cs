using UnityEngine;
using System.Collections;

public class headStatus : MonoBehaviour {

public Transform torso;
	public bool isDucking=false;
	public float headData=0;
	public float torsoData=0;
	
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		headData=transform.position.y;
		torsoData=torso.position.y;
		
		if(headData - torsoData<0.35) {
			 isDucking=true;
		}
		
				if(headData - torsoData>0.35) {
			 isDucking=false;
		}
		
	}
}