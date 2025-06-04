using UnityEngine;
using UniVRM10;
using BaseController;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class VRMCharacterController : CharacterControllerBase
{
    [Header("Neck Rotation")]
    public float neckSmoothSpeed = 10f;

    private float yaw;
    private float pitch;
    private float roll;

    private Quaternion currentNeckRotation = Quaternion.identity;
    private Quaternion targetNeckRotation = Quaternion.identity;

    [Header("Jaw Rotation")]
    public float jawSmoothSpeed = 10f;

    public bool isUseJawOnly = false;
    private Quaternion currentJawRotation = Quaternion.identity;
    private Quaternion targetJawRotation = Quaternion.identity;
[SerializeField]
    private float smoothedMouthOpen;
    private float smoothedMouthOpenWJaw;

    [Header("VRM Settings")]
    private Vrm10Instance inst;
    public GameObject vrmRoot;
    private Animator animator;
    private Vrm10RuntimeLookAt lookAt;
    private Vrm10RuntimeControlRig controlRig;
    private UniHumanoid.Humanoid h;

    private Dictionary<string, UnifiedSmoother> smoothers = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void Initialize()
    {
        vrmRoot ??= GetComponentInChildren<Vrm10Instance>()?.gameObject;
        inst = vrmRoot?.GetComponent<Vrm10Instance>();

        if (inst == null)
        {
            Debug.LogError("VRM 로드 실패");
            return;
        }

        controlRig = inst.Runtime?.ControlRig;

        if (controlRig == null)
        {
            Debug.LogError("❌ ControlRig 생성 안 됨");
            return;
        }

        currentNeckRotation = targetNeckRotation = Quaternion.identity;
        currentJawRotation = targetJawRotation = Quaternion.identity;

        lookAt = inst.Runtime.LookAt;

        animator = vrmRoot.GetComponent<Animator>();

        SetupAnimator();
    }

    void SetupAnimator()
    {
        if (animator == null) return;

        if (animator.runtimeAnimatorController == null)
        {
            var controller = Resources.Load<RuntimeAnimatorController>("Anim/VRMSample");
            if (controller != null)
            {
                animator.runtimeAnimatorController = controller;
            }
            else
            {
                Debug.LogWarning("⚠️ VRM AnimatorController가 설정되지 않았습니다.");
            }
        }
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
    }

    // Update is called once per frame
    void Update()
    {
        ApplyParameters();

        UpdateLookAt();

        UpdateNeckRotation();

        UpdateJawRotation();

        UpdateAnimation();
    }

    void UpdateLookAt()
    {
        // Set VRM LookAt (눈 방향 계산)
        lookAt.SetYawPitchManually(yaw, pitch);
    }

    void UpdateNeckRotation()
    {
        // 머리 회전 제어
        if (controlRig == null)
        {
            Debug.LogError("No ControlRig At Update");
        }

        else
        {
            var neck = controlRig.GetBoneTransform(HumanBodyBones.Neck);
            if (neck != null)
            {
                currentNeckRotation = Quaternion.Slerp(
                    currentNeckRotation,
                    targetNeckRotation,
                    Time.deltaTime * neckSmoothSpeed
                );

                neck.localRotation = currentNeckRotation;
            }
        }
    }

    void UpdateJawRotation()
    {
        if (controlRig == null) return;

        var jaw = controlRig.GetBoneTransform(HumanBodyBones.Jaw);

        if (jaw != null)
        {
            if (isUseJawOnly)
            {
                // 표정 끄고 Jaw 본으로만 립싱크
                currentJawRotation = Quaternion.Slerp(currentJawRotation, targetJawRotation, Time.deltaTime * jawSmoothSpeed);

                jaw.localRotation = currentJawRotation;
            }
            else
            {
                // 본 회전은 끄고, 표정으로만 립싱크
                jaw.localRotation = Quaternion.identity;
                inst.Runtime.Expression.SetWeight(ExpressionKey.Aa, smoothedMouthOpen);
            }
        }
    }

    void UpdateAnimation()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            animator.SetTrigger("Hello");
        }

            animator.SetFloat("Surprise", smoothedMouthOpen);

    }

    protected override void ApplyParameter(string key, float value)
    {
        if (inst == null) return;

        float smoothedValue = GetSmoothedValue(key, value);

        // 표정 처리
        if (FaceCapToVRMParamMap.Map.TryGetValue(key, out var preset))
        {
            var expressionKey = ExpressionKey.CreateFromPreset(preset);
            inst.Runtime.Expression.SetWeight(expressionKey, smoothedValue);
            if (expressionKey.Equals(ExpressionKey.Aa))
            {
                smoothedMouthOpen = smoothedValue;
            }
            return;
        }

        // 회전값 누적
        switch (key.ToLower())
        {
            case "headyaw": yaw = smoothedValue; break;
            case "headpitch": pitch = smoothedValue; break;
            case "headroll": roll = smoothedValue; break;
            case "mouthOpen": smoothedMouthOpenWJaw = smoothedValue; break;
        }

        targetNeckRotation = Quaternion.Euler(pitch, yaw, roll);
        targetJawRotation = Quaternion.Euler(smoothedMouthOpenWJaw * 20f, 0, 0); // 보간된 입벌림 각도
    }

    float GetSmoothedValue(string key, float value)
    {
        // 보간기 준비
        if (!smoothers.TryGetValue(key, out var smoother))
        {
            // 기본값으로 보간기 생성
            if (key.StartsWith("eyeBlink"))
            {
                smoother = new UnifiedSmoother(factor: 0.8f, deadzone: 0.001f, snapThreshold: 0.3f);
            }
            else
            {
                smoother = new UnifiedSmoother(factor: 0.15f, deadzone: 0.01f, snapThreshold: 0.2f);
            }

            smoothers[key] = smoother;
        }

        return smoother.Apply(value);
    }

    void SetExpression(ExpressionKey Key)
    {
        foreach (var k in new[] {
            ExpressionKey.Aa,
            ExpressionKey.Angry,
            ExpressionKey.Blink,
            ExpressionKey.BlinkLeft,
            ExpressionKey.BlinkRight,
            ExpressionKey.Ee,
            ExpressionKey.Happy,
            ExpressionKey.Ih,
            ExpressionKey.LookDown,
            ExpressionKey.LookLeft,
            ExpressionKey.LookRight,
            ExpressionKey.LookUp,
            ExpressionKey.Neutral,
            ExpressionKey.Oh,
            ExpressionKey.Ou,
            ExpressionKey.Relaxed,
            ExpressionKey.Sad,
            ExpressionKey.Surprised
        })
        {
            inst.Runtime.Expression.SetWeight(k, 0f);
        }

        inst.Runtime.Expression.SetWeight(Key, 1f);
    }
}
