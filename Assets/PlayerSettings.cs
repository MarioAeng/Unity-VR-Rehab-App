using UnityEngine;

public static class PlayerSettings
{
    private const string HandednessKey = "PlayerIsLeftHanded";

    public static bool IsLeftHanded
    {
        get => PlayerPrefs.GetInt(HandednessKey, 0) == 1;
        set => PlayerPrefs.SetInt(HandednessKey, value ? 1 : 0);
    }
}