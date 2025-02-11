using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using static UIManager;
using System.Configuration;
using NUnit.Framework.Constraints;
using System.Threading;
using DG.Tweening.Core.Easing;
using NUnit.Framework.Interfaces;
using System;
using System.Threading.Tasks;
using System.IO.IsolatedStorage;

public class UIManager : MonoBehaviour
{
    public enum ShapeType
    {
        None,
        Sphere,
        Rect,
        Hexagon,
        Max
    }

    public enum GameState
    {
        Title,
        InGame,
        End
    }

    public static UIManager Instance;

    [Header("UIs")]
    [SerializeField] GameObject title;
    [SerializeField] GameObject gameEnd;
    [SerializeField] GameObject inGame;
    [SerializeField] GameObject pleaseTakeOutShapeUI;
    [SerializeField] GameObject insertNewShapeUI;
    [SerializeField] GameObject WrongShapeUI;
    [SerializeField] InGameManager gameManager;
    [SerializeField] GameEndUI gameEndUI;
    [SerializeField] GameState currnetGameState;
    public Shape shape;

    [Header("Serial Ports")]
    [SerializeField] SerialPort serialPort;
    [SerializeField] bool[] sensors = new bool[61];
    int currentLastSensingIndex = 0;
    int currentStartSensingIndex = 0;
    int previousStartSensingIndex = 0;
    int previousLastSensingIndex = 0;
    public int baudRate = 9600; // �ø��� ��� �ӵ�
    private byte[] receivedBytes = new byte[19]; // 5����Ʈ ������ �����
    private byte[] latestData = new byte[19];    // �ֽ� ������ �����

    [Header("Game Status")]
    public bool isGameEnd = false;
    public bool IsTimeOut = false;
    float CheckingTime = 0.0f;
    public ShapeType shapeType;
    bool isCheckingShape;

    [Header("Shape Setting")]
    [Tooltip("�ϴ��� ���� �����ϸ� ���� �迭�� �ش� �ε��� ���� ������ ����� �������� ������ �������� �ν��մϴ�.")]
    [SerializeField] int RectIndex = 1;
    [SerializeField] int HexagonIndex = 2;
    [SerializeField] string[] portNames;
    [SerializeField] string portName;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(Instance);

        portNames = SerialPort.GetPortNames();

        if (portNames.Length > 0)
        {
            string selectedPort = "";

            foreach (var port in portNames)
            {
                Debug.Log("Port Name : " + port);

                if (port != "COM1" && port != "LP1")
                {
                    selectedPort = port;
                    Debug.Log(port);
                    break;
                }
            }

            if (!string.IsNullOrEmpty(selectedPort))
                portName = selectedPort;
            else
                Debug.Log("Not Connection");

        }
        else
            Debug.LogWarning("No serial ports found.");

        LoadTitleUI();


    }
    async void Start()
    {
        if (string.IsNullOrEmpty(portName))
        {
            Debug.LogError("Port name is empty. Cannot initialize serial port.");
            return;
        }

        InitializeSerialPort();
        await StartListening(); // �񵿱� ������ ���� ����
    }

    private void Update()
    {
        CheckArray();      
    }

    public void LoadTitleUI()
    {
        currnetGameState = GameState.Title;
        isGameEnd = false;
        shapeType = ShapeType.None;
        title.SetActive(true);
        gameEnd.SetActive(false);
        inGame.SetActive(false);
        IsTimeOut = false;
        StopAllCoroutines();

    }

    public void LoadGameEndUI()
    {
        StartCoroutine(CheckingTakeOutShape());
        isGameEnd = true;
        currnetGameState = GameState.End;
        title.SetActive(false);
        gameEnd.SetActive(true);
        inGame.SetActive(false);

        if (IsTimeOut)
        {
            gameEndUI.ShowUI(GameEndUI.UIType.MissionFail);
        }
        else if(shapeType == ShapeType.Sphere)
        {
            gameEndUI.ShowUI(GameEndUI.UIType.SphereEnd);
        }
        else
        {
            gameEndUI.ShowUI(GameEndUI.UIType.MissionSucceed);
        }
    }

    void LoadInsertNewShapeUI()
    {
        insertNewShapeUI.SetActive(true);
        isGameEnd = false;
        IsTimeOut = false;
    }

    void UnloadInsertNewShapeUI() 
    {
        insertNewShapeUI.SetActive(false);
    }

    void LoadInGameUI()
    {
        if (shapeType == ShapeType.Max)
            return;

        UnloadInsertNewShapeUI();
        currnetGameState = GameState.InGame;
        SettingShape();
        gameManager.GameStart();
        isGameEnd = false;
        gameEnd.SetActive(false);
        title.SetActive(false);
        inGame.SetActive(true);
    }

    void LoadPleaseTakeOutShapeUI()
    {
        pleaseTakeOutShapeUI.SetActive(true);
    }

    void UnloadPleaseTakeOutShapeUI()
    {
        pleaseTakeOutShapeUI.SetActive(false);
    }

    void LoadWrongShapeUI()
    {
        WrongShapeUI.SetActive(true);
        StartCoroutine(CheckingTakeOutShape());
    }

    void UnloadWrongShapeUI()
    {
        WrongShapeUI.SetActive(false);
    }

    IEnumerator CheckingTakeOutShape()
    {
        isCheckingShape = true;

        yield return new WaitForSecondsRealtime(5.0f);

        while (true)
        {
            bool isShapeDetected = false;

            foreach (var sensor in sensors)
            {
                if (sensor)
                {
                    isShapeDetected = true;

                    LoadPleaseTakeOutShapeUI();
                    break;

                }
                
            }

            if (!isShapeDetected)
            {
                if (shapeType == ShapeType.Sphere && !IsTimeOut)
                {
                    UnloadPleaseTakeOutShapeUI();
                    LoadInsertNewShapeUI();
                    isCheckingShape = false;
                    yield break;
                }
                else
                {
                    UnloadPleaseTakeOutShapeUI();
                    LoadTitleUI();
                    yield break;
                }
            }

            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    void CheckArray()
    {
        previousStartSensingIndex = currentStartSensingIndex;
        previousLastSensingIndex = currentLastSensingIndex;

        bool isAnySensorActive = false;
        int Result = -1;

        for (int i = 0; i < sensors.Length; i++)
        {
            if (sensors[i])
            {
                isAnySensorActive = true;
                currentLastSensingIndex = i;
            }
        }

        Result = previousLastSensingIndex - currentLastSensingIndex;

        if (isGameEnd)
            return;

        if (currnetGameState == GameState.Title && isAnySensorActive)
        {
            UnloadInsertNewShapeUI();
            LoadInGameUI();
        }
        else if (currnetGameState == GameState.InGame)
        {
            if (Result != 0)
            {
                shape.UpdateRotateAndLocation(Result);
            }
        }
        else if (insertNewShapeUI.activeSelf && isAnySensorActive)
        {
            LoadInGameUI();
        }
    }

    void SettingShape()
    {
        if (shapeType == ShapeType.Sphere)
        {
            if (currentLastSensingIndex == RectIndex)
                shapeType = ShapeType.Rect;
            else if (currentLastSensingIndex >= HexagonIndex)
                shapeType = ShapeType.Hexagon;
            else
            {
                shapeType = ShapeType.Max;
                LoadWrongShapeUI();
            }
        }
        else if (shapeType == ShapeType.None)
        {
            shapeType = ShapeType.Sphere;
        }
    }

    void InitializeSerialPort()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate)
            {
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
                ReadTimeout = 500,
                WriteTimeout = 500
            };

            serialPort.Open();
            Debug.Log("Serial port opened successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to open serial port: {e.Message}");
        }
    }

    async Task StartListening()
    {
        while (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                if (serialPort.BytesToRead >= receivedBytes.Length)
                {
                    // �񵿱�� ������ �б�
                    await Task.Run(() => serialPort.Read(receivedBytes, 0, receivedBytes.Length));
                    SaveData(receivedBytes); // ������ ����
                }
                else
                {
                    await Task.Delay(50); // �����͸� ��ٸ��� ���� CPU ��뷮 �ּ�ȭ
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error reading from serial port: {e.Message}");
                break; // ���� �߻� �� ���� ����
            }
        }
    }

    void SaveData(byte[] data)
    {
        if (data == null || data.Length < 9)
        {
            Debug.LogError("���ŵ� ������ ����(�����Ͱ� ª���ϴ�.)");
            return;
        }

        Array.Copy(data, latestData, data.Length);

        if (latestData[0] == 0xFA)
        {
            int bitIndex = 0;

            for (int i = 1; i < 9; i++)
            {
                if (i == 1 || i == 4 || i == 7)
                {
                    for (int j = 7; j >= 0; j--)
                    {
                        bool bit = (latestData[i] & (1 << j)) != 0;

                        if (bitIndex < sensors.Length)
                        {
                            sensors[bitIndex] = bit;

                            if (bit && bitIndex < currentStartSensingIndex)
                            {
                                currentStartSensingIndex = bitIndex;
                            }
                        }

                        bitIndex++;
                    }
                }
            }

            CheckArray();
        }
    }
}
