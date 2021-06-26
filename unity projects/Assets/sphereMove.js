#pragma strict

var speed:float=1.5;

//function Start () {

//}

function Update () {
	transform.Translate(Vector3(0,0,-speed)*Time.deltaTime);

}