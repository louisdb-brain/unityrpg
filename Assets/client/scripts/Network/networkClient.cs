using System;
using UnityEngine;
using NativeWebSocket;

public class NetworkClient : MonoBehaviour
{
    private WebSocket ws;

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