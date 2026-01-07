using UnityEngine;

public static class SaveService
{
    const string Key = "active_session_v1";

    const string RowsKey = "active_session_rows_v1";
    const string ColsKey = "active_session_cols_v1";

    public static bool HasActive()
    {
        return PlayerPrefs.HasKey(Key) && PlayerPrefs.HasKey(RowsKey) && PlayerPrefs.HasKey(ColsKey);
    }

    public static void Save(int rows, int cols, SessionSaveData data)
    {
        if (data == null)
            return;

        PlayerPrefs.SetInt(RowsKey, rows);
        PlayerPrefs.SetInt(ColsKey, cols);
        PlayerPrefs.SetString(Key, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public static bool TryLoad(out int rows, out int cols, out SessionSaveData data)
    {
        rows = 0;
        cols = 0;
        data = null;

        if (!HasActive())
            return false;

        rows = PlayerPrefs.GetInt(RowsKey, 0);
        cols = PlayerPrefs.GetInt(ColsKey, 0);

        var json = PlayerPrefs.GetString(Key, string.Empty);
        if (string.IsNullOrEmpty(json))
            return false;

        data = JsonUtility.FromJson<SessionSaveData>(json);
        return data != null;
    }

    public static void Clear()
    {
        if (PlayerPrefs.HasKey(Key)) PlayerPrefs.DeleteKey(Key);
        if (PlayerPrefs.HasKey(RowsKey)) PlayerPrefs.DeleteKey(RowsKey);
        if (PlayerPrefs.HasKey(ColsKey)) PlayerPrefs.DeleteKey(ColsKey);
    }
}
