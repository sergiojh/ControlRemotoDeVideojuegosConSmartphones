using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KartGame.KartSystems
{
    public class MobileInput : MonoBehaviour, IInput, Server_CSharp.InputMovileInterface
    {
        private int widthScreen;
        private int heightScreen;
        [SerializeField]
        private Server server;
        [SerializeField]
        private SelectController selectController;
        [SerializeField]
        private Reconnect reconnectOBJ;
        [SerializeField]
        private QR qr;
        //Para 2220x1080
        private struct ButtonMobile{
            public ButtonMobile(int x,int y,int alto, int ancho)
            {
                this.x = x;
                this.y = y;
                this.alto = alto;
                this.ancho = ancho;
            }
            public int x;
            public int y;
            public int alto;
            public int ancho;
        }
        private ButtonMobile Arriba = new ButtonMobile(230,221,182,233);
        private ButtonMobile Derecha = new ButtonMobile(463,403,137,257);
        private ButtonMobile Abajo = new ButtonMobile(230,540,150,233);
        private ButtonMobile Izquierda = new ButtonMobile(5,403,137,225);
        private ButtonMobile A = new ButtonMobile(990,100,310,560);
        private ButtonMobile B = new ButtonMobile(1450,380,330,470);
        private ButtonMobile Start = new ButtonMobile(780,620,60,550);
        private ButtonMobile Select = new ButtonMobile(740,760,80,630);
        
        public float Acceleration
        {
            get { return m_Acceleration; }
        }
        public float Steering
        {
            get { return m_Steering; }
        }
        public bool BoostPressed
        {
            get { return m_BoostPressed; }
        }
        public bool FirePressed
        {
            get { return m_FirePressed; }
        }
        public bool HopPressed
        {
            get { return m_HopPressed; }
        }
        public bool HopHeld
        {
            get { return m_HopHeld; }
        }

        float m_Acceleration;
        float m_Steering;
        bool m_HopPressed;
        bool m_HopHeld;
        bool m_BoostPressed;
        bool m_FirePressed;

        bool m_FixedUpdateHappened;
        bool pressedJump = false;
        public bool RecieveTouch(int x, int y, int typeOfPress, ref bool vibrate)
        {
            vibrate = false;

            if (x > A.x && x < A.x + A.ancho && y > A.y && y < A.y + A.alto && typeOfPress == 0)
            {
                vibrate = true;
                m_Acceleration = -1f;
            }
            else if (x > A.x && x < A.x + A.ancho && y > A.y && y < A.y + A.alto && typeOfPress == 1)
                m_Acceleration = 1f;
            if (x > Izquierda.x && x < Izquierda.x + Izquierda.ancho && y > Izquierda.y && y < Izquierda.y + Izquierda.alto && typeOfPress == 0)
            {
                vibrate = true;
                m_Steering = -1f;
            }
            else if (x > Derecha.x && x < Derecha.x + Derecha.ancho && y > Derecha.y && y < Derecha.y + Derecha.alto && typeOfPress == 0)
            {
                vibrate = true;
                m_Steering = 1f;
            }               
            else if (x > Izquierda.x && x < Izquierda.x + Izquierda.ancho && y > Izquierda.y && y < Izquierda.y + Izquierda.alto && typeOfPress == 1 ||
                x > Derecha.x && x < Derecha.x + Derecha.ancho && y > Derecha.y && y < Derecha.y + Derecha.alto && typeOfPress == 1)
                m_Steering = 0f;


            if (pressedJump)
            {
                m_HopPressed = false;
                m_HopHeld = true;
            }

            if (x > B.x && x < B.x + B.ancho && y > B.y && y < B.y + B.alto && typeOfPress == 0 && !pressedJump)
            {
                vibrate = true;
                pressedJump = true;
                m_HopPressed = true; //NORMAL JUMP
            }
            else if(x > B.x && x < B.x + B.ancho && y > B.y && y < B.y + B.alto && typeOfPress == 1)//dejo de pulsar b
            {
                pressedJump = false;
                m_HopPressed = false;
                m_HopHeld = false;
            }

          
            m_BoostPressed |= false;
            m_FirePressed |= false;
     
            return true;
        }

        public bool EndOfConection()
        {
            reconnectOBJ.LostConnection();
            return true;
        }

        public bool ScreenSize(int width, int height)
        {
            qr.endQRShow();
            pressedJump = false;
            m_Acceleration = 1f;
            widthScreen = width;
            heightScreen = height;
            selectController.MobileConected();
            return true;
        }
    }
}