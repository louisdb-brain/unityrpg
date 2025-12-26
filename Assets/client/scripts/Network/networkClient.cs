using System;
using UnityEngine;
using NativeWebSocket;

public class NetworkClient : MonoBehaviour
{
    public static NetworkClient Instance; 
    
    private WebSocket ws;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    async void Start()
    {
        ws = new WebSocket("ws://localhost:3000");
        

        ws.OnMessage += (bytes) =>
        {
            string json = System.Text.Encoding.UTF8.GetString(bytes);
            NetworkMessageRouter.Handle(json);
        };


        await ws.Connect();
        Debug.Log("Connected to server!");
    }
    public async void Send(string type, object payload)
    {
        NetworkPacket packet = new NetworkPacket
        {
            type = type,
            data = payload != null ? JsonUtility.ToJson(payload) : "{}"
        };

        string json = JsonUtility.ToJson(packet);
        
        await ws.SendText(json);
    }




    void Update()
    {
        if (ws != null)
        {
            ws.DispatchMessageQueue();
        }
        

        
    }


    private async void OnApplicationQuit()
    {
        await ws.Close();
    }
}