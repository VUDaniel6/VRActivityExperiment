using Oculus.Platform;
using Oculus.Platform.Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DesktopPlayer : Photon.Pun.MonoBehaviourPun
{
    // Start is called before the first frame update
    public Camera cam;
    public ExtendedFlyCam fly;
    void Start()
    {
        if (!photonView.IsMine) {
            Destroy(cam);
            Destroy(fly);
        } else {
            //StartCoroutine(FindObjectOfType<Trello>().GetTrelloBoards());

        }
        Users.GetLoggedInUser().OnComplete(GetLoggedInUserCallback);

    }
    private void GetLoggedInUserCallback(Message<User> message) {
        if (!message.IsError) {
            print(message.Data.ID.ToString());
            //avatar.oculusUserID = message.Data.ID.ToString();
            //avatar.enabled = true;
        }

    }
    // Update is called once per frame
    void Update()
    {
       
    }
}
