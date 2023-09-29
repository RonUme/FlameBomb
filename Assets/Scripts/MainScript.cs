using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 以下追加したもの
/// </summary>
using System.Windows.Forms; // ダイアログ表示用 (Unityインストールフォルダ)/Editor/Data/Mono/lib/mono\2.0からSystem.Windows.Forms.dllをPluginsフォルダにコピーする
using UnityEngine.UI;       // UI関係(Text、Image等)

public class MainScript : MonoBehaviour
{
    /// <summary>
    /// シリアル通信用パラメータ
    /// </summary>
    [Header("Serial Communication")]
    public SerialHandlerScript SerialHandler;   // シリアル通信制御ハンドラ
    public bool SerialCommunicationEnable;      // シリアル通信の有効/無効
    public string PortName = "COM3";            // シリアル通信用ポート番号
    public int Baudrate = 115200;               // シリアル通信用ボーレート
    //
    // [Header("Gripper Serial Communication")]
    // public GripperSerialHandler GripperSerialHandler;   // グリッパー用シリアル通信制御ハンドラ
    // public bool GripperSerialCommunicationEnable;      // グリッパー用シリアル通信の有効/無効
    // public string GripperPortName = "COM20";            // グリッパー用シリアル通信用ポート番号
    // public int GripperBaudrate = 38400;  
    /// <summary>
    /// iniファイルアクセスハンドラ
    /// ※注意 iniファイルはUTF-8で保存すること (日本語対応のため)
    ///        また、最初の1行目は空欄もしくはコメントにすること(読み込みエラーになるため)
    /// </summary>
    [Header("Setting File Handler")]
    public SettingFileHandlerScript SettingFileHandler; // iniファイルアクセス用ハンドラ

    [Header("Servo Controller")]
    //public GameObject[] ServoController = new GameObject[16];


    //[Header("Sliders")]
    //public Slider[] Sliders = new Slider[16];
    private Slider[] Sliders = new Slider[16];


    //[Header("Angle Texts")]
    //public Text[] AngleTexts = new Text[16];
    private Text[] AngleTexts = new Text[16];

    //[Header("Pressure Texts")]
    //public Text[] PressureTexts = new Text[16];
    private Text[] PressureTexts = new Text[16];

    private Image[] AngleLeftGauge = new Image[16];     // 角度ゲージ左側
    private Image[] AngleRightGauge = new Image[16];    // 角度ゲージ右側
    private Image[] PressureLeftGauge = new Image[16];  // 圧力ゲージ左側
    private Image[] PressureRightGauge = new Image[16]; // 圧力ゲージ右側
    GameObject[] Cubes = new GameObject[8];
    
    private float timeOut = 0.01f;
    private float timeElapsed;

    private byte BoardId = 0;           // 基板ID
    private bool IsReceivedData = true; // 受信フラグ

    float pauseStart;

    /// <summary>
    /// サーボ用パラメータ
    /// </summary>
    public class ServoParameter
    {
        public byte id;         // 基板ID
        public float angle1;    // 角度1
        public float angle2;    // 角度2
        public float angle3;    // 角度3
        public float angle4;    // 角度4
    }

    /// <summary>
    /// テスト用コード
    /// </summary>
    public void ButtonOnClick()
    {
        byte[] data = new byte[7];

        data[0] = 0xFF;         // ヘッダー
        data[1] = 0x07;         // コマンドモード
        data[2] = 0x02;         // 基板ID
        data[3] = 0x00;         // LEDの点灯モード
        data[4] = 0x00;        // チェックサム
        data[5] = (byte)'\r';  // EOL
        data[6] = (byte)'\n';  // EOL

        SerialHandler.Write(data);

        
    }

    /// <summary>
    /// テスト用コード
    /// </summary>
    public void ButtonOnClick2()
    {
        byte[] data = new byte[7];

        data[0] = 0xFF;         // ヘッダー
        data[1] = 0x07;         // コマンドモード
        data[2] = 0x02;         // 基板ID
        data[3] = 0x01;         // LEDの点灯モード
        data[4] = 0x00;        // チェックサム
        data[5] = (byte)'\r';  // EOL
        data[6] = (byte)'\n';  // EOL

        SerialHandler.Write(data);
    }

    /// <summary>
    /// テスト用コード
    /// </summary>
    public void ButtonOnClick3()
    {
        byte[] data = new byte[6];

        data[0] = 0xFF;         // ヘッダー
        data[1] = 0x05;         // コマンドモード
        data[2] = 0x02;         // 基板ID
        data[3] = 0x00;        // チェックサム
        data[4] = (byte)'\r';  // EOL
        data[5] = (byte)'\n';  // EOL

        //SerialHandler.Write(data);

        pauseStart = Time.time;
        AngleSet(2, 0, 0, 0, 0);
    }

