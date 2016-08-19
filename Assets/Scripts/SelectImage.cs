using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/**
    This class is used by the image tiles in the GalleryUI.
    Objects must respond to pinches via a highlight effect.
*/

public class SelectImage : MonoBehaviour {
    
    
    public RectTransform rt, canvasRT;
    public GameObject highlight;    
    public RawImage raw;
    
    Camera cam;
	
    void Start() {
        rt = GetComponent<RectTransform>();
        canvasRT = GameObject.Find("GalleryUI").GetComponent<RectTransform>();
        highlight = transform.Find("Highlight").gameObject;        
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        if (raw == null) { raw = GetComponentInChildren<RawImage>(); }
    }
    
    public bool IsSelected() {
        return highlight.activeSelf;
    }
    
    public void DeSelect() {
        if (highlight == null) { GetHighlight(); }
        highlight.SetActive(false);
    }
    
    public void Toggle() {
        if (highlight == null) { GetHighlight(); }        
        highlight.SetActive(!highlight.activeSelf);
    }
    
    void GetHighlight() {
        highlight = transform.Find("Highlight").gameObject;
    }
    
    //Used to check whether a pinch position was over this image
    public bool PinchCheck(Vector3 pos) {
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam,pos);
        return RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, cam);
    }
    
    //Check a pinch is on to of this image and report results. -1 failed, 0 deslect, 1 select
    public int Pinch(Vector3 pos, int force = -1) {
        //Position must be translated to screenPosition to check against UI elements
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam,pos);            
        if (RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, cam)) {
            if (highlight != null) { 
                bool state = !highlight.activeSelf;
                if (force == 0) { state = false; }
                else if (force == 1) { state = true; }
                highlight.SetActive(state);
                if (highlight.activeSelf) { return 1; } else { return 0; }
            }
        }
        return -1;
    }
    
    
    
}
