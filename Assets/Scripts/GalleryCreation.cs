using UnityEngine;
using System.Collections;

public class GalleryCreation : MonoBehaviour {

    public Material[] images;
    public float imageCountOverride;

    float width;
    float edgePercent = 0.05f;
    float gapPercent = 0.025f;
    
    

	// Use this for initialization
	void Start () {
        //Load images and scale to screen size        
        width = Screen.width;
        
        
        float count = Mathf.Max(imageCountOverride, images.Length);
        float edge = width * edgePercent;
        float gap = width * gapPercent;
        
        float imgWidth = (width - edge*2 - gap*(count-1)) / count;
        
        //Debug.Log("imgWidth: " + imgWidth);
        
        float x = 0;
        
        
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
