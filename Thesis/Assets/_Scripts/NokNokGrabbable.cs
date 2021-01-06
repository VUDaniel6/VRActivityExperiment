using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
/// <summary>
/// This script is currently only used for data
/// </summary>
public class NokNokGrabbable : MonoBehaviour {
    // Start is called before the first frame update
    public Rigidbody rb;
    public bool snapTransform;
    public float grabDistance = 0.25f;
    public float distanceGrabDistance = 3f;
    public Vector3 snapPosition;
    public Vector3 snapRotation;
    public PhotonView photonView;
    public bool isDistanceGrabbable = false;
    public bool isKinematic;
    public bool isGrabbed = false;
    public bool snapRelease = false;
    public bool discardable = true;
    public bool isPrintable = false;
    private Transform snapReleaseTransform;
    void Start() {
        isKinematic = rb.isKinematic;
    }

    private void OnTriggerEnter(Collider other) {
        if (!photonView.IsMine) {
            return;
        }
        if (isPrintable) {
            
            print(other.transform.name);
        }
    }

}
