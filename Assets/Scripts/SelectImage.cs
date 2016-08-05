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
    
    //public void Activate() { test = true; }
    
    public void Pinch(Vector3 pos) {        
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam,pos);
        //Vector2 screenPos = cam.WorldToScreenPoint(pos) * 5;
        Debug.Log("Pinch Gallery - ScreenPos: " + screenPos + ", Pos: " + pos);
        if (test != null) { test.position = pos; }
        //rt.anchoredPosition = screenPos - canvasRT.sizeDelta / 2;
        if (RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, cam)) {
            if (highlight != null) { highlight.SetActive(!highlight.activeSelf); }
        }
        
        /* Vector2 vp = cam.WorldToViewportPoint(pos);
        Vector2 vz = new Vector2(
        ((vp.x*canvasRT.sizeDelta.x) - (canvasRT.sizeDelta.x*0.5f)),
        ((vp.y*canvasRT.sizeDelta.y) - (canvasRT.sizeDelta.y*0.5f)));
        
        Debug.Log("vz : " + vz.x + "," + vz.y); */
        
        /* Vector3[] fc = new Vector3[4];;
        rt.GetWorldCorners(fc);
        Debug.Log("Corners: " + fc[0] + "," + fc[1] + "," + fc[2] + "," + fc[3]);
        //Debug.Log("canvas pos: " + WorldToCanvasPosition(canvasRT, cam, pos)); */
    }
    
    
    
}
