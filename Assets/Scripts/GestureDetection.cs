using UnityEngine;
using System.Collections;
using Leap.Unity;
using System;
using UnityEngine.UI;


public class GestureDetection : MonoBehaviour
{
    public PinchDetector pincher;
    public ExtendedFingerDetector efd;
    public enum Handedness {Left, Right};
    public Handedness handedness;
    public HandModel handModel;
    public GameObject imagePanelPrefab;
    public GestureDetection otherHand;
    public GameObject rotateDisplay;
    public Transform rotatePointer;

    public bool inGallery = false;

    bool isPinching = false;
    bool selectionFromGallery = false;

    //Handles difference in location between hands and images
    Vector3 cameraOffset;
    Vector3 origin;

    int layerMask = (1 << 9);                           //Used for raycasting between hands and images
    int id = 0;                                         //0 is Left, 1 is Right
    float pinchDepth = 50;                              //How far to raycast in +z direction (50 ~= inf)
    float spawnOffsetZ = .05f;                          //Image spawn offset in z axis, relative to pinch
    float spawnSize = .75f;                             //Size of spawned images

    //Pinch delay variables
    bool pinchTimer = false;
    int pinchFrames = 30;
    int pinchCounter = 0;
    Leap.Hand hand;

    MeshRenderer[] panelRends;                          //Used to fade panels
    Transform parentPanel;                              //Deprecated
    InteractableObject parentImage;                     //Controls images dragged from UI
    InteractableObject targetImage;                     //The image the hand is currently interacting with
    public SelectImage[] selectImage;                   //The images in the gallery
    public ArrayList selectedImages;                    //The current gallery selection
    public GameObject[] imagePanels;                    //Images dragged into main area

    float curAngle;                                     //Used by rotation UI display
    bool hover = false;                                 //Helps prevent hover particles showing up too soon


    // Variables for load image gesture
    float tempHandPos = 0.0f;                           //Position of hand when palmUp triggered
    bool isPalmUp = false;
    ToggleGallery tg;

    void Start() {
        origin = GameObject.Find("Main Camera").transform.position;
        cameraOffset = GameObject.Find("DrawingCamera").transform.position - origin;
        id = (int)handedness;
        origin += cameraOffset;

        //Find all UI images tiles
        if (selectImage == null || selectImage.Length == 0) {
            GameObject g = GameObject.Find("SlidePanel");
            selectImage = new SelectImage[g.transform.childCount];
            for (int i = 0; i < selectImage.Length; i++) {
                //In order list
                selectImage[i] = g.transform.GetChild(i).GetComponent<SelectImage>();
            }

            //Might no longer be required...
            selectedImages = new ArrayList();
        }

        // repeatedly call CheckLoadImage function every 0.5 seconds
        InvokeRepeating("CheckLoadImage", 1f, 0.5f);
        tg = GameObject.FindObjectOfType<ToggleGallery>();
    }

    void Update() {
        if (hand == null && handModel.GetLeapHand()!=null) {
            hand = handModel.GetLeapHand();
        }

        // if palm normal is > 0.1 then palm is facing up
        // only check if palm is up if we're not pinching, this is to avoid clashes
        // with rotation when the palm might be facing up
        if (!isPinching && handModel.GetPalmNormal().y >= 0.1) {
            if (!isPalmUp) { // only store the position the first time the palm faces up
                tempHandPos = handModel.fingers[0].GetTipPosition().y + cameraOffset.y; // store current hand position temporarily
                isPalmUp = true;
            }
        }
        else {
            isPalmUp = false;
            //imageLoaded = false; // reset image loaded (used for when an image is loaded)
        }

        //Rotate UI
        RotateCheck();

        //Hover particles
        Vector3 pos = handModel.fingers[0].GetTipPosition();
        if (hand != null && hover && !inGallery) {
            HoverCheck(pos + cameraOffset);
        }

        //Delayed pinch
        if (pinchTimer) {
            if (pinchCounter == pinchFrames) {
                //Debug.Log("Update Pincher");
                PinchGalleryArea();
                pinchCounter = 0;
                pinchTimer = false;
            } else {
                pinchCounter++;
            }
        }

        //Handle image manipulation
        if (isPinching) {
            if (!inGallery && !selectionFromGallery) {
                    if (targetImage == null) { return; }
                    if (targetImage.Rotate(pos + cameraOffset, pincher.Rotation.eulerAngles.z, id)) { return; }
                    targetImage.Drag(pos+cameraOffset, id);
            } else {
                //Manipulate thumbnails in gallery
                if (parentPanel != null) {
                    parentPanel.transform.position = pos + new Vector3(0,0,spawnOffsetZ);
                }
            }
        }
    }

    //Called by the GalleryTransition script collider
    public void GalleryTrigger(bool on) {
        if (!inGallery && on) {
            inGallery = true;
            UnPinchDrawingArea();
        }
        if (inGallery && !on) {
            inGallery = false;
            if (!otherHand.inGallery) {
                DeSelectGallery();
            }
        }
    }
    
