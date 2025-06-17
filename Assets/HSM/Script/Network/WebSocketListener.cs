using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Networking;

using NativeWebSocket;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Zenject;
using UniRx;
using Cysharp.Threading.Tasks;

public class WebSocketListener : MonoBehaviour
{
    [Inject] CharacterManager _characterManager;
    [Inject] TTSQueue _ttsQueue;

    public Subject<string> OnRawMessage = new Subject<string>();

    private WebSocket webSocket;

    private bool shouldReconnect = true;
    private int reconnectDelayMs = 3000; // 3초
    private bool isReconnecting = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
            Debug.Log("WebSocketListener.Start() _ttsQueue=" + (_ttsQueue == null ? "NULL" : "OK"));

        webSocket = new WebSocket("ws://localhost:8080");

        ConnectWebSocketAsync().Forget();

        OnRawMessage
            .ObserveOnMainThread()
            .Subscribe(HandleRawMessage)
            .AddTo(this);
    }

    private async UniTask ConnectWebSocketAsync()
    {
        RegisterWebSocketEvents();
        
        await webSocket.Connect();
    }

    private async UniTask DownloadAndPlayAudio(string url, string userId, string text)
    {
        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV);
        await www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Audio download failed: " + www.error);
        }
        else
        {
            var clip = DownloadHandlerAudioClip.GetContent(www);

            _ttsQueue.Enqueue(userId, text, clip); // ❗ 여기서 큐에 전달

            Debug.Log("🔊 오디오 큐 등록 완료");
        }
    }

    private async UniTaskVoid TryReconnect()
    {
        if (isReconnecting || !shouldReconnect) return;

        isReconnecting = true;

        Debug.Log("🔄 WebSocket 재연결 시도 중...");

        while (webSocket == null || webSocket.State != WebSocketState.Open)
        {
            try
            {
                webSocket = new WebSocket("ws://localhost:8080");

                RegisterWebSocketEvents(); // 이벤트 재등록

                await webSocket.Connect();

                Debug.Log("✅ WebSocket 재연결 성공");
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"WebSocket 재연결 실패: {ex.Message}");
            }

            if (webSocket.State == WebSocketState.Open) break;

            await UniTask.Delay(reconnectDelayMs);
        }

        isReconnecting = false;
    }

    private void HandleRawMessage(string json)
    {
        try
        {
            var parsed = JObject.Parse(json);
            Debug.Log($"📩 수신된 메시지 원문:\n{json}");

            var userId = parsed["userId"]?.ToString();
            var audioUrl = parsed["audioUrl"]?.ToString();
            var paramObj = parsed["parameters"] as JObject;
            var vrmPath = parsed["vrmPath"]?.ToString(); // Optional

            if (string.IsNullOrEmpty(userId) || paramObj == null)
            {
                Debug.LogWarning("❗ [WS] Invalid data: userId or parameters missing");
                return;
            }

            var paramDict = new Dictionary<string, float>();
            foreach (var pair in paramObj)
            {
                if (pair.Key == null || pair.Value == null)
                    continue;

                var key = pair.Key.Trim();
                var valueToken = pair.Value;

                if (key.Equals("mouthOpen", StringComparison.OrdinalIgnoreCase))
                {
                    if (!string.IsNullOrEmpty(audioUrl))
                    {
                        string text = parsed["text"]?.ToString() ?? "(no text)";
                        DownloadAndPlayAudio(audioUrl, userId, text).Forget(); // ✅ userId, text 전달
                        continue;
                    }
                }

                if (float.TryParse(valueToken.ToString(), out var value))
                {
                    paramDict[key] = value;
                }
            }

            _characterManager.ApplyUserFaceAndInput(userId, paramDict, vrmPath);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("❌ JSON Parse Error: " + ex.Message);
        }
    }

    private void RegisterWebSocketEvents()
    {
        webSocket.OnOpen += () =>
        {
            Debug.Log("WebSocket connected");

            // ✅ 서버에게 '준비 완료' 메시지 전송
            var readyPayload = new { ready = true, userId = "user_b" };
            string message = JsonConvert.SerializeObject(readyPayload);
            webSocket.SendText(message);
        };

        webSocket.OnMessage += (bytes) =>
        {
            var json = Encoding.UTF8.GetString(bytes).Trim('\0', ' ', '\n', '\r');
    Debug.Log("수신된 원문: " + json); // 반드시 출력

            OnRawMessage.OnNext(json); // Message to Rx Stream            
        };

        webSocket.OnError += (e) =>
        {
            Debug.LogError("WebSocket Error: " + e);
            TryReconnect().Forget();
        };

        webSocket.OnClose += (e) =>
        {
            Debug.Log("WebSocket Closed: " + e);
            TryReconnect().Forget();
        };
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        webSocket.DispatchMessageQueue();
#endif
    }

    private async void OnApplicationQuit()
    {
        await webSocket.Close();

        OnRawMessage?.Dispose();
    }
}
