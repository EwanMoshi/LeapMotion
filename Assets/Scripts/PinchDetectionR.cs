using UnityEngine;
using System.Collections;
using Leap.Unity;
using System;

public class PinchDetectionR : MonoBehaviour
{
    
    public bool DebugSpam = false;
    
    public PinchDetectionR otherHand;

    [SerializeField]
    private float maxPinchDistance;
    [SerializeField]
    private float minPinchDistance;
    //[SerializeField]
    private float pinchStart;

    private HandModel handModel;
    private HandDrop hand;

    public float force = 50.0f;
    public float magnetDistance = 0.05f;

    public bool isPinching = false;
    public Vector3 pinchPosR;
    public Vector3 previousPinchPosR = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 prevPinchDistance;
    
    public float pinchDepth = 1;
    //Vector3 z = new Vector3(0,0,1);
    Vector3 origin = new Vector3(0,0,-10);
    int layerMask = 1 << 9;
    Vector3 pos;
    public enum Handedness {Left, Right};
    public Handedness handedness;
    int id = 0;
    Vector3 cameraOffset = new Vector3(-50,-50,0);

    InteractableObject targetImage;

    private bool togglePinch = false;
    private bool isPointing = false;

    protected Collider grabbedImage;

    // Use this for initialization
    void Start()
    {
        handModel = transform.GetComponent<HandModel>(); //transform.GetComponent<HandModel>();
        id = (int)handedness;
        origin += cameraOffset;
    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 thumbPos = handModel.fingers[0].GetBoneCenter(3);
        //Vector3 indexPos = handModel.fingers[1].GetBoneCenter(3);

        // store position of the pinch (at thumb)
        //pinchPosR = handModel.fingers[0].GetTipPosition();
        pos = handModel.fingers[0].GetTipPosition();
        pos += cameraOffset;

        //Pointing
        togglePinch = false;
        isPointing = false;
        if (targetImage != null ) { // only perform pointing check if we have something grabbed already
            if (calculatePointing()) {
                if (isPinching) {
                    OnReleasePinch();
                }
                
                targetImage = otherHand.GetImage();
                if (targetImage == null) {                 
                    if (isPointing) { isPointing = false; }
                    return; 
                }
                
                if (!isPointing) {
                    Debug.Log("Hand: " + id + ", is pointing");                                        
                    isPointing = true;
                    targetImage.OnPoint(pos, id);
                } else {
                    //targetImage.PointCheck(id);
                    targetImage.Rotate(pos, id);
                }
                return;
            }
            //Pointing is now deactivated during the other hands UnPinch
            /*  else {
                if (isPointing) {
                    targetImage.OnReleasePoint(id);
                }
            } */
        }
        

        // check if the user has successfully pinched an image and toggle pinching if they have
        if (handModel.GetLeapHand().PinchStrength > pinchStart) // make sure we're not pointing (because a point registers as a pinch at the moment)
        {
            togglePinch = true;
        }

        if (togglePinch && !isPinching) {
            FindImage(pos);
            isPinching = true;            
        } else if (!togglePinch && isPinching) {
            OnReleasePinch();
        }

        if (targetImage != null) {
            //if (isPointing) {
            //    targetImage.Rotate(pos, id);
            //} else {
                //Dragging an image
                //Debug.Log("dragging: " + id);
                targetImage.Drag(pos, id);
            //}
        }
    }
    
    //Used by pinching finger to get image
    public InteractableObject GetImage() {
        return targetImage;
    }

    void OnReleasePinch() {
        //Debug.Log("Release: " + id);
        isPinching = false;
        if (targetImage != null) {
            targetImage.ReleasePinch(id);
            targetImage = null;
        }        
    }
    
    
    void FindImage(Vector3 pos) {
        //Raycast from index finger position along the +z axis
        RaycastHit hit;
        Physics.Raycast(origin, pos-origin, out hit, pinchDepth, layerMask);        
        //if (hit.collider != null) { Debug.Log("Hit: " + hit.collider.name); }
        
        if (hit.collider != null) {
            //Hit an image, do something...
            targetImage = hit.collider.gameObject.GetComponent<InteractableObject>();
            targetImage.Pinch(pos, id);            
        }
    }

    private bool calculatePointing()
    {
        Vector3 indexDirection = handModel.fingers[1].GetBoneDirection(3);
        Vector3 middleDirection = handModel.fingers[2].GetBoneDirection(3);
        Vector3 ringDirection = handModel.fingers[3].GetBoneDirection(3);
        Vector3 pinkyDirection = handModel.fingers[4].GetBoneDirection(3);

        if (indexDirection.z >= 0.8 && middleDirection.z < 0.8 && ringDirection.z < 0.8 && pinkyDirection.z < 0.8) {
            return true;
        }
        else {
            return false;
        }
    }



}
