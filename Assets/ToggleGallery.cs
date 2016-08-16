using UnityEngine;
using System.Collections;

public class ToggleGallery : MonoBehaviour {

    public Transform galleryBG;
    public Transform slidePanel;
    
    bool raised = false;    
    bool raising = false;
    Vector3 downPos;
    bool up = true;
    
    

	// Use this for initialization
	void Start () {
        downPos = galleryBG.position;        
	}
	
	// Update is called once per frame
	void Update () {
        
	}
    
    public void Toggle() {
        if (!raised) {
            //Raise
            raising = true;
            up = true;
        } else {
            raising = true;            
            up = false;
        }
    }
}
