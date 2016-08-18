using UnityEngine;
using System.Collections;

public class FollowTarget : MonoBehaviour {
    public Transform target;    
    public Vector3 distance = new Vector3(0,1,0);

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (target != null) {
            transform.position = target.position + distance;
        }
	}
}
