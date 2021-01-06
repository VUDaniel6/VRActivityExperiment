using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTurnOn : MonoBehaviour
{
    //This script is just a quick fix to ease the startup lag of the application
    public void Start()
    {
        Invoke("TurnOn", 5);
    }

    // Update is called once per frame
    void TurnOn()
    {
        transform.GetChild(0).gameObject.SetActive(true);
    }
}
