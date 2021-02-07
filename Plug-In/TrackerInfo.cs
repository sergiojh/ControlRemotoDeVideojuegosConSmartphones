using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class TrackerInfo : MonoBehaviour
{
    private string path = "";
    private int latency;
    private int [] timePerConvertImage;
    private int [] timePerImageAndroid;
    private bool writing;
    // Start is called before the first frame update
    void Start()
    {
        latency = -1;
        writing = false;
        timePerConvertImage = new int[61];
        for (int i = 0; i < timePerConvertImage.Length; i++)
            timePerConvertImage[i] = 0;

        path = Application.dataPath + "Log.txt";
        var GPUName = SystemInfo.graphicsDeviceName;
        File.WriteAllText(path, "Nombre grafica: " + GPUName + "\n");//esto crea y cierra el archivo solo usar 1 vez

        var CPUName = "Procesador: " + SystemInfo.processorType;
        CPUName += " Nucleos: " + SystemInfo.processorCount + "\n";
        File.AppendAllText(path, CPUName); //para despues abrir donde me he quedado abre y cierra el archivo donde se quedo el puntero por ultima vez

        var memorySize = "RAM: " + SystemInfo.systemMemorySize + " MB\n";
        File.AppendAllText(path, memorySize);
    }

    void Update()
    {
        if (writing)
        {
            writing = false;
            string timePerImage = "Tiempo en conversion de imagen de ANDROID: " + 1 + " " + timePerImageAndroid[1];

            for (int i = 2; i < timePerImageAndroid.Length; i++)
                timePerImage += " " + i + " " + timePerImageAndroid[i];

            timePerImage += "\n";

            File.AppendAllText(path, timePerImage);
        }
    }

    public void AddLatencyOfNetwork(int latency)
    {
        this.latency = latency;
    }
    public void AddTimeConvertImage(int timeInMiliseconds)
    {
        if(timeInMiliseconds < timePerConvertImage.Length && timeInMiliseconds >= 0)
            timePerConvertImage[timeInMiliseconds] += 1;
    }

    public void AddTimePerImageAndroid(int [] timePerImageAndroid)
    {
        writing = true;
        this.timePerImageAndroid = timePerImageAndroid;
    }


    void OnApplicationQuit()
    {
        string timePerConvertion = "Tiempo en conversion de imagen de la camara: " + 1 + " " + timePerConvertImage[1];


        for (int i = 2; i < timePerConvertImage.Length; i++)
            timePerConvertion += " " + i + " " + timePerConvertImage[i];

        timePerConvertion += "\n" + "La latencia de red es " + latency + "\n";

        File.AppendAllText(path, timePerConvertion);
    }
}
