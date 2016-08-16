using UnityEngine;
using System.Collections;

public class ImageLoader{

    private int _imagesLoaded = 0;
    public int imagesLoaded {
        get { return _imagesLoaded; }
        set { _imagesLoaded = value; }
    }

    static GameObject[] pictures = new GameObject[8]; //store all the pictures in an array

    public void loadPictures() {
        /* for (int i = 0; i < 8; i++) {            
            GameObject picturePanel = GameObject.Find("PicturePanel ("+i+")");
            if (picturePanel != null) {
                pictures[i] = picturePanel;
                picturePanel.SetActive(false); // store picture in array and disable it
            }
        } */
    }

    public void loadImage() {
        if (_imagesLoaded < 8) { //check to avoid IndexOutOfRange
            GameObject picturePanel = pictures[_imagesLoaded];
            if (picturePanel != null) {
                // enable (or "load" - we can assume it's loaded even though we're just enabling) image
                picturePanel.SetActive(true);
                _imagesLoaded++;
            }
        }
    }

}
