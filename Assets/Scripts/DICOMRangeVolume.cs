using UnityEngine;

[ExecuteAlways]
public class DICOMRangeVolume : MonoBehaviour
{
    [Header("CTモデル")]
    public Transform ctVolume;

    [Header("Texture3D")]
    public Texture3D volumeTexture;

    [Header("Z方向の中心座標")]
    [Range(0, 550)]
    public int slicePosition = 275;

    [Header("Z方向の厚み")]
    [Min(1)]
    public int thickness = 50;

    [Header("範囲表示用Cube")]
    public Transform rangeCube;

    private int lastSlicePosition = -1;
    private int lastThickness = -1;

    void Update()
    {
        UpdateRangeIfNeeded();
    }

    void OnValidate()
    {
        UpdateRange();
    }

    void UpdateRangeIfNeeded()
    {
        if (slicePosition != lastSlicePosition ||
            thickness != lastThickness)
        {
            UpdateRange();
        }
    }

    void UpdateRange()
    {
        if (ctVolume == null ||
            volumeTexture == null ||
            rangeCube == null)
        {
            return;
        }

        int maxSlice = volumeTexture.depth - 1;

        if (maxSlice <= 0)
            return;

        // -----------------------------
        // 座標と厚みを制限
        // -----------------------------

        thickness = Mathf.Clamp(
            thickness,
            1,
            maxSlice
        );

        slicePosition = Mathf.Clamp(
            slicePosition,
            0,
            maxSlice
        );

        // -----------------------------
        // 範囲を計算
        // -----------------------------

        int halfThickness = thickness / 2;

        int minSlice = Mathf.Max(
            0,
            slicePosition - halfThickness
        );

        int maxRangeSlice = Mathf.Min(
            maxSlice,
            slicePosition + halfThickness
        );

        // -----------------------------
        // 0～1に変換
        // -----------------------------

        float start =
            (float)minSlice / maxSlice;

        float end =
            (float)maxRangeSlice / maxSlice;

        float center =
            (start + end) * 0.5f;

        float rangeSize =
            end - start;

        // -----------------------------
        // CTモデルの子にする
        // -----------------------------

        if (rangeCube.parent != ctVolume)
        {
            rangeCube.SetParent(ctVolume);
        }

        // -----------------------------
        // ★ Unity Y方向に範囲を作る
        // -----------------------------

        rangeCube.localPosition =
            new Vector3(
                0f,
                center - 0.5f,
                0f
            );

        // -----------------------------
        // ★ Y方向だけ厚くする
        // -----------------------------

        rangeCube.localScale =
            new Vector3(
                1f,
                rangeSize,
                1f
            );

        // -----------------------------
        // 更新記録
        // -----------------------------

        lastSlicePosition = slicePosition;
        lastThickness = thickness;
    }
}