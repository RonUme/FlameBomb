using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 以下追加したもの
/// </summary>
using System;           // Exception用
using System.IO.Ports;  // シリアル通信用
using System.Threading; // スレッド処理用

public class SerialHandlerScript : MonoBehaviour
{
    //public delegate void SerialDataReceivedEventHandler(string message);    // データ受信イベントハンドラ
    public delegate void SerialDataReceivedEventHandler(List<byte> message);    // データ受信イベントハンドラ
    public event SerialDataReceivedEventHandler OnDataReceived;             // データ受信イベント
    

    private SerialPort MySerialPort;            // シリアルポート
    private Thread MyThread;                    // スレッド
    public bool isRunning = false;              // シリアルポートの動作状態
    private string ReceivedMessage;             // 受信メッセージ
    private bool isNewMessageReceived = false;  // 新しいメッセージの有無

    List<byte> Buffer = new List<byte>();
    List<byte> Message;

    /// <summary>
    /// 更新
    /// </summary>
    void Update ()
    {
        // if (isNewMessageReceived)
        // {
        //     //OnDataReceived(ReceivedMessage);
        //     OnDataReceived(Message);
        //     isNewMessageReceived = false;
        // }
    }

    /// <summary>
    /// 終了処理
    /// </summary>
    void  OnDestroy()
    {
        Close();    // シリアルポートのクローズ処理
    }

    /// <summary>
    /// シリアルポートオープン
    /// </summary>
    public void Open(string port, int baud)
    {
        MySerialPort = new SerialPort("\\\\.\\" + port, baud, Parity.None, 8, StopBits.One);    // シリアルポート初期化
        MySerialPort.ReadTimeout = 100;                                                        // シリアルポートのリードタイムアウトの設定
        MySerialPort.WriteTimeout = 500;                                                      
        //MySerialPort.NewLine = "\r\n";                                                          // 改行コードの指定

        for (int retryCount = 0; retryCount <= 10; retryCount++) // シリアルポートのオープン(Bluetooth SPPに対応するためにリトライ処理)
        {
            try
            {
                MySerialPort.Open();
                if (MySerialPort.IsOpen)
                {
                    Debug.Log("serial port open");
                }
                break;
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex);
                Debug.Log("serial port not open");
            }
        }

        if (MySerialPort.IsOpen)            // シリアルポートがオープンできたらスレッドをスタートさせる
        {
            isRunning = true;
            MyThread = new Thread(Read);
            MyThread.Start();
        }
    }

    /// <summary>
    /// スレッドおよびシリアルポートの終了処理
    /// </summary>
    private void Close()
    {
        isRunning = false;

        if (MyThread != null && MyThread.IsAlive)
        {
            MyThread.Join();
        }

        if (MySerialPort != null && MySerialPort.IsOpen)
        {
            MySerialPort.Close();
            MySerialPort.Dispose();
        }
    }

    /// <summary>
    /// シリアルポートからメッセージを受信
    /// </summary>
    private void Read()
    {
        while (isRunning && MySerialPort != null && MySerialPort.IsOpen)
        {
            try
            {
                //ReceivedMessage = MySerialPort.ReadLine();  // シリアルポートからメッセージを受信
                byte[] data = new byte[2];
                MySerialPort.Read(data, 0, 1);  // シリアルポートからメッセージを受信
                Buffer.Add(data[0]);

                if (data[0] == '\n')
                {
                    Message = new List<byte>(Buffer);
                    Buffer = new List<byte>();
                    isNewMessageReceived = true;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);                // エラーが発生した場合コンソールにメッセージを表示
                Message = new List<byte>();
                isNewMessageReceived = true;
            }
        }
    }

    /// <summary>
    /// シリアルポートからメッセージを受信
    /// </summary>
    public void Write(string message)
    {
        try
        {
            MySerialPort.WriteLine(message);    // シリアルポートからメッセージを送信(改行コード付き)
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.Message);    // エラーが発生した場合コンソールにメッセージを表示
        }
    }

    /// <summary>
    /// シリアルポートからデータを送信
    /// </summary>
    public void Write(byte[] buffer)
    {
        try
        {
            MySerialPort.Write(buffer, 0, buffer.Length);    // シリアルポートからデータを送信
        }
        catch (Exception e)
        {
            Debug.LogWarning(e.Message);    // エラーが発生した場合コンソールにメッセージを表示
        }
    }
}
