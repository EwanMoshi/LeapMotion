using UnityEngine;
using System.Collections;

public class SelectImage : MonoBehaviour {
    
    //bool test = true;
    public RectTransform rt, canvasRT;
    public GameObject highlight;
    Camera cam;
	
    void Start() {
        rt = GetComponent<RectTransform>();
        canvasRT = GameObject.Find("GalleryUI").GetComponent<RectTransform>();
        highlight = transform.Find("Highlight").gameObject;
        cam = GameObject.Find("GalleryCamera").GetComponent<Camera>();
        
    }
    
    //public void Activate() { test = true; }
    
    public void Pinch(Vector3 pos) {
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam,pos);
        //rt.anchoredPosition = screenPos - canvasRT.sizeDelta / 2;
        if (RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, cam)) {
            highlight.SetActive(!highlight.activeSelf);
        }
    }
    
    
}
