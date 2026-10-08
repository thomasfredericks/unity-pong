using System.Collections;
using System.Collections.Generic;
using extOSC;
using UnityEngine;

public class OscProcess : MonoBehaviour
{
    public extOSC.OSCReceiver oscReceiver;

    public int betaInMin = 0;
    public int betaInMax = 1023;
    public float betaOutMin = 0.0f;
    public float betaOutMax = 1.0f;

    public PlayerPaddle player;

    void TraiterMessageBeta(OSCMessage message)
    {
        // Traiter un entier et l’appliquer à une propriété (flux continu)

        // Récupérer la valeur du premier argument en taant qu’entier :
        int valeur = message.Values[0].IntValue;

        // Deboguer
        // Debug.Log("Reçu : " + message.Address + " " + valeur);

        // Ajustement de la valeur
        float ajustee = (
            ((float)valeur - betaInMin) / (betaInMax - betaInMin) * (betaOutMax - betaOutMin) + betaOutMin
        );
        
        player.SetPosition(ajustee);
    }


    void Start()
    {
        oscReceiver.Bind("/beta", TraiterMessageBeta);
    }

    // Update is called once per frame
    void Update() { }
}
