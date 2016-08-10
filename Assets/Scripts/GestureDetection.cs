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

    bool isPinching = false;
    bool selectionFromGallery = false;
    bool inGallery = false;
    Leap.Controller leapController;
    LeapServiceProvider provider;
    GameObject[] imagePanels;
    Transform parentPanel;
    

    Vector3 cameraOffset = new Vector3(-50,-50,0);
    Vector3 origin = new Vector3(0,0,-10);
    Vector3 rot;

    int layerMask = (1 << 9);                           //Used for raycasting between hands and images
    int id = 0;                                         //0 is Left, 1 is Right
    float pinchDepth = 50;                              //How far to raycast in +z direction (50 ~= inf)
    //float spawnOffsetY = .1f;
    float spawnOffsetZ = .05f;
    float spawnSize = .75f;


    InteractableObject targetImage;                     //The image the hand is currently interacting with
    public SelectImage[] selectImage;                   //The images in the gallery
    public ArrayList selectedImages;

    void Start()
    {
        id = (int)handedness;
        origin += cameraOffset;
        if (selectImage == null || selectImage.Length == 0) {
            selectImage = GameObject.FindObjectsOfType<SelectImage>();
            selectedImages = new ArrayList();
        }
    }

    void Awake() {
        if (provider == null) {
            provider = GameObject.FindObjectOfType<LeapServiceProvider>();
        }

        if (leapController == null) {
            leapController = provider.GetLeapController();
        }        
    }

    void Update() {
        //Debug.Log("Pinching: " + isPinching + ", selectionFromGallery: " + selectionFromGallery + ", parentPanel: " + parentPanel);
        if (isPinching) {
            if (!inGallery && !selectionFromGallery) {
                    if (targetImage == null) { return; }
                    //Manipulate Image (drag, rotate, scale)
                    Vector3 pos = handModel.fingers[0].GetTipPosition() + cameraOffset;
                    if (targetImage.Rotate(pos, pincher.Rotation.eulerAngles.z, id)) { return; }
                    targetImage.Drag(pos, id);
            } else {
                //Manipulate thumbnails in gallery
                Vector3 pos = handModel.fingers[0].GetTipPosition();
                if (parentPanel != null) {
                    parentPanel.transform.position = pos + new Vector3(0,0,spawnOffsetZ);
                }
            }
        } else {
            if (CalculatePointing()) {
                //Pointing selection
                
            }
        }
    }

    public void GalleryTrigger(bool on) {
        if (!inGallery && on) {
            inGallery = true;
            UnPinchDrawingArea();
        }
        if (inGallery && !on) {
            inGallery = false;
            
            //Logic for transition from gallery (dragging selection...)

            //If any images are selected create the 'full size' versions of them
            //and attach them to the pinching hand in some fashion




            //Cancel previous selection
        }
    }

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
        //Debug.Log("Pinch: " + handedness + ", isPinching: " + isPinching + ", Image: " + f + ", pos: " + pos);

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
        if (isPinching && parentPanel != null) { // && !inGallery) {
            Debug.Log("UnPinchDrawingArea");
            DropGalleryImage();
        } else if (isPinching) {
            targetImage.UnPinch(id);
            isPinching = false;
        }
    }
    
    void DropGalleryImage(bool del = false) {
        //Transition newly created panels to drawing area camera
        if (del) { Destroy(parentPanel.gameObject); }
        else {
            foreach (GameObject go in imagePanels) {
                go.transform.position += cameraOffset;
                go.transform.parent = null;
            }
        }
        parentPanel = null;
        imagePanels = null;
        selectionFromGallery = false;
        isPinching = false;
        selectedImages.Clear();
    }

    public void SpawnImages(Vector3 pos) {
        //Use selectedImages array list to instantiate some images
        float imgSize = spawnSize * .5f;
        imagePanels = new GameObject[selectedImages.Count];
        int i = 0;        
        foreach (SelectImage s in selectedImages) {
            Debug.Log("selectedImages: " + s.gameObject.name);
            int offset = i - selectedImages.Count / 2;
            Vector3 imgPos = new Vector3(pos.x + imgSize * offset, pos.y, pos.z + spawnOffsetZ);
            imagePanels[i] = (GameObject)Instantiate(imagePanelPrefab, imgPos, Quaternion.identity);
            imagePanels[i].transform.localScale *= imgSize;
            
            if (s.raw != null) {
                imagePanels[i].GetComponent<MeshRenderer>().material.mainTexture = s.raw.mainTexture;
            }
            if (offset == 0) {
                //Make panels follow the 'center' panel (not fixed for even):
                //[ ][x][ ]
                //[ ][ ][x][ ]
                parentPanel = imagePanels[i].transform;
            }
            
            i++;
        }
        foreach (GameObject go in imagePanels) {
            go.transform.parent = parentPanel;
        }
        if (parentPanel != null) { isPinching = true; }
    }

    public void PinchGalleryArea() {
        Vector3 pos = handModel.fingers[0].GetTipPosition();
        Debug.Log("selectedImages: " + selectedImages.Count);
        if (selectedImages.Count == 0) {
            //New Selection - single image only
            foreach (SelectImage s in selectImage) {
                if (s.Pinch(pos, 0) == 0) {
                    selectedImages.Add(s);
                    if (!selectionFromGallery) { selectionFromGallery = true; }                
                    SpawnImages(pos);
                    isPinching = true;
                    break;
                }
            }
        } else {
            //Multi selection in progress - if the pinch was on one of the selected items
            //then instantiate
            bool pinchedSelected = false;
            foreach (SelectImage s in selectedImages) {
                if (s.PinchCheck(pos)) {
                    SpawnImages(pos);
                    pinchedSelected = true;
                    isPinching = true;
                    if (!selectionFromGallery) { selectionFromGallery = true; }
                    break;
                }
            }
            if (!pinchedSelected) {
                //Pinch was not in correct area, cancel.
                UnPinchGalleryArea();
            } else {
                //Disable highlight of images in gallery
                foreach (SelectImage s in selectedImages) {
                    if (s.IsSelected()) {
                        s.DeSelect();
                    }
                }
            }
        }
    }

    public void UnPinchGalleryArea() {
        if (!selectionFromGallery) { return; }
        if (isPinching && parentPanel != null) { 
            DropGalleryImage(true);
        }
    }


    void OnDisable() {
        //ReleasePoint(true);
        //ReleasePinch(true);
        UnPinchDrawingArea();
        UnPinchGalleryArea();
        inGallery = false;
    }

    bool FindImage(Vector3 pos) {
        //Raycast from index finger position along the +z axis
        RaycastHit hit;
        Physics.Raycast(origin, pos-origin, out hit, pinchDepth, layerMask);
        if (hit.collider != null) { Debug.Log("Hit: " + hit.collider.name); }
        //Debug.DrawRay(origin, pos-origin, Color.blue, 5);

        if (hit.collider != null) {
            //Hit an image, do something...
            targetImage = hit.collider.gameObject.GetComponent<InteractableObject>();
            return true;
        }
        return false;
    }

    
    private bool CalculatePointing() {
        Vector3 indexDirection = handModel.fingers[1].GetBoneDirection(3);
        Vector3 middleDirection = handModel.fingers[2].GetBoneDirection(3);
        Vector3 ringDirection = handModel.fingers[3].GetBoneDirection(3);
        Vector3 pinkyDirection = handModel.fingers[4].GetBoneDirection(3);

        //Leap seems to like pointing with index and middle for some reason
        if (indexDirection.z >= 0.8  && middleDirection.z < 0.8  && ringDirection.z < 0.8 && pinkyDirection.z < 0.8) {
            return true;
        }
        else {
            return false;
        }
    }



}
