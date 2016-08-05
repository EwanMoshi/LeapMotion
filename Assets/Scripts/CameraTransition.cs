using UnityEngine;
using System.Collections;
using Leap.Unity;

public class CameraTransition : MonoBehaviour {

    public HandModel handModel;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        
	}
    
    void OnTriggerEnter(Collider other) {
        if (other != null) {
            Debug.LogError("collision: " + other.gameObject.name);
        }
    }
    
}
