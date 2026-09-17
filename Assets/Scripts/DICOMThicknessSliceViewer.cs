using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
public class DICOMThicknessSliceViewer : MonoBehaviour
{
    [Header("CT Texture3D")]
    public Texture3D volumeTexture;

    [Header("表示するRaw Image")]
    public RawImage targetImage;

    [Header("基準スライス")]
    [Range(0, 550)]
    public int slicePosition = 275;

    [Header("厚み")]
    [Min(1)]
    public int thickness = 20;

    private Texture2D displayTexture;

    private int lastSlicePosition = -1;
    private int lastThickness = -1;
    private Texture3D lastVolumeTexture = null;

    void OnEnable()
    {
        UpdateImage();
    }

    void Start()
    {
        UpdateImage();
    }

    void Update()
    {
        if (volumeTexture != lastVolumeTexture ||
            slicePosition != lastSlicePosition ||
            thickness != lastThickness)
        {
            UpdateImage();
        }
    }

    void OnValidate()
    {
        UpdateImage();
    }

    void UpdateImage()
    {
        if (volumeTexture == null)
        {
            return;
        }

        if (targetImage == null)
        {
            return;
        }

        int width = volumeTexture.width;
        int height = volumeTexture.height;
        int depth = volumeTexture.depth;

        if (depth <= 0)
        {
            return;
        }

        // --------------------------------
        // 値を補正
        // --------------------------------

        thickness = Mathf.Clamp(thickness, 1, depth);

        slicePosition = Mathf.Clamp(
            slicePosition,
            0,
            depth - 1
        );

        // --------------------------------
        // 表示範囲を計算
        // --------------------------------

        int halfThickness = thickness / 2;

        int startSlice =
            Mathf.Max(
                0,
                slicePosition - halfThickness
            );

        int endSlice =
            Mathf.Min(
                depth - 1,
                slicePosition + halfThickness
            );

        int sliceCount =
            endSlice - startSlice + 1;

        // --------------------------------
        // Texture3Dのピクセル取得
        // --------------------------------

        Color[] volumePixels =
            volumeTexture.GetPixels();

        // --------------------------------
        // 表示用Texture2Dを作成
        // --------------------------------

        if (displayTexture == null ||
            displayTexture.width != width ||
            displayTexture.height != height)
        {
            if (displayTexture != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(displayTexture);
                else
                    Destroy(displayTexture);
#else
                Destroy(displayTexture);
#endif
            }

            displayTexture = new Texture2D(
                width,
                height,
                TextureFormat.RGBA32,
                false
            );

            displayTexture.filterMode =
                FilterMode.Point;

            displayTexture.wrapMode =
                TextureWrapMode.Clamp;
        }

        // --------------------------------
        // 範囲内のスライスを平均
        // --------------------------------

        Color[] result =
            new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color sum = Color.black;

                for (int z = startSlice;
                     z <= endSlice;
                     z++)
                {
                    int index =
                        z * width * height
                        + y * width
                        + x;

                    sum += volumePixels[index];
                }

                result[
                    y * width + x
                ] = sum / sliceCount;
            }
        }

        // --------------------------------
        // Texture2Dへ反映
        // --------------------------------

        displayTexture.SetPixels(result);
        displayTexture.Apply();

        // --------------------------------
        // Raw Imageへ表示
        // --------------------------------

        targetImage.texture =
            displayTexture;

        // --------------------------------
        // 状態を保存
        // --------------------------------

        lastSlicePosition =
            slicePosition;

        lastThickness =
            thickness;

        lastVolumeTexture =
            volumeTexture;

        Debug.Log(
            "CT断面更新: " +
            "基準=" + slicePosition +
            " / 範囲=" +
            startSlice + "～" + endSlice +
            " / 枚数=" + sliceCount
        );
    }

    void OnDestroy()
    {
        if (displayTexture != null)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(displayTexture);
            else
                Destroy(displayTexture);
#else
            Destroy(displayTexture);
#endif
        }
    }
}