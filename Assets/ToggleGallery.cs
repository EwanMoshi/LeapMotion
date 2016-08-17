using UnityEngine;
using System.Collections;

public class ToggleGallery : MonoBehaviour {

    public Transform galleryBG;
    public Transform slidePanel;
    
    bool raised = false;    
    bool raising = false;
    Vector3 downPos;
    bool up = true;
    int steps = 10;
    int stepCount = 0;
    float top = -0.6f;
    float bottom = -1.4f;
    
    float slideT = -455;
    float slideB = -578.2f;
    float bgT = -91;
    float bgB = -214.2f;
    
    
	// Use this for initialization
	void Start () {
        downPos = slidePanel.localPosition;
        //Debug.Log(downPos + ", " + galleryBG.localPosition);
	}
	
	// Update is called once per frame
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
                return;
            }
            stepCount++;
        }
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
