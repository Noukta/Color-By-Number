//#define UAS
#define CHUPA
//#define SMA

using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SellReadMe))]
public class SellReadMeInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("1. Edit Game Settings (Admob, In-app Purchase..)", EditorStyles.boldLabel);

        if (GUILayout.Button("Edit Game Settings", GUILayout.MinHeight(40)))
        {
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/MyCombo/GameMaster.prefab");
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("2. Game Documentation", EditorStyles.boldLabel);

        if (GUILayout.Button("Open Full Documentation", GUILayout.MinHeight(40)))
        {
            Application.OpenURL("https://drive.google.com/open?id=1_YZ74zQrpN6ost-POM7IMUr-BeFuuc-IqQ8Wd-APb68");
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Add more pixel images", GUILayout.MinHeight(40)))
        {
            Application.OpenURL("https://www.youtube.com/watch?v=zmxYr_FHoOg");
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("How to convert color images to pixel images", GUILayout.MinHeight(40)))
        {
            Application.OpenURL("https://www.youtube.com/watch?v=GM0x7cAZBMM");
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Load more pixel images from server", GUILayout.MinHeight(40)))
        {
            Application.OpenURL("https://drive.google.com/open?id=16mCJyivzlVqtGrKxAMwPb24eurbTowdi93T2bC4XhL8");
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Setup In-app Purchase Guide", GUILayout.MinHeight(40)))
        {
            Application.OpenURL("https://drive.google.com/open?id=1hcB7gxL-DYy12VOA-h78Xl5FshwM7jhRcjzGQnL6BJw");
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Build For iOS Guide", GUILayout.MinHeight(40)))
        {
            Application.OpenURL("https://drive.google.com/open?id=1rkgXuyFlJ2BhyNZkcn5ASuHunNExDwW5ypmFdXcd0uA");
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("3. My Other Great Source Codes", EditorStyles.boldLabel);
        if (GUILayout.Button("Knife Hit", GUILayout.MinHeight(30)))
        {
#if UAS
            Application.OpenURL("https://www.chupamobile.com/unity-arcade/knife-hit-unity-clone-20090");
#elif CHUPA
            Application.OpenURL("https://www.chupamobile.com/unity-arcade/knife-hit-unity-clone-20090");
#elif SMA
            Application.OpenURL("https://www.sellmyapp.com/downloads/knife-hit-unity-clone/");
#endif
        }

        EditorGUILayout.Space();

        EditorGUILayout.LabelField("4. Contact Us For Support", EditorStyles.boldLabel);
        EditorGUILayout.TextField("Email: ", "moana.gamestudio@gmail.com");
    }
}