using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reconnect : MonoBehaviour
{
    [SerializeField]
    private Server server;
    [SerializeField]
    private KartGame.KartSystems.MobileInput mobile;

    bool tryToReconnect;

    private void Start()
    {
        tryToReconnect = false;
    }
    // Update is called once per frame
    void Update()
    {
        if (tryToReconnect)
        {
            tryToReconnect = false;
            server.IniciarServer();
            server.AddListener(mobile);
        }
    }

    public void LostConnection()
    {
        tryToReconnect = true;
    }
}