    public void IncrementSlider(int num)
    {
        Sliders[num].value += 10f / 90f;
    }

    public void DecrementSlider(int num)
    {
        Sliders[num].value -= 10f / 90f;
    }

    /// <summary>
    /// 初期化処理
    /// </summary>
    void Start()
    {
        // 各オブジェクトの取得
        for(int i=0; i<=7; i++)
        {
           
            Cubes[i] = GameObject.Find("Cubes/Cube ("+(i + 1)+")");
        }
        print(Cubes[1]);

        // iniファイルのファイルパス取得
        // string iniFileName;
        // if (UnityEngine.Application.isEditor)
        //     iniFileName = UnityEngine.Application.dataPath.ToString() + "/Settings.ini";    // エディタ実行時のファイルパス
        // else
        //     iniFileName = UnityEngine.Application.dataPath.ToString() + "/../Settings.ini"; // アプリケーション実行時のファイルパス

        // シリアル通信の初期化
        if (SerialCommunicationEnable)                                                  // シリアル通信が有効の場合
        {
            // string port = SettingFileHandler.GetIniValue(iniFileName, "PORT", "PORT1"); // iniファイルからCOM番号を取得
            // if (port == string.Empty)                                                   // COM番号の取得に失敗した場合
            //     port = PortName;                                                        // Editerで指定されたCOM番号を設定

            // Debug.Log(port);
            string port = PortName;

            SerialHandler.Open(port, Baudrate);                 // シリアルポートオープン
            // GripperSerialHandler.Open(GripperPortName,GripperBaudrate);

            // 有線接続に失敗した場合
            /*if (SerialHandler.isRunning == false)
            {
                port = SettingFileHandler.GetIniValue(iniFileName, "PORT", "PORT2");        // iniファイルからCOM番号を取得
                if (port == string.Empty)                                                   // COM番号の取得に失敗した場合
                    port = PortName;                                                        // Editerで指定されたCOM番号を設定

                SerialHandler.Open(port, Baudrate);                 // シリアルポートオープン
            }*/

            if (SerialHandler.isRunning == true)
                SerialHandler.OnDataReceived += OnDataReceived;     // 受信イベント登録
        }

        // if (SerialHandler.isRunning == false)                                                           // シリアルハンドラが有効でない場合
        // {
        //     System.Windows.Forms.MessageBox.Show("Connection failed.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // エラーメッセージボックスを表示
        // }
        // else
        // {
        //     SerialHandler.Write("D,0,0,0");
        //     SerialHandler.Write("C,C");
        // }
    }

    /// <summary>
    /// 更新処理
    /// </summary>
    void Update()
    {
        
        float[] angles = new float[8]; // 16個分の指示角度格納用

        // 16個分の指示角度の取得とテキスト表示、指示角度に応じたアニメーション処理
        for (int i = 0; i < 8; i++)
        {
            angles[i] = Cubes[i].transform.eulerAngles.z;           // 指示角度を格納
            //AngleTexts[i].text = angles[i].ToString("F1");  // 指示角度をテキスト表示

            // 指示角度に応じたアニメーション処理
            // if (angles[i] >= 0)
            // {
            //     AngleLeftGauge[i].fillAmount = 0.0f;
            //     AngleRightGauge[i].fillAmount = angles[i] / 90.0f;
            // }
            // else
            // {
            //     AngleLeftGauge[i].fillAmount = angles[i] / -90.0f;
            //     AngleRightGauge[i].fillAmount = 0.0f;
            // }
        }

        timeElapsed += Time.deltaTime;

        if (timeElapsed >= timeOut)
        {
            
            timeElapsed = 0.0f;
            // Board0のみでレスポンスを読みながら運用
            // if (IsReceivedData == true)
            // {
            //     Debug.Log(angles[0]);
            //     AngleSet(0, angles[0], angles[1], angles[2], angles[3]);
            //     // Debug.Log(Time.realtimeSinceStartup);
            //     
            // }

            // 前回のコマンドに対するレスポンスが受信された場合(コマンドとレスポンスの衝突を避けるため)
            // if (IsReceivedData == true)
            // {
                // 指定されたIDの基板に4つのサーボの角度を指示する(圧力センサのレスポンスあり)
                Debug.Log(angles[0]);
                AngleSet(BoardId, angles[0 + (BoardId * 4)], angles[1 + (BoardId * 4)], angles[2 + (BoardId * 4)], angles[3 + (BoardId * 4)]);
                
                IsReceivedData = false;   // フラグ更新
                
                BoardId++;
                if (BoardId > 1)
                {
                    BoardId = 0;
                }
            // }

             
             // 3つの基板に同時に角度を指示する(レスポンス無し)
            // ServoParameter para1 = new ServoParameter();
            // ServoParameter para2 = new ServoParameter();
            // ServoParameter para3 = new ServoParameter();
            //
            // para1.id = 0;
            // para1.angle1 = angles[0];
            // para1.angle2 = angles[1];
            // para1.angle3 = angles[2];
            // para1.angle4 = angles[3];
            //
            // para2.id = 1;
            // para2.angle1 = angles[4];
            // para2.angle2 = angles[5];
            // para2.angle3 = angles[6];
            // para2.angle4 = angles[7];
            //
            // para3.id = 2;
            // // para3.angle1 = angles[8];
            // // para3.angle2 = angles[9];
            // // para3.angle3 = angles[10];
            // // para3.angle4 = angles[11];
            // para3.angle1 = 0;
            // para3.angle2 = 0;
            // para3.angle3 = 0;
            // para3.angle4 = 0;
            //
            // AngleSet(para1, para2);
            
           
        }
    }

