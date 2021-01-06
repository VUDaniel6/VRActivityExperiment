using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.Networking;
/// <summary>
/// This script is responsible for dispatching images, pdfs, and messages to printable books
/// </summary>
public class Printer : Photon.Pun.MonoBehaviourPun {
    // Start is called before the first frame update
    public GameObject blankBook;
    public Transform spawnPosition;
    public Dictionary<string, bool> files = new Dictionary<string, bool>();
    public string baseURL;
    public int printCount = 0;
    public Queue<string> printQueue = new Queue<string>();
 
    //take in an image in the form of a bytearray takes in a position as well where the image will appear int the 
    //world, then uploads the image to the imgbb database then takes the url of the image to call PrintURL
    public IEnumerator Upload(byte[] image, Vector3 position, Vector3 rotation) {
        WWWForm form = new WWWForm();
        form.AddField("key", "b0464a4731fbebbb209904edfef6ef51");
        form.AddBinaryData("image", image);
        form.AddField("expiration", "600");
        using (UnityWebRequest www = UnityWebRequest.Post("https://api.imgbb.com/1/upload", form)) {
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError) {
                Debug.Log(www.error);
            } else {
                Debug.Log("Form upload complete!");
                Root r = JsonUtility.FromJson<Root>(www.downloadHandler.text);
                PrintURL(r.data.url, position, rotation);
            }
        }
    }


    // Update is called once per frame
    //this is for the companion app, if the player presses P a link of an image/pdf or just any message will be 
    //printed out by PrintURL
    void Update() {
        if (Input.GetKeyDown(KeyCode.P)) {
            if (photonView.IsMine) {
                PrintURL(GUIUtility.systemCopyBuffer, transform.position + transform.forward, transform.eulerAngles);
            }
        }
    }
    //based on the name it will dispatch the right function
    public void PrintURL(string url, Vector3 position, Vector3 rotation) {
        if (url.Contains(".png") || url.Contains(".jpg") || url.Contains(".jpeg")) {
            PrintImage(url, position, rotation);
            return;
        }
        PrintMessage(url, position, rotation);
    }

    private void PrintMessage(string message, Vector3 position, Vector3 rotation) {
        GameObject book = PhotonNetwork.Instantiate("Printable", position, Quaternion.Euler(rotation));
        book.GetComponentInChildren<PrintableBook>().LoadText(message,true);
        photonView.RPC("PrintMessageRPC", RpcTarget.Others, book.GetComponent<PhotonView>().ViewID, message);
    }
    public void PrintPrivateMessage(string message, Vector3 position, Vector3 rotation, int recieverID) {

        GameObject book = PhotonNetwork.Instantiate("Printable", position, Quaternion.Euler(rotation));
        book.GetComponentInChildren<PrintableBook>().LoadText(message,true);
        photonView.RPC("PrintPrivateMessageRPC", RpcTarget.Others, book.GetComponent<PhotonView>().ViewID, message, recieverID);
    }
    [PunRPC]
    private void PrintPrivateMessageRPC(int photonId, string url, int recieverID) {
        PhotonView view = PhotonView.Find(photonId);
        view.GetComponentInChildren<PrintableBook>().LoadText(url, recieverID == NokNokPlayerManager.localInstance.photonView.ViewID);
        

    }
    [PunRPC]
    private void PrintMessageRPC(int photonId, string url) {
        PhotonView view = PhotonView.Find(photonId);
        view.GetComponentInChildren<PrintableBook>().LoadText(url,true);

    }
    private void PrintImage(string url, Vector3 position, Vector3 rotation) {
        //GameObject book = Instantiate(blankBook, spawnPosition.position, spawnPosition.rotation);
        GameObject book = PhotonNetwork.Instantiate("Printable", position, Quaternion.Euler(rotation));
        //book.transform.DOMove(book.transform.position + book.transform.up, 2);   
        //book.GetComponent<Rigidbody>().velocity = book.transform.up*4 + book.transform.forward;
        StartCoroutine(book.GetComponentInChildren<PrintableBook>().LoadImage(url));

        photonView.RPC("PrintImageRPC", RpcTarget.Others, book.GetComponent<PhotonView>().ViewID, url);
    }
    [PunRPC]

    private void PrintImageRPC(int photonId, string url) {
        PhotonView view = PhotonView.Find(photonId);
        StartCoroutine(view.GetComponentInChildren<PrintableBook>().LoadImage(url));
    }
   

   
}
[System.Serializable]
public class TempImage {
    public string filename;
    public string name;
    public string mime;
    public string extension;
    public string url;

}
[System.Serializable]
public class Data {
    public string id;
    public string url_viewer;
    public string url;
    public string display_url;
    public string time;
    public string expiration;
    public TempImage image;
    public TempImage thumb;
    public TempImage medium;
    public string delete_url;
    public string title;

}
[System.Serializable]
public class Root {
    public Data data;
    public bool success;
    public int status;

}

