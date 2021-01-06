using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.XR;
/// <summary>
/// This script is responsible for managing the connected users.
/// it helps other scripts reference the local player easily and the body parts
/// </summary>
public class NokNokPlayerManager : Photon.Pun.MonoBehaviourPun {
    // Start is called before the first frame update
    public Transform leftHand, rightHand, head;
    public static NokNokPlayerManager localInstance;
    public Printer printer;
    public OvrAvatar networkedAvatar;
    public GameObject browser;
    public GameObject cube;
    public GameObject menu, menuInstance;
    GameObject pointL, pointR;
    private bool initialized;

    //setup references

    // Start is called before the first frame update


    void Start() {

        Camera.main.nearClipPlane = 0.01f;
        if (photonView.IsMine) {
            localInstance = this;
            XRSettings.eyeTextureResolutionScale = 1.5f;
            OVRManager.fixedFoveatedRenderingLevel = OVRManager.FixedFoveatedRenderingLevel.HighTop;
            OVRManager.display.displayFrequency = 72.0f;
            head = GameObject.Find("CenterEyeAnchor").transform;
            leftHand.SetParent(GameObject.Find("LeftHandAnchor").transform);
            leftHand.localPosition = leftHand.localEulerAngles = Vector3.zero;
            rightHand.SetParent(GameObject.Find("RightHandAnchor").transform);
            rightHand.localPosition = rightHand.localEulerAngles = Vector3.zero;
            printer = localInstance.GetComponent<Printer>();
            var net = FindObjectOfType<NetworkController>().transform;
            if (FindObjectsOfType<NokNokPlayerManager>().Length == 1) {
                transform.position = new Vector3(net.position.x - 0.75f, transform.position.y, net.position.z);
            } else {
                transform.position = new Vector3(net.position.x + 0.75f, transform.position.y, net.position.z);

            }
        }
        Invoke("LoadHand", 2);
    }
    //set the reference for hands after the avatar is loaded 
    IEnumerator LoadReferences() {
        //avatar finally loaded set up references
        print("Initialized!!!!");
        while (networkedAvatar.GetHandTransform(OvrAvatar.HandType.Left, OvrAvatar.HandJoint.HandBase) == null) {
            yield return null;
        }
        while (networkedAvatar.GetHandTransform(OvrAvatar.HandType.Right, OvrAvatar.HandJoint.HandBase) == null) {
            yield return null;
        }
        leftHand = networkedAvatar.GetHandTransform(OvrAvatar.HandType.Left, OvrAvatar.HandJoint.HandBase);
        rightHand = networkedAvatar.GetHandTransform(OvrAvatar.HandType.Right, OvrAvatar.HandJoint.HandBase);
        //pointer
        if (photonView.IsMine) {
            pointL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pointR = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(pointR.GetComponent<MeshRenderer>());
            Destroy(pointL.GetComponent<MeshRenderer>());
            pointL.transform.parent = networkedAvatar.GetHandTransform(OvrAvatar.HandType.Left, OvrAvatar.HandJoint.IndexTip);
            pointL.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            pointL.transform.localPosition = Vector3.zero;
            pointR.transform.parent = networkedAvatar.GetHandTransform(OvrAvatar.HandType.Right, OvrAvatar.HandJoint.IndexTip);
            pointR.transform.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            pointR.transform.localPosition = Vector3.zero;
        }


    }
    //set interestgroups for voice bubbles
    public void EnableInterestGroup(int i) {
        PhotonNetwork.SetInterestGroups(System.Convert.ToByte(i), true);
        photonView.Group = System.Convert.ToByte(i);
    }
    public void DisableInterestGroup(int i) {
        PhotonNetwork.SetInterestGroups(System.Convert.ToByte(i), false);
        photonView.Group = System.Convert.ToByte(i);

    }
    //spawn a browser
    public void CreateBrowser(string url = "www.google.com") {
        
        
        browser = Photon.Pun.PhotonNetwork.Instantiate("Browser", GetInstantiationPosition(), transform.rotation);
    }
    public void StartZoom() {
        if (photonView.IsMine) {
            string invite = GUIUtility.systemCopyBuffer;
            string url = "https://us02web.zoom.us/wc/join/" + invite.Substring(26, invite.Length - 26);
            CreateBrowser("https://meet.jit.si/NokNok");
            Photon.Pun.PhotonNetwork.Instantiate("ZoomThirdPersonCamera", head.position + head.forward / 2, head.rotation);

        }
    }
    // Update is called once per frame
    void Update() {
        if (!initialized) {
            if (networkedAvatar.initialized) {
                StartCoroutine(LoadReferences());

                initialized = true;
            }
        }

    }
    public Vector3 GetInstantiationPosition() {
        Vector3 pos = head.position + (head.forward / 1.5f);
        pos.y = head.position.y;
        return pos;
    }
    public void SetPosition(Vector3 vector3) {
        photonView.RPC("SetPositionRPC", RpcTarget.All, vector3);
    }
    [PunRPC]
    public void SetPositionRPC(Vector3 vector3) {
        transform.position = vector3;
    }
}
