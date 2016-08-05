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
    Leap.Controller leapController;
    LeapServiceProvider provider;
    
    Vector3 cameraOffset = new Vector3(-50,-50,0);
    Vector3 origin = new Vector3(0,0,-10);
    Vector3 rot;                                        
    
    int layerMask = 1 << 9;                             //Used for raycasting between hands and images
    int id = 0;                                         //0 is Left, 1 is Right
    float pinchDepth = 50;                              //How far to raycast in +z direction (50 ~= inf)                

    InteractableObject targetImage;                     //The image the hand is currently interacting with

    
    void Start()
    {
        id = (int)handedness;
        origin += cameraOffset;
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
        
        if (id == 0) { 
            Vector3 pos = handModel.fingers[0].GetTipPosition();
            Debug.Log("Hand: " + pos.x + ", " + pos.y + ", " + pos.z);
        }
        if (isPinching) {
            //Manipulate Image (drag, rotate, scale)
            //drag if no rotation, else rotate, or scale if both hands are pinched on same image            
            //Handled by the image script
            
            if (targetImage.Rotate(pincher.Rotation.eulerAngles.z, id)) { return; }
            targetImage.Drag(handModel.fingers[0].GetTipPosition() + cameraOffset, id);
        }
    }
    
    public void Pinch() {
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
    
    public void UnPinch() {        
        if (isPinching) {
            targetImage.UnPinch(id);
            isPinching = false;
        }
    }
    
    void OnDisable() {
        //ReleasePoint(true);
        //ReleasePinch(true);
        UnPinch();
    }
    
    bool FindImage(Vector3 pos) {
        //Raycast from index finger position along the +z axis
        RaycastHit hit;
        Physics.Raycast(origin, pos-origin, out hit, pinchDepth, layerMask);        
        //if (hit.collider != null) { Debug.Log("Hit: " + hit.collider.name); }
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
