using UnityEngine;
using System.Collections;
using System.IO.Ports;
using System.Threading;
using UnityEngine.UI;
using UniRx;
using System;
using System.Linq;
//using UniRx.Async; //必要

public class SerialHandler : MonoBehaviour
{
    public delegate void SerialDataReceivedEventHandler(string message);
    public event SerialDataReceivedEventHandler OnDataReceived;

    public Dropdown Port;               //comポート
    public Dropdown Speed;              //スピード
    public Button GetComButton;     //Comポート取得button

    public GameObject ConnectionButton;

    private bool ComCheckFlag;
    private bool ReComCheckFlag;    //抜け確認

    private string portName;        //各自のマイコンのCOMポート
    private int baudRate;

    private SerialPort serialPort_;
    private Thread thread_;
    private bool isRunning_ = false;

    private string message_;
    private bool isNewMessageReceived_ = false;

    void Start()
    {
        ComCheckFlag = false;
        portName = null;
        baudRate = 9600;

    }

    void Update()
    {
        if (ComCheckFlag == true)
        {/* ポート接続 */

            /*=== 接続状態の確認 ===*/
            var Getports = SerialPort.GetPortNames();
            int GetportsIndex = Array.LastIndexOf(Getports.ToArray(), portName);

            if (GetportsIndex <= -1)
            {/*=== ポートが存在しない ===*/

                OnDestroy();    //一度閉じる
                ReComCheckFlag = false;
                Port.interactable = true;
                Speed.interactable = true;
                GetComButton.interactable = true;
                ConnectionButton.GetComponent<Image>().color = Color.red;
                ConnectionButton.GetComponentInChildren<Text>().color = Color.white;
                ConnectionButton.GetComponentInChildren<Text>().text = "接続エラー";

            }/*=== END_ポートが存在しない ===*/
            else
            {
                if (ReComCheckFlag == false)
                {

                    try
                    {
                        //プルダウンから値の取得
                        portName = Port.captionText.text;
                        baudRate = int.Parse(Speed.captionText.text);
                        Open();         //再度開く
                        Port.interactable = false;
                        Speed.interactable = false;
                        GetComButton.interactable = false;
                        ConnectionButton.GetComponent<Image>().color = Color.cyan;
                        ConnectionButton.GetComponentInChildren<Text>().color = Color.white;
                        ConnectionButton.GetComponentInChildren<Text>().text = "接続中";
                    }
                    catch
                    {
                        ReComCheckFlag = false;
                        ConnectionButton.GetComponent<Image>().color = Color.yellow;
                        ConnectionButton.GetComponentInChildren<Text>().color = Color.white;
                        ConnectionButton.GetComponentInChildren<Text>().text = "接続待機中";
                    }


                }
                else
                {
                    if (isNewMessageReceived_)
                    {
                        ConnectionButton.GetComponentInChildren<Text>().text = "接続中";
                        Debug.Log("UpdateHandler");
                        OnDataReceived(message_);
                        Debug.Log(message_);        //受信したデータの確認表示用

                    }
                    //追加
                    isNewMessageReceived_ = false;
                }
            }
        }
    }

    public void OnClickStartButton()
    {/* ポート接続ボタン */

        Debug.Log("接続ボタン");

        ComCheckFlag = !ComCheckFlag;
        Debug.Log(ComCheckFlag);

        if (ComCheckFlag == true)
        {/* ポート接続 */

            //プルダウンから値の取得
            portName = Port.captionText.text;
            baudRate = int.Parse(Speed.captionText.text);


            Debug.Log(portName);
            Debug.Log(baudRate);

            Open();

            Port.interactable = false;
            Speed.interactable = false;
            GetComButton.interactable = false;
            ConnectionButton.GetComponent<Image>().color = Color.cyan;
            ConnectionButton.GetComponentInChildren<Text>().color = Color.white;
            ConnectionButton.GetComponentInChildren<Text>().text = "接続中";


        }
        else
        {
            Debug.Log("接続を解除しました");
            OnDestroy();
            Port.interactable = true;
            Speed.interactable = true;
            GetComButton.interactable = true;
            ConnectionButton.GetComponent<Image>().color = Color.white;
            ConnectionButton.GetComponentInChildren<Text>().color = Color.black;
            ConnectionButton.GetComponentInChildren<Text>().text = "接続";

        }/* END_ポート接続 */

    }/* END_ポート接続ボタン */


    void OnDestroy()
    {
        Close();
    }

    
    private void Open()
    {
        serialPort_ = new SerialPort(portName, baudRate, Parity.None, 8, StopBits.One);
        serialPort_.ReadTimeout = 1500;
        serialPort_.Open();
        serialPort_.NewLine = "\r\n";

        isRunning_ = true;

        thread_ = new Thread(Read);
        thread_.Start();

        ReComCheckFlag = true;
    }
 

    private void Close()
    {
        isNewMessageReceived_ = false;
        isRunning_ = false;

        if (thread_ != null && thread_.IsAlive)
        {
            thread_.Join();
        }

        if (serialPort_ != null && serialPort_.IsOpen)
        {
            serialPort_.Close();
            serialPort_.Dispose();
        }
    }

    private void Read()
    {

        Debug.Log("リードきた");
        while (isRunning_ && serialPort_ != null && serialPort_.IsOpen)
        {
            Debug.Log("wileの中");
            try
            {
                message_ = serialPort_.ReadLine();　
                isNewMessageReceived_ = true;
                Debug.Log("文字いれた");
            }
            catch (System.Exception e)
            {

                ReComCheckFlag = false;
                Debug.Log("だめでした");
                Debug.LogWarning(e.Message);

            }
        }
        Debug.Log("終わった");
    }

    public void Write(string message)
    {
        try
        {
            serialPort_.Write(message);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
        }
    }

}