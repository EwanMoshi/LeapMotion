using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Leap.Unity;

public class GalleryTransition : MonoBehaviour {


    public GestureDetection leftHand;
    public GestureDetection rightHand;
    bool on = true;
    public Camera cam;
    float h;
    float w;    
    float galleryHeight = 118.38f;        //This is the height value of the GalleryUI BG as seen in Unity editor
    float galleryZ = 1;                   //Position in world of Canvas (-9 is +1 away from 10 the camera.z value)
    float galleryHeightMult = -0.008538899f;
    float galleryHeightMod;               //Height works perfectly in Scene view but is somehow not aligned in Game
                                          //view so we must add some arbitrary modifier


	// Use this for initialization
	void Start () {        
        Resize();
	}

    void Resize() {
        //Resize gallery trigger area based on screen size
        h = cam.pixelHeight;
        w = cam.pixelWidth;        
        galleryHeightMod = h * galleryHeightMult;
        //Debug.Log((-4.5f / cam.pixelHeight));

        //The collision area needs to be a cube that is as wide as the screen
        //and the top face must be aligned with the cam center position
        //and the top of the galleryUI - this involves some screenspace to worldspace
        //shenaningans.
        transform.rotation = Quaternion.identity;
        float xScale = (cam.ScreenToWorldPoint(new Vector3(w, 0, galleryZ)) - cam.ScreenToWorldPoint(new Vector3(0, 0, galleryZ))).x;
        //yScale + zScale depend on rotation so overcompensate instead!
        float yScale = 1;
        float zScale = 2;
        Vector3 scale = new Vector3(xScale, yScale, zScale);

        //Rotate collider
        Vector3 topOfUI = cam.ScreenToWorldPoint(new Vector3(w*.5f,  galleryHeight + galleryHeightMod, galleryZ));
        Vector3 topDir = cam.transform.position - cam.ScreenToWorldPoint(new Vector3(w*.5f, galleryHeight, galleryZ));
        float angleDif = Vector3.Angle(cam.transform.position, topDir);
        transform.Rotate(new Vector3(angleDif,0,0));
        Vector3 pos = topOfUI + -transform.up*(yScale*.5f);

        transform.localScale = scale;
        transform.position = pos;
    }

	// Update is called once per frame
	void Update () {
        if (h != cam.pixelHeight || w != cam.pixelWidth) {
            Resize();
        }
	}

    void OnTriggerEnter(Collider other) {
        if (other != null) {
            if (other.gameObject.name.Equals("palmL")) {
                leftHand.GalleryTrigger(on);
            } else if  (other.gameObject.name.Equals("palmR")) {
                rightHand.GalleryTrigger(on);
            }
        }
        on = true;
    }

    void OnTriggerExit(Collider other) {
        on = false;
        OnTriggerEnter(other);
    }

    void OnTriggerStay(Collider other) {
        //If required...
        OnTriggerEnter(other);

    }

}
