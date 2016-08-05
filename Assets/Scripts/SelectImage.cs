using UnityEngine;
using System.Collections;

public class SelectImage : MonoBehaviour {
    
    //bool test = true;
    public RectTransform rt, canvasRT;
    public GameObject highlight;
    Camera cam;
    public Transform test;
    bool done = false;
	
    void Start() {
        rt = GetComponent<RectTransform>();
        canvasRT = GameObject.Find("GalleryUI").GetComponent<RectTransform>();
        highlight = transform.Find("Highlight").gameObject;
        cam = GameObject.Find("GalleryCamera").GetComponent<Camera>();
        //cam = Camera.main;
    }
    
    void Update() { 
        if (!done) {
        Debug.Log(name + " - screenPos: " + transform.position);
        done = true;
        }
    }
    
    public bool Selected() {
        return highlight.activeSelf;
    }
    
    public int Pinch(Vector3 pos, int force = -1) {
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam,pos);
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
