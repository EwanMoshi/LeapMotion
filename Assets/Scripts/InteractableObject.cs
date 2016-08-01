using UnityEngine;
using System.Collections;

public class InteractableObject : MonoBehaviour {


    public Transform img;
    
    enum Hands {None, Left, Right, Both};
    Hands hands;
    
    Vector3 origin = new Vector3(-50,-50,-10);
    
    Vector3 hand1;
    Vector3 hand2;
    Vector3 drag1;
    Vector3 drag2;
    //Vector3 scaleChange;
    
    void Start() {
        if (img == null) {
            img = transform.parent.transform;
        }
    }
    
    void Update() {
        //Delays scaling by 1 frame to possibly avoid conflicts between two hands?
        if (hands == Hands.Both) {
            if (drag1.x >= 0 && drag2.x >= 0) { return; }
            if (drag1.x < 0 && drag2.x < 0) { return; }
            if (drag1.y >= 0 && drag2.y >= 0) { return; }
            if (drag1.y < 0 && drag2.y < 0) { return; }
            
            float c1 = Mathf.Abs(drag1.x) + Mathf.Abs(drag1.y);
            float c2 = Mathf.Abs(drag2.x) + Mathf.Abs(drag2.y);
            
            
            Vector3 dir;
            Vector3 pos;
            
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
    }
    
    //// set which hand we pointed with
    //public void OnPoint(int id) {
    //    if (id == 0) {
    //        if (hands == Hands.None) {
    //            hands = Hands.Left;
    //        }
    //        else if (hands == Hands.Right) {
    //            hands = Hands.Both;
    //        }
    //    }
    //    else {
    //        if (hands == Hands.None) {
    //            hands = Hands.Right;
    //        }
    //        else if (hands == Hands.Left) {
    //            hands = Hands.Both;
    //        }
    //    }
    //}

    //// release the pointing hand
    //public void OnReleasePoint(int id) { 
    //    if (id == 0) { //if left hand
    //        if (hands == Hands.Left) {
    //            hands = Hands.None;
    //        }
    //        else if (hands == Hands.Both) {
    //            hands = Hands.Right;
    //        }
    //    }
    //    else { // else if it was the right hand
    //        if (hands == Hands.Right) {
    //            hands = Hands.None;
    //        }
    //        else if (hands == Hands.Both) {
    //            hands = Hands.Left;
    //        }
    //    }
    //}

    public void Drag(Vector3 pos, int id) {
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
            
            //float xMod = Mathf.Abs(dragDistance.x - transform.position.x) - Mathf.Abs(transform.localScale.x - transform.position.x);
            //float yMod = Mathf.Abs(dragDistance.y - transform.position.y) - Mathf.Abs(transform.localScale.y - transform.position.y);
            if (id == 0) {
                drag1 = dragDistance;
            } else {
                drag2 = dragDistance;
            }
            
            
            //img.localScale += new Vector3(dragDistance.x, dragDistance.y, 0);
        }
    }

    public void Rotate(Vector3 pos, int id)
    {
    }
}
