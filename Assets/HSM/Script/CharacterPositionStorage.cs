using UnityEngine;

public static class CharacterPositionStorage
{
    public static void SavePosition(string userId, Vector3 position)
    {
        PlayerPrefs.SetFloat($"{userId}_x", position.x);
        PlayerPrefs.SetFloat($"{userId}_y", position.y);
        PlayerPrefs.SetFloat($"{userId}_z", position.z);
    }
    public static Vector3 LoadPosition(string userId, Vector3 defaultPos)
    {
        if (!PlayerPrefs.HasKey($"{userId}_x"))
        {
            Debug.Log($"No Key Check UserId : {userId}");
            return defaultPos;
        }

        float x = PlayerPrefs.GetFloat($"{userId}_x");
        float y = PlayerPrefs.GetFloat($"{userId}_y");
        float z = PlayerPrefs.GetFloat($"{userId}_z");

        Vector3 newPos = new Vector3(x, y, x);

        Debug.Log($"Check UserId and Pos : {userId} / {newPos}");

        return newPos;
    }
}
