using System.Collections.Generic;
using UniVRM10;

public static class FaceCapToVRMParamMap
{
    public static readonly Dictionary<string, ExpressionPreset> Map = new()
    {
            // 표정 관련
            { "eyeBlink_L", ExpressionPreset.blinkLeft },
            { "eyeBlink_R", ExpressionPreset.blinkRight },
            { "mouthOpen", ExpressionPreset.aa }, // 립싱크 파라미터 (Aa/Ee/Ih/Oh/Ou)

            { "mouthSmile_L", ExpressionPreset.happy },
            { "mouthSmile_R", ExpressionPreset.happy },

            { "browDown_L", ExpressionPreset.angry },
            { "browDown_R", ExpressionPreset.angry },

            // 필요 시 추가
            { "eyeSquint_L", ExpressionPreset.relaxed },
            { "eyeSquint_R", ExpressionPreset.relaxed },
            { "eyeWide_L", ExpressionPreset.surprised },
            { "eyeWide_R", ExpressionPreset.surprised },
    };
}