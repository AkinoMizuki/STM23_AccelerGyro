using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Management;
using System.IO.Ports;

public class GetComPort : MonoBehaviour
{

    public Dropdown dropdown;    //操作するオブジェクトを設定する


    public void Start()
    {
        if (dropdown)
        {

            dropdown.ClearOptions();    //現在の要素をクリアする
            List<string> list = new List<string>();
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                list.Add(port);

            }
            Console.ReadLine();
            dropdown.AddOptions(list);  //新しく要素のリストを設定する
            dropdown.value = 0;         //デフォルトを設定(0～n-1)

        }

    }

    public void GetComPorts(int value)
    {

        if (dropdown)
        {

            dropdown.ClearOptions();    //現在の要素をクリアする
            List<string> list = new List<string>();
            string[] ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                list.Add(port);

            }
            Console.ReadLine();
            dropdown.AddOptions(list);  //新しく要素のリストを設定する
            dropdown.value = 0;         //デフォルトを設定(0～n-1)

        }

    }
}