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
    public float maxStepPerJoint = 0.05f;

    // Angles stored in radians (3 per joint: X, Y, Z)
    private float[] theta;

    // Offsets locales (distancia entre hueso i y i+1)
    private VectorUtils3D[] localOffsets;

    // Optimizador Adam
    private float beta1 = 0.9f;
    private float beta2 = 0.999f;
    private float epsilon = 1e-8f;
    private int t = 1;
    private float[] m_t;
    private float[] v_t;

    // Auxiliar para crear cuaterniones sin instanciar tanto
    private QuaternionUtils qUtilsHelper = new QuaternionUtils();

    void Start()
    {
        // 1. Inicializar Theta (3 ángulos por joint)
        int paramCount = joints.Count * 3;
        theta = new float[paramCount];
        m_t = new float[paramCount];
        v_t = new float[paramCount];

        // 2. Calcular Offsets Locales 
        // (Aunque no uses jerarquía para moverlos, necesitamos saber la distancia "rígida" entre ellos)
        localOffsets = new VectorUtils3D[joints.Count - 1];
        for (int i = 0; i < joints.Count - 1; i++)
        {
            Vector3 worldDiff = joints[i + 1].position - joints[i].position;
            // Guardamos el offset relativo a la rotación inicial del padre
            Vector3 localOff = Quaternion.Inverse(joints[i].rotation) * worldDiff;
            localOffsets[i] = VectorUtils3D.ToVectorUtils3D(localOff);
        }

        // 3. Copiar rotación inicial (Bind Pose)
        for (int i = 0; i < joints.Count; i++)
        {
            Vector3 euler = joints[i].localEulerAngles;
            theta[i * 3 + 0] = euler.x * Mathf.Deg2Rad;
            theta[i * 3 + 1] = euler.y * Mathf.Deg2Rad;
            theta[i * 3 + 2] = euler.z * Mathf.Deg2Rad;
        }
    }

    void Update()
    {
        // Gradient Descent logic
        float currentCost = Cost(theta);
        if (currentCost <= tolerance * tolerance) return;

        float[] gradient = CalculateGradient(theta);
        float[] step = AdaptiveLearningRate(gradient);

        for (int i = 0; i < theta.Length; i++)
        {
            float delta = Mathf.Clamp(step[i], -maxStepPerJoint, maxStepPerJoint);
            theta[i] -= delta;
        }

        // Aplicamos la FK manual a los objetos de Unity
        ForwardKinematics();
    }

    // --- FUNCIONES CORE ---

    // Calcula la posición y rotación GLOBAL de cada joint basándose en theta
    // Devuelve una Tupla o usamos structs, aquí uso dos arrays por claridad.
    void ComputeFKChain(float[] angles, out VectorUtils3D[] outPos, out QuaternionUtils[] outRot)
    {
        int n = joints.Count;
        outPos = new VectorUtils3D[n];
        outRot = new QuaternionUtils[n];

        // Inicializamos con la posición de la base (fija)
        VectorUtils3D currentPos = VectorUtils3D.ToVectorUtils3D(joints[0].position);

        // Rotación acumulada (Global). Empezamos con Identidad (o rotación mundo de la base si aplicara)
        QuaternionUtils accumulatedRot = new QuaternionUtils(); // Identity

        for (int i = 0; i < n; i++)
        {
            // 1. Construir rotación LOCAL combinada (X, Y, Z) para este joint
            QuaternionUtils localRot = GetRotationFromXYZ(angles, i);

            // 2. Acumular rotación: Global = ParentGlobal * Local
            accumulatedRot.Multiply(localRot);

            // 3. Guardamos la configuración de este joint
            outRot[i] = new QuaternionUtils(accumulatedRot); // Copia
            outPos[i] = currentPos;

            // 4. Si hay siguiente joint, avanzamos la posición usando el offset rotado
            if (i < n - 1)
            {
                // Pos += GlobalRot * LocalOffset
                VectorUtils3D rotOffset = accumulatedRot.Rotate(localOffsets[i]);
                currentPos = currentPos + rotOffset;
            }
        }
    }

    float Cost(float[] angles)
    {
        // Solo nos interesa la posición del último joint (End Effector)
        VectorUtils3D[] positions;
        QuaternionUtils[] rotations;
        ComputeFKChain(angles, out positions, out rotations);

        VectorUtils3D endEffector = positions[positions.Length - 1];
        VectorUtils3D targetPos = VectorUtils3D.ToVectorUtils3D(target.position);

        return VectorUtils3D.Distance(endEffector, targetPos);
    }

    void ForwardKinematics()
    {
        VectorUtils3D[] positions;
        QuaternionUtils[] rotations;

      
        ComputeFKChain(theta, out positions, out rotations);

        // 2. FORZAMOS la posición y rotación en Unity (ignorando jerarquía)
        for (int i = 0; i < joints.Count; i++)
        {
            // Asignar Posición Global
            
            joints[i].position = new Vector3(positions[i].x, positions[i].y, positions[i].z);

            // Asignar Rotación Global
            joints[i].rotation = rotations[i].ToUnityQuaternion();
        }
    }

    // --- UTILS ---

    // Construye un Cuaternión combinando X, Y, Z (Orden ZXY estilo Unity)
    QuaternionUtils GetRotationFromXYZ(float[] angles, int jointIndex)
    {
        float x = angles[jointIndex * 3 + 0];
        float y = angles[jointIndex * 3 + 1];
        float z = angles[jointIndex * 3 + 2];

       
        QuaternionUtils qx = qUtilsHelper.AngleToQuaternion(new VectorUtils3D(1, 0, 0), x);
        QuaternionUtils qy = qUtilsHelper.AngleToQuaternion(new VectorUtils3D(0, 1, 0), y);
        QuaternionUtils qz = qUtilsHelper.AngleToQuaternion(new VectorUtils3D(0, 0, 1), z);

        // Combinamos: Rot = Qy * Qx * Qz (Orden típico de Euler, ajustable si necesitas)
        // Usamos una identidad base para acumular
        QuaternionUtils finalQ = new QuaternionUtils();

        finalQ.Multiply(qy);
        finalQ.Multiply(qx);
        finalQ.Multiply(qz);

        return finalQ;
    }

    float[] AdaptiveLearningRate(float[] gradient)
    {
        float[] alpha_t = new float[gradient.Length];
        t++;
        for (int i = 0; i < gradient.Length; i++)
        {
            m_t[i] = beta1 * m_t[i] + (1 - beta1) * gradient[i];
            v_t[i] = beta2 * v_t[i] + (1 - beta2) * (gradient[i] * gradient[i]);
            float m_hat = m_t[i] / (1 - Mathf.Pow(beta1, t));
            float v_hat = v_t[i] / (1 - Mathf.Pow(beta2, t));
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
            theta[i] += step;
            float newCost = Cost(theta);
            gradient[i] = (newCost - baseCost) / step;
            theta[i] = old;
        }
        return gradient;
    }
}