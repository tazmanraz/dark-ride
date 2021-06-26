#pragma strict

var speed:float=2.0;

//function Start () {

//}

function Update () {
	transform.Translate(Vector3(-speed,0,0)*Time.deltaTime);

}