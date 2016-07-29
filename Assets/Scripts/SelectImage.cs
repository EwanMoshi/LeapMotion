using UnityEngine;
using System.Collections;

public class SelectImage : MonoBehaviour {
    
    //bool test = true;
    public RectTransform rt, canvasRT;
    public GameObject highlight;
	
    void Start() {
        rt = GetComponent<RectTransform>();
        canvasRT = GameObject.Find("CanvasSlider").GetComponent<RectTransform>();
        highlight = transform.Find("Highlight").gameObject;
    }
    
    //public void Activate() { test = true; }
    
    public void Pinch(Vector3 pos) {
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(Camera.main,pos);
        //rt.anchoredPosition = screenPos - canvasRT.sizeDelta / 2;
        if (RectTransformUtility.RectangleContainsScreenPoint(rt, screenPos, Camera.main)) {
            highlight.SetActive(!highlight.activeSelf);
        }
    }
    
    
    
    
    
}
