using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DICOMRangeVolume))]
public class DICOMRangeVolumeEditor : Editor
{
    SerializedProperty ctVolume;
    SerializedProperty volumeTexture;
    SerializedProperty slicePosition;
    SerializedProperty thickness;
    SerializedProperty rangeCube;


    void OnEnable()
    {
        ctVolume =
            serializedObject.FindProperty("ctVolume");

        volumeTexture =
            serializedObject.FindProperty("volumeTexture");

        slicePosition =
            serializedObject.FindProperty("slicePosition");

        thickness =
            serializedObject.FindProperty("thickness");

        rangeCube =
            serializedObject.FindProperty("rangeCube");
    }


    public override void OnInspectorGUI()
    {
        serializedObject.Update();


        // =================================
        // CT設定
        // =================================

        EditorGUILayout.LabelField(
            "CT設定",
            EditorStyles.boldLabel
        );

        EditorGUILayout.PropertyField(
            ctVolume,
            new GUIContent("CTモデル")
        );

        EditorGUILayout.PropertyField(
            volumeTexture,
            new GUIContent("Texture3D")
        );


        EditorGUILayout.Space(10);


        // =================================
        // Z方向
        // =================================

        EditorGUILayout.LabelField(
            "Z方向の範囲指定",
            EditorStyles.boldLabel
        );


        int maxSlice =
            volumeTexture.objectReferenceValue != null
                ? ((Texture3D)volumeTexture.objectReferenceValue).depth - 1
                : 550;


        maxSlice = Mathf.Max(maxSlice, 1);


        // ---------------------------------
        // 厚み
        // ---------------------------------

        int currentThickness =
            thickness.intValue;


        currentThickness =
            EditorGUILayout.IntField(
                "厚み",
                currentThickness
            );


        currentThickness =
            Mathf.Clamp(
                currentThickness,
                1,
                maxSlice
            );


        thickness.intValue =
            currentThickness;


        EditorGUILayout.Space(5);


        // ---------------------------------
        // 中心座標
        // ---------------------------------

        int halfThickness =
            currentThickness / 2;


        int minCenter =
            halfThickness;

        int maxCenter =
            maxSlice - halfThickness;


        if (minCenter > maxCenter)
        {
            minCenter = 0;
            maxCenter = maxSlice;
        }


        int currentPosition =
            slicePosition.intValue;


        currentPosition =
            Mathf.Clamp(
                currentPosition,
                minCenter,
                maxCenter
            );


        // =================================
        // 範囲スライダー
        // =================================

        EditorGUILayout.LabelField(
            "Z座標"
        );


        currentPosition =
            EditorGUILayout.IntSlider(
                currentPosition,
                minCenter,
                maxCenter
            );


        slicePosition.intValue =
            currentPosition;


        // =================================
        // 範囲を表示
        // =================================

        int rangeStart =
            Mathf.Max(
                0,
                currentPosition - halfThickness
            );


        int rangeEnd =
            Mathf.Min(
                maxSlice,
                currentPosition + halfThickness
            );


        EditorGUILayout.Space(5);


        EditorGUILayout.HelpBox(
            "選択範囲 : " +
            rangeStart +
            " ～ " +
            rangeEnd +
            "\n中心 : " +
            currentPosition +
            "\n厚み : " +
            (rangeEnd - rangeStart),
            MessageType.Info
        );


        EditorGUILayout.Space(10);


        // =================================
        // 範囲表示Cube
        // =================================

        EditorGUILayout.LabelField(
            "範囲表示",
            EditorStyles.boldLabel
        );


        EditorGUILayout.PropertyField(
            rangeCube,
            new GUIContent("Range Cube")
        );


        serializedObject.ApplyModifiedProperties();
    }
}