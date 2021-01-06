using OVRTouchSample;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
/// <summary>
/// This script is responsible for holding pdfs, images and messages
/// pdf has been a little neglected lately...
/// </summary>
public class PrintableBook : MonoBehaviour {
    public int m_Page = 0;
    public int pageCount;

    public Transform handlePosition, handle;
    public Transform right, left;
    float distance;
    bool isBook = false;
    int lastPage = 0;
    public string content = "";
    public TextMeshPro text;
    //loads the pdf into the material on the mesh from the url
    
    //load image into the mesh material from the url
    public IEnumerator LoadImage(string url) {
        content = url;
        Destroy(handlePosition.gameObject);
        Destroy(text.gameObject);
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url)) {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError) {
                Debug.Log(uwr.error);
            } else {
                // Get downloaded asset bundle
                var texture = DownloadHandlerTexture.GetContent(uwr);
                texture.filterMode = FilterMode.Bilinear;
                texture.anisoLevel = 8;
                //float ySize = transform.root.localScale.x * (texture.width / texture.height);
                print(texture.width + " " + texture.height);
                transform.root.localScale = new Vector3(transform.root.localScale.x, transform.root.localScale.x * ((float)texture.height / (float)texture.width), transform.root.localScale.z);
                Material m = GetComponent<MeshRenderer>().materials[1];
                m.mainTexture = texture;
                m.SetTextureScale(1, new Vector2(1.2f, 2f));
            }
        }
    }
    //loads text into the textfield from the message
    public void LoadText(string message, bool canRead) {
        content = message;
        transform.root.localScale = Vector3.one / 4;
        Destroy(handlePosition.gameObject);
        if (canRead) { 
            text.text = message;
        }

    }
    
    
    //handles setting the page on the pdf
    
   
}

