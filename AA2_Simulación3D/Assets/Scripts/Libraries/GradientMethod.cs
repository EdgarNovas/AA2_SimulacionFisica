using UnityEngine;
using QuaternionUtility;
using System;
using System.Collections.Generic;

public class GradientMethod : MonoBehaviour
{
    [Header("Joints")]
    public List<Transform> joints = new List<Transform>();
    public Transform target;

    [Header("Learning parameters")]
    public float alpha = 0.1f;              // base learning rate
    public float tolerance = 0.001f;        // squared distance tolerance
    public float maxStepPerJoint = 0.05f;   // max radians per update to avoid jumps


    [Header("Angle limits per joint (degrees)")]
    public List<VectorUtils2D> angleLimits = new List<VectorUtils2D>();  // One per joint (min,max)

    // Internal: angles stored in radians
    private float[] theta;


    // distance between the joints
    private VectorUtils3D[] localOffsets;

    private float beta1 = 0.9f;
    private float beta2 = 0.999f;
    private float epsilon = 1e-8f;
    private int t = 1;

    private float[] m_t;   // momento
    private float[] v_t;   // RMS

    void Start()
    {
        theta = new float[joints.Count];

        // Initialize angle limits if missing
        if (angleLimits.Count != joints.Count)
        {
            angleLimits.Clear();
            for (int i = 0; i < joints.Count; i++)
                angleLimits.Add(new VectorUtils2D(-180, 180));
        }

        m_t = new float[joints.Count];
        v_t = new float[joints.Count];

        //The vector from joint i to joint i+1 expressed in joint i's local space.
        localOffsets = new VectorUtils3D[joints.Count - 1];
        for (int i = 0; i < joints.Count - 1; i++)
        {
            // Use localPosition of child relative to parent: in Unity, child.localPosition works if hierarchy set.
            // If joints aren't parented, compute from world positions but store them in parent-local basis:
            Vector3 worldOffset = joints[i + 1].position - joints[i].position;
            // Express offset in parent's rotation inverse (parent local)
            Vector3 parentInvRotated = Quaternion.Inverse(joints[i].rotation) * worldOffset;
            localOffsets[i] = VectorUtils3D.ToVectorUtils3D(parentInvRotated);
        }

        // Initialize theta from current local Y-rotation of each joint (assumes rotation around Y)
        for (int i = 0; i < joints.Count; i++)
        {
            float yDeg = joints[i].localEulerAngles.y;
            // Unity eulerAngles gives [0,360); convert to -180..180
            if (yDeg > 180f) yDeg -= 360f;
            theta[i] = yDeg * Mathf.Deg2Rad;
        }
    }

    
    void Update()
    {
        float currentCost = Cost(theta);
        if (currentCost <= tolerance * tolerance)
            return;

        float[] gradient = CalculateGradient(theta);

        float[] adaptiveStep = AdaptiveLearningRate(gradient);

        for (int i = 0; i < theta.Length; i++)
        {
            // adaptiveStep[i] already equals alpha * m_hat/(sqrt(v_hat)+eps) — treat it as delta (signed)
            // but limit magnitude per joint to avoid large jumps
            float delta = adaptiveStep[i];
            delta = Mathf.Clamp(delta, -maxStepPerJoint, maxStepPerJoint);

            theta[i] -= delta;
        }

        ApplyLimits();

        ForwardKinematics();
    }

    float Cost(float[] angles)
    {
        VectorUtils3D effectorPos = ComputeFK(angles);
        VectorUtils3D targetPos = VectorUtils3D.ToVectorUtils3D(target.position);

        float d = VectorUtils3D.Distance(effectorPos, targetPos);
        return d;
    }

    public VectorUtils3D ComputeFK(float[] angles)
    {
        QuaternionUtils rotation = new QuaternionUtils();
        VectorUtils3D pos = VectorUtils3D.ToVectorUtils3D(joints[0].position);

        for (int i = 0; i < joints.Count - 1; i++)
        {
            // Y-axis rotation
            QuaternionUtils r = new QuaternionUtils();
            r.FromYRotation(angles[i]);
            rotation.Multiply(r);

            // Rotate cached local offset by cumulative rotation and add
            VectorUtils3D rotated = rotation.Rotate(localOffsets[i]);
            pos = pos + rotated;
        }

        return pos;
    }

    void ApplyLimits()
    {
        for (int i = 0; i < theta.Length; i++)
        {
            float minRad = angleLimits[i].x * Mathf.Deg2Rad;
            float maxRad = angleLimits[i].y * Mathf.Deg2Rad;

            theta[i] = Mathf.Clamp(theta[i], minRad, maxRad);
        }
    }

    void ForwardKinematics()
    {
        VectorUtils3D[] chain = ComputeFKChain(theta);

        // Aplicamos las posiciones calculadas a los joints de Unity
        for (int i = 0; i < joints.Count; i++)
        {
            joints[i].position = chain[i].GetAsUnityVector();
        }
    }

    float[] AdaptiveLearningRate(float[] gradient)
    {
        float[] alpha_t = new float[gradient.Length];

        t++;

        for (int i = 0; i < gradient.Length; i++)
        {
            // Update moments
            m_t[i] = beta1 * m_t[i] + (1 - beta1) * gradient[i];
            v_t[i] = beta2 * v_t[i] + (1 - beta2) * (gradient[i] * gradient[i]);

            // Bias correction
            float m_hat = m_t[i] / (1 - Mathf.Pow(beta1, t));
            float v_hat = v_t[i] / (1 - Mathf.Pow(beta2, t));

            // Adaptive step
            alpha_t[i] = alpha * (m_hat / (Mathf.Sqrt(v_hat) + epsilon));
        }

        return alpha_t;
    }

    float[] CalculateGradient(float[] theta)
    {
        float[] gradient = new float[theta.Length];
        float step = 0.0001f;

        float baseCost = Cost(theta);

        for (int i = 0; i < theta.Length; i++)
        {
            float old = theta[i];

            // Perturbation
            theta[i] += step;

            float newCost = Cost(theta);

            // Numerical derivative
            gradient[i] = (newCost - baseCost) / step;

            // Restore
            theta[i] = old;
        }

        return gradient;
    }

    VectorUtils3D[] ComputeFKChain(float[] angles)
    {
        int n = joints.Count;
        VectorUtils3D[] positions = new VectorUtils3D[n];

        // pos inicial = posición del primer joint
        positions[0] = VectorUtils3D.ToVectorUtils3D(joints[0].position);

        QuaternionUtils rotation = new QuaternionUtils();

        // Recorremos como la FK original
        for (int i = 0; i < n - 1; i++)
        {
            // rotación incremental sobre el eje Y
            QuaternionUtils r = new QuaternionUtils();
            r.FromYRotation(angles[i]);
            rotation.Multiply(r);

            // offset LOCAL cacheado (como ya arreglamos antes)
            VectorUtils3D local = localOffsets[i];

            // aplicar rotación acumulada
            VectorUtils3D rotated = rotation.Rotate(local);

            // next joint = anterior + offset rotado
            positions[i + 1] = positions[i] + rotated;
        }

        return positions;
    }

}
