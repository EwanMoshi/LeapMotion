using UnityEngine;
using System.Collections;

public class ToggleGallery : MonoBehaviour {
    /**
        This script controls the raising (and collapsing) of the gallery UI window that contains
        the images.
    */
    
    //The two UI objects that are manipulated.
    public Transform galleryBG;
    public Transform slidePanel;
    
    bool raised = false;    
    bool raising = false;
    //Vector3 downPos;
    bool up = true;
    int steps = 20;
    int stepCount = 0;
    float slideT = -455;
    float slideB = -578.2f;
    bool preFix = true;
    bool done = false;
    
	
	void Start () {        
        //downPos = slidePanel.localPosition;
        if (preFix) {
            slideT = slidePanel.localPosition.y;            
            slideB = slideT - 200;
            Vector3 v = slidePanel.localPosition;
            v.y = slideB;
            slidePanel.localPosition = v;            
            galleryBG.localPosition = v;
        }
	}
	
	
	void Update () {
        if (raising) {
            float t = stepCount / (steps + 0.0f);
            if (up) { 
                slidePanel.localPosition = new Vector3(slidePanel.localPosition.x,Mathf.Lerp(slideB, slideT, t), slidePanel.localPosition.z);
                galleryBG.localPosition = new Vector3(galleryBG.localPosition.x,Mathf.Lerp(slideB, slideT, t), galleryBG.localPosition.z);
            } else {
                slidePanel.localPosition = new Vector3(slidePanel.localPosition.x,Mathf.Lerp(slideT, slideB, t), slidePanel.localPosition.z);
                galleryBG.localPosition = new Vector3(galleryBG.localPosition.x,Mathf.Lerp(slideT, slideB, t), galleryBG.localPosition.z);
            }
            if (stepCount == steps) {
                stepCount = 0;
                if (up) { raised = true; } else { raised = false; }
                raising = false;
                done = true;
                return;
            }
            stepCount++;
        }
	}
    
    public bool IsToggled() {
        return done;
    }
    
    public void Toggle() {
        if (raising) { return; }
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
