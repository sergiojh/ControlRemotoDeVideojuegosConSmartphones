using UnityEngine;
using System.Net.Sockets;
using Server_CSharp;
using System.Net.NetworkInformation;


public class Server : MonoBehaviour
{
    public System.Int32 Port;
    private UDPSocket s;
    [SerializeField]
    private QR qr;
    [SerializeField]
    private Camera camera;
    [SerializeField]
    private TrackerInfo trackerInfo;
    // Use this for initialization
    //WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();

    public System.Int32 getPort()
    {
        return Port;
    }

    void LateUpdate()
    {
        if (s != null && s.checkSending() && !s.getSended())
        {

            var watch = System.Diagnostics.Stopwatch.StartNew();


            Texture2D texture = new Texture2D(camera.targetTexture.width, camera.targetTexture.height, TextureFormat.RGB24, false);
            //Read the pixels in the Rect starting at 0,0 and ending at the screen's width and height
            RenderTexture.active = camera.targetTexture;
            texture.ReadPixels(new Rect(0, 0, camera.targetTexture.width, camera.targetTexture.height), 0, 0, false);
            //texture.Compress(false);
            byte[] Bytes2Send = texture.EncodeToPNG();
            //byte[] Bytes2Send = texture.GetRawTextureData(); 
            s.setTexture2D(ref Bytes2Send);
            Destroy(texture);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            trackerInfo.AddTimeConvertImage((int)elapsedMs);
        }
    }
    public void IniciarServer()
    {          
        s = new UDPSocket();
        qr.Generate_QR();
        s.init(Port,trackerInfo,60000,15); 
    }

    public void endQRShow()
    {
        qr.endQRShow();
    }
    public void CerrarServer()
    {
        s.StopSending();
    }
   
    public void AddListener(InputMovileInterface listener)
    {
        s.AddListener(listener);
    }


}



