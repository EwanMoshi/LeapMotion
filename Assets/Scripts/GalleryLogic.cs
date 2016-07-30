using UnityEngine;
using System.Collections;

public class GalleryLogic : MonoBehaviour {
    
    GameObject[] selected;
    Vector3 downPos;                           //The position of a tapDown or PinchDown event
    int dragDirection;
    float dragDistance;
    
    enum State {None, Pinched, Tapped};
    State handState = State.None;
    
    
    
    public void TapDown(Vector3 pos) {
        if (handState != State.None) { return; }
        
        
        
        downPos = pos;
        handState = State.Tapped;        
    }
    
    public void TapDrag(Vector3 pos) {
        if (handState != State.Tapped) { return; }
        
    }
    
    public void TapUp(Vector3 pos) {
        if (handState != State.Tapped) { return; }
        
        handState = State.None;
    }
    
    
    public void PinchDown(Vector3 pos) {
        if (handState != State.None) { return; }
        
        
        handState = State.Pinched;
    }
    
    public void PinchDrag(Vector3 pos) {
        if (handState != State.Pinched) { return; }
        
        //Watch for direction change
        
        
    }
    
    public void PinchUp(Vector3 pos) {
        if (handState != State.Pinched) { return; }
        
        
        
        handState = State.None;
    }
    
    void SelectAtPosition(Vector3 pos) {
        
        
    }
    
    void SelectInArea(Vector3 pos) {
        //From pinchDownPos to pos
        
        
    }
    
    void DuplicateSelection() {
        
    }
    
    void ReleaseSelection() {
                
    }
   
    
    
    
}
