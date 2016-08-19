using UnityEngine;
using System.Collections;

public class FollowTarget : MonoBehaviour {
    public Transform target;    
    public Vector3 distance = new Vector3(0,1,0);

	
	//Used by rotation UI elements to track the hands position
	void Update () {
        if (target != null) {
            transform.position = target.position + distance;
        }
	}
}
