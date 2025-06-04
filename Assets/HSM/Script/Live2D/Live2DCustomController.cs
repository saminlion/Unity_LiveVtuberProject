using UnityEngine;
using Live2D.Cubism.Core;
using Live2D.Cubism.Framework;
using Live2D.Cubism.Framework.Expression;
using Live2D.Cubism.Framework.Motion;
using System.Collections.Generic;
using BaseController;
using UnityEngine.AI;

public class Live2DCustomController : CharacterControllerBase
{
    [Header("Live2D 모델 설정")]
    private CubismModel cubismModel;
    private CubismMotionController motionController;
    private Dictionary<string, CubismParameter> parameters = new();

    [Header("보정 설정")]
    public List<UnifiedSmootherConfig> smootherConfigs; //Inspeactor에서 등록
    private Dictionary<string, UnifiedSmoother> smoothers = new();

    [Header("기본 정보")]
    public string spawnCharacterName = "";

    private Dictionary<string, AnimationClip> clipMap = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitModel();
        InitSmoothers();
        LoadGestureClips(); // 모션 클립들 불러오기
    }

    void InitModel()
    {
        cubismModel = GetComponent<CubismModel>();
        cubismModel.GetComponent<CubismParameterStore>().enabled = false;

        motionController = GetComponent<CubismMotionController>() ?? gameObject.AddComponent<CubismMotionController>();

        foreach (var param in cubismModel.Parameters)
        {
            parameters[param.Id] = param;
        }
    }

    void InitSmoothers()
    {
        var allConfigs = Resources.LoadAll<UnifiedSmootherConfig>("SmootherConfigs");

        foreach (var config in allConfigs)
        {
            if (!string.IsNullOrEmpty(config.live2DParamId))
            {
                smoothers[config.live2DParamId] =
                    new UnifiedSmoother(config.factor, config.deadzone, config.snapThreshold);
            }
        }
    }

    void LoadGestureClips()
    {
        if (string.IsNullOrEmpty(spawnCharacterName))
        {
            Debug.LogWarning("⚠️ spawnCharacterName 미지정");
            return;
        }

        //Resources/Motions/캐릭터이름 폴더에 있는 AnimationClip 불러오기
        string path = $"Motions/{spawnCharacterName}";
        var clips = Resources.LoadAll<AnimationClip>(path);

        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning($"⚠️ Resources/Motions/{spawnCharacterName} 내 AnimationClip이 없습니다.");
        }

        foreach (var clip in clips)
        {
            clipMap[clip.name.ToLower()] = clip;
        }
    }

    protected override void ApplyParameter(string key, float value)
    {
        if (!FaceCapToLive2DParamMap.Map.TryGetValue(key, out var modelParam))
            return;

        if (!parameters.TryGetValue(modelParam, out var param))
            return;

        // Flip headYaw / headRoll 방향
        if (key == "headYaw" || key == "headRoll") value *= -1f;
        if (key == "eyeBlink_L" || key == "eyeBlink_R") value = 1.0f - value;

        if (smoothers.TryGetValue(modelParam, out var smoother))
            value = smoother.Apply(value);

        param.Value = value;
    }

    //    LateUpdate is called once per frame
    void LateUpdate()
    {
        ApplyParameters();

        if (Input.GetKeyDown(KeyCode.A))
        {
            PlayGestureMotion("handwave");
        }
    }

    public void PlayGestureMotion(string motionName)
    {
        if (motionController == null)
        {
            Debug.LogWarning("⚠️ CubismMotionController가 없습니다.");
            return;
        }

        if (!clipMap.TryGetValue($"{motionName}".ToLower(), out var clip))
        {
            Debug.LogWarning($"❌ 모션 '{motionName}' 없음");
            return;
        }

        motionController.StopAllAnimation();
        motionController.PlayAnimation(clip, layerIndex: 0, priority: 100, isLoop: false);

        StartCoroutine(WaitAndReturnToIdle(clip.length));
    }

    private System.Collections.IEnumerator WaitAndReturnToIdle(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!clipMap.TryGetValue("idle".ToLower(), out var clip))
        {
            Debug.LogWarning("❌ 모션 idle 없음");
            yield break;
        }

        motionController.StopAllAnimation();
        motionController.PlayAnimation(clip, layerIndex: 0, priority: 1, isLoop: true);
    }

}