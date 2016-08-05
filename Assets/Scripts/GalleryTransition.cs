using UnityEngine;
using System.Collections;
using Leap.Unity;

public class GalleryTransition : MonoBehaviour {

    
    public GestureDetection leftHand;
    public GestureDetection rightHand;    
    

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
        
	}
    
    void OnTriggerEnter(Collider other) {
        if (other != null) {
            if (other.gameObject.name.Equals("bone3IL")) {
                //Debug.Log("Left in Gallery");
                leftHand.GalleryTrigger(true);
            } else if (other.gameObject.name.Equals("bone3IR")) {
                //Debug.Log("Right in Gallery");
                rightHand.GalleryTrigger(true);
            }
        }
    }
    
}
