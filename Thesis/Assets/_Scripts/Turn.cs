using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This script is responsible for the ability to turn using the right joystick on the touch controller
/// </summary>
public class Turn : MonoBehaviour {
    private const float turnThreshold = 0.4f;
    private const int rotateIncrement = 20;

    // Start is called before the first frame update
    public float sensitivity = 10;
    Transform headTransform;
    private bool canRotate = true;

    void Start() {
        headTransform = Camera.main.transform;
    }

    // Update is called once per frame
    void Update() {
        //when the joystick is overstepping a threshold on the x axis turn the user
        float f = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick, OVRInput.Controller.RTouch).x;
        if (Mathf.Abs(f) > turnThreshold) {
            Rotate(f);
        }

    }
    //rotate in the x direction with an increment of 
    void Rotate(float f) {
        if (canRotate) {
            if (f > 0) {
                f = rotateIncrement;
            } else {
                f = -rotateIncrement;
            }
            transform.root.RotateAround(new Vector3(headTransform.position.x, 0, headTransform.position.z), Vector3.up, f);
            canRotate = false;
            Invoke("CoolDown", 0.5f);
        }
    }
    void CoolDown() {
        canRotate = true;
    }
}
