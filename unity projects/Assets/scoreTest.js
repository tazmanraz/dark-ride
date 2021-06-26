#pragma strict

static var score : int=score;
public var guiScore : GUIText;

function Start () {
	guiScore.text="Score: 0";
}

function Update (){
}

function OnCollisionEnter(col : Collision){
	if(col.collider.name=="Cylinder"){
		Destroy(col.gameObject);
		score -= 5;
		guiScore.text="Score: "+score;
		
}

if(col.collider.name=="Sphere"){
		Destroy(col.gameObject);
		score += 5;
		guiScore.text="Score: "+score;

}}