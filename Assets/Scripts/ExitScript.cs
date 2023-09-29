using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitScript : MonoBehaviour
{
    public GameObject ExitPannel;   // 終了画面ダイアログPannelオブジェクト
    public bool EscapeExitEnable;   // エスケープキー終了の有効/無効

    /// <summary>
    /// 終了ダイアログの表示
    /// </summary>
    public void ShowExitDialog()
    {
        ExitPannel.SetActive(true); // 終了画面ダイアログを表示
    }

    /// <summary>
    /// アプリケーションを終了する
    /// </summary>
    public void ExitApplication()
    {
        Application.Quit(); // アプリケーションの終了
    }

    /// <summary>
    /// アプリケーションを終了する
    /// </summary>
    public void ExitCancel()
    {
        ExitPannel.SetActive(false);    // 終了画面ダイアログを非表示化
    }

    // Use this for initialization
    void Start ()
    {
        ExitPannel.SetActive(false);    // アプリ起動時に終了画面ダイアログを非表示化
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (EscapeExitEnable && Input.GetKeyDown(KeyCode.Escape))   // エスケープキーが押されたときに終了画面ダイアログを表示
            ShowExitDialog();
    }
}
