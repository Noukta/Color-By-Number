using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class MoanaWindowEditor
{
    [MenuItem("Moana Games/Clear all playerprefs")]
    static void ClearAllPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    [MenuItem("Moana Games/Set all images painted")]
    static void SetPainted()
    {
        for (int i = 1; i <= 100; i++)
        {
            PlayerPrefs.SetString("item_status_" + i, "complete");
        }
        PlayerPrefs.Save();
    }

    [MenuItem("Moana Games/Set all images unpainted")]
    static void SetUnPainted()
    {
        for (int i = 1; i <= 100; i++)
        {
            PlayerPrefs.DeleteKey("item_status_" + i);
        }
        PlayerPrefs.Save();
    }
}