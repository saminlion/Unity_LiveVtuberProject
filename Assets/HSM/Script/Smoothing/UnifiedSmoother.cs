using UnityEngine;

public class UnifiedSmoother : IParameterSmoother
{
    private float factor, deadzone, snapThreshold;
    private float last;

    public UnifiedSmoother(float factor, float deadzone = 0f, float snapThreshold = 1f)
    {
        this.factor = factor;
        this.deadzone = deadzone;
        this.snapThreshold = snapThreshold;
    }

    public float Apply(float input)
    {
        float delta = Mathf.Abs(input - last);

        // 급격한 변화는 그대로 반영 (snap)
        if (delta > snapThreshold)
        {
            last = input;
            return last;
        }

        // 너무 작은 변화는 무시 (지터 방지)
        if (delta < deadzone)
        {
            return last;
        }

        // 보간 처리 (EMA)
        last = (1f - factor) * input + factor * last;
        return last;
    }
}
