/* using UnityEngine;
using System.Collections;
using Leap.Unity;
using System;

public class PinchDetectionR : MonoBehaviour
{
    public PinchDetector pcd;
    public ExtendedFingerDetector efd;
    public enum Handedness {Left, Right};
    public Handedness handedness;    
    public HandModel handModel;
    
    bool isPinching = false;
    bool isPointing = false;    
    
    Vector3 cameraOffset = new Vector3(-50,-50,0);
    Vector3 origin = new Vector3(0,0,-10);
    Vector3 pos;
    
    int layerMask = 1 << 9;
    int pointTimer = 10;
    int pointCounter;
    int id = 0;
    float pinchStart = 0.5f;
    float pinchDepth = 50;

    InteractableObject targetImage;

    
    void Start()
    {
        id = (int)handedness;
        origin += cameraOffset;
    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 thumbPos = handModel.fingers[0].GetBoneCenter(3);
        //Vector3 indexPos = handModel.fingers[1].GetBoneCenter(3);
        //Debug.Log("Tracked: " + handModel.IsTracked);
        //if (!handModel.IsTracked) {
        //    ReleasePoint(true);
        //    ReleasePinch(true);
        //}
        
        // store position of the pinch (at thumb)        
        pos = handModel.fingers[0].GetTipPosition();
        pos += cameraOffset;
        
        //Check pointing and pinch status
        bool pinchFrame = false;
        bool pointFrame = CalculatePointing();
        if (!pointFrame) {             
            pinchFrame = CalculatePinching();
        }
        
        //If neither gesture is active, cancel interaction as needed
        if (!pointFrame && !pinchFrame) { 
            ReleasePoint(true);
            ReleasePinch(true);
            return;
        }
        
        //Both gestures require an image in line with the index finger
        if (targetImage == null) {
            FindImage(pos);
        }
        
        //If none is found, cancel
        if (targetImage == null) { return; }
        
                
        if (pointFrame) {// || pointCounter < pointTimer) {
            //if (!pointFrame) { pointCounter++; }
            Point(pos);
        } else if (pinchFrame) {
            Pinch(pos);
        }
    }
    
    void OnDisable() {
        ReleasePoint(true);
        ReleasePinch(true);
    }
        
    void Point(Vector3 pos) {
        //Cancel pinch if active
        if (isPinching) {
            ReleasePinch(false);
            FindImage(pos);
            if (targetImage == null) { return; }
        }        
        
        if (!isPointing) {
            pointCounter = 0;
            Debug.Log("Point Start: " + id);
            //Start of new pointing gesture
            if (!targetImage.Point(pos, id)) {
                //Can't Point
                targetImage = null;
                return;
            }
            isPointing = true;            
        } else {
            //Continue previous pointing
            targetImage.Rotate(pos, id);
        }
    }
    
    void ReleasePoint(bool drop) {
        if (isPointing) {
            Debug.Log("Release Point: " + id);
            targetImage.ReleasePoint(id);
            if (drop) { targetImage = null; }
            isPointing = false;
        }
    }
    
    void ReleasePinch(bool drop) {
        if (isPinching) {
            Debug.Log("Release Pinch: " + id);
            targetImage.ReleasePinch(id);            
            if (drop) { targetImage = null; }
            isPinching = false;
        }
    }
    
    
    void Pinch(Vector3 pos) {        
        if (isPointing) {
            ReleasePoint(false);
            FindImage(pos);
            if (targetImage == null) { return; }
        }
        
        if (!isPinching) {
            Debug.Log("Pinch Start: " + id);
            //New pinch gesture
            if (!targetImage.Pinch(pos, id)) {
                targetImage = null;
                return;
            }
            isPinching = true;
        } else {
            //Continue previous pinch
            targetImage.Drag(pos, id);
        }
    }   
    
    
    bool CalculatePinching() {
        // make sure we're not pointing (because a point registers as a pinch at the moment)
        //if (handModel.GetLeapHand().PinchStrength > pinchStart) { return true; }
        
        return pcd.IsPinching;
    }
    
    //Used by pinching finger to get image - obsolete atm
    public InteractableObject GetImage() {
        return targetImage;
    }
    
    
    void FindImage(Vector3 pos) {
        //Raycast from index finger position along the +z axis
        RaycastHit hit;
        Physics.Raycast(origin, pos-origin, out hit, pinchDepth, layerMask);        
        //if (hit.collider != null) { Debug.Log("Hit: " + hit.collider.name); }
        
        if (hit.collider != null) {
            //Hit an image, do something...
            targetImage = hit.collider.gameObject.GetComponent<InteractableObject>();
        }
    }

    private bool CalculatePointing()
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
    }



}
 */