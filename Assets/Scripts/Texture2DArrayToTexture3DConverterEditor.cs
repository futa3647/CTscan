using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

[CustomEditor(typeof(Texture2DArrayToTexture3DConverter))]
public class Texture2DArrayToTexture3DConverterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var converter =
            (Texture2DArrayToTexture3DConverter)target;

        GUILayout.Space(10);

        if (GUILayout.Button("DICOMを自動登録"))
        {
            string folder =
                EditorUtility.OpenFolderPanel(
                    "DICOMフォルダを選択",
                    Application.dataPath,
                    "");

            if (!string.IsNullOrEmpty(folder))
            {
                string[] files =
                    Directory.GetFiles(
                        folder,
                        "*.dcm")
                    .OrderBy(x => x)
                    .ToArray();

                converter.texture2DArray.Clear();

                foreach (string file in files)
                {
                    string assetPath =
                        "Assets" +
                        file.Substring(
                            Application.dataPath.Length);

                    Object[] assets =
                        AssetDatabase.LoadAllAssetsAtPath(
                            assetPath);

                    Texture2D tex =
                        assets
                        .OfType<Texture2D>()
                        .FirstOrDefault();

                    if (tex != null)
                    {
                        converter.texture2DArray.Add(tex);
                    }
                }

                EditorUtility.SetDirty(converter);

                Debug.Log(
                    "DICOM登録完了: " +
                    converter.texture2DArray.Count +
                    "枚");
            }
        }

        if (GUILayout.Button("Convert"))
        {
            Convert(converter);
        }
    }

    void Convert(
        Texture2DArrayToTexture3DConverter converter)
    {
        if (converter.texture2DArray == null ||
            converter.texture2DArray.Count == 0)
        {
            Debug.LogError(
                "Texture2Dが登録されていません");

            return;
        }

        List<Texture2D> textures =
            new List<Texture2D>(
                converter.texture2DArray);

        // スライス順を反転
        textures.Reverse();

        int width =
            textures[0].width;

        int height =
            textures[0].height;

        TextureFormat format =
            textures[0].format;

        // 全Texture2Dのサイズ・Formatを確認
        foreach (Texture2D tex in textures)
        {
            if (tex.width != width ||
                tex.height != height ||
                tex.format != format)
            {
                Debug.LogError(
                    "Texture2DのサイズまたはFormatが一致しません");

                return;
            }
        }

        // DICOMの枚数
        int depth =
            textures.Count;

        // Texture3D用のPixel配列
        Color32[] colors =
            new Color32[
                width *
                height *
                depth
            ];

        // Texture2DをTexture3Dへコピー
        for (int z = 0; z < depth; z++)
        {
            Color32[] slice =
                textures[z].GetPixels32();

            System.Array.Copy(
                slice,
                0,
                colors,
                z * width * height,
                slice.Length);
        }

        // Texture3Dを作成
        //
        // X = Texture2Dの横
        // Y = Texture2Dの縦
        // Z = DICOMのスライス方向
        Texture3D texture3D =
            new Texture3D(
                width,
                height,
                depth,
                format,
                false);

        texture3D.SetPixels32(colors);
        texture3D.Apply();

        string savePath =
            "Assets/VolumeTexture3D.asset";

        // 既存のTexture3Dがあれば削除
        if (File.Exists(savePath))
        {
            AssetDatabase.DeleteAsset(savePath);
        }

        // Assetとして保存
        AssetDatabase.CreateAsset(
            texture3D,
            savePath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "Texture3D作成完了: " +
            savePath);
    }
}