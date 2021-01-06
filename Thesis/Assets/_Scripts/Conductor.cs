using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
public class Conductor : MonoBehaviour {
    public TextAsset experiment;
    public Qs quest;
    public Printer p;
    public int iterator = 0;
    public NokNokPlayerManager[] participants;
    // Start is called before the first frame update
    void Start() {
        print(experiment.text);
        //quest = JsonUtility.FromJson<Questions>(experiment.text);
        //quest = JsonConvert.DeserializeObject<Questions>(experiment.text);
        quest = JsonConvert.DeserializeObject<Qs>(experiment.text);

    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.L)) {
            participants = FindObjectsOfType<NokNokPlayerManager>();
            var controller = FindObjectOfType<NetworkController>();
            var posa = controller.transform.position + Vector3.forward / 2f;
            posa.y = participants[0].transform.position.y;
            participants[0].SetPosition(posa);
            if (participants.Length == 2) {
                print("All participants loaded");
                var posb = controller.transform.position - Vector3.forward / 2f;
                posb.y = participants[1].transform.position.y;
                participants[1].SetPosition(posb);
            }
        }
        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            iterator = 8;
            print("Switched to Phase 2");
        }
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            iterator = 8;
            print("Switched to Phase 1");
        }
        if (participants.Length == 2) {
            if (Input.GetKeyDown(KeyCode.RightArrow)) {
                Question q = quest.Question[iterator];
                string text = "Participant: " + q.Participant + System.Environment.NewLine + "Type: " + q.Type + System.Environment.NewLine + "Phrase: " + q.Phrase;
                print(text);
                participants[q.Participant].head = participants[q.Participant].networkedAvatar.Body.RenderParts[2].transform;
                Transform participantHead = participants[q.Participant].head;
                p.PrintPrivateMessage(text, participants[q.Participant].networkedAvatar.HandRight.transform.position, participantHead.eulerAngles, participants[q.Participant].photonView.ViewID);
                print("ID " + participants[q.Participant].photonView.ViewID);
                iterator++;
            }
        }
    }
}
[System.Serializable]
public class Question {
    public int Participant;
    public string Type;
    public string Phrase;
    public int Round;
}
[System.Serializable]
public class Qs {
    public List<Question> Question;
}
