using UnityEngine;
using System.Collections;

public class InteractableObject : MonoBehaviour {

	public enum ObjectType {Corner, Edge, Center};
    public ObjectType objectType;
    
    public Transform img;
    
    Vector3 origin = new Vector3(0,0,-10);
    
    
    
    Vector3 dragStart;        
    
    void Start() {
        if (img == null) {
            img = transform.parent.transform;
        }
    }
    
    public void Pinch(Vector3 pos) {
        if (objectType == ObjectType.Center) {
            //Start dragging - Calculate grabbed location offset
            dragStart = pos;
            
            
            //dragOffset = pos - img.position;
            //dragOffset.z = transform.position.z;
            //dragStart = pos;
            
        }
    }
    
    public void Drag(Vector3 pos) {
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
        
        dragStart = pos;
        
        if (objectType == ObjectType.Center) {
            //Move to new position            
            img.position += dragDistance;
        }
        
        if (objectType != ObjectType.Center) {
            //Vector3 expandDir = transform.position - img.position;
            //expandDir.z = 0;
            //expandDir.Normalize();            
            //img.localScale += new Vector3(expandDir.x * ,expandDir.y,0);
            //img.position += new Vector3(expandDir.x*.5f,expandDir.y*.5f,0);
            if (name.Equals("EdgeL")) {

            } else if(name.Equals("EdgeR")) {
                img.localScale += new Vector3(-dragDistance.x, 0, 0);
            } else if (name.Equals("EdgeT")) {
                
            } else if (name.Equals("EdgeB")) {
                img.localScale += new Vector3(0, dragDistance.y, 0);
            } else if (objectType == ObjectType.Corner) {
                img.localScale += new Vector3(-dragDistance.x, dragDistance.y, 0);
            }  
        }
            
            
            //float change = 0;
            //One directional resize
            //Center moves by half the size change
            //if (name.equals("EdgeL")) {
                
                
            //}
            
        
    }
    
    
    
    
    
}
