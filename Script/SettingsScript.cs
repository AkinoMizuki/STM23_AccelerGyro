using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsScript : MonoBehaviour
{

    public Text TimeText ;     //Comポート取得button
    public Text FpsText;

    // 属性の設定
    [RuntimeInitializeOnLoadMethod]
    static void OnRuntimeMethodLoad()
    {
        Debug.Log("After Scene is loaded and game is running");
        // スクリーンサイズの指定
        //Screen.SetResolution(1920, 1080, false);
        
        Screen.SetResolution(640, 480, false);

        // Update is called once per frame
    }


    void Update()
    {
        float fps = 1f / Time.deltaTime;
        DateTime dt = DateTime.Now;
        FpsText.GetComponentInChildren<Text>().text = fps + "fps";
        TimeText.GetComponentInChildren<Text>().text = dt.ToString("yyyy/MM/dd HH:mm:ss");
;
    }
}
