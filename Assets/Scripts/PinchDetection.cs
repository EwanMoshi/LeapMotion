using UnityEngine;
using System.Collections;
using Leap.Unity;

public class PinchDetection : MonoBehaviour {

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

    private bool isPinching = false;
    private bool togglePinch = false;

    protected Collider grabbedImage;
    protected RigidbodyConstraints previousConstraints;

    // Use this for initialization
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

        //Debug.Log(distance + "  >>>>>>>   DISTANCE");
        //Debug.Log(normalizedDistance + "  >>>>>>>   normDISTANCE");
        //Debug.Log(">>>>>>>  "+handModel.GetLeapHand().PinchStrength);

        togglePinch = false;

        // check if the user has successfully pinched an image and toggle pinching if they have
        if (handModel.GetLeapHand().PinchStrength > pinchStart) {
            togglePinch = true;
        }

        // store position of the pinch (at thumb)
        Vector3 pinchPos = handModel.fingers[0].GetTipPosition(); 

        if (togglePinch && !isPinching) {
            OnPinch(pinchPos);
        }
        else if (!togglePinch && isPinching) {
            OnReleasePinch();
        }

        if (grabbedImage != null) {
            //Physics.IgnoreCollision(handModel.GetComponent<Collider>(), grabbedImage.GetComponent<Collider>(), false);
            //Debug.Log("Something has been grabbed");
            FreePositionFreezeRotation();
            Vector3 moveDistance = pinchPos - grabbedImage.transform.position;
            //Debug.Log("moveDistance >>>>  "+moveDistance);
            //Debug.Log("force >>>>> " + force * moveDistance);
            grabbedImage.GetComponent<Rigidbody>().AddForce(force * moveDistance);
        }
       
        //if(grabbedImage != null) {
           //Debug.Log(grabbedImage.ToString());
           //Debug.Log("grabbed");
        //}
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
            //grabbedImage.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll; // freeze all (position and rotation)   
            grabbedImage.GetComponent<Rigidbody>().constraints = previousConstraints; //we can get rid of this if we don't want other things in the world that use gravity
        }
        //Physics.IgnoreCollision(handModel.GetComponent<Collider>(), grabbedImage.GetComponent<Collider>(), false);
        grabbedImage = null;
        isPinching = false;
        //Debug.Log("ReleasePinch");
    }

    void OnPinch(Vector3 pinchPos) {
        isPinching = true;
        //Debug.Log("OnPinch");
        Collider[] nearImages = Physics.OverlapSphere(pinchPos, magnetDistance);
        Vector3 dist = new Vector3(magnetDistance, 0.0f, 0.0f);

        for(int i = 0; i < nearImages.Length; i++) {
            Vector3 newDistance = pinchPos - nearImages[i].transform.position;
            if(nearImages[i].GetComponent<Rigidbody>() != null && newDistance.magnitude < dist.magnitude && !nearImages[i].transform.IsChildOf(transform)) {
                //Debug.Log("NEAR IMAGE >>>> " +nearImages[i].ToString());
                //Debug.Log("pinchPos >>>> " + pinchPos);
                Debug.Log("ImagePos >>>> " + nearImages[i].gameObject.name);
                grabbedImage = nearImages[i];
                dist = newDistance;
                previousConstraints = grabbedImage.GetComponent<Rigidbody>().constraints; // only use this for things like cubes in the world otherwise just RigidbodyConstraints.FreezeAll;
            }
        }
    }
}
