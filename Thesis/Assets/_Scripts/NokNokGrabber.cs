using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Photon.Pun;
using DG.Tweening;
/// <summary>
/// This script is responsible for grabbing objects in the scene
/// </summary>
public class NokNokGrabber : Photon.Pun.MonoBehaviourPun {
    private float summonAngle = 20f;
    public OVRInput.Button grabButton = OVRInput.Button.PrimaryHandTrigger;
    public OVRInput.Controller leftController = OVRInput.Controller.LTouch;
    public OVRInput.Controller rightController = OVRInput.Controller.RTouch;
    public List<NokNokGrabbable> grabbables = new List<NokNokGrabbable>();
    public NokNokGrabbable leftGrab, rightGrab;
    public NokNokPlayerManager manager;

    private float distance;
    void Awake() {
        //get all the items that can be grabbed
        grabbables = FindObjectsOfType<NokNokGrabbable>().ToList();
        //call UpdateGrabbles every 2 seconds to see if there are new items to grab or we can get rid of old ones
        InvokeRepeating("UpdateGrabbables", 2, 2);
        //this is a work around until we have private pens and erasers
        
    }
    public void UpdateGrabbables() {
        grabbables = FindObjectsOfType<NokNokGrabbable>().ToList();
    }
    // Update is called once per frame
    void Update() {
        //if its my local player
        if (photonView.IsMine) {
            //left hand
            if (OVRInput.GetDown(grabButton, leftController)) {
                //look through all the grabbables and check if any is within range
                for (int i = 0; i < grabbables.Count; i++) {
                    //get the distance from hand to object
                    distance = Vector3.Distance(manager.leftHand.position, grabbables[i].transform.position);
                    //check if it can be distance grabbed
                    if (grabbables[i].isDistanceGrabbable) {
                        //if so check if the angle is within range
                        //else check if its within grab range
                        if (Vector3.Angle(manager.leftHand.forward, grabbables[i].transform.position - manager.leftHand.position) < summonAngle) {
                            //check if the grab distance is withing range too
                            if (distance < grabbables[i].distanceGrabDistance) {
                                GrabObject(grabbables[i], manager.leftHand, true);
                                return;
                            }
                        } else {
                            if (distance < grabbables[i].grabDistance) {
                                GrabObject(grabbables[i], manager.leftHand, false);
                                return;
                            }
                        }
                    } else {
                        //its not distance grabbable but its within range
                        if (distance < grabbables[i].grabDistance) {
                            GrabObject(grabbables[i], manager.leftHand, false);
                            return;
                        }
                    }
                }
            }
            //same thing but right hand
            if (OVRInput.GetDown(grabButton, rightController)) {
                for (int i = 0; i < grabbables.Count; i++) {
                    distance = Vector3.Distance(manager.rightHand.position, grabbables[i].transform.position);
                    if (grabbables[i].isDistanceGrabbable) {
                        if (Vector3.Angle(manager.rightHand.forward, grabbables[i].transform.position - manager.rightHand.position) < summonAngle) {
                            if (distance < grabbables[i].distanceGrabDistance) {
                                GrabObject(grabbables[i], manager.rightHand, true);
                                return;
                            }
                        } else {
                            if (distance < grabbables[i].grabDistance) {
                                GrabObject(grabbables[i], manager.rightHand, false);
                                return;
                            }
                        }
                    } else {
                        if (distance < grabbables[i].grabDistance) {
                            GrabObject(grabbables[i], manager.rightHand, false);
                            return;
                        }
                    }
                }
            }
            //if trigger release drop the item if there is any
            if (OVRInput.GetUp(grabButton, leftController)) {
                if (leftGrab != null) {
                    LetGo(manager.leftHand);
                }
            }
            if (OVRInput.GetUp(grabButton, rightController)) {
                if (rightGrab != null) {
                    LetGo(manager.rightHand);
                }
            }
        }
    }