    void DeSelectGallery() {
        foreach (SelectImage s in selectImage) {
            s.DeSelect();
        }
    }

    //Called by PinchDetector LeapMotion script
    public void PinchGate(bool on) {        
        //Decide which pinch action to take - Gallery or Main Area, On or Off
        if (!inGallery) {
            if (on) {
                PinchDrawingArea();
            } else {
                UnPinchDrawingArea();
            }
        } else {
            if (on) {
                PinchGalleryArea();
            } else {
                UnPinchGalleryArea();
            }

        }
    }
    
    public void PinchDrawingArea() {
        //Begins a pinch if requirements are met
        Vector3 pos = handModel.fingers[0].GetTipPosition() + cameraOffset;
        bool f = FindImage(pos);        

        if (!isPinching && f) {
            //Hand has pinched an image
            isPinching = targetImage.Pinch(pos,id);
            if (!isPinching) {
                //targetImage denies pinch request, abort.
                targetImage = null;
                return;
            }
        }
    }

    public void UnPinchDrawingArea() {
        //This method can be triggered by a pinch release in the
        //drawing area or in the gallery area with differing results

        //Disable gallery pinch timer
        if (pinchTimer) { pinchTimer = false; }

        if (isPinching && parentPanel != null) {
            //Image is being dragged from the galleryUI
            DropGalleryImage();
        } else if (isPinching) {
            //Image is pinched in the drawing area
            if (targetImage != null) {
                //Release image
                targetImage.UnPinch(id);                
            }
            isPinching = false;
        }
    }

    void DropGalleryImage(bool del = false) {
        //Transition newly created panels to drawing area camera
        //Del is true if this drop occured within the galleryUI area - do not allow drops here
        if (del) { parentImage.FadeDestroy(); }
        else {
            //Images being dragged are anchored to a single panel move this panel into the
            //drawing camera and then remove the parent relation to allow independant drag after release
            //parentPanel.transform.position += cameraOffset;
            for (int i = 0; i < imagePanels.Length; i++) {
                imagePanels[i].transform.position += cameraOffset;
                Color c0 = panelRends[i].materials[0].color, c1 = panelRends[i].materials[1].color;
                c0.a = c1.a = 1;
                panelRends[i].materials[0].color = c0;
                panelRends[i].materials[1].color = c1;
                InteractableObject io = imagePanels[i].GetComponentInChildren<InteractableObject>();
                if (io !=  null) { io.EnableHover(); }
                //imagePanels[i].transform.parent = null;
            }
        }
        //Reset related vars
        parentImage.UnAttachAll();
        parentPanel = null;
        parentImage = null;
        imagePanels = null;
        panelRends = null;
        selectionFromGallery = false;
        isPinching = false;
        selectedImages.Clear();
    }

    public void SpawnImages(Vector3 pos) {
        //This creates images from the galleryUI selection via cloning.
        //Note: Local variables may not reflect current selection as this script runs
        //on both hands, and both can select independantly, instead use the SelectImage list
        //and poll each object for its selection status
        float imgSize = spawnSize * .5f;
        int i = 0;
        //Check how many images are selected
        selectedImages = new ArrayList();
        foreach (SelectImage s in selectImage) {
            if (s.IsSelected()) {
                selectedImages.Add(s);
            }
        }
        imagePanels = new GameObject[selectedImages.Count];
        panelRends = new MeshRenderer[selectedImages.Count];
        Vector3[] imgPos = new Vector3[selectedImages.Count];
        foreach (SelectImage s in selectedImages) {
            //Decide position of images based on how many images were selected
            int offset = i - selectedImages.Count / 2;
            imgPos[i] = new Vector3(pos.x + imgSize * offset * .6f, pos.y, pos.z + spawnOffsetZ);
            imagePanels[i] = (GameObject)Instantiate(imagePanelPrefab);
            imagePanels[i].transform.position = new Vector3(pos.x, pos.y, pos.z + spawnOffsetZ);
            imagePanels[i].transform.localScale *= imgSize;

            if (s.raw != null) {
                //Apply image to imagePanel and transparency effect
                panelRends[i] = imagePanels[i].transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>();
                panelRends[i].materials[1].mainTexture = s.raw.mainTexture;
                Color c0 = panelRends[i].materials[0].color, c1 = panelRends[i].materials[1].color;
                c0.a = c1.a = .25f;
                panelRends[i].materials[0].color = c0;
                panelRends[i].materials[1].color = c1;
            }
            if (offset == 0) {
                //Make panels follow the 'center' panel (not fixed for even):
                //[ ][x][ ]
                //[ ][ ][x][ ]
                parentPanel = imagePanels[i].transform;
                parentImage = parentPanel.GetComponentInChildren<InteractableObject>();
            }
            i++;
        }
        //Attach objects to center panel
        for (int j = 0; j < imagePanels.Length; j++) {            
            parentImage.AttachObject(imagePanels[j].transform, new Vector3(parentPanel.position.x - imgPos[j].x, 0, 0));
        }
        if (parentPanel != null) { isPinching = true; }
    }

