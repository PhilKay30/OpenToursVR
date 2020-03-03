using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class PlayerSetup : MonoBehaviour
{
    public GameObject menuPanel;
    // Start is called before the first frame update
    void Start()
    {
        menuPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        //foreach (var hand in Player.instance.hands)
        //{
        //    if (hand.name == "LeftHand")
        //    {
        //        hand.HideController();
        //    }

        //}
    }
}
