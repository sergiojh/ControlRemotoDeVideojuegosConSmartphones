using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class SelectController : MonoBehaviour
{
    [SerializeField]
    private Canvas SelectionMode;
    [SerializeField]
    private Server server;
    [SerializeField]
    private KartGame.KartSystems.KeyboardInput keyInput;
    [SerializeField]
    private KartGame.KartSystems.GamepadInput gamePadInput;
    [SerializeField]
    private KartGame.KartSystems.MobileInput mobileInput;
    [SerializeField]
    private KartGame.KartSystems.KartMovement kartMovement;
    [SerializeField]
    private KartGame.KartSystems.KartAnimation kartAnimation;
    [SerializeField]
    private TrackerInfo trackerInfo;
    [SerializeField]
    private GameObject directorTrigger;

    public bool finish;
    private void Start()
    {
        finish = false;
        directorTrigger.SetActive(false);//CongelarJuego
        kartMovement.enabled = false;
        kartAnimation.enabled = false;
    }

    private void Update()
    {
        if (finish)
        {
            directorTrigger.SetActive(true);//DescongelarJuego
            Destroy(this.gameObject);
        }
    }


    public void ControlerSelected()
    {
        Destroy(trackerInfo.gameObject);
        gamePadInput.enabled = true;
        keyInput.enabled = true;
        SelectionMode.enabled = false;
        mobileInput.enabled = false;
        kartMovement.input = keyInput;
        kartAnimation.input = keyInput;
        kartMovement.enabled = true;
        kartAnimation.enabled = true;
        finish = true;
    }

    public void MobileSelected()
    { 
        mobileInput.enabled = true;
        SelectionMode.enabled = false;
        gamePadInput.enabled = false;
        keyInput.enabled = false;
        kartMovement.input = mobileInput;
        kartAnimation.input = mobileInput;
        kartMovement.enabled = true;
        kartAnimation.enabled = true;
        server.IniciarServer();
        server.AddListener(mobileInput);
    }

    public void MobileConected()
    {
        finish = true;
    }
}
