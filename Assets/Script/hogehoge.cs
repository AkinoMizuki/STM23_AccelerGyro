using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class hogehoge : MonoBehaviour
{
    //先ほど作成したクラス
    public SerialHandler serialHandler;
    public Text Text;

    public Text AngieText;
    public Text DirText;
    public GameObject ObliqueAngle; //釘の点P
    public GameObject HouiAngle;    //傾き矢印

    public Button SelectSnsButton;     //セレクトbutton

    public string R_message;

    /*=== 加速度 ===*/
    private float Acc_x;
    private float Acc_y;
    private float Acc_z;
    /*=== ジャイロ ===*/
    private float Gyro_x;
    private float Gyro_y;
    private float Gyro_z;
    /*=== 地磁気 ===*/
    private float Compass_x;
    private float Compass_y;
    private float Compass_z;
    private float Hall;
    private float magR;

    /*=== 加速度・ロール_ピッチ ===*/
    private float acc_roll_x;
    private float acc_pitch_y;

    /*=== ジャイロ・ロール_ピッチ ===*/
    private float inc_Gyro_x;
    private float inc_Gyro_y;
    private float inc_Gyro_z;

    /*=== 地磁気・ロール_ピッチ ===*/
    private float mag_roll_x;
    private float mag_pitch_y;
    private float mag_yaw_z;


    /*=== 1回の測定時間 ===*/
    private float Time;
    private float time_i2c;
    private float theta_update_interval;

    /*=== 各センサ傾き ===*/
    private float acc_tilt;
    private float mag_tilt;
    private float gyro_tilt;

    /*=== 各センサ傾き方向 ===*/
    private float acc_dir;
    private float mag_dir;
    private float gyro_dir;

    /*=== 温度 ===*/
    private float Temp;

    /*=== ローカル計算用 ===*/
    private float Local_x;
    private float Local_y;

    private float Localgyro_tilt;
    private float Localgyro_dir;

    private int SelectIndex = 0;
    private string[] SelectSns = { "inc_Gyro", "Angle_Acc", "gyro_tilt", "acc_tilt" };

    private bool SelectTilt = true;

    void Start()
    {
        
        /*=== 加速度 ===*/
        Acc_x = 0;
        Acc_y = 0;
        Acc_z = 0;

        /*=== ジャイロ ===*/
        Gyro_x = 0;
        Gyro_y = 0;
        Gyro_z = 0;

        /*=== 地磁気 ===*/
        Compass_x = 0;
        Compass_y = 0;
        Compass_z = 0;
        Hall = 0;
        magR = 0;

        /*=== 加速度・ロール_ピッチ ===*/
        acc_roll_x = 0;
        acc_pitch_y = 0;

        /*=== ジャイロ・ロール_ピッチ ===*/
        inc_Gyro_x = 0;
        inc_Gyro_y = 0;
        inc_Gyro_z = 0;

        /*=== 地磁気・ロール_ピッチ ===*/
        mag_roll_x = 0;
        mag_pitch_y =0;
        mag_yaw_z = 0;

        /*=== 1回の測定時間 ===*/
        Time = 0;
        time_i2c = 0;
        theta_update_interval = 0;

        /*=== 各センサ傾き ===*/
        acc_tilt = 0;
        mag_tilt = 0;
        gyro_tilt = 0;

        /*=== 各センサ傾き方向 ===*/
        acc_dir = 0;
        mag_dir = 0;
        gyro_dir = 0;

        /*=== 温度 ===*/
        Temp = 0;

        /*=== ローカル計算用 ===*/
        Localgyro_tilt = 0;
        Localgyro_dir = 0;

        //セレクトbuttonセット
        SelectSnsButton.GetComponentInChildren<Text>().text = SelectSns[SelectIndex];

        GameObject Oblique_Angle = ObliqueAngle.GetComponent<GameObject>();
        GameObject Houi_Angle = HouiAngle.GetComponent<GameObject>();

        //信号を受信したときに、そのメッセージの処理を行う
        serialHandler.OnDataReceived += OnDataReceived;
        
    }

    public void OnClickSelectSnsButton()
    {/*=== セレクトbutton切り替え ===*/
        if (SelectIndex <= 2)
        {
            SelectIndex++;
        }
        else
        {
            SelectIndex = 0;
        }

        SelectSnsButton.GetComponentInChildren<Text>().text = SelectSns[SelectIndex];

    }/*=== END_セレクトbutton切り替え ===*/

    void OnDataReceived(string message)
    {/* 受信メッセージ */

        try
        {

            //ロガー表示
            Text.GetComponentInChildren<Text>().text = message;
            //角度計算
            ObliqueMeter(message);
            Debug.Log("OnDataReceived");
            Debug.Log(message);

        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e.Message);
            Text.GetComponentInChildren<Text>().text = message;
        }

    }/* END_受信メッセージ */


    void ObliqueMeter(string message)
    {/* 角度計算 */
        string[,] ObliqueData;        //出力結果
        ObliqueData = new string[1, 1];

        //Messageの配列解体
        ObliqueData = ReceiveEncode(message);

        //データ格納
        SensSetData(ObliqueData);

        try
        {
            switch (SelectSns[SelectIndex])
            {/*=== 参照演算変更 ===*/
                case "Angle_Acc":
                    {
                        Local_x = acc_roll_x;
                        Local_y = acc_pitch_y;
                        SelectTilt = true;
                        break;
                    }
                case "gyro_tilt":
                    {

                        Localgyro_tilt = gyro_tilt;
                        Localgyro_dir = gyro_dir;
                        SelectTilt = false;
                        break;
                    }
                case "acc_tilt":
                    {
                        Localgyro_tilt = acc_tilt;
                        Localgyro_dir = acc_dir;
                        SelectTilt = false;
                        break;
                    }

                default:
                    {
                        Local_x = inc_Gyro_x;
                        Local_y = inc_Gyro_y;
                        SelectTilt = true;
                        break;
                    }
            }/*=== END_参照演算変更 ===*/

            if (SelectTilt == true)
            {/*=== 演算切り替え ===*/
                float red_gyroX = (float)(Local_x * (Mathf.Atan(1.0f) * 4.0 / 180));
                float red_gyroY = (float)((float)Local_y * (Mathf.Atan(1.0f) * 4.0 / 180));

                float gyro_ab = Mathf.Tan((float)(90.0 / 180.0 * 3.14159 - Mathf.Abs(red_gyroY)));
                float gyro_ad = Mathf.Tan((float)(90.0 / 180.0 * 3.14159 - Mathf.Abs(red_gyroX)));
                float gyro_bd = Mathf.Sqrt(gyro_ab * gyro_ab + gyro_ad * gyro_ad);
                float gyro_bp = (float)((gyro_ab * gyro_ab - gyro_ad * gyro_ad + gyro_bd * gyro_bd) / (gyro_bd * 2.0));
                float gyro_ap = Mathf.Sqrt(gyro_ab * gyro_ab - gyro_bp * gyro_bp);

                //傾き計算
                Localgyro_tilt = (float)(Mathf.Atan((float)(1.0 / gyro_ap)) * 180.0 / 3.14159);
                Localgyro_dir = (int)(Mathf.Acos(gyro_ap / gyro_ab) * 180.0 / 3.14159 + 0.5);

                if (Local_y < 0 && Local_x >= 0)
                {
                    Localgyro_dir = 180 - Localgyro_dir;
                }
                else if (Local_y < 0 && Local_x < 0)
                {
                    Localgyro_dir = 180 + Localgyro_dir;
                }
                else if (Local_y >= 0 && Local_x < 0)
                {
                    Localgyro_dir = 360 - Localgyro_dir;
                }
                else if (Local_y >= 0 && Local_x >= 0)
                {
                    Localgyro_dir += 0;
                }
            }/*=== END_演算切り替え ===*/

            Localgyro_tilt *= 100;
            Localgyro_tilt = Mathf.Floor(Localgyro_tilt) / 100;

            // テキストの表示を入れ替える
            AngieText.GetComponentInChildren<Text>().text = string.Format("{0}°", Localgyro_tilt);
            DirText.GetComponentInChildren<Text>().text = string.Format("{0}deg", Localgyro_dir);
            //オブジェクトを回転させる
            ObliqueAngle.transform.localRotation = Quaternion.Euler(0, 0, -Localgyro_tilt);
            HouiAngle.transform.localRotation = Quaternion.Euler(0, 0, 180) * Quaternion.Euler(0, 0, Localgyro_dir);
        }
        catch
        {
            // テキストの表示を入れ替える
            AngieText.GetComponentInChildren<Text>().text = "Over Flow";
            DirText.GetComponentInChildren<Text>().text = "Over Flow";
            //オブジェクトを回転させる
            ObliqueAngle.transform.localRotation = Quaternion.Euler(0, 0, 0);
            HouiAngle.transform.localRotation = Quaternion.Euler(0, 0, 180) * Quaternion.Euler(0, 0, Localgyro_dir);
        }

    }/* END_角度計算 */

    /*
    $Acc_x:-0.00,Acc_y:-0.00,Acc_z:1.04,Gyro_x:-0.05,Gyro_y:0.19,Gyro_z:0.11,Compass_x:4090.50,Compass_y:4047.50,Compass_z:16274.00,Hall:-83.00,acc_tilt:0.20,mag_tilt:0.02,gyro_tilt:0.19,Temp:0.00
    */

    private string[,] ReceiveEncode(string message)
    {/* Messageの配列解体 */

        string[,] ReceiveResult;        //出力結果
        ReceiveResult = new string[1, 1];


        //先頭確認
        int FastString_int = message.IndexOf('$');

        if (FastString_int == 0)
        {/* 先頭チェック */

            //先頭文字消去
            message = message.Replace("$", "");

            //カンマ解体
            //int Max_KanmaCount = message.Length - message.Replace(",", "").Length + 1;
            string[] MessageTypeArray = Regex.Split(message,@"[,:]");

            var TypeListToArray = MessageTypeArray.ToArray();
            // コピー先の配列を用意します。
            ReceiveResult = new string[TypeListToArray.Length / 2, 2];
            int DataCount = 0;

            //コロンの解体
            for (int KanmaCount = 0; KanmaCount <= (TypeListToArray.Length / 2) - 1; KanmaCount++)
            {
                

                ReceiveResult[KanmaCount, 0] = MessageTypeArray[DataCount];
                ReceiveResult[KanmaCount, 1] = MessageTypeArray[DataCount + 1];

                DataCount += 2;
            }

        }
        else
        {/* 先頭ではなかった */

            ReceiveResult[0, 0] = "false";

        }/* END_先頭チェック */

        return ReceiveResult;

    }/* END_Messageの配列解体 */

    void SensSetData(string[,] ObliqueData)
    {/* センサ数値を格納 */

        int MaxDataTable = ObliqueData.GetLength(0);

        for (int DataTableCount = 0; DataTableCount <= MaxDataTable -1; DataTableCount++)
        {/* 配列の仕分け */

            try
            {
                switch (ObliqueData[DataTableCount, 0])
                {

                    /*=== 加速度 ===*/
                    case "Acc_x":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                Acc_x = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    case "Acc_y":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                Acc_y = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    case "Acc_z":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                Acc_z = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    /*=== ジャイロ ===*/
                    case "Gyro_x":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                Gyro_x = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    case "Gyro_y":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                Gyro_y = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    case "Gyro_z":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                Gyro_z = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    /*=== 地磁気 ===*/
                    case "Compass_x":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                Compass_x = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    case "Compass_y":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                Compass_y = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    case "Compass_z":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                Compass_z = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    case "Hall":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                Hall = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    case "magR":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                magR = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    /*=== 加速度・ロール_ピッチ ===*/
                    case "acc_roll_x":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                acc_roll_x = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    case "acc_pitch_y":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                acc_pitch_y = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    /*=== ジャイロ・ロール_ピッチ ===*/
                    case "inc_Gyro_x":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                inc_Gyro_x = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    case "inc_Gyro_y":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                inc_Gyro_y = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    case "inc_Gyro_z":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                inc_Gyro_z = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    /*=== 地磁気・ロール_ピッチ ===*/
                    case "mag_roll_x":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                mag_roll_x = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    case "mag_pitch_y":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                mag_pitch_y = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    case "mag_yaw_z":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                mag_yaw_z = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    /*=== 1回の測定時間 ===*/
                    case "Time":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                Time = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    case "time_i2c":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                time_i2c = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    case "theta_update_interval":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                theta_update_interval = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    /*=== 各センサ傾き ===*/
                    case "acc_tilt":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                acc_tilt = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    case "mag_tilt":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                mag_tilt = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    case "gyro_tilt":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                gyro_tilt = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    /*=== 各センサ傾き方向 ===*/
                    case "acc_dir":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                acc_dir = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    case "mag_dir":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                mag_dir = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    case "gyro_dir":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                gyro_dir = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                    /*=== 温度 ===*/
                    case "Temp":
                        {
                            if (ObliqueData[DataTableCount, 1] != null || ObliqueData[DataTableCount, 1] != "")
                            {
                                Temp = float.Parse(ObliqueData[DataTableCount, 1]);
                            }
                            break;
                        }

                }
            }
            catch
            { }

        }/* END_配列の仕分け */

    }/* END_センサ数値を格納 */
}