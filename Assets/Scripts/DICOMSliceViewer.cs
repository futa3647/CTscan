using UnityEngine;

public class DICOMSliceViewer : MonoBehaviour
{
    public Texture3D volumeTexture;

    [Header("表示・判定するZ範囲")]
    [Range(0, 550)]
    public int startSlice = 100;

    [Range(0, 550)]
    public int endSlice = 150;

    private Renderer targetRenderer;
    private Material material;

    void Start()
    {
        targetRenderer = GetComponent<Renderer>();
        material = targetRenderer.material;

        material.SetTexture("_VolumeTex", volumeTexture);

        UpdateSliceRange();
    }

    void Update()
    {
        UpdateSliceRange();
    }

    void UpdateSliceRange()
    {
        if (volumeTexture == null || material == null)
            return;

        int minSlice = Mathf.Min(startSlice, endSlice);
        int maxSlice = Mathf.Max(startSlice, endSlice);

        float startPosition =
            1.0f -
            (float)minSlice /
            (volumeTexture.depth - 1);

        float endPosition =
            1.0f -
            (float)maxSlice /
            (volumeTexture.depth - 1);

        material.SetFloat("_StartSlice", startPosition);
        material.SetFloat("_EndSlice", endPosition);
    }
}