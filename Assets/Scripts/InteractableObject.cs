using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InteractableObject : MonoBehaviour {


    //public bool Debug = false;
    public Transform img;           //The image this script controls
    public Transform scaleTarget;   //Top most parent object, used for scaling
    
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
    float rotation;
    
    public GameObject highlight;
    
    //Vars for handling image incorrect placement fading
    float redTime = 30;
    float destroyStart = 10;
    float destroyTime = 60;
    float counter = 0;
    Color red = new Color(.2f,0,.05f);    
    //MeshRenderer[] rends;
    List<MeshRenderer> rends;
    GameObject top;
    Dictionary<Transform, Vector3> attachedObjects;
    int attachTimer = 0;
    public ParticleSystem[] pss;
    int hoverCounter;
    bool hover = false;
    bool canHover = false;

    void Start() {        
        if (img == null) {
            img = transform.parent.transform;
        }
        Transform t = transform;
        while (t.parent != null) {
            t = t.parent;
        }
        top = t.gameObject;
        pss = top.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in pss) {
            ps.gameObject.SetActive(false);                
            //Debug.Log(ps.gameObject.activeSelf);
        }
        
    }
    
    public void FadeDestroy() {
        //Fade out and then destroy the gameObject with this script + any attached items
        
        //First find top most parent        
        
        rends = new List<MeshRenderer>();
        //Then find all MeshRenderers and setup update timer
        foreach (KeyValuePair<Transform,Vector3> pair in attachedObjects) {
            Transform t = pair.Key;
            while (t.parent != null) {
                t = t.parent;
            }
            rends.AddRange(t.GetComponentsInChildren<MeshRenderer>());
        }
        counter = 0;
    }
    
    public void UnAttachAll() {
        attachedObjects = null;
    }
    
    public void AttachObject(Transform t, Vector3 xOffset) {
        if (t == transform) { return; }
        if (attachedObjects == null) {
            attachedObjects = new Dictionary<Transform, Vector3>();
        }
        //As using transform.parent broke localScale upon unattaching, an alternative is needed
        attachedObjects[t] = -xOffset;
        attachTimer = 2;
    }
    
    void AttachedObjectStepper() {
        foreach (KeyValuePair<Transform,Vector3> pair in attachedObjects) {
            pair.Key.position = top.transform.position + pair.Value;
        }
    }
    
    public void EnableHover() {
        canHover = true;
    }
    
    void FadeDestroyStepper() {
        float t = 0;
            if (counter < redTime) {
                //Transition to red
                t = counter / (redTime+0.0f);
                foreach (MeshRenderer r in rends) {
                    foreach (Material m in r.materials) {
                        //Quadratic like this...
                        m.color = Color.Lerp(m.color, red, t);
                    }
                }
            }
            if ( counter < destroyStart || counter < destroyTime) {
                //Fade out
                t = (counter - destroyStart) / (destroyTime - destroyStart + 0.0f);
                foreach (MeshRenderer r in rends) {
                    foreach (Material m in r.materials) {
                        //Quadratic like this...                        
                        Color c = m.color;
                        c.a = Mathf.Lerp(c.a, 0, t);
                        m.color = c;
                    }
                }
            } else {
                //Destroy
                rends = null;
                if (attachedObjects != null) {
                    foreach (KeyValuePair<Transform,Vector3> pair in attachedObjects) {
                        Destroy(pair.Key.gameObject);
                    }
                }
                Destroy(top);
            }
            counter++;
    }
    
    public void Hover() {        
        if (pss == null || !canHover) { return; }        
        foreach (ParticleSystem ps in pss) {
            ps.gameObject.SetActive(true);
        }        
        hoverCounter = 5;
        hover = true;
    }
    
    void PinchParticles() {
        if (pss == null) { return; }
        foreach (ParticleSystem ps in pss) {
            ps.startColor = Color.green;
        }
    }
    
    void UnPinchParticles() {
        if (pss == null) { return; }
        foreach (ParticleSystem ps in pss) {
            ps.startColor = Color.white;
        }
    }
    
    
    
    void Update() {
        if (pss != null) {            
            if (hover) {
                if (hoverCounter == 0) {
                    foreach (ParticleSystem ps in pss) {
                        ps.gameObject.SetActive(false);                 
                    }
                    hover = false;
                } else {
                    hoverCounter--;
                }
            }
        }
        
        //Destroy object
        if (rends != null) {
            FadeDestroyStepper();
            return;
        }
        
        if (attachedObjects != null && attachedObjects.Count != 0) {
            if (attachTimer != 0) { attachTimer--; }
            else {
                AttachedObjectStepper();
            }
        }
        
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
            
            //Determine which direction to scale the object
            float x = 0, y = 0;
            x = dir.x*2;
            if (dir.x > 0) {
                if (pos.x < scaleTarget.position.x) {
                    x = -x;
                }
            } else if (dir.x < 0) {
                if (pos.x < scaleTarget.position.x) {
                    x = -x;
                }
            }
            y = dir.y*2;
            if (dir.y > 0) {
                if (pos.y < scaleTarget.position.y) {
                    y = -y;
                }
            } else if (dir.y < 0) {
                if (pos.y < scaleTarget.position.y) {
                    y = -y;
                }
            }
            
            //Append scale change
            scaleTarget.localScale += new Vector3(x, y, 0);
            drag1 = drag2 = Vector3.zero;
        }
    }
    
    public bool Pinch(Vector3 pos, int id) {        
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
        
        PinchParticles();
        //Enable highlight
        if (highlight != null) {
            if (!highlight.activeSelf) {
                highlight.SetActive(true);
            }
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
            UnPinchParticles();
            if (highlight != null) { 
                highlight.SetActive(false);
            }
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
        float zDif = (scaleTarget.position - origin).z / posMod.z;        
        posMod = posMod*zDif;
        //Debug.LogError("zDif: " + zDif + ", pos: " + pos.x + "," + pos.y + "," + pos.z + " , posMod: " + posMod.x + "," + posMod.y + "," + posMod.z);
        Vector3 startMod = dragStart - origin;
        zDif = (scaleTarget.position - origin).z / startMod.z;
        startMod = startMod*zDif;
        //Debug.LogError("zDif: " + zDif + ", dragStart: " + dragStart.x + "," + dragStart.y + "," + dragStart.z + " , startMod: " + startMod.x + "," + startMod.y + "," + startMod.z);
        Vector3 dragDistance = posMod - startMod;
        //Debug.Log("dragDistance: " + dragDistance.x + ", " + dragDistance.y + ", " + dragDistance.z);
        
        
        //Apply the z movement unscaled
        dragDistance.z = pos.z - dragStart.z;
            
        if (hands != Hands.Both) {
            //Move to new position            
            scaleTarget.position += dragDistance;
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
        float angle = 0;
        if (zAngle >= 180) { angle = 360 - zAngle; }
        else { angle = 0 - zAngle; }          
            
        //Scale rotation down and ignore if below threshold
        //Threshold depends upon hand and direction, i.e. easier to rotate hand away from body
        //Also adjusts angle by mininum, so it starts at 0
        //angle *= rotationScale;
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
        
        
        //Continue to update drag position for smoothness
        if (id == 0) { hand1 = pos; }
        if (id == 1) { hand2 = pos; }
        
        //Rotate
        //img.Rotate(new Vector3(0,0,modAngle));
        img.Rotate(new Vector3(0,0,angle));
        rotation += angle;
        if (rotation > 360) { rotation -= 360; }
        if (rotation < -360) { rotation += 360; }
        return true;
    }

   
}
