using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZXing;
using ZXing.QrCode;
using System.Net.NetworkInformation;
using System.Net.Sockets;
public class QR : MonoBehaviour
{
    [SerializeField]
    System.Int32 port;
    private Texture2D myQR;
    private GameObject button;
    private bool conected = false;
    // Start is called before the first frame update
     public void Generate_QR()
    {
        conected = false;
        string ip = GetIP();
        myQR = generateQR(port + ":" + ip);
    }

    public void endQRShow()
    {
        conected = true;
    }
 
    void OnGUI()
    {

       
        if (!conected && GUI.Button(new Rect((Screen.width/2) - 150, (Screen.height/2) - 150, 256, 256), myQR, GUIStyle.none)) { }

    }
private static Color32[] Encode(string textForEncoding, int width, int height)
    {
        var writer = new BarcodeWriter
        {
            Format = BarcodeFormat.QR_CODE,
            Options = new QrCodeEncodingOptions
            {
                Height = height,
                Width = width
            }
        };
        return writer.Write(textForEncoding);
    }

    private Texture2D generateQR(string text)
    {
        var encoded = new Texture2D(256, 256);
        var color32 = Encode(text, encoded.width, encoded.height);
        encoded.SetPixels32(color32);
        encoded.Apply();
        return encoded;
    }

    public static string GetIP()
    {
        string output = "";

        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            NetworkInterfaceType _type1 = NetworkInterfaceType.Wireless80211;
            NetworkInterfaceType _type2 = NetworkInterfaceType.Ethernet;

            if ((item.NetworkInterfaceType == _type1 || item.NetworkInterfaceType == _type2) && item.OperationalStatus == OperationalStatus.Up)
#endif 
            {
                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        output = ip.Address.ToString();
                    }
                }
            }
        }
        return output;
    }


}
