using UnityEngine;
using System.Collections;

namespace Leap.Unity {

  /// <summary>
  /// A basic utility class to aid in creating pinch based actions.  Once linked with an IHandModel, it can
  /// be used to detect pinch gestures that the hand makes.
  /// </summary>
  public class TapDetector : Detector {
    public SelectImage[] selectImage;
    
    
    public int zHistoryFrames = 10;         //History of z location over this many frames
    int minFrames = 5;                      //The minimum frames required to check for a tap
    float tapDistance = 0.06f;              //The distance required to trigger a tap
    float tapUpDistance = -1;
    float errorDistance = 0.11f;            //Prevents bugged z movement from triggering a tap (may not be needed)
    Vector3 tapPosition;
    bool tap = false;                       //State of tap
    float tapDown;                          //The z location of the current or last tap
    Queue zQueue;                           //Stores the z locations
    
      
    protected const float MM_TO_M = 0.001f;

    [SerializeField]
    protected IHandModel _handModel;

    protected int _lastUpdateFrame = -1;
    
    protected bool _didChange = false;

    protected virtual void OnValidate() {
      if (_handModel == null) {
        _handModel = GetComponentInParent<IHandModel>();
      }
    }
    
    
    

    protected virtual void Awake() {
      if (GetComponent<IHandModel>() != null) {
        Debug.LogWarning("LeapPinchDetector should not be attached to the IHandModel's transform. It should be attached to its own transform.");
      }
      if (_handModel == null) {
        Debug.LogWarning("The HandModel field of LeapPinchDetector was unassigned and the detector has been disabled.");
        enabled = false;
      }
      if (selectImage == null || selectImage.Length == 0) {
        selectImage = GameObject.FindObjectsOfType<SelectImage>();
      }
      if (zQueue == null) {
          zQueue = new Queue(zHistoryFrames);
      }
      if (tapUpDistance == -1) {
          tapUpDistance = tapDistance / 2.0f;
      }
      
      
    }

    protected virtual void Update() {
      //Append new z position
      if (zQueue.Count > zHistoryFrames) {
          zQueue.Dequeue();          
      }
      zQueue.Enqueue(transform.position.z);
      
      //Check for tap
      if (zQueue.Count < minFrames) { return; } 
        TapTracking();
    }

    
    void TapDownEvent() {
        //Respond to Tap Down
        Debug.LogWarning("TapDown");
    }
    
    void TapUpEvent() {
        //...
        Debug.LogWarning("TapUp");
        for (int i = 0; i < selectImage.Length; i++) {
              //selectImage[i].Pinch(transform.position);
              selectImage[i].Pinch(tapPosition);
        }
    }


    protected virtual void TapTracking() {
      if (Time.frameCount == _lastUpdateFrame) {
        return;
      }
      _lastUpdateFrame = Time.frameCount;

      _didChange = false;

      Hand hand = _handModel.GetLeapHand();

      if (hand == null || !_handModel.IsTracked) {        
        return;
      }

      
      //transform.rotation = hand.Basis.CalculateRotation();

      //Get finger position
      var fingers = hand.Fingers;
      transform.position = Vector3.zero;
      for (int i = 0; i < fingers.Count; i++) {
        Finger finger = fingers[i];
        if (finger.Type == Finger.FingerType.TYPE_INDEX) {
            //finger.Type == Finger.FingerType.TYPE_THUMB) {
          transform.position += finger.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint.ToVector3();
        }
      }
      
      
      //Tap events are found by checking the distance travelled by the hand
      //over a number of frames. If the distance passes the threshold a tap is triggered.
      //This works both ways, but a tap up event can only occur after a tap down event.
      
      //Look for forwards Tap
      bool start = true;
      float z1 = (float)zQueue.Peek();
      float z2 = z1;      
      float dist;
      if (!tap) {
        foreach (float i in zQueue) {
            if (start) { start = false; continue; }          
            //Reset Check
            dist = i - z1;
            //if (i < z1 - zThresh) {
            //dist = z2-z1;
            
            if (dist >= tapDistance && dist < errorDistance) {
                Debug.Log("z1: " + z1 + ", z2: " + z2);
                //Tap achieved
                tap = true;
                tapDown = i;
                zQueue.Clear();
                TapDownEvent();
                tapPosition = transform.position;
                return;
            }
        }
      } else {
        //Look for backwards tap
        
        z1 = tapDown;         
        foreach (float i in zQueue) {
            dist = z1-i;
            if (dist > tapUpDistance && dist < errorDistance) {
                tap = false;
                zQueue.Clear();
                TapUpEvent();                    
                return;
            }
        }
      }
      
    }
  }
}