    private void LetGo(Transform hand) {
        Vector3 linearVelocity;
        Vector3 angularVelocity;
        int id = -1;
        //if letgo was called on the left hand
        if (hand == manager.leftHand) {
            //unparent the object
            leftGrab.transform.SetParent(null);
            //set back whatever option it had for rigidbody
            leftGrab.rb.isKinematic = leftGrab.isKinematic;
            //throw it with the proper amount of linear and angular velocity
            linearVelocity = OVRInput.GetLocalControllerVelocity(leftController);
            angularVelocity = OVRInput.GetLocalControllerAngularVelocity(leftController);
            //adjust the velocity for the rotation of the player
            linearVelocity = Quaternion.Euler(0, transform.root.eulerAngles.y, 0) * linearVelocity;
            angularVelocity = Quaternion.Euler(0, transform.root.eulerAngles.y, 0) * angularVelocity;
            //apply forces
            leftGrab.rb.velocity = linearVelocity;
            leftGrab.rb.angularVelocity = angularVelocity;
            //get the id so rpc calls can reference the item
            if (leftGrab.photonView != null) {
                id = leftGrab.photonView.ViewID;
            }
            //dereference the pen (even if it wasn't holding one
            //if the grabbable object is a pic or message
        } else {
            //same but right hand
            rightGrab.transform.SetParent(null);
            rightGrab.rb.isKinematic = rightGrab.isKinematic;
            linearVelocity = OVRInput.GetLocalControllerVelocity(rightController);
            angularVelocity = OVRInput.GetLocalControllerAngularVelocity(rightController);
            linearVelocity = Quaternion.Euler(0, transform.root.eulerAngles.y, 0) * linearVelocity;
            angularVelocity = Quaternion.Euler(0, transform.root.eulerAngles.y, 0) * angularVelocity;

            rightGrab.rb.velocity = linearVelocity;
            rightGrab.rb.angularVelocity = angularVelocity;
            if (rightGrab.photonView != null) {
                id = rightGrab.photonView.ViewID;
            }
            //if the grabbable object is a pic or message
            
        }
        //call rpc so others will also see that the item was dropped
        //if id is -1 it means there is no photonview on the object as its a non-synced object.
        if (id != -1) {
            photonView.RPC("LetGoRPC", RpcTarget.All, id, linearVelocity, angularVelocity);
        }
        if (hand == manager.leftHand) {
            if (leftGrab.discardable) {
                StickToSurface(leftGrab);     
                if (leftGrab.photonView == null) {
                    if (linearVelocity.magnitude > 2f) {
                        leftGrab.rb.isKinematic = false;
                        leftGrab.rb.useGravity = true;
                        StartCoroutine(Discard(leftGrab));
                    }
                }
            }
        } else {
            if (rightGrab.discardable) {
                StickToSurface(rightGrab);
                if (rightGrab.photonView == null) {
                    if (linearVelocity.magnitude > 2f) {
                        rightGrab.rb.isKinematic = false;
                        rightGrab.rb.useGravity = true;
                        StartCoroutine(Discard(rightGrab));
                    }
                }
            }
        }

    }
    void StickToSurface(NokNokGrabbable grabbable) {
        RaycastHit hit;
        if (Physics.Raycast(grabbable.transform.position, grabbable.transform.forward, out hit)) {           
            if (Vector3.Distance(grabbable.transform.position, hit.point) < 0.2f) {
                grabbable.transform.DOMove(hit.point + (hit.normal /30f),0.3f);
                grabbable.transform.forward = -hit.normal;
                if (grabbable.photonView != null) {
                    photonView.RPC("StickToSurface", RpcTarget.Others, grabbable.photonView.ViewID, hit.point + (hit.normal / 30f), -hit.normal);

                }
            }
        }
    }

