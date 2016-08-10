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
    
    public float rotationMin = 2;    
    public float rotationScale = 0.2f;   
    
    
    GameObject highlight;
    
    

    void Start() {        
        if (img == null) {
            img = transform.parent.transform;
        }
        if (highlight == null) {
            highlight = transform.Find("Highlight").gameObject;
        }
    }
    
    
    void Update() {
        //Scaling is done here.
        if (hands == Hands.Both) {
            //Drag motion is restricted to only trigger when both hands are moving in opposing directions
            //on at least one of the x and y axes.
            if (drag1.x >= 0 && drag2.x >= 0 || drag1.x < 0 && drag2.x < 0) { drag1.x = drag2.x = 0; }
            if (drag1.y >= 0 && drag2.y >= 0 || drag1.y < 0 && drag2.y < 0) { drag1.y = drag2.y = 0; }
            if (drag1.x == 0 && drag2.x == 0 && drag1.y == 0 && drag2.y == 0) { return; }
            
            //Rather than have the hands fight over who is scaling the object,
            //the hand with the largest distance travelled scales the object
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
            //dir = img.InverseTransformDirection(dir);
            //pos = img.InverseTransformDirection(pos);
            
            //Determine which direction to scale the object
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
            //Append scale change
            img.localScale += new Vector3(x, y, 0);
            drag1 = drag2 = Vector3.zero;
        }
    }
    
    public bool Pinch(Vector3 pos, int id) {
        //Debug.Log("Pinch: " + id);
        //if (pointingHands != Hands.None) { return false; }
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
        //Enable highlight
        if (!highlight.activeSelf) {
            highlight.SetActive(true);
        }
        
        //Always true... yay?
        return true;
    }
    
    
    public void UnPinch(int id) {
        //Debug.Log("UnPinch: " + id);
        if (id == 0) {        
            if (hands == Hands.Left) { hands = Hands.None; }
            else if (hands == Hands.Both) { hands = Hands.Right; }            
        } else {
            if (hands == Hands.Right) { hands = Hands.None; }
            else if (hands == Hands.Both) { hands = Hands.Left; }            
        } 
        if (hands == Hands.None) {
            highlight.SetActive(false);
        }
        
    }
     
    
    public void Drag(Vector3 pos, int id) {        
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

    public bool Rotate(Vector3 pos, float zAngle, int id) {
        if (id == 0 && hands != Hands.Left || id == 1 && hands != Hands.Right) { return false; }
        
        //Assuming the original rotation to be 0 (or 360 if required) seems to work well            
        float angle = 0, min = 0;
        if (zAngle >= 180) { angle = 360 - zAngle; }
        else { angle = 0 - zAngle; }          
            
        //Scale rotation down and ignore if below threshold
        //Threshold depends upon hand and direction, i.e. easier to rotate hand away from body
        //Also adjusts angle by mininum, so it starts at 0
        angle *= rotationScale;
        if (id == 0) {
            angle -= 2;
        } else {
            angle += 2;
        }
        if (Mathf.Abs(angle) < rotationMin) {
            return false;
        }
        
        //Ensure first acceptable rotation remains slow
        if (angle < 0) { angle += rotationMin; }
        else { angle -= rotationMin; }
        
        
        //Continue to update drag position for smoothness
        if (id == 0) { hand1 = pos; }
        if (id == 1) { hand2 = pos; }
        
        //Debug.Log("ModAngel: " + modAngle + ", Angle: " + angle);
        Debug.Log("zAngle: " + zAngle + ", Angle: " + angle);
        
        //Rotate
        //img.Rotate(new Vector3(0,0,modAngle));
        img.Rotate(new Vector3(0,0,angle));
        return true;
    }

   
}
