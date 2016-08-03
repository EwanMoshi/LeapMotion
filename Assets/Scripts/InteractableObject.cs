using UnityEngine;
using System.Collections;

public class InteractableObject : MonoBehaviour {


    //public bool Debug = false;
    public Transform img;           //The image this script controls
    
    enum Hands {None, Left, Right, Both};
    Hands hands;                    //Moving + Scaling
    Hands pointingHands;            //Rotation

    //Offset for camera
    Vector3 origin = new Vector3(-50,-50,-10);
    
    //Used for storing locations and applying translations/scalings/rotations
    Vector3 hand1;
    Vector3 hand2;
    Vector3 drag1;
    Vector3 drag2;
    Vector3 point;
    
    float circleRadius = 4;             //Max Radius before ignoring rotation
    float circleRadiusMin = 0.2f;   
    float distanceThresh = 0.2f;    //Acceptable distance from circle edge to still rotate
    float awayFramesLimit = 5;      //After this many frames reset circle radius
    float awayFrames;
    Queue angleHistory;
    int historyLimit = 10;
    int historyMin;
    
    //# Currently obsolete
    float angleScale = 0.5f;        //How much rotation in degrees one unit of x movement equals
    float rotation;                 //Current rotation    
    float rotationGrid = 15;        //Values to snap too - 0, rotationGrid, rotG*2, etc
    float rotationSnap = 3;         //How close until a snap occurs <=
    
    


    void Start() {        
        if (img == null) {
            img = transform.parent.transform;
        }   
        historyMin = historyLimit / 2;
        angleHistory = new Queue();
    }
    
    void Update() {
        //Delays scaling by 1 frame to possibly avoid conflicts between two hands?
        if (hands != Hands.None) { Debug.Log("Hands: " + hands); }
        
        if (hands == Hands.Both) {
            //Attempt 2 at restricting scaling gestures:
            if (drag1.x >= 0 && drag2.x >= 0 || drag1.x < 0 && drag2.x < 0) { drag1.x = drag2.x = 0; }
            if (drag1.y >= 0 && drag2.y >= 0 || drag1.y < 0 && drag2.y < 0) { drag1.y = drag2.y = 0; }
            if (drag1.x == 0 && drag2.x == 0 && drag1.y == 0 && drag2.y == 0) { return; }
            
            float c1 = Mathf.Abs(drag1.x) + Mathf.Abs(drag1.y);
            float c2 = Mathf.Abs(drag2.x) + Mathf.Abs(drag2.y);
            Vector3 dir, pos;
            
            //Rather than have the hands fight over who is scaling the object,
            //the hand with the largest distance travelled scales the object
            if (c1 > c2) {
                //Scale using Hand 1 pos
                dir = drag1;
                pos = hand1;
            } else {
                //Hand 2
                dir = drag2;
                pos = hand2;
            }
            
            float x = 0, y = 0;
            
            //Determine which direction to scale the object
            x = dir.x*2;
            if (dir.x > 0) {
                if (pos.x < img.position.x) {
                    x = -x;
                }
            } else if (dir.x < 0) {
                if (pos.x < img.position.x) {
                    x = -x;
                }
            }
            y = dir.y*2;
            if (dir.y > 0) {
                if (pos.y < img.position.y) {
                    y = -y;
                }
            } else if (dir.y < 0) {
                if (pos.y < img.position.y) {
                    y = -y;
                }
            }
            
            //Debug.Log("dir.x: " + dir.x + ", pos.x: " + pos.x + ", x: " + x);
            //Append scale change
            img.localScale += new Vector3(x, y, 0);
            drag1 = drag2 = Vector3.zero;
        }
    }
    
    public bool Pinch(Vector3 pos, int id) {
        //Debug.Log("Pinch: " + id);
        if (pointingHands != Hands.None) { return false; }
        //Setup pinch original location
        if (id == 0) {
            if (hands == Hands.None) { hands = Hands.Left; }
            else if (hands == Hands.Right) { hands = Hands.Both; }
            hand1 = pos;
        } else {
            if (hands == Hands.None) { hands = Hands.Right; }
            else if (hands == Hands.Left) { hands = Hands.Both; }
            hand2 = pos;
        }
        return true;
    }
    
    
    public void ReleasePinch(int id) {
        Debug.Log("ReleasePinch: " + id);
        if (id == 0) {        
            if (hands == Hands.Left) { hands = Hands.None; }
            else if (hands == Hands.Both) { hands = Hands.Right; }            
        } else {
            if (hands == Hands.Right) { hands = Hands.None; }
            else if (hands == Hands.Both) { hands = Hands.Left; }            
        }               
    }
    