    //Quick pinch selects an image, Longer pinch starts a drag
    public void PinchGalleryArea() {        
        if (!pinchTimer) { pinchTimer = true; pinchCounter = 0; return; }

        Vector3 pos = handModel.fingers[0].GetTipPosition();
        if (pinchCounter >= pinchFrames) {
            //Pinch and Drag
            foreach (SelectImage s in selectImage) {
                if (s.Pinch(pos, 1) == 1) {
                    if (!selectionFromGallery) { selectionFromGallery = true; }
                    SpawnImages(pos);
                    isPinching = true;
                    break;
                }
            }
        } else {
            //Pinch only
            foreach (SelectImage s in selectImage) {
                if (s.Pinch(pos) != -1) {
                    if (!selectionFromGallery) { selectionFromGallery = true; } //??
                    break;
                }
            }
        }
        pinchCounter = 0;
        pinchTimer = false;
    }

    public void UnPinchGalleryArea() {
        if (inGallery && pinchTimer) { PinchGalleryArea(); return; }
        if (pinchTimer) { pinchTimer = false; pinchCounter = 0; }

        if (!selectionFromGallery) {
            if (selectedImages.Count != 0) {
                foreach (SelectImage s in selectedImages) {
                    s.DeSelect();
                }
                selectedImages.Clear();
            }
            return;
        }
        if (isPinching && parentPanel != null) {
            DropGalleryImage(true);
        }
    }

    //Enable and Disable are controlled by the LeapMotionController,
    //when hands leave and enter the tracking area
    void OnEnable() {
        hover = true;
    }

    void OnDisable() {
        UnPinchDrawingArea();
        UnPinchGalleryArea();
        inGallery = false;
        hand = null;
        hover = false;
    }

    bool FindImage(Vector3 pos) {
        //Raycast from index finger position along the +z axis
        RaycastHit hit;
        Physics.Raycast(origin, pos-origin, out hit, pinchDepth, layerMask);

        if (hit.collider != null) {            
            //Hit an image, do something...
            targetImage = hit.collider.gameObject.GetComponent<InteractableObject>();
            return true;
        }
        return false;
    }

    // palm facing up - initializes timer
    void CheckLoadImage() {
        float currentHandPos = handModel.fingers[0].GetTipPosition().y + cameraOffset.y;
        if (isPalmUp && !tg.IsToggled()) {
            if (currentHandPos - tempHandPos >= 0.01) { // if the hand creates a moving up gesture
                tg.Toggle();
            }
        }
    }

    //Raycast image and trigger hover particles if something is hit
    void HoverCheck(Vector3 pos) {
        RaycastHit hit;
        Physics.Raycast(origin, pos-origin, out hit, pinchDepth, layerMask);

        if (hit.collider != null) {
            //Hit an image, do something...
            InteractableObject io = hit.collider.gameObject.GetComponent<InteractableObject>();
            if (io != null) {
                //Debug.Log("Hover");
                io.Hover();
            }
        }
    }

    //Controls the rotationUI element, the rotation of the hands if affected by the settings within
    //the InteractableObject script, which affects how much hand rotation is ignored before rotation
    //starts, which must be reflected by the rotation tracking cursor/object. Generally the deadzone
    //is 30 degrees either side of Vector3.up which is +y. This is scaled down to match the UI element
    //by 2/3
    void RotateCheck() {
        if (rotatePointer == null) { return; }
        bool pinch = pincher.IsPinching;
        //This should be active when the hand is pinching an image in the main area,
        //but not when both hands are performing scaling as it gets in the way.
        if (!pinch) { rotateDisplay.SetActive(false); return; }
        if (isPinching && targetImage != null && otherHand.isPinching && otherHand.targetImage != null && otherHand.targetImage == targetImage) { rotateDisplay.SetActive(false); return; }
        if (isPinching && targetImage != null && parentPanel == null) { rotateDisplay.SetActive(true); }
        float zAngle = pincher.Rotation.eulerAngles.z;
        float angle = 0;
        if (zAngle >= 180) { angle = 360 - zAngle; }
        else { angle = 0 - zAngle; }
        //Debug.Log("Angle: " + angle);
        if (id == 0) {
            if (angle > 0) {
                angle *= (30/70.0f) * .66f;
                angle = Mathf.Min(angle, 50);
            } else {
                angle *= .66f;
                angle = Mathf.Max(angle, -50);
            }
        } else {
            if (angle > 0) {
                angle *= .66f;
                angle = Mathf.Min(angle, 50);
            } else {
                angle *= (30/70.0f) * .66f;
                angle = Mathf.Max(angle, -50);
            }
        }
        float angleChange = angle - curAngle;            
        rotatePointer.Rotate(new Vector3(0,0,angleChange));
        curAngle = curAngle + angleChange;        
    }
}
