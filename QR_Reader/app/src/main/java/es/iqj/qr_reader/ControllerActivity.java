package es.iqj.qr_reader;

import android.app.Activity;
import android.content.Intent;
import android.graphics.drawable.BitmapDrawable;
import android.os.Bundle;
import android.os.Handler;
import android.util.DisplayMetrics;
import android.view.MotionEvent;
import android.view.View;
import android.widget.RelativeLayout;
import android.os.Vibrator;




public class ControllerActivity extends Activity  {

    UdpClientHandler udpClientHandler;
    UdpClientThread udpClientThread;
    ReceiveData receiveData;
    //variables de botones
    float x =0;
    float y = 0;
    Vibrator v;
    String puerto = "";
    String ip = "";
    private ControllerActivity myController;
    Intent activityThatCalled;

    RelativeLayout myLayout = null;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        myController = this;
        // Set the layout for the layout we created
        setContentView(R.layout.second_layout);

        // Get the Intent that called for this Activity to open


        activityThatCalled = getIntent();

        // Get the data that was sent

        String port_ip = activityThatCalled.getExtras().getString("token");
        v = (Vibrator) getSystemService(this.VIBRATOR_SERVICE);

        char index;
        puerto = "";
        ip = "";
        boolean flag = false;

        int i = 0;
        while(i<port_ip.length()){
            index = port_ip.charAt(i);
            if(index == ':'){
                flag= true;
            }
            else if(flag){
                ip +=String.valueOf(index);
            }
            else  puerto +=String.valueOf(index);

            i++;

        }

        myLayout = findViewById(R.id.fondo);


        myLayout.setOnTouchListener(new View.OnTouchListener(){

            @Override
            public boolean onTouch(View v, MotionEvent event){
                int pointerIndex = event.getActionIndex();
                int maskedAction = event.getActionMasked();

                if(maskedAction == MotionEvent.ACTION_DOWN || maskedAction == MotionEvent.ACTION_POINTER_DOWN) {
                    //TODO: get x and y coordenates and calculate wich hotspot has been touched
                    x = event.getX(pointerIndex);
                    y = event.getY(pointerIndex);

                    if(udpClientThread == null) {
                        DisplayMetrics displayMetrics = new DisplayMetrics();
                        getWindowManager().getDefaultDisplay().getMetrics(displayMetrics);
                        int height = displayMetrics.heightPixels;
                        int width = displayMetrics.widthPixels;
                        udpClientThread = new UdpClientThread(
                                ip,
                                Integer.parseInt(puerto),
                                myController,
                                (int)x,
                                (int)y,
                                0,
                                width,
                                height);
                        udpClientThread.start();
                    }
                    else
                        udpClientThread.clicked((int)x,(int)y,0);
                }

                //LEVANTAR

                if(maskedAction == MotionEvent.ACTION_UP || maskedAction == MotionEvent.ACTION_POINTER_UP) {
                    x = event.getX(pointerIndex);
                    y = event.getY(pointerIndex);

                    if(udpClientThread == null) {
                        DisplayMetrics displayMetrics = new DisplayMetrics();
                        getWindowManager().getDefaultDisplay().getMetrics(displayMetrics);
                        int height = displayMetrics.heightPixels;
                        int width = displayMetrics.widthPixels;
                        udpClientThread = new UdpClientThread(
                                ip,
                                Integer.parseInt(puerto),
                                myController,
                                (int)x,
                                (int)y,
                                1,
                                width,
                                height);
                        udpClientThread.start();
                    }
                    else
                        udpClientThread.clicked((int)x,(int)y,1);

                }
                return true;
            }
        });


        receiveData = new ReceiveData(ip,Integer.parseInt(puerto),this);
        receiveData.start();

        udpClientHandler = new UdpClientHandler(this);
    }


    public int[] getTimePerImage(){
        return receiveData.getTimePerImage().clone();
    }

    public void onPause(){
        super.onPause();
        udpClientThread.setRunning(false);
        receiveData.setRunning(false);
        try {
            udpClientThread.join();
            receiveData.join();
        }
        catch (java.lang.InterruptedException a){

        }
        System.out.print("He terminado");
    }

    //cambiamos la img de id="fondo" en este metodo
    public void setByteMap(BitmapDrawable bit){
        myLayout.setBackground(bit);
    }

    public void VibrateTimer(int miliseconds){
        v.vibrate(miliseconds);
    }

    public void keepAlive(){udpClientThread.keepAlive();}

    public static class UdpClientHandler extends Handler {
        public static final int UPDATE_STATE = 0;
        public static final int UPDATE_MSG = 1;
        public static final int UPDATE_END = 2;
        private ControllerActivity parent;

        public UdpClientHandler(ControllerActivity parent) {
            super();
            this.parent = parent;
        }
    }

}