    [PunRPC]
    public void LetGoRPC(int photonId, Vector3 linearVelocity, Vector3 angularVelocity) {
        //find the item by its photon id
        PhotonView view = PhotonView.Find(photonId);
        //get the reset its rigidbody properties
        NokNokGrabbable grabbable = view.gameObject.GetComponent<NokNokGrabbable>();
        grabbable.transform.SetParent(null);
        grabbable.rb.isKinematic = grabbable.isKinematic;
        if (grabbable.discardable) {
            if (linearVelocity.magnitude > 2f) {
                grabbable.rb.isKinematic = false;
                grabbable.rb.useGravity = true;
                StartCoroutine(Discard(grabbable));
            }
        }
        //apply the forces
        grabbable.rb.velocity = linearVelocity;
        grabbable.rb.angularVelocity = angularVelocity;
    }
    [PunRPC]
    public void StickToSurfaceRPC(int photonId, Vector3 position, Vector3 normal) {
        PhotonView view = PhotonView.Find(photonId);
        NokNokGrabbable grabbable = view.gameObject.GetComponent<NokNokGrabbable>();
        grabbable.transform.DOMove(position, 0.3f);
        grabbable.transform.forward = normal;
    }
    IEnumerator Discard(NokNokGrabbable grabbable) {
        //the menu is weird if its thrown around so disable it if it is a menu object
        yield return new WaitForSeconds(1);
        if (grabbable.photonView != null) {
            PhotonNetwork.Destroy(grabbable.photonView);
        } else {
            Destroy(grabbable.transform.root.gameObject);
        }
        UpdateGrabbables();
    }
    //discard that can be called statically
    public static void QuickDiscard(NokNokGrabbable grabbable) {
        if (grabbable.photonView != null) {
            PhotonNetwork.Destroy(grabbable.photonView);
        } else {
            Destroy(grabbable.transform.root.gameObject);
        }
    }
    private void GrabObject(NokNokGrabbable grabbable, Transform hand, bool distanceGrabbed) {
        bool isLeft;
        if (hand == manager.leftHand) {
            //set leftGrab to the object
            leftGrab = grabbable;
            isLeft = true;
            //if its a pen set the drawer //this is a temporary solution
            
        } else {
            //same but left side
            rightGrab = grabbable;
            isLeft = false;
            
        }
        //make it kinematic so it doesnt move
        grabbable.rb.isKinematic = true;
        //parent under the hand
        grabbable.transform.SetParent(hand);
        //if was distance grabbed put it to the grab location
        if (distanceGrabbed) {
            grabbable.transform.localPosition = grabbable.snapPosition;
            grabbable.transform.localEulerAngles = grabbable.snapRotation;
        }
        //same but can apply to non distance grabbed items
        if (grabbable.snapTransform) {
            grabbable.transform.localPosition = grabbable.snapPosition;
            grabbable.transform.localEulerAngles = grabbable.snapRotation;
            if (isLeft) {
                grabbable.transform.localPosition -= transform.right / 20;
            }

        }
        if (grabbable.isPrintable) {
            GUIUtility.systemCopyBuffer = grabbable.GetComponentInChildren<PrintableBook>().content;
        }
        //sync it
        if (grabbable.photonView != null) {
            photonView.RPC("GrabObjectRPC", RpcTarget.All, grabbable.photonView.ViewID, isLeft, grabbable.transform.localPosition, grabbable.transform.localEulerAngles);
        }
    }
    [PunRPC]
    public void GrabObjectRPC(int photonId, bool isLeft, Vector3 localPos, Vector3 localRot) {
        //find the object
        PhotonView view = PhotonView.Find(photonId);
        NokNokGrabbable grabbable = view.gameObject.GetComponent<NokNokGrabbable>();
        //parent under hand
        if (isLeft) {
            grabbable.transform.SetParent(manager.leftHand);
            grabbable.transform.localPosition -= transform.right / 10;
        } else {
            grabbable.transform.SetParent(manager.rightHand);
        }
        //make it kinematic
        grabbable.rb.isKinematic = true;
        //position it correctly
        grabbable.transform.localPosition = localPos;
        grabbable.transform.localEulerAngles = localRot;


    }
}
