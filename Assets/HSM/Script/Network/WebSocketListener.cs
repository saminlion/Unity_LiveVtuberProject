using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.Networking;

using NativeWebSocket;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public class WebSocketListener : MonoBehaviour
{
    private CharacterManager characterManager;

    private WebSocket webSocket;

    private bool shouldReconnect = true;
    private int reconnectDelayMs = 3000; // 3초
    private bool isReconnecting = false;

    void Awake()
    {
        characterManager = GetComponent<CharacterManager>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        webSocket = new WebSocket("ws://localhost:8080");
        RegisterWebSocketEvents();
        await webSocket.Connect();
    }

    System.Collections.IEnumerator DownloadAndPlayAudio(string url, string userId, string text)
    {
        using UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Audio download failed: " + www.error);
        }
        else
        {
            var clip = DownloadHandlerAudioClip.GetContent(www);
            TTSQueue.Instance.Enqueue(userId, text, clip); // ❗ 여기서 큐에 전달
            Debug.Log("🔊 오디오 큐 등록 완료");
        }
    }

    private async void TryReconnect()
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

            await Task.Delay(reconnectDelayMs);
        }

        isReconnecting = false;
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

            try
            {
                var parsed = JObject.Parse(json);
                //Debug.Log($"📩 수신된 메시지 원문:\n{json}");

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
                            StartCoroutine(DownloadAndPlayAudio(audioUrl, userId, text)); // ✅ userId, text 전달
                            continue;
                        }
                    }

                    if (float.TryParse(valueToken.ToString(), out var value))
                    {
                        paramDict[key] = value;
                    }
                }

                characterManager.ApplyUserFaceAndInput(userId, paramDict, vrmPath);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("❌ JSON Parse Error: " + ex.Message);
            }
        };

        webSocket.OnError += (e) =>
        {
            Debug.LogError("WebSocket Error: " + e);
            TryReconnect();
        };

        webSocket.OnClose += (e) =>
        {
            Debug.Log("WebSocket Closed: " + e);
            TryReconnect();
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
    }
}
