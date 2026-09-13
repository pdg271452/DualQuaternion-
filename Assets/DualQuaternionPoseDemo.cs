using UnityEngine;
using UnityEngine.UI;

public class DualQuaternionPoseDemo : MonoBehaviour
{
    [Header("Anchors")]
    public Transform poseA;
    public Transform poseB;

    [Header("Interpolation Control")]
    [Range(0f, 1f)]
    public float u = 0.5f;

    [Header("UI Slider (Optional)")]
    public Slider sliderU;

    private void Start()
    {
        if (sliderU != null)
        {
            sliderU.minValue = 0f;
            sliderU.maxValue = 1f;
            sliderU.value = u;
            sliderU.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    private void Update()
    {
        if (poseA == null || poseB == null) return;

        // Extract dual quaternions from pose anchors
        DualQuaternion dqA = DualQuaternion.FromPose(poseA.rotation, poseA.position);
        DualQuaternion dqB = DualQuaternion.FromPose(poseB.rotation, poseB.position);

        // Perform Dual Quaternion Slerp
        DualQuaternion dqInterp = DualQuaternion.Slerp(dqA, dqB, u);

        // Apply back to transform
        dqInterp.ToPose(out Quaternion rot, out Vector3 trans);
        transform.rotation = rot;
        transform.position = trans;
    }

    public void OnSliderValueChanged(float value)
    {
        u = value;
    }
}