using UnityEngine;
using System.Collections;

public class skewcontrol2 : MonoBehaviour {
	
	public GameObject target;
	public GameObject toTrack;
	
	MegaModifyObject	modObj;
	MegaSkew			skewMod;
	float[]				xhistory;
	float				avgval;
	int					count=0;

	
	
	
	// Use this for initialization
	void Start () {
		
		//initialx = toTrack.transform.position.x;
		//initialz = toTrack.transform.position.z;
		
		xhistory = new float[10];
		
		modObj = target.AddComponent<MegaModifyObject>();
		skewMod = target.AddComponent<MegaSkew>();
		modObj.MeshUpdated ();
		
		skewMod.Offset.z = -52.4f;
		skewMod.dir = 90.0f;
		skewMod.axis = MegaAxis.Y;
		
	}
	
	// Update is called once per frame
	void Update () {
	
	xhistory[count] = toTrack.transform.position.y;
		if (count==9)
			count=0;
		else {
			count++;
		}
		
		avgval = (xhistory[0]+xhistory[1]+xhistory[2]+xhistory[3]+xhistory[4]+xhistory[5]+xhistory[6]+xhistory[7]+xhistory[8]+xhistory[9])/10; 
		
		//skewMod.amount = 100*(toTrack.transform.position.x);
		skewMod.amount = 100*avgval-30;
		//xscale = initialx/(toTrack.transform.localScale.x);
		//toTrack.transform.localScale = new Vector3(xscale,1,1);
		
	}
}
