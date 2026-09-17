using UnityEngine;

[ExecuteAlways]
public class NeedleCTCoordinateTracker : MonoBehaviour
{
    [Header("CT 3Dモデル")]
    public Transform ctVolume;

    [Header("針")]
    public Transform needle;

    [Header("表示する針の基準点")]
    public Transform needleTip;

    [Header("計算結果")]
    [SerializeField]
    private Vector3 ctLocalPosition;

    [SerializeField]
    private Vector3 textureCoordinate;

    [SerializeField]
    private Vector3 needleDirection;

    [Header("デバッグ表示")]
    public bool showDebugLog = true;

    private Vector3 lastPosition;
    private Quaternion lastRotation;

    void Update()
    {
        UpdateCoordinate();
    }

    void OnValidate()
    {
        UpdateCoordinate();
    }

    void UpdateCoordinate()
    {
        if (ctVolume == null)
            return;

        if (needle == null)
            return;

        // --------------------------------
        // 針の基準点
        // --------------------------------

        Vector3 worldPosition;

        if (needleTip != null)
        {
            worldPosition = needleTip.position;
        }
        else
        {
            worldPosition = needle.position;
        }

        // --------------------------------
        // ワールド座標
        // → CTモデルのローカル座標
        // --------------------------------

        ctLocalPosition =
            ctVolume.InverseTransformPoint(
                worldPosition
            );

        // --------------------------------
        // CTローカル座標
        // → Texture3D座標
        //
        // Texture3D
        // X → Unity X
        // Y → Unity Z
        // Z → Unity Y
        // --------------------------------

        textureCoordinate = new Vector3(
            ctLocalPosition.x + 0.5f,
            ctLocalPosition.z + 0.5f,
            ctLocalPosition.y + 0.5f
        );

        // --------------------------------
        // 針の方向
        // --------------------------------

        Vector3 worldDirection =
            needle.forward;

        needleDirection =
            ctVolume.InverseTransformDirection(
                worldDirection
            ).normalized;

        // --------------------------------
        // 値が変化したときだけログ
        // --------------------------------

        if (showDebugLog &&
            (Vector3.Distance(
                lastPosition,
                worldPosition) > 0.001f ||
             Quaternion.Angle(
                lastRotation,
                needle.rotation) > 0.1f))
        {
            Debug.Log(
                "===== Needle CT Coordinate =====\n" +
                "World Position : " +
                worldPosition + "\n" +
                "CT Local       : " +
                ctLocalPosition + "\n" +
                "Texture3D      : " +
                textureCoordinate + "\n" +
                "Direction      : " +
                needleDirection
            );

            lastPosition =
                worldPosition;

            lastRotation =
                needle.rotation;
        }
    }

    public Vector3 GetCTLocalPosition()
    {
        return ctLocalPosition;
    }

    public Vector3 GetTextureCoordinate()
    {
        return textureCoordinate;
    }

    public Vector3 GetNeedleDirection()
    {
        return needleDirection;
    }
}