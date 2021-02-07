using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Net;
using System;
using System.Text;

namespace Server_CSharp
{
    public interface InputMovileInterface
    {
        bool RecieveTouch(int x, int y, int typeOfPress, ref bool vibrate);

        bool EndOfConection();

        bool ScreenSize(int width, int height);
    }

    public class UDPSocket
    {
        Thread receiveThread;
        Thread sendThread;
        int puerto;
        UdpClient client;
        byte[] data;
        bool continua = true;
        bool sending = true;
        bool send = false;
        bool vibrate = false;
        IPEndPoint anyIP;
        bool conectado = false;
        byte[] byteImg = new byte[201];
        int timeWait;
        int versionProtocolMobile;
        int vibrationTime;
        bool keepAlive = false;
        bool alive = true;
        bool sendVibrationAgain = false;
        List<InputMovileInterface> listeners;
        TrackerInfo trackerInfo;
        // init
        public void init(int port, TrackerInfo trackerInfo, int timeWaitMiliseconds = 60000 ,int milisecondsForVibration = 500)
        {
            puerto = port;
            this.trackerInfo = trackerInfo;
            timeWait = timeWaitMiliseconds;
            vibrationTime = milisecondsForVibration;
            listeners = new List<InputMovileInterface>();
            receiveThread = new Thread(
                new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();
            sendThread = new Thread(
                new ThreadStart(SendData));
            sendThread.IsBackground = true;
            sendThread.Start();
        }

        public void AddListener(InputMovileInterface i)
        {
            listeners.Add(i);
        }

        public bool getSended()
        {
            return send;
        }

        public void setTexture2D(ref byte[] arrayImg)
        {
            byte[] aux = new byte[arrayImg.Length + 1];

            aux[0] = 3;

            for(int i = 1; i < arrayImg.Length + 1; i++)
                aux[i] = arrayImg[i - 1];

            byteImg = aux;
            send = true;
        }
        public void StopSending()
        {
            sending = false;
        }
        public bool checkSending()
        {
            return sending;
        }

        public void activeVibration(){
        	vibrate = true;
        }

        public void ReSendVibration(int vibration)
        {
            vibrationTime = vibration;
            sendVibrationAgain = true;
        }

        public bool conected()
        {
            return conectado;
        }
        // send thread
        private void SendData()
        {

            System.Net.Sockets.UdpClient cliente = new System.Net.Sockets.UdpClient();



            while (!conectado) ;
            cliente.Connect(anyIP.Address, puerto);
            byte[] timeToVibrate = new byte[3];
            timeToVibrate[0] = 5;
            timeToVibrate[1] = (byte)(vibrationTime & 0x000000FF);
            timeToVibrate[2] = (byte)((vibrationTime >> 4) & 0x000000FF);
            cliente.Send(timeToVibrate, timeToVibrate.Length);
            
            //miramos latencia con un ping
            Ping pingSender = new Ping();
            PingReply reply = pingSender.Send(anyIP.Address);
            if (reply.Status == IPStatus.Success)
            {
                trackerInfo.AddLatencyOfNetwork((int)(reply.RoundtripTime/2));
            }
            pingSender.Dispose();
            

            while (sending)
            {
                if (vibrate)
                {
                    byte[] vibrateMessage = new byte[1];
                    vibrateMessage[0] = 2;
                    vibrate = false;
                    cliente.Send(vibrateMessage, vibrateMessage.Length);
                }
                if (send)
                {
                    send = false;
                    cliente.Send(byteImg, byteImg.Length); //este mensaje tiene de latencia 5ms aprox cuando hace ping
                }
                if (sendVibrationAgain)
                {
                    sendVibrationAgain = false;
                    timeToVibrate[0] = 5;
                    timeToVibrate[1] = (byte)(vibrationTime & 0x000000FF);
                    timeToVibrate[2] = (byte)((vibrationTime >> 4) & 0x000000FF);
                    cliente.Send(timeToVibrate, timeToVibrate.Length);
                }
                if(keepAlive){
                    byte[] keepAliveByte = new byte[1];
                    keepAliveByte[0] = 6;
                    keepAlive = false;
                    alive = false;
                    cliente.Send(keepAliveByte, keepAliveByte.Length);
                }

            }
            byte[] sendData = new byte[1];
            //fin de comunicación
            sendData[0] = 1;
            cliente.Send(sendData, sendData.Length);
        }
        // receive thread
        private void ReceiveData()
        {

            client = new UdpClient(puerto);
            IPAddress a = GetAddress();
            anyIP = new IPEndPoint(a, puerto);


            do
            {
                data = client.Receive(ref anyIP);//recieve screen size
            } while (data.Length != 1); //compruebo que el mensaje es correcto

            versionProtocolMobile = data[0];

            do
            {
                data = client.Receive(ref anyIP);//recieve screen size
            } while (data.Length != 4); //compruebo que el mensaje es correcto


            // Get the size for the listener
            int pos0 = data[0];
            int pos1 = (data[1] << 4);

            int pos2 = data[2];
            int pos3 = (data[3] << 4);

            int x = pos0 + pos1;
            int y = pos2 + pos3;

            foreach (InputMovileInterface i in listeners)
            {
                if (i.ScreenSize(x, y))//if the event is consumed we stop passing that event
                    break;
            }

            conectado = true;//activamos mandar img
            while (continua)
            {

                try
                {
                    //TODO: Desbloquear este receive o algo para no bloquear la aplicacion en el caso de que queramos salir y no se conecte nadie.
                    var asyncResult = client.BeginReceive(null, null);
                    asyncResult.AsyncWaitHandle.WaitOne(timeWait);
                    if (asyncResult.IsCompleted)
                    {
                        try
                        {
                            
                            data = client.EndReceive(asyncResult, ref anyIP);
                            // EndReceive worked and we have received data and remote endpoint
                            if (data[0] == 0)
                            {
                                //Get the position where the user clicked
                                int type = data[1];
                                pos0 = data[2];
                                pos1 = (data[3] << 4);

                                pos2 = data[4];
                                pos3 = (data[5] << 4);

                                x = pos0 + pos1;
                                y = pos2 + pos3;

                                foreach (InputMovileInterface i in listeners)
                                {
                                    bool v = false;
                                    if (i.RecieveTouch(x, y, type, ref v))//Pass the event, if its consumed we stop passing the event
                                    {
                                        vibrate = v;
                                        break;
                                    }
                                }
                            }
                            else if (data[0] == 1)
                            {
                                int[] timePerImage = new int[(data.Length-1) / 4];

                                for (int i = 1; i < timePerImage.Length + 1; i++)
                                {
                                    int posAct = ((i - 1) * 4) + 1;
                                    pos0 = data[posAct];
                                    pos1 = (data[posAct + 1] << 4);
                                    pos2 = (data[posAct + 2] << 8);
                                    pos3 = (data[posAct + 3] << 16);
                                    timePerImage[i] = pos0 + pos1 + pos2 + pos3;

                                    trackerInfo.AddTimePerImageAndroid(timePerImage);
                                }
                            }
                            if (data[0] == 4)
                            {
                                continua = false;
                                sending = false;
                            }
                            if (data[0] == 6)
                            {
                                alive = true;
                            }
                        }
                        catch (Exception err)
                        {
                            break;
                        }
                    }
                    else //no responde
                    {
                        if(!alive){
                            continua = false;
                            sending = false;
                        }
                        else{
                            keepAlive = true;
                        }
                    }
                }
                catch (Exception err)
                {
                    break;
                }
            }
            foreach (InputMovileInterface i in listeners)
            {
                if (i.EndOfConection())
                    break;
            }

            client.Close();
        }

        private IPAddress GetAddress()
        {

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
                            return ip.Address;
                        }
                    }
                }
            }
            return null;
        }
    }
}