    public void ReleasePoint(int id) {
        //Debug.Log("ReleasePoint: " + id);
        if (id == 0) { //if left hand
            if (pointingHands == Hands.Left) { pointingHands = Hands.None; }
            else if (pointingHands == Hands.Both) { pointingHands = Hands.Right; }
        } else { // else if it was the right hand
            if (pointingHands == Hands.Right) { pointingHands = Hands.None; }
            else if (pointingHands == Hands.Both) { pointingHands = Hands.Left; }
        }
        awayFrames = 0;
    }
    

    // Set which hand we pointed with and the start location
    public bool Point(Vector3 pos, int id) {  
        //Debug.Log("Point: " + id);
        if (hands != Hands.None) {
            Debug.LogWarning("Tried to point with hand: " + id + ", but Hands are: " + hands);
            return false;
        }
        pointingHands = (Hands) id;
        point = pos;
        /* Vector3 circle = transform.position - origin;
        Vector3 posMod = pos - origin;
        float zDif = (img.position - origin).z / posMod.z;        
        posMod = posMod*zDif;
        posMod.z = circle.z;        
        circleRadius = Vector3.Distance(posMod, circle);
        if (circleRadius < circleRadiusMin) { 
            Debug.LogWarning("Rotation circle is too small (hand: " + id + "), r: " + circleRadius); 
            return false;
        } */
        return true;
    }
    

    public void Drag(Vector3 pos, int id) {
        if (pointingHands != Hands.None) { return; }
        //Debug.Log("Drag: " + id);
        //if (pointingHands != Hands.None) { return; } //Shouldn't be needed
        //Debug.Log("Hands: " + hands);
        Vector3 dragStart;
        if (id == 0) {
            if (hands == Hands.None || hands == Hands.Right) { return; }
            dragStart = hand1;
            hand1 = pos;
        } else {
            if (hands == Hands.None || hands == Hands.Left) { return; }
            dragStart = hand2;
            hand2 = pos;
        }
        if (pos == dragStart) { return; }

        //Project both positions onto the same xy axis at the z location of the image        
        Vector3 posMod = pos - origin;
        float zDif = (img.position - origin).z / posMod.z;        
        posMod = posMod*zDif;
        //Debug.LogError("zDif: " + zDif + ", pos: " + pos.x + "," + pos.y + "," + pos.z + " , posMod: " + posMod.x + "," + posMod.y + "," + posMod.z);
        Vector3 startMod = dragStart - origin;
        zDif = (img.position - origin).z / startMod.z;
        startMod = startMod*zDif;
        //Debug.LogError("zDif: " + zDif + ", dragStart: " + dragStart.x + "," + dragStart.y + "," + dragStart.z + " , startMod: " + startMod.x + "," + startMod.y + "," + startMod.z);
        Vector3 dragDistance = posMod - startMod;
        
        
        //Apply the z movement unscaled
        dragDistance.z = pos.z - dragStart.z;
            
        if (hands != Hands.Both) {
            //Move to new position            
            img.position += dragDistance;
        } else {
            //Scale (on next update)
            if (id == 0) {
                drag1 = dragDistance;
            } else {
                drag2 = dragDistance;
            }
        }
    }


    public void Rotate(Vector3 pos, int id) {
        if (hands != Hands.None) { return; }
        if (id != (int)pointingHands) { return; }
        //Rotate object around its center, depending on index finger movement
        //Debug.LogError("Rotate: " + id + ", Hands: " + hands);    
        
        Vector3 circle = transform.position - origin;
        
        //Use This.point and pos, to calculate rotation        
        Vector3 posMod = pos - origin;
        float zDif = (img.position - origin).z / posMod.z;        
        posMod = posMod*zDif;
        Vector3 startMod = point - origin;
        zDif = (img.position - origin).z / startMod.z;
        startMod = startMod*zDif;
        
        //Ignore z value
        posMod.z = startMod.z = circle.z;
        
        //Check both points are within range of circle edge
        float distP = Vector3.Distance(posMod,circle);
        float distS = Vector3.Distance(startMod,circle);
        
        if (Mathf.Max(distP, distS) > circleRadius) {
            return;
        }
        
        //if ((distP < circleRadius - distanceThresh || distP > circleRadius + distanceThresh) ||
        //     (distS < circleRadius - distanceThresh || distS > circleRadius + distanceThresh)) {
        //    awayFrames++;
            //Debug.LogWarning("Point not close enough to circle: distP: " + distP + ", distS: " + distS + ", radius: " + circleRadius);
        //    return;
        //}
        
        
        //Calculate rotation amount
        float angle = Vector3.Angle(posMod, startMod);
        //Debug.Log("distP: " + distP + ", distS: " + distS);
        Debug.Log("posMod: " + posMod + ", startMod: " + startMod);
        Debug.Log("Rotating by: " + angle);
        
        
        
        
        //Not quite good enough for direction
        if (posMod.x < startMod.x && posMod.y < 0) {
            angle = -angle;
        } else if (posMod.x >= startMod.x && posMod.y >= 0) { 
            angle = -angle;
        }
            
        img.Rotate(new Vector3(0, 0, angle));
        point = pos;
    }
}
