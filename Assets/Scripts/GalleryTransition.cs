using UnityEngine;
using System.Collections;
using Leap.Unity;

public class GalleryTransition : MonoBehaviour {

    
    public GestureDetection leftHand;
    public GestureDetection rightHand;    
    bool on = true;

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        
	}
    
    void OnTriggerEnter(Collider other) {
        if (other != null) {            
            //if (other.gameObject.name.Equals("bone3IL")) {
            if (other.gameObject.name.Equals("palmL")) {
                //Debug.Log("Left in Gallery");
                leftHand.GalleryTrigger(on);
            //} else if (other.gameObject.name.Equals("bone3IR")) {
            } else if  (other.gameObject.name.Equals("palmR")) {
                //Debug.Log("Right in Gallery");
                rightHand.GalleryTrigger(on);
            }
        }
        on = true;
    }
    
    void OnTriggerExit(Collider other) {
        on = false;
        OnTriggerEnter(other);
    }
    
    void OnTriggerStay(Collider other) {
        //If required...
        OnTriggerEnter(other);
        
    }
    
}
