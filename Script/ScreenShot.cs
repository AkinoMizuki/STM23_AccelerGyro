using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
//using UnityEditor;

public class ScreenShot : MonoBehaviour
{

    public UnityEngine.UI.Button ScreenShotButton;         //スクリーンショットボタン
    public GameObject ScreenShotItem;       //スクリーンショット白ベース
    public Image ScreenShotImage;           //スクリーンショット画像表示用
    public Animation ScreenShotAnimation;   //撮影アクションアニメーション

    string FolderAddress = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + @"\AccelerGyro";
    string FileName = "_Image.png";
    private Animation anim;
    private int anim_Count = 0;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void OnClickScreenShotButton()
    {/*=== 撮影ボタン ===*/
        try
        {
            string NewFileName = "";

            //フォルダーのパス確認
            FolderAddressCheck();

            //タイムスタンプ取得
            NewFileName = GetTimeStamp();

            //スクリーンショット取得
            ScreenCapture.CaptureScreenshot(FolderAddress + @"\" + NewFileName);

            anim_Count++;

            //数秒間画像クリック有効
            StartCoroutine(sleep(NewFileName));

            
        }
        catch
        {
            //EditorUtility.DisplayDialog("Error", "スクリーンショットは失敗しました。", "OK");
            Debug.Log("スクリーンショットは失敗しました");
        }

    }/*=== END_撮影ボタン ===*/

    public void OnClickImageButton()
    {/*=== 表示画像 ===*/

        //画像のパスチェック
        FolderAddressCheck();

        //画像が保存されているフォルダを開く
        System.Diagnostics.Process.Start(FolderAddress);


    }/*=== END_表示画像 ===*/
    

    public string GetTimeStamp()
    {/*=== ファイル名を作成 ===*/

        string PhotoName = "";

        //日時を秒まで取得し
        DateTime dt = DateTime.Now;
        PhotoName = dt.ToString("yyyyMMddHHmmss_") + FileName;
        //ファイル名を生成

        return PhotoName;

    }/*=== END_ファイル名を作成 ===*/

    public void FolderAddressCheck()
    {/*=== フォルダーパス確認 ===*/

        if (!File.Exists(FolderAddress))
        {
            Directory.CreateDirectory(FolderAddress);
            Debug.Log("フォルダーを作成しました");
        }

    }/*=== END_フォルダーパス確認 ===*/


    IEnumerator sleep(string NewFileName)
    {/*=== スリープコルーチン ===*/

        yield return new WaitForSeconds((float)0.5);  //0.5秒待つ


        //画像を読み込み
        //ScreenShotImage.material.mainTexture = ReadPng(FolderAddress + @"\" + NewFileName);
        Texture2D texture = ReadPng(FolderAddress + @"\" + NewFileName);

        ScreenShotImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        //画像を描画表示
        ScreenShotItem.gameObject.SetActive(true);

        //アニメーションを再生
        anim = ScreenShotAnimation.gameObject.GetComponent<Animation>();
        anim.Play();

        yield return new WaitForSeconds(5);  //10秒待つ

        if (anim_Count == 1)
        {
            //画像描画を初期化し非アクティブに戻す
            ScreenShotItem.gameObject.SetActive(false);
            ScreenShotImage.material.mainTexture = null;
            ScreenShotImage.sprite = null;
            MonoBehaviour.Destroy(texture);
            Destroy(ScreenShotImage.material.mainTexture);
            Destroy(ScreenShotImage.sprite);
            anim_Count--;
        }
        else
        {
            MonoBehaviour.Destroy(texture);
            Destroy(ScreenShotImage.material.mainTexture);
            Destroy(ScreenShotImage.sprite);
            anim_Count--;
        }

    }/*=== END_スリープコルーチン ===*/

    public byte[] ReadPngFile(string path)
    {
        FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        BinaryReader bin = new BinaryReader(fileStream);
        byte[] values = bin.ReadBytes((int)bin.BaseStream.Length);

        bin.Close();

        return values;
    }

    public Texture2D ReadPng(string path)
    {
        byte[] readBinary = ReadPngFile(path);

        int pos = 16; // 16バイトから開始

        int width = 0;
        for (int i = 0; i < 4; i++)
        {
            width = width * 256 + readBinary[pos++];
        }

        int height = 0;
        for (int i = 0; i < 4; i++)
        {
            height = height * 256 + readBinary[pos++];
        }
        
        Texture2D texture = new Texture2D(width, height);
        texture.LoadImage(readBinary);
        texture.Apply(true, true);

        return texture;
    }

}
