using UnityEngine;

public class DICOMSliceViewer : MonoBehaviour
{
    // Texture3D
    public Texture3D volumeTexture;

    // 表示するスライス番号
    [Range(0, 550)]
    public int sliceIndex = 0;

    // 前回のスライス番号
    private int lastSliceIndex = -1;

    // 表示用Renderer
    private Renderer targetRenderer;

    // 使用するMaterial
    private Material material;

    void Start()
    {
        targetRenderer = GetComponent<Renderer>();

        material = targetRenderer.material;

        material.SetTexture(
            "_VolumeTex",
            volumeTexture);

        UpdateSlice();
    }

    void Update()
    {
        if (sliceIndex != lastSliceIndex)
        {
            UpdateSlice();
        }
    }

    // スライスを更新
    void UpdateSlice()
    {
        if (volumeTexture == null || material == null)
        {
            return;
        }

        float slicePosition = 0.0f;

        if (volumeTexture.depth > 1)
        {
            slicePosition =
                 1.0f -
                 (float)sliceIndex /
                 (volumeTexture.depth - 1);
        }

        slicePosition = Mathf.Clamp01(slicePosition);

        material.SetFloat(
            "_SlicePosition",
            slicePosition);

        lastSliceIndex = sliceIndex;

        Debug.Log(
            "Slice Index: " + sliceIndex +
            " / Slice Position: " + slicePosition);
    }
}