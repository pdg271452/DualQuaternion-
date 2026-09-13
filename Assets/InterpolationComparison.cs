using UnityEngine;

public class InterpolationComparison : MonoBehaviour
{
    [Header("Pose Anchors")]
    public Transform poseA;
    public Transform poseB;

    [Header("Visual Interpolators")]
    public Transform standardObject;      // Object for Lerp(Pos) + Slerp(Rot)
    public Transform dualQuatObject;      // Object for Dual Quaternion Slerp

    [Header("Settings")]
    public float noiseIntensity = 0.05f;  // Simulates controller noise
    public float animationSpeed = 0.5f;

    private TrailRenderer standardTrail;
    private TrailRenderer dualQuatTrail;

    void Start()
    {
        standardTrail = SetupTrail(standardObject, Color.red);
        dualQuatTrail = SetupTrail(dualQuatObject, Color.cyan);
    }

    void Update()
    {
        if (poseA == null || poseB == null) return;

        float t = Mathf.PingPong(Time.time * animationSpeed, 1.0f);

        Vector3 noisyPosA = poseA.position + Random.insideUnitSphere * noiseIntensity;
        Vector3 noisyPosB = poseB.position + Random.insideUnitSphere * noiseIntensity;

        // 1. Standard Linear + Spherical Interpolation
        if (standardObject != null)
        {
            standardObject.position = Vector3.Lerp(noisyPosA, noisyPosB, t);
            standardObject.rotation = Quaternion.Slerp(poseA.rotation, poseB.rotation, t);
        }

        // 2. Dual Quaternion Slerp
        if (dualQuatObject != null)
        {
            // Constructing dual quaternions directly from rotation and position
            DualQuaternion dqA = MakeDualQuaternion(poseA.rotation, noisyPosA);
            DualQuaternion dqB = MakeDualQuaternion(poseB.rotation, noisyPosB);

            DualQuaternion dqInterpolated = DualQuaternion.Slerp(dqA, dqB, t);

            // Extract position and rotation from DualQuaternion
            Quaternion rot = dqInterpolated.r;
            Quaternion dual = dqInterpolated.d;
            Quaternion transQuat = dual * Quaternion.Inverse(rot);
            Vector3 pos = new Vector3(transQuat.x, transQuat.y, transQuat.z) * 2.0f;

            dualQuatObject.position = pos;
            dualQuatObject.rotation = rot;
        }
    }

    private DualQuaternion MakeDualQuaternion(Quaternion q, Vector3 v)
    {
        Quaternion real = q;
        Quaternion pureVector = new Quaternion(v.x, v.y, v.z, 0);
        Quaternion dualMult = pureVector * real;
        Quaternion dual = new Quaternion(dualMult.x * 0.5f, dualMult.y * 0.5f, dualMult.z * 0.5f, dualMult.w * 0.5f);
        return new DualQuaternion(real, dual);
    }

    private TrailRenderer SetupTrail(Transform target, Color color)
    {
        if (target == null) return null;
        TrailRenderer trail = target.GetComponent<TrailRenderer>();
        if (trail == null) trail = target.gameObject.AddComponent<TrailRenderer>();

        trail.time = 2.0f;
        trail.startWidth = 0.05f;
        trail.endWidth = 0.01f;
        trail.material = new Material(Shader.Find("Sprites/Default"));
        trail.startColor = color;
        trail.endColor = color;
        return trail;
    }
}