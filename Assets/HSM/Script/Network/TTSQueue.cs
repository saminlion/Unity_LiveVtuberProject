using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TTSQueue : MonoBehaviour
{
    public static TTSQueue Instance { get; private set; }

    public bool isJsonLipSync = false;

    private class TTSEntry
    {
        public string userId;
        public string text;
        public AudioClip clip;
        public float[] jsonFrames;
    }

    private readonly Queue<TTSEntry> queue = new();
    public AudioSource audioSource;
    private TTSEntry currentEntry;
    private float[] samples = new float[512];

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // ✅ GameObject 자체 제거
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // ✅ 씬 전환 시에도 유지
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = 0.3f;

    }

    public void Enqueue(string userId, string text, AudioClip clip = null, string jsonUrl = null)
    {
        if (clip == null)
        {
            clip = Resources.Load<AudioClip>("tts_output");
            if (clip == null)
            {
                Debug.LogWarning("TTS 오디오 클립이 존재하지 않음. Resources/tts_output.wav 필요");
                return;
            }
        }

        var entry = new TTSEntry { userId = userId, text = text, clip = clip };

        queue.Enqueue(entry);

        Debug.Log($"🗣️ TTS 립싱크 큐 등록: \"{text}\"");

        if (isJsonLipSync && !string.IsNullOrEmpty(jsonUrl))
        {
            StartCoroutine(DownloadJsonFrames(jsonUrl, entry));
        }
    }

    void Update()
    {
        if (currentEntry == null && queue.Count > 0)
        {
            currentEntry = queue.Dequeue();
            audioSource.clip = currentEntry.clip;
            audioSource.Play();
            Debug.Log($"▶️ 오디오 재생 시작: {currentEntry.text}");
        }

        if (currentEntry == null || !audioSource.isPlaying) return;

        CharacterManager mgr = GetComponent<CharacterManager>();
        if (mgr == null)
        {
            Debug.LogError($"캐릭터 매니져 못 찾음");
            return;
        }

        float mouth = 0f;

        if (isJsonLipSync && currentEntry.jsonFrames != null)
        {
            int index = audioSource.timeSamples / 512;
            if (index < currentEntry.jsonFrames.Length)
            {
                mouth = Mathf.Clamp01(currentEntry.jsonFrames[index]);
            }
        }

        else
        {
            audioSource.GetOutputData(samples, 0);
            float volume = GetRMSVolume(samples);
            mouth = Mathf.Clamp01(volume * 20f);
        }


        var param = new Dictionary<string, float> { { "mouthOpen", mouth } };
        mgr.ApplyUserFaceAndInput(currentEntry.userId, param);

        if (!audioSource.isPlaying && audioSource.time > 0)
        {
            Debug.Log($"🔇 TTS 립싱크 종료: {currentEntry.userId}");
            currentEntry = null;
        }
    }

    IEnumerator DownloadJsonFrames(string url, TTSEntry entry)
    {
        using UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning("립싱크 JSON 다운로드 실패: " + www.error);
            yield break;
        }

        var text = www.downloadHandler.text;
        var json = JsonUtility.FromJson<FrameData>(text);
        entry.jsonFrames = json.frames;
    }

    [System.Serializable]
    private class FrameData
    {
        public float[] frames;
    }

    float GetRMSVolume(float[] data)
    {
        float sum = 0f;
        for (int i = 0; i < data.Length; i++)
        {
            sum += data[i] * data[i];
        }
        return Mathf.Sqrt(sum / data.Length);
    }
}