    /// <summary>
    /// シリアルポートからのメッセージ受信処理
    /// </summary>
    private void OnDataReceived(List<byte> message)
    {
        IsReceivedData = true;  // データ受信フラグ更新

        if (message.Count < 10) // タイムアウトなどの時に処理をスルー
            return;

        // 受信したレスポンスから基板IDと圧力センサーの値を取り出す チェックサムの比較は省略
        uint[] val = new uint[4];
        val[0] = (uint)((message[4] << 8) | message[3]);
        val[1]= (uint)((message[6] << 8) | message[5]);
        val[2] = (uint)((message[8] << 8) | message[7]);
        val[3] = (uint)((message[10] << 8) | message[9]);
        byte boardId = message[2];

        // 圧力センサーの値の表示とアニメーション
        for (int i = 0; i < 4; i++)
        {
            PressureTexts[i + (boardId * 4)].text = val[i].ToString();

            PressureLeftGauge[i + (boardId * 4)].fillAmount = val[i] / 1023f;
            PressureRightGauge[i + (boardId * 4)].fillAmount = val[i] / 1023f;
        }
    }

    /// <summary>
    /// 指定されたIDの基板に4つのサーボの角度を指示する(圧力センサのレスポンスあり)
    /// 角度はfloat型度数法で指定(例：45°⇒45.0f)
    /// </summary>
    private void AngleSet(byte id, float angle1, float angle2, float angle3, float angle4)
    {
        float resolution = 65535.0f / 360.0f;   // サーボ分解能
        uint pos1 = (uint)(angle1 * resolution);
        uint pos2 = (uint)(angle2 * resolution);
        uint pos3 = (uint)(angle3 * resolution);
        uint pos4 = (uint)(angle4 * resolution);

        byte[] data = new byte[14];

        data[0] = 0xFF;                 // ヘッダー
        data[1] = 0x02;                 // コマンドモード
        data[2] = id;                   // 基板ID
        data[3] = (byte)pos1;           // pos1_l
        data[4] = (byte)(pos1 >> 8);    // pos1_h
        data[5] = (byte)pos2;           // pos2_l
        data[6] = (byte)(pos2 >> 8);    // pos2_h
        data[7] = (byte)pos3;           // pos3_l
        data[8] = (byte)(pos3 >> 8);    // pos3_h
        data[9] = (byte)pos4;           // pos4_l
        data[10] = (byte)(pos4 >> 8);   // pos4_h
        data[11] = 0x00;                // チェックサム
        data[12] = (byte)'\r';          // EOL
        data[13] = (byte)'\n';          // EOL

        byte sum = 0;
        for (int i = 0; i < 11; i++)
        {
            sum += data[i];
        }
        data[11] = (byte)(~sum + 1);

        SerialHandler.Write(data);
    }

