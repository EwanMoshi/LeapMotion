using UnityEngine;
using System.Collections;
using Leap.Unity;
using System;

public class PinchDetectionR : MonoBehaviour
{
    
    public bool DebugSpam = false;

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

    public bool isPinchingR = false;
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
        Vector3 thumbPos = handModel.fingers[0].GetBoneCenter(3);
        Vector3 indexPos = handModel.fingers[1].GetBoneCenter(3);

        togglePinch = false;
        isPointing = false;

        if (targetImage != null ) // only perform pointing check if we have something grabbed already
        {
            if (calculatePointing())
            {
                isPointing = true;
                //OnPoint();
            }
            //else
            //{
            //    targetImage.OnReleasePoint(id);
            //}
        }
        

        // check if the user has successfully pinched an image and toggle pinching if they have
        if (handModel.GetLeapHand().PinchStrength > pinchStart)
        {
            togglePinch = true;
        }

        // store position of the pinch (at thumb)
        //pinchPosR = handModel.fingers[0].GetTipPosition();
        pos = handModel.fingers[0].GetTipPosition();
        pos += cameraOffset;

        if (togglePinch && !isPinchingR)
        {
            OnPinch(pos);
        }
        else if (!togglePinch && isPinchingR)
        {
            OnReleasePinch();
        }


        if (targetImage != null) {
            if (isPointing) {
                targetImage.Rotate(pos, id);
            }
            else {
                //Dragging an image
                //Debug.Log("dragging: " + id);
                targetImage.Drag(pos, id);
            }
        }
        
        /* if (grabbedImage != null) {
            if(PinchDetectionL.isPinchingL) { // ensure both hands are pinching
                scaleImage();
            }
            else {
                
                //Physics.IgnoreCollision(handModel.GetComponent<Collider>(), grabbedImage.GetComponent<Collider>(), false);
                //Debug.Log("Something has been grabbed");
                //FreePositionFreezeRotation();
                //Vector3 moveDistance = pinchPosR - grabbedImage.transform.position;
                //Debug.Log("moveDistance >>>>  "+moveDistance);
                //Debug.Log("force >>>>> " + force * moveDistance);
                //grabbedImage.GetComponent<Rigidbody>().AddForce(force * moveDistance);
            }
        } */

        //if(grabbedImage != null) {
        //Debug.Log(grabbedImage.ToString());
        //Debug.Log("grabbed");
        //}
    }

    private void scaleImage() {
        //Renderer rend = grabbedImage.GetComponent<Renderer>();
        grabbedImage.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;    

        float newX =  Mathf.Abs(pinchPosR.x - PinchDetectionL.pinchPosL.x);
        float newY = Mathf.Abs(pinchPosR.y - PinchDetectionL.pinchPosL.y);


        grabbedImage.GetComponent<Rigidbody>().transform.localScale = new Vector3(newX, newY, 1.0f);


    }

    // This function frees the constraints so the image can be moved around and rotated
    // but then freeze rotation so we don't rotate it while moving
    // RigibodyConstraints API has no support for only freeing the position so we have 
    // free all, then freeze rotation
    void FreePositionFreezeRotation()
    {
        grabbedImage.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        grabbedImage.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
    }
    
    void OnPoint()
    {
        //targetImage.OnPoint(id);
    }

    void OnReleasePinch()
    {
        //Debug.Log("Release: " + id);
        isPinchingR = false;
        if (targetImage != null) {
            targetImage.ReleasePinch(id);
            targetImage = null;
        }
        
        
        //if (grabbedImage != null) {
        //    grabbedImage.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll; // freeze all (position and rotation)   
        //}
        //Physics.IgnoreCollision(handModel.GetComponent<Collider>(), grabbedImage.GetComponent<Collider>(), false);
        //grabbedImage = null;
        //isPinchingR = false;
        //Debug.Log("ReleasePinch");
    }

    void OnPinch(Vector3 pos)
    {
        //Debug.Log("Pinch: " + id);
        //Raycast from pinch position along the +z axis
        RaycastHit hit;
        Physics.Raycast(origin, pos-origin, out hit, pinchDepth, layerMask);        
        if (hit.collider != null) { Debug.Log("Hit: " + hit.collider.name); }
        
        if (hit.collider != null) {
            //Hit an image, do something...
            targetImage = hit.collider.gameObject.GetComponent<InteractableObject>();
            targetImage.Pinch(pos, id);            
        }
        isPinchingR = true;
        
        
        /* isPinchingR = true;
        //Debug.Log("OnPinch");
        Collider[] nearImages = Physics.OverlapSphere(pinchPosR, magnetDistance);
        Vector3 dist = new Vector3(magnetDistance, 0.0f, 0.0f);

        for (int i = 0; i < nearImages.Length; i++)
        {
            Vector3 newDistance = pinchPosR - nearImages[i].transform.position;
            if (nearImages[i].GetComponent<Rigidbody>() != null && newDistance.magnitude < dist.magnitude && !nearImages[i].transform.IsChildOf(transform))
            {
                grabbedImage = nearImages[i];
                dist = newDistance;
            }
        } */
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
