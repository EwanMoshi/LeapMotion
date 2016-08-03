using UnityEngine;
using System.Collections;

public class InteractableObject : MonoBehaviour {

    public Transform img;
    
    enum Hands {None, Left, Right, Both};
    Hands hands;
    Hands pointingHands;
    float angleScale = 0.5f; //How much rotation in degrees one unit of x movement equals



    Vector3 origin = new Vector3(-50,-50,-10);
    
    Vector3 hand1;
    Vector3 hand2;
    Vector3 drag1;
    Vector3 drag2;
    Vector3 point;
    float rotation;
    float rotationGrid = 15;        //Values to snap too - 0, rotationGrid, rotG*2, etc
    float rotationSnap = 3;         //How close until a snap occurs <=
    //Vector3 scaleChange;


    void Start() {        
        if (img == null) {
            img = transform.parent.transform;
        }
    }
    
    void Update() {
        //Delays scaling by 1 frame to possibly avoid conflicts between two hands?
        if (hands == Hands.Both) {
            //Attempt 2 at restricting scaling gestures:
            if (drag1.x >= 0 && drag2.x >= 0 || drag1.x < 0 && drag2.x < 0) { drag1.x = drag2.x = 0; }
            if (drag1.y >= 0 && drag2.y >= 0 || drag1.y < 0 && drag2.y < 0) { drag1.y = drag2.y = 0; }            
            if (drag1.x == 0 && drag2.x == 0 && drag1.y == 0 && drag2.y == 0) { return; }
            
            float c1 = Mathf.Abs(drag1.x) + Mathf.Abs(drag1.y);
            float c2 = Mathf.Abs(drag2.x) + Mathf.Abs(drag2.y);
            Vector3 dir, pos;            
            
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
            
            img.localScale += new Vector3(x, y, 0);
            drag1 = drag2 = Vector3.zero;
        }
    }
    
    public void Pinch(Vector3 pos, int id) {
        //Start dragging - Calculate grabbed location offset
        if (id == 0) {
            if (hands == Hands.None) { hands = Hands.Left; }
            else if (hands == Hands.Right) { hands = Hands.Both; }
            hand1 = pos;
        } else {
            if (hands == Hands.None) { hands = Hands.Right; }
            else if (hands == Hands.Left) { hands = Hands.Both; }
            hand2 = pos;
        }        
    }
    
    
    public void ReleasePinch(int id) {
        if (id == 0) {
            if (hands == Hands.Left) { hands = Hands.None; }
            else if (hands == Hands.Both) { hands = Hands.Right; }            
        } else {
            if (hands == Hands.Right) { hands = Hands.None; }
            else if (hands == Hands.Both) { hands = Hands.Left; }            
        }
        pointingHands = Hands.None;        
    }

    // set which hand we pointed with
    public void OnPoint(Vector3 pos, int id)
    {        
        if (hands == Hands.Left && id == 1) {
            pointingHands = Hands.Right;
        } else if (hands == Hands.Right && id == 0) {
            pointingHands = Hands.Left;
        }
        point = pos;
        
        /* if (hands == Hands.Right && id == 1) { //if we're pinching with right hand and the point is with right hand
            return;                           // do nothing
        } else if (hands == Hands.Left && id == 0) { //if we're pinching with left hand and the point is with left hand
            return;                                // do nothing
        }

        if (id == 0) { //if pointing with left hand
            if (pointingHands == Hands.pointNone) {
                pointingHands = Hands.pointLeft;
            }
            else if (pointingHands == Hands.pointRight) {
                pointingHands = Hands.pointBoth;
            }
        }
        else {
            if (pointingHands == Hands.pointNone) {
                pointingHands = Hands.pointRight;
            }
            else if (pointingHands == Hands.pointLeft) {
                pointingHands = Hands.pointBoth;
            }
        } */

    }

    /* // release the pointing hand
    public void OnReleasePoint(int id) {
        //Debug.Log("RELEASE POINT >>>>> " + id);
        if (id == 0) { //if left hand
            if (pointingHands == Hands.Left) {
                pointingHands = Hands.None;
            }
            else if (pointingHands == Hands.Both) {
                pointingHands = Hands.Right;
            }
        }
        else { // else if it was the right hand
            if (pointingHands == Hands.Right) {
                pointingHands = Hands.None;
            }
            else if (pointingHands == Hands.Both) {
                pointingHands = Hands.Left;
            }
        }
    } */

    public void Drag(Vector3 pos, int id) {
        if (pointingHands != Hands.None) { return; }
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


    public void Rotate(Vector3 pos, int id)
    {
        
        Debug.LogError("Rotate: " + id + ", Hands: " + hands);
        Vector3 pos1, pos2; //Improve genericity between hand instance
        //Original seemed to be checking wrong hand
        //If hand 0 is pointing, hand 1 must be grabbing
        if (id == 0) {
            if (hands != Hands.Right) { return; }
            pos1 = hand1;            
        } else {
            if (hands != Hands.Left) { return; }
            pos1 = hand2;            
        }        
        pos2 = pos;
        
        if (pos1 == pos2) {         
            return;
        }
        
        //Super rotate hack :- rotation is controlled x axis movement only via a slider
        //The cheap and easy way out, hooray.        
        float xd = pos2.x - pos1.x;
        if (xd == 0) { return; }       
        
        transform.Rotate(new Vector3(xd * angleScale, 0, 0));
        rotation += xd * angleScale;        
    }
}
