using System.Collections.Generic;

public static class FaceCapToLive2DParamMap
{
    public static readonly Dictionary<string, string> Map = new()
    {
        { "mouthOpen", "PARAM_MOUTH_OPEN_Y" },
        { "headYaw", "PARAM_ANGLE_X" },
        { "headPitch", "PARAM_ANGLE_Y" },
        { "headRoll", "PARAM_ANGLE_Z" },

        { "eyeBlink_L", "PARAM_EYE_L_OPEN" },
        { "eyeBlink_R", "PARAM_EYE_R_OPEN" },

        { "mouthSmile_L", "PARAM_MOUTH_FORM" },
        { "mouthSmile_R", "PARAM_MOUTH_FORM" },

        { "cheekPuff", "PARAM_CHEEK" },
        
        { "browDown_L", "PARAM_BROW_L_Y" },
        { "browDown_R", "PARAM_BROW_R_Y" },
        { "browInnerUp", "PARAM_BROW_L_Y" }, // + R_Y 혼합 가능
    };
}
