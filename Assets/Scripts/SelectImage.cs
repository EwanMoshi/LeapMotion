using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SelectImage : MonoBehaviour {
    
    //bool test = true;
    public RectTransform rt, canvasRT;
    public GameObject highlight;
    Camera cam;
    public Transform test;
    public RawImage raw;
    
    
	
    void Start() {
        rt = GetComponent<RectTransform>();
        canvasRT = GameObject.Find("GalleryUI").GetComponent<RectTransform>();
        highlight = transform.Find("Highlight").gameObject;
        //cam = GameObject.Find("GalleryCamera").GetComponent<Camera>();
        cam = GameObject.Find("Main Camera").GetComponent<Camera>();
        if (raw == null) { raw = GetComponentInChildren<RawImage>(); }
    }
    
   
    
    public bool IsSelected() {
        return highlight.activeSelf;
    }
    
    public void DeSelect() {
        highlight.SetActive(false);
    }
    
    public void Toggle() {
        highlight.SetActive(!highlight.activeSelf);
    }
    
    public bool PinchCheck(Vector3 pos) {
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam,pos);
        return RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, cam);
    }
    
    public int Pinch(Vector3 pos, int force = -1) {
        //pos.x *= (60/45);
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam,pos);
        screenPos.x *= (60/45);
        //Debug.Log("Pinch Gallery - ScreenPos: " + screenPos + ", Pos: " + pos);
        if (test != null) { test.position = pos; }
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
