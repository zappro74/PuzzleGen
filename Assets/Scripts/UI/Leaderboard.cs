using UnityEngine;

public static class Leaderboard
{
    public static void SubmitTime(GameMode mode, float time)
    {
        string key = GetKey(mode);
        float current = GetBestTime(mode);

        if (current <= 0f || time < current)
        {
            PlayerPrefs.SetFloat(key, time);
            PlayerPrefs.Save();
        }
    }

    public static float GetBestTime(GameMode mode)
    {
        return PlayerPrefs.GetFloat(GetKey(mode), -1f);
    }

    public static string GetBestTimeFormatted(GameMode mode)
    {
        float time = GetBestTime(mode);

        if (time < 0f) return "--:--";

        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);

        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public static void ClearAll()
    {
        foreach (GameMode mode in System.Enum.GetValues(typeof(GameMode)))
        {
            PlayerPrefs.DeleteKey(GetKey(mode));
        }
        PlayerPrefs.Save();
    }

    private static string GetKey(GameMode mode)
    {
        return $"BestTime_{mode}";
    }
}