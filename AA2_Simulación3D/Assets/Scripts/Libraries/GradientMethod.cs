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
    public float alpha = 0.1f;
    public float tolerance = 0.001f;

    [Header("Angle limits per joint (degrees)")]
    public List<VectorUtils2D> angleLimits = new List<VectorUtils2D>();  // One per joint

    // Internal: angles stored in radians
    private float[] theta;

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
    }

    
    void Update()
    {
        float[] gradient = CalculateGradient(theta);

        float[] adaptiveStep = AdaptiveLearningRate(gradient);

        for (int i = 0; i < theta.Length; i++)
            theta[i] -= adaptiveStep[i];
        
        ApplyLimits();

        ForwardKinematics();
    }

    float Cost(float[] angles)
    {
        VectorUtils3D effectorPos = ComputeFK(angles);
        VectorUtils3D targetPos = VectorUtils3D.ToVectorUtils3D(target.position);

        float d = VectorUtils3D.Distance(effectorPos, targetPos);
        return d * d;
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

            // Compute local offset = child.position - parent.position
            VectorUtils3D child = VectorUtils3D.ToVectorUtils3D(joints[i + 1].position);
            VectorUtils3D parent = VectorUtils3D.ToVectorUtils3D(joints[i].position);
            VectorUtils3D local = child - parent;

            // Rotate that offset
            VectorUtils3D rotated = rotation.Rotate(local);

            // Add to the current chain pos
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
        QuaternionUtils rotation = new QuaternionUtils();

        for (int i = 0; i < joints.Count; i++)
        {
            QuaternionUtils r = new QuaternionUtils();
            r.FromYRotation(theta[i]);
            rotation.Multiply(r);

            // Convert custom quaternion to Unity's quaternion to apply it
            joints[i].localRotation = rotation.GetAsUnityQuaternion();
        }
    }

    float[] AdaptiveLearningRate(float[] gradient)
    {
        float[] alpha_t = new float[gradient.Length];

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

        t++;
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

}
