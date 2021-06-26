using UnityEngine;
using System.Collections;

public class destroyBall : MonoBehaviour {


    void OnCollisionEnter (Collision col)
    {
        if(col.gameObject.name == "Cylinder")
        {
            Destroy(col.gameObject);
        }
    }
}
