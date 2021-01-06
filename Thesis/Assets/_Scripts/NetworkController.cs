using Oculus.Platform;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using System;
using Oculus.Platform.Models;
/// <summary>
/// this component is responsible for managing the networking of the application
/// this is where players join to lobbys and rooms
/// this component is also responsible for setting an expiration date to builds 
/// altought i think this functionality should be moved elsewhere as it has no connection to networking
/// </summary>
public class NetworkController : MonoBehaviourPunCallbacks {
    // Start is called before the first frame update
    public GameObject offlinePlayer;
    public MeshRenderer quad;
    public string roomName = "";
    public TMPro.TextMeshPro nameText;
    public AudioSource music, bell;
    public GameObject desktopCanvas;
    bool isVR = true;
    public DateTime date = DateTime.Today;
    public string roomToJoin = "Dome";
    public Vector3[] startPos;
    public static Dictionary<string, string> userIds = new Dictionary<string, string> {
        { "Eszti", "3889085591106839" },
        { "Meruz", "2650293371739675" },
        { "Daniel", "2041239002667549" },
        { "Khach", "3242815912481669" },
        { "Taron", "3028086823971588" }
    };
    public static string userID = "3889085591106839";
    public bool isTestMode = false, isExperimentMode = false;
    private bool ready;

    void Start() {
        print("COPY " + (GUIUtility.systemCopyBuffer).Length);
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(FindObjectOfType<OVRCameraRig>().transform.root.gameObject);
        Debug.Log("Connect Lobby");
        PhotonNetwork.ConnectUsingSettings();
        DateTime end = new DateTime(2021, 10, 30);
        if (PlayerPrefs.HasKey("userID")) {
            userID = PlayerPrefs.GetString("userID");
        }
        if (date > end) {

            UnityEngine.Application.Quit();

        }
        Oculus.Platform.Core.Initialize();
        Oculus.Platform.Users.GetLoggedInUser().OnComplete(GetLoggedInUserCallback);
        Oculus.Platform.Request.RunCallbacks();
    }
    private void GetLoggedInUserCallback(Message<User> message) {
        if (!message.IsError) {
            userID = message.Data.ID.ToString();
        }
        print("JE");
    }
    public void SetID(string name) {
        //userID = userIds[name];
        //PlayerPrefs.SetString("userID", userID);
    }
    public void SetRoom(string roomName) {
        roomToJoin = roomName;
    }
    //this function called everytime the player presses a digit button in the elevator lobby
    public void EnterDigit(int i) {
        if (roomName.Length < 4) {
            roomName += i.ToString();
            nameText.text = roomName;
        }
        if (roomName.Length == 4) {
            if (roomName == "9999") {
                SceneManager.LoadSceneAsync(1);
            } else if (roomName == "8888") {
                SceneManager.LoadSceneAsync(2);
            } else if (roomName == "7777" && false) {
                SceneManager.LoadSceneAsync(3);
            } else {
                StartCoroutine(PhotonRoom(roomName));
            }
        }
    }
    public void BackSpace() {
        roomName = roomName.Substring(0, roomName.Length - 1);
        nameText.text = roomName;
    }
    //callbacks
    public override void OnConnectedToMaster() {
        Debug.Log("ConnectedToMaster");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby() {
        Debug.Log("LobbyJoined");
        if (isTestMode) {
            JoinRoom("TEST");
        } else if (isExperimentMode){
            JoinRoom("EXPERIMENT");
        }

    }
    public void JoinRoom(string roomName) {
        this.roomName = roomName;
        ready = true;
    }
    public void PhotonRoom() {
        Debug.Log("Joined Lobby");

        Photon.Realtime.RoomOptions roomOptions = new Photon.Realtime.RoomOptions() { };
        //print(roomOptions.MaxPlayers + "Limit");
        if (!isTestMode) {
            SceneManager.LoadScene(roomToJoin);
        }
        //PhotonNetwork.JoinOrCreateRoom("Dome0000", roomOptions, TypedLobby.Default);
        PhotonNetwork.JoinOrCreateRoom("Dome0000", roomOptions, TypedLobby.Default);

    }
    public IEnumerator PhotonRoom(string roomName) {
        Debug.Log("Joined Lobby");
        yield return new WaitForSeconds(0.5f);

        Photon.Realtime.RoomOptions roomOptions = new Photon.Realtime.RoomOptions() { };
        //print(roomOptions.MaxPlayers + "Limit");
        music.DOFade(0, 0.5f);
        Destroy(music, 0.5f);
        bell.Play();
        Destroy(bell.gameObject, 2);
        //Invoke("UnFade", 2);
        quad.material.DOFade(1, 0.5f);
        yield return new WaitForSeconds(0.1f);
        if (!isTestMode) {
            SceneManager.LoadScene(roomToJoin);
        }
        yield return new WaitForSeconds(0.05f);

        PhotonNetwork.JoinOrCreateRoom(roomToJoin + nameText.text, roomOptions, TypedLobby.Default);


    }
    public override void OnCreatedRoom() {
        print("NewRoom"); 
    }

    
    public override void OnJoinedRoom() {
        Vector3 spawnPosition = roomToJoin == "Dome" ? startPos[0] : startPos[1];
        if (isExperimentMode) {
            #if UNITY_STANDALONE_WIN || UNITY_EDITOR
            Destroy(offlinePlayer);
            PhotonNetwork.Instantiate("DesktopPlayer", spawnPosition + RandomPosition(), Quaternion.identity);
            return;
            #endif
            PhotonNetwork.Instantiate("NetworkedPlayer", spawnPosition + RandomPosition(), Quaternion.identity);
            Invoke("UnFade", 2);

        } else {

            Destroy(offlinePlayer);
            PhotonNetwork.Instantiate("DesktopPlayer", spawnPosition + RandomPosition(), Quaternion.identity);

        }
    }
    Vector3 RandomPosition() {
        Vector3 v = new Vector3();
        v.x = UnityEngine.Random.Range(-5, 5);
        v.z = UnityEngine.Random.Range(-5, 5);
        v.y = 0;
        return v;
    }
    void UnFade() {
        if (quad != null) {
            quad.material.DOFade(0, 2).OnComplete(DisableFader);
        }

    }
    void DisableFader() {
        if (quad != null) {
            quad.gameObject.SetActive(false);
        }
    }
    //for debugging purpuses and the companion app
    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            isVR = false;
            PhotonRoom();
            //desktopCanvas.SetActive(true);
        }
        if (isExperimentMode && userID != "" && ready) {
            StartCoroutine(PhotonRoom(roomName));
            ready = false;

        }
    }
}
