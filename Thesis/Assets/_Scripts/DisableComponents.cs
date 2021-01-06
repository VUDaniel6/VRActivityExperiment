
using AOT;
using Oculus.Platform;
using Oculus.Platform.Models;
using Photon.Pun;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// this component is responsible for setting up the networked players on local and remote end as well
/// this requires to turn on or off certain components
/// </summary>
public class DisableComponents : Photon.Pun.MonoBehaviourPun {
    public Behaviour[] components;
    public GameObject[] destroyables;
    public CharacterController cc;
    public OvrAvatarRemoteDriver remote;
    public OvrAvatarLocalDriver local;
    public OvrAvatar avatar;
    public Transform OVRPlayerController;
    public Recorder recorder;
    public PhotonVoiceNetwork voiceNetwork;
    public Speaker speaker;
    public static DisableComponents instance;
    
    // Start is called before the first frame update
    void Awake() {

        avatar.enabled = false;

    }
    //login and get to oculus id so we can later on load personalized avatars
    private void GetLoggedInUserCallback(Message<User> message) {
        if (!message.IsError) {
            print(avatar.oculusUserID);
            avatar.LevelOfDetail = ovrAvatarAssetLevelOfDetail.Highest;
            avatar.oculusUserID = message.Data.ID.ToString();
            avatar.enabled = true;
        }
    }
    //senduserid so we can load the apropriate avatars for each user on remote end
    [PunRPC]
    void SendUserID(string id) {
        avatar.oculusUserID = id;
        Invoke("LoadAvatar", Random.Range(0.5f, 1f));
        print("RPCCALLED");
    }
    
    void LoadAvatar() {
        avatar.enabled = true;

    }
    
    void Start() {
        if (photonView.IsMine) {
            var cam = GameObject.Find("OVRCameraRig");
            Destroy(GameObject.Find("CustomHandLeft"));
            Destroy(GameObject.Find("CustomHandRight"));
            cam.transform.SetParent(OVRPlayerController);
            cam.transform.rotation = Quaternion.identity;
            cam.transform.localPosition = Vector3.zero;
            Destroy(remote);
            avatar.Driver = local;
            avatar.ShowFirstPerson = true;
            avatar.ShowThirdPerson = false;
            avatar.LevelOfDetail = ovrAvatarAssetLevelOfDetail.Highest;
            avatar.oculusUserID = NetworkController.userID;
            photonView.RPC("SendUserID", RpcTarget.OthersBuffered, NetworkController.userID);
            Invoke("LoadAvatar", Random.Range(0.1f, 2f));
            instance = this;
        } else {
            foreach (var component in components) {
                component.enabled = false;
            }
            //remote
            Destroy(local);
            avatar.Driver = remote;
            avatar.ShowFirstPerson = false;
            avatar.ShowThirdPerson = true;
            cc.enabled = false;
        }
    }
}