    /// <summary>
    /// 3つの基板に同時に角度を指示する(レスポンス無し)
    /// 角度はfloat型度数法で指定(例：45°⇒45.0f)
    /// </summary>
    private void AngleSet(ServoParameter para1, ServoParameter para2, ServoParameter para3)
    {
        byte[] data = new byte[32];
        float resolution = 65535.0f / 360.0f;   // サーボ分解能

        data[0] = 0xFF;                 // ヘッダー
        data[1] = 0x03;                 // コマンドモード

        uint pos1 = (uint)(para1.angle1 * resolution);
        uint pos2 = (uint)(para1.angle2 * resolution);
        uint pos3 = (uint)(para1.angle3 * resolution);
        uint pos4 = (uint)(para1.angle4 * resolution);

        data[2] = para1.id;             // 基板ID
        data[3] = (byte)pos1;           // pos1_l
        data[4] = (byte)(pos1 >> 8);    // pos1_h
        data[5] = (byte)pos2;           // pos2_l
        data[6] = (byte)(pos2 >> 8);    // pos2_h
        data[7] = (byte)pos3;           // pos3_l
        data[8] = (byte)(pos3 >> 8);    // pos3_h
        data[9] = (byte)pos4;           // pos4_l
        data[10] = (byte)(pos4 >> 8);   // pos4_h

        pos1 = (uint)(para2.angle1 * resolution);
        pos2 = (uint)(para2.angle2 * resolution);
        pos3 = (uint)(para2.angle3 * resolution);
        pos4 = (uint)(para2.angle4 * resolution);

        data[11] = para2.id;            // 基板ID
        data[12] = (byte)pos1;          // pos1_l
        data[13] = (byte)(pos1 >> 8);   // pos1_h
        data[14] = (byte)pos2;          // pos2_l
        data[15] = (byte)(pos2 >> 8);   // pos2_h
        data[16] = (byte)pos3;          // pos3_l
        data[17] = (byte)(pos3 >> 8);   // pos3_h
        data[18] = (byte)pos4;          // pos4_l
        data[19] = (byte)(pos4 >> 8);   // pos4_h

        pos1 = (uint)(para3.angle1 * resolution);
        pos2 = (uint)(para3.angle2 * resolution);
        pos3 = (uint)(para3.angle3 * resolution);
        pos4 = (uint)(para3.angle4 * resolution);

        data[20] = para3.id;            // 基板ID
        data[21] = (byte)pos1;          // pos1_l
        data[22] = (byte)(pos1 >> 8);   // pos1_h
        data[23] = (byte)pos2;          // pos2_l
        data[24] = (byte)(pos2 >> 8);   // pos2_h
        data[25] = (byte)pos3;          // pos3_l
        data[26] = (byte)(pos3 >> 8);   // pos3_h
        data[27] = (byte)pos4;          // pos4_l
        data[28] = (byte)(pos4 >> 8);   // pos4_h

        data[29] = 0x00;                // チェックサム
        data[30] = (byte)'\r';          // EOL
        data[31] = (byte)'\n';          // EOL

        byte sum = 0;
        for (int i = 0; i < 29; i++)
        {
            sum += data[i];
        }
        data[29] = (byte)(~sum + 1);

        SerialHandler.Write(data);
        string output = BitConverter.ToString(data);
        Debug.Log(output);
    }
    
     private void AngleSet(ServoParameter para1, ServoParameter para2)
    {
        byte[] data = new byte[24];
        float resolution = 65535.0f / 360.0f;   // サーボ分解能

        data[0] = 0xFF;                 // ヘッダー
        data[1] = 0x03;                 // コマンドモード
        data[2] = 0x02;

        uint pos1 = (uint)(para1.angle1 * resolution);
        uint pos2 = (uint)(para1.angle2 * resolution);
        uint pos3 = (uint)(para1.angle3 * resolution);
        uint pos4 = (uint)(para1.angle4 * resolution);

        data[2+1] = para1.id;             // 基板ID
        data[3+1] = (byte)pos1;           // pos1_l
        data[4+1] = (byte)(pos1 >> 8);    // pos1_h
        data[5+1] = (byte)pos2;           // pos2_l
        data[6+1] = (byte)(pos2 >> 8);    // pos2_h
        data[7+1] = (byte)pos3;           // pos3_l
        data[8+1] = (byte)(pos3 >> 8);    // pos3_h
        data[9+1] = (byte)pos4;           // pos4_l
        data[10+1] = (byte)(pos4 >> 8);   // pos4_h

        pos1 = (uint)(para2.angle1 * resolution);
        pos2 = (uint)(para2.angle2 * resolution);
        pos3 = (uint)(para2.angle3 * resolution);
        pos4 = (uint)(para2.angle4 * resolution);

        data[11+1] = para2.id;            // 基板ID
        data[12+1] = (byte)pos1;          // pos1_l
        data[13+1] = (byte)(pos1 >> 8);   // pos1_h
        data[14+1] = (byte)pos2;          // pos2_l
        data[15+1] = (byte)(pos2 >> 8);   // pos2_h
        data[16+1] = (byte)pos3;          // pos3_l
        data[17+1] = (byte)(pos3 >> 8);   // pos3_h
        data[18+1] = (byte)pos4;          // pos4_l
        data[19+1] = (byte)(pos4 >> 8);   // pos4_h

        data[20+1] = 0x00;                // チェックサム
        data[21+1] = (byte)'\r';          // EOL
        data[22+1] = (byte)'\n';          // EOL

        byte sum = 0;
        for (int i = 0; i < 21; i++)
        {
            sum += data[i];
        }
        data[21] = (byte)(~sum + 1);

        SerialHandler.Write(data);
        string output = BitConverter.ToString(data);
        // Debug.Log(output);
    }
}
