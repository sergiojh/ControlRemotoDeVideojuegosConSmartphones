package es.iqj.qr_reader;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.SocketException;
import java.net.UnknownHostException;

public class UdpClientThread extends Thread{

    boolean ready = false;

    String dstAddress;
    int dstPort;
    private boolean running;
    ControllerActivity.UdpClientHandler handler;
    ControllerActivity controller;

    DatagramSocket socket;
    InetAddress address;
    boolean keepAlive = false;
    int xPos;
    int yPos;
    int width;
    int height;
    int typeClick;
    int versionNumber = 1;
    public UdpClientThread(String addr, int port, ControllerActivity control ,int x, int y,int type, int widthScreen, int heightScreen){
        super();
        dstAddress = addr;
        dstPort = port;
        this.handler = control.udpClientHandler;
        xPos = x;
        yPos = y;
        width = widthScreen;
        height = heightScreen;
        typeClick = type;
        controller = control;

    }

    public void keepAlive(){
        this.keepAlive = true;
    }

    public void setRunning(boolean running){
        this.running = running;
    }

    public void clicked(int x, int y, int type){
        xPos = x;
        yPos = y;
        ready = true;
        typeClick = type;
    }


    @Override
    public void run() {
        try {
            socket = new DatagramSocket();
        } catch (SocketException e) {
            e.printStackTrace();
        }

        //Mandamos la version del protocolo para que el pc sepa que version tenemos
        byte[] version = new byte[1];
        running = true;
        try {
            address = InetAddress.getByName(dstAddress);
        } catch (UnknownHostException e) {
            e.printStackTrace();
        }

        version[0] = (byte)(versionNumber & (0x000000FF));

        DatagramPacket packet0 =
                new DatagramPacket(version, version.length, address, dstPort);

        try {
            socket.send(packet0);
        } catch (IOException e) {
            e.printStackTrace();
        }

        byte[] buf = new byte[4];

        //envio tamaño de la pantalla
        buf[0] = (byte)(width & (0x000000FF));
        buf[1] = (byte)((width & (0x0000FF00)) >> 4);

        buf[2] = (byte)(height & (0x000000FF));
        buf[3] = (byte)((height & (0x0000FF00)) >> 4);

        DatagramPacket packet1 =
                new DatagramPacket(buf, buf.length, address, dstPort);

        try {
            socket.send(packet1);
        } catch (IOException e) {
            e.printStackTrace();
        }

        buf = new byte[6];
        DatagramPacket packet =
                new DatagramPacket(buf, buf.length, address, dstPort);

        while(running) {
            if(ready) {
                try {
                    ready = false;

                    address = InetAddress.getByName(dstAddress);
                    System.out.println(address);
                    // send request
                    buf[0] = 0;//cabecera
                    buf[1] = (byte)(typeClick & (0x000000FF));
                    buf[2] = (byte)(xPos & (0x000000FF));
                    buf[3] = (byte)((xPos & (0x0000FF00)) >> 4);

                    buf[4] = (byte)(yPos & (0x000000FF));
                    buf[5] = (byte)((yPos & (0x0000FF00)) >> 4);

                    socket.send(packet);

                } catch (SocketException e) {
                    e.printStackTrace();
                } catch (UnknownHostException e) {
                    e.printStackTrace();
                } catch (IOException e) {
                    e.printStackTrace();
                }
            }
            if(keepAlive) {
                keepAlive = false;
                byte[] keepAlive = new byte[1];

                keepAlive[0] = 6;//cabecera

                packet =
                        new DatagramPacket(keepAlive, keepAlive.length, address, dstPort);

                try {
                    socket.send(packet);
                }
                catch (IOException e){
                    e.printStackTrace();
                }

            }
        }

        int [] timePerImage = controller.getTimePerImage();

        byte[] bufferImgMessage = new byte[timePerImage.length * 4 + 1];
        bufferImgMessage[0] = 1; //cabecera
        for(int i = 1; i < timePerImage.length + 1; i++){
            int pos = ((i - 1) * 4) + 1;
            bufferImgMessage[pos] = (byte)(timePerImage[i - 1] & (0x000000FF));
            bufferImgMessage[pos + 1] = (byte)((timePerImage[i - 1] & (0x0000FF00)) >> 4);
            bufferImgMessage[pos + 2] = (byte)((timePerImage[i - 1] & (0x00FF0000)) >> 8);
            bufferImgMessage[pos + 3] = (byte)((timePerImage[i - 1] & (0xFF000000)) >> 16);
        }
        packet =
                new DatagramPacket(bufferImgMessage, bufferImgMessage.length, address, dstPort);

        try {
            socket.send(packet);
        }
        catch (IOException e){
            e.printStackTrace();
        }


        byte[] buff = new byte[1];

        buff[0] = 4;//cabecera

        packet =
                new DatagramPacket(buff, buff.length, address, dstPort);

        try {
            socket.send(packet);
        }
        catch (IOException e){
            e.printStackTrace();
        }
        System.out.print("Mensaje mandado");
        socket.close();
        handler.sendEmptyMessage(ControllerActivity.UdpClientHandler.UPDATE_END);
    }
}
