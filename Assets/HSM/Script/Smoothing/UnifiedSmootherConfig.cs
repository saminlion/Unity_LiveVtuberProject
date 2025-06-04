using UnityEngine;

[CreateAssetMenu(fileName = "SmootherConfig", menuName = "Live2D/SmootherConfig", order = 1)]
public class UnifiedSmootherConfig : ScriptableObject
{
    public string live2DParamId; // 예: PARAM_EYE_L_OPEN
    public float factor = 0.15f;
    public float deadzone = 0.01f;
    public float snapThreshold = 0.2f;
}
