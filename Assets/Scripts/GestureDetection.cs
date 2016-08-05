using UnityEngine;
using System.Collections;
using Leap.Unity;
using System;


public class GestureDetection : MonoBehaviour
{
    public PinchDetector pincher;
    public ExtendedFingerDetector efd; 
    public enum Handedness {Left, Right};
    public Handedness handedness;    
    public HandModel handModel;
    
    bool isPinching = false;
    bool selectionFromGallery = false;
    bool inGallery = false;
    Leap.Controller leapController;
    LeapServiceProvider provider;
    
    Vector3 cameraOffset = new Vector3(-50,-50,0);
    Vector3 origin = new Vector3(0,0,-10);
    Vector3 rot;                                        
    
    int layerMask = (1 << 9);                           //Used for raycasting between hands and images
    int id = 0;                                         //0 is Left, 1 is Right
    float pinchDepth = 50;                              //How far to raycast in +z direction (50 ~= inf)
    

    InteractableObject targetImage;                     //The image the hand is currently interacting with
    public SelectImage[] selectImage;                   //The images in the gallery   
    public ArrayList selectedImages;
    
    void Start()
    {
        id = (int)handedness;
        origin += cameraOffset;
        if (selectImage == null || selectImage.Length == 0) {
            selectImage = GameObject.FindObjectsOfType<SelectImage>();
            selectedImages = new ArrayList();
        }
    }
    
    void Awake() {   
        if (provider == null) {
            provider = GameObject.FindObjectOfType<LeapServiceProvider>();
        }
    
        if (leapController == null) {
            leapController = provider.GetLeapController();
        }
    }
    
    void Update() {
        if (isPinching) {
            if (!inGallery) {
                if (!selectionFromGallery) {
                    //Manipulate Image (drag, rotate, scale)
                    Vector3 pos = handModel.fingers[0].GetTipPosition() + cameraOffset;
                    if (targetImage.Rotate(pos, pincher.Rotation.eulerAngles.z, id)) { return; }
                    targetImage.Drag(pos, id);
                } else {
                    //Some selected images dragged from gallery
                    
                    
                }
            } else {
                //Manipulate thumbnails in gallery
                
                
                
            }
        }
    }
    
    public void GalleryTrigger(bool on) {
        if (!inGallery && on) {
            inGallery = true;
            UnPinchDrawingArea();
        }
        if (inGallery && !on) {
            inGallery = false;
            
            //Logic for transition from gallery (dragging selection...)
            
            //If any images are selected create the 'full size' versions of them
            //and attach them to the pinching hand in some fashion
            
            
            
            
            //Cancel previous selection
        }
    }
    
    public void PinchGate(bool on) {
        //Decide which pinch action to take - Gallery or Main Area, On or Off
        if (!inGallery) {
            if (on) {
                PinchDrawingArea();
            } else {
                UnPinchDrawingArea();
            }
        } else {
            PinchGalleryArea();
        }
    }
    
    public void PinchDrawingArea() {
        //Begins a pinch if requirements are met
        Vector3 pos = handModel.fingers[0].GetTipPosition() + cameraOffset;
        bool f = FindImage(pos);
        //Debug.Log("Pinch: " + handedness + ", isPinching: " + isPinching + ", Image: " + f + ", pos: " + pos);
        
        if (!isPinching && f) {
            //Hand has pinched an image
            isPinching = targetImage.Pinch(pos,id);
            if (!isPinching) {
                //targetImage denies pinch request, abort.
                targetImage = null;
                return;
            }
        }
    }
    
    public void UnPinchDrawingArea() {        
        if (isPinching) {
            targetImage.UnPinch(id);
            isPinching = false;
        }
    }
    
    public void PinchGalleryArea() {        
        Vector3 pos = handModel.fingers[0].GetTipPosition();        
        foreach (SelectImage s in selectImage) {
            s.Pinch(pos);
        }
    }
    
    public void UnPinchGalleryArea() {
        Vector3 pos = handModel.fingers[0].GetTipPosition();
        foreach (SelectImage s in selectImage) {
            s.Pinch(pos, 0);
        }
        
    }
    
    public void TapGalleryArea() {
        //Multi select
        if (!inGallery) { return; }
        
    }
    
    
    
    void OnDisable() {
        //ReleasePoint(true);
        //ReleasePinch(true);
        UnPinchDrawingArea();
        UnPinchGalleryArea();
    }
    
    bool FindImage(Vector3 pos) {
        //Raycast from index finger position along the +z axis
        RaycastHit hit;
        Physics.Raycast(origin, pos-origin, out hit, pinchDepth, layerMask);        
        if (hit.collider != null) { Debug.Log("Hit: " + hit.collider.name); }
        //Debug.DrawRay(origin, pos-origin, Color.blue, 5);
        
        if (hit.collider != null) {
            //Hit an image, do something...
            targetImage = hit.collider.gameObject.GetComponent<InteractableObject>();
            return true;
        }
        return false;
    }

    //Whats the point??
    /* private bool CalculatePointing()
    {
        //return efd.IsPointed();
        Vector3 indexDirection = handModel.fingers[1].GetBoneDirection(3);
        Vector3 middleDirection = handModel.fingers[2].GetBoneDirection(3);
        Vector3 ringDirection = handModel.fingers[3].GetBoneDirection(3);
        Vector3 pinkyDirection = handModel.fingers[4].GetBoneDirection(3);

        //Leap seems to like pointing with index and middle for some reason
        if (indexDirection.z >= 0.8  && middleDirection.z < 0.8  && ringDirection.z < 0.8 && pinkyDirection.z < 0.8) {
            return true;
        }
        else {
            return false;
        }
    } */



}
