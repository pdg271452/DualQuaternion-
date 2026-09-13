using UnityEngine;

[System.Serializable]
public struct DualQuaternion
{
    public Quaternion r; // real part
    public Quaternion d; // dual part

    public DualQuaternion(Quaternion real, Quaternion dual)
    {
        r = real;
        d = dual;
    }

    public static DualQuaternion identity =>
        new DualQuaternion(Quaternion.identity, Quaternion.identity);

    public static DualQuaternion FromPose(Quaternion rotation, Vector3 translation)
    {
        rotation = rotation.normalized;
        Quaternion tAsQuat = new Quaternion(translation.x, translation.y, translation.z, 0f);
        Quaternion dual = MultiplyScalar(0.5f, tAsQuat * rotation);
        return new DualQuaternion(rotation, dual);
    }

    public void ToPose(out Quaternion rotation, out Vector3 translation)
    {
        rotation = r.normalized;
        Quaternion rConj = Quaternion.Inverse(rotation);
        Quaternion tAsQuat = MultiplyScalar(2f, d * rConj);
        translation = new Vector3(tAsQuat.x, tAsQuat.y, tAsQuat.z);
    }

    public static DualQuaternion Multiply(DualQuaternion a, DualQuaternion b)
    {
        Quaternion r = a.r * b.r;
        Quaternion d = AddQuat(a.r * b.d, a.d * b.r);
        return new DualQuaternion(r, d);
    }

    /// <summary>
    /// Normalize a unit dual quaternion by normalizing real part and orthogonalizing dual part.
    /// </summary>
    public DualQuaternion Normalized()
    {
        // Calculate magnitude of real quaternion: sqrt(x^2 + y^2 + z^2 + w^2)
        float normR = Mathf.Sqrt(Quaternion.Dot(r, r));
        if (normR < 1e-6f)
        {
            return identity;
        }

        Quaternion rn = DivideScalar(r, normR);
        float dot = Quaternion.Dot(rn, d);
        Quaternion dn = DivideScalar(SubtractQuat(d, MultiplyScalar(dot, rn)), normR);
        return new DualQuaternion(rn, dn);
    }

    public DualQuaternion Conjugate()
    {
        return new DualQuaternion(
            new Quaternion(-r.x, -r.y, -r.z, r.w),
            new Quaternion(-d.x, -d.y, -d.z, d.w)
        );
    }

    public static DualQuaternion Slerp(DualQuaternion a, DualQuaternion b, float u)
    {
        u = Mathf.Clamp01(u);
        if (Quaternion.Dot(a.r, b.r) < 0f)
        {
            b = new DualQuaternion(
                new Quaternion(-b.r.x, -b.r.y, -b.r.z, -b.r.w),
                new Quaternion(-b.d.x, -b.d.y, -b.d.z, -b.d.w)
            );
        }
        Quaternion r = Quaternion.Slerp(a.r, b.r, u);
        Quaternion d = Quaternion.Slerp(a.d, b.d, u);
        return new DualQuaternion(r, d).Normalized();
    }

    public Vector3 TransformPoint(Vector3 p)
    {
        ToPose(out Quaternion rot, out Vector3 trans);
        return rot * p + trans;
    }

    #region Quaternion Math Helpers
    private static Quaternion MultiplyScalar(float scalar, Quaternion q) =>
        new Quaternion(q.x * scalar, q.y * scalar, q.z * scalar, q.w * scalar);

    private static Quaternion DivideScalar(Quaternion q, float scalar) =>
        new Quaternion(q.x / scalar, q.y / scalar, q.z / scalar, q.w / scalar);

    private static Quaternion AddQuat(Quaternion a, Quaternion b) =>
        new Quaternion(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);

    private static Quaternion SubtractQuat(Quaternion a, Quaternion b) =>
        new Quaternion(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
    #endregion
}