﻿using UnityEngine;
using System.Collections;
using Leap.Unity;

public class PinchDetectionL : MonoBehaviour {

    [SerializeField]
    private float maxPinchDistance;
    [SerializeField]
    private float minPinchDistance;
    [SerializeField]
    private float pinchStart;

    private HandModel handModel;
    private HandDrop hand;

    public float force = 50.0f;
    public float magnetDistance = 0.05f;

    public static bool isPinchingL = false;
    public static Vector3 pinchPosL;
    public static Vector3 previousPinchPosL = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 prevPinchDistance;

    private bool togglePinch = false;
    protected Collider grabbedImage;

    /* // Use this for initialization
    void Start () {
        handModel = transform.GetComponent<HandModel>(); //transform.GetComponent<HandModel>();
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 thumbPos = handModel.fingers[0].GetBoneCenter(3);
        Vector3 indexPos = handModel.fingers[1].GetBoneCenter(3);

        float distance = (indexPos - thumbPos).magnitude;
        float normalizedDistance = (distance - minPinchDistance) / (maxPinchDistance - minPinchDistance);
        float pinch = 1.0f - Mathf.Clamp01(normalizedDistance);


        togglePinch = false;

        // check if the user has successfully pinched an image and toggle pinching if they have
        if (handModel.GetLeapHand().PinchStrength > pinchStart) {
            togglePinch = true;
        }

        // store position of the pinch (at thumb)
        pinchPosL = handModel.fingers[0].GetTipPosition(); 

        if (togglePinch && !isPinchingL) {
            OnPinch(pinchPosL);
        }
        else if (!togglePinch && isPinchingL) {
            OnReleasePinch();
        }

        if (grabbedImage != null) {
            if (PinchDetectionR.isPinchingR) { // if right hand is also pinching, then we need to scale the image
                grabbedImage.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll; // freeze all (position and rotation)   
                Vector3 pinchDistance = pinchPosL - PinchDetectionR.pinchPosR;

                pinchDistance = new Vector3(pinchDistance.x, pinchDistance.y, pinchDistance.z) * 0.03f;

                if (pinchDistance.magnitude < prevPinchDistance.magnitude)
                {
                    grabbedImage.GetComponent<Rigidbody>().transform.localScale -= pinchDistance;
                }
                else
                {
                    grabbedImage.GetComponent<Rigidbody>().transform.localScale += pinchDistance;
                }

                prevPinchDistance = pinchDistance;
            }
            else { // if just this hand (L) is pinching, we only move image around

                FreePositionFreezeRotation();
                Vector3 moveDistance = pinchPosL - grabbedImage.transform.position;

                grabbedImage.GetComponent<Rigidbody>().AddForce(force * moveDistance);
            }
        }
    }

    // This function frees the constraints so the image can be moved around and rotated
    // but then freeze rotation so we don't rotate it while moving
    // RigibodyConstraints API has no support for only freeing the position so we have 
    // free all, then freeze rotation
    void FreePositionFreezeRotation() {
        grabbedImage.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        grabbedImage.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
    }

    void OnReleasePinch() {
        if (grabbedImage != null) {
            grabbedImage.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll; // freeze all (position and rotation)   
        }
        //Physics.IgnoreCollision(handModel.GetComponent<Collider>(), grabbedImage.GetComponent<Collider>(), false);
        grabbedImage = null;
        isPinchingL = false;
        //Debug.Log("ReleasePinch");
    }

    void OnPinch(Vector3 pinchPosL) {
        isPinchingL = true;
        //Debug.Log("OnPinch");
        Collider[] nearImages = Physics.OverlapSphere(pinchPosL, magnetDistance);
        Vector3 dist = new Vector3(magnetDistance, 0.0f, 0.0f);

        for(int i = 0; i < nearImages.Length; i++) {
            Vector3 newDistance = pinchPosL - nearImages[i].transform.position;
            if(nearImages[i].GetComponent<Rigidbody>() != null && newDistance.magnitude < dist.magnitude && !nearImages[i].transform.IsChildOf(transform)) {
                grabbedImage = nearImages[i];
                dist = newDistance;
            }
        }
    } */

}
