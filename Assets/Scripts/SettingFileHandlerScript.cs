using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 以下追加したもの
/// </summary>
using System.Runtime.InteropServices;   // WIN32APIインポート用
using System.Text;                      // StringBuilderを使用するため
using System;                           // Convert用

public class SettingFileHandlerScript : MonoBehaviour
{
    [DllImport("KERNEL32.DLL")] // iniファイル用
    private static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, uint nSize, string lpFileName); // iniファイル解析関数

    [DllImport("kernel32.dll")] // iniファイル用
    private static extern int WritePrivateProfileString(string lpApplicationName, string lpKeyName, string lpstring, string lpFileName);

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    /// <summary>
    /// iniファイルから指定されたセクションとキーの設定値を取得する
    /// 取得に失敗した場合はEmptyを返す
    /// </summary>
    public string GetIniValue(string path, string section, string key)
    {
        StringBuilder stringBuilder = new StringBuilder(1024);
        GetPrivateProfileString(section, key, string.Empty, stringBuilder, Convert.ToUInt32(stringBuilder.Capacity), path);
        return stringBuilder.ToString();
    }

    /// <summary>
    /// iniファイルへ指定されたセクションとキーの設定値をセットする
    /// 失敗した場合はfalseを返す
    /// </summary>
    public bool SetIniValue(string path, string section, string key, string value)
    {
        int result = WritePrivateProfileString(section, key, value, path);

        if (result == 0)
            return false;
        else
            return true;
    }
}
