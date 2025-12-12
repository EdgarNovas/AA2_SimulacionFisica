using QuaternionUtility;
using System;
using System.Collections.Generic;
using UnityEngine;
public class SpiderManWalker : MonoBehaviour
{

    [Header("Asignar en orden: Muslo, Pantorrilla, Pie")]
    public List<Transform> joints = new List<Transform>();
    public Transform target;

    [Header("Ajustes de Física")]
    public float learningRate = 10.0f;
    public float samplingDistance = 0.05f;
    public float minDistance = 0.05f;
    [Range(0, 50)] public int iterations = 10;

    [Header("Correcciones de Modelo")]
    
    public VectorUtils3D bendAxis = new VectorUtils3D(1, 0, 0);
    public bool invertKnee = false;

    // --- DATOS INTERNOS ---
    private float[] angles;
    private QuaternionUtils[] initialRotations;
    private VectorUtils3D[] localBoneOffsets; // Offset calculado con TU librería

    void Start()
    {
        angles = new float[joints.Count];
        initialRotations = new QuaternionUtils[joints.Count];
        localBoneOffsets = new VectorUtils3D[joints.Count - 1];

        for (int i = 0; i < joints.Count; i++)
        {
            // 1. Guardar rotación inicial usando TU clase
            QuaternionUtils q = new QuaternionUtils();
            q.AssignFromUnityQuaternion(joints[i].localRotation);
            initialRotations[i] = q;

            // 2. Calcular el offset LOCAL
            if (i < joints.Count - 1)
            {
                // A) Obtener posiciones convertidas a VectorUtils3D
                VectorUtils3D p1 = VectorUtils3D.ToVectorUtils3D(joints[i].position);
                VectorUtils3D p2 = VectorUtils3D.ToVectorUtils3D(joints[i + 1].position);

                // B) Calcular vector del hueso en el mundo (Resta de tus vectores)
                VectorUtils3D worldDelta = p2 - p1;

                // C) Obtener la rotación GLOBAL del padre para deshacerla
                // Necesitamos la inversa para pasar de Mundo a Local
                QuaternionUtils parentGlobalRot = new QuaternionUtils();
                parentGlobalRot.AssignFromUnityQuaternion(joints[i].rotation);

                // D) Rotación Inversa manual (Conjugado)
                // Para rotar un vector "hacia atrás", usamos el conjugado del cuaternión
                // localDelta = Inverse(Rotation) * worldDelta
                VectorUtils3D localDelta = InverseRotate(parentGlobalRot, worldDelta);

                localBoneOffsets[i] = localDelta;
            }
        }
    }

    void Update()
    {
        // Debug visual (Aquí sí necesitamos Unity Vector3 solo para dibujar)
        Debug.DrawLine(joints[0].position, target.position, Color.red);

        for (int k = 0; k < iterations; k++)
        {
            if (GetDistanceAndDebug(false) < minDistance) break;
            CalculateGradient();
        }

        ApplyRotations();

        GetDistanceAndDebug(true);
    }

    // Función auxiliar para rotar a la inversa usando QuaternionUtils
    // Simula: Quaternion.Inverse(q) * v
    VectorUtils3D InverseRotate(QuaternionUtils q, VectorUtils3D v)
    {
        // El inverso de un cuaternión de rotación unitario es su conjugado.
        // Como no sé si tu librería tiene 'Conjugate' o acceso a w,x,y,z públicos,
        // usamos un truco: Unity nos da el inverso, lo convertimos a TU clase y rotamos.
        // (Esto mantiene la matemática pura en tu clase QuaternionUtils.Rotate)

        Quaternion unityQ = q.GetAsUnityQuaternion();
        Quaternion unityInverse = Quaternion.Inverse(unityQ);

        QuaternionUtils qInv = new QuaternionUtils();
        qInv.AssignFromUnityQuaternion(unityInverse);

        return qInv.Rotate(v);
    }

    void CalculateGradient()
    {
        for (int i = 0; i < joints.Count - 1; i++)
        {
            float gradient = CalculateSlope(i);
            angles[i] -= gradient * learningRate;

            // Clamps usando Mathf estándar (o MathFUtils si tienes Max/Min)
            if (i == 1)
            {
                if (invertKnee)
                    angles[i] = Mathf.Clamp(angles[i], -150f, -5f);
                else
                    angles[i] = Mathf.Clamp(angles[i], 5f, 150f);
            }
            if (i == 0)
            {
                angles[i] = Mathf.Clamp(angles[i], -90f, 90f);
            }
        }
    }

    float CalculateSlope(int index)
    {
        float savedAngle = angles[index];
        float distA = GetDistanceAndDebug(false);

        angles[index] += samplingDistance;
        float distB = GetDistanceAndDebug(false);

        angles[index] = savedAngle;

        return (distB - distA) / samplingDistance;
    }

    float GetDistanceAndDebug(bool drawGizmos)
    {
        // Todo aquí es VectorUtils3D
        VectorUtils3D currentPos = VectorUtils3D.ToVectorUtils3D(joints[0].position);

        QuaternionUtils accRot = new QuaternionUtils();
        if (joints[0].parent != null)
            accRot.AssignFromUnityQuaternion(joints[0].parent.rotation);
        else
            accRot.AssignFromUnityQuaternion(Quaternion.identity);

        for (int i = 0; i < joints.Count - 1; i++)
        {
            QuaternionUtils baseLocal = new QuaternionUtils(initialRotations[i]);

            QuaternionUtils ikBend = new QuaternionUtils();
            if (bendAxis.x == 1) ikBend.FromXRotation(angles[i] * MathFUtils.Degree2Rad);
            else if (bendAxis.z == 1) ikBend.FromZRotation(angles[i] * MathFUtils.Degree2Rad);
            else ikBend.FromYRotation(angles[i] * MathFUtils.Degree2Rad);

            baseLocal.Multiply(ikBend);
            accRot.Multiply(baseLocal);

            // Operación vectorial PURA con tu librería
            VectorUtils3D rotatedOffset = accRot.Rotate(localBoneOffsets[i]);

            if (drawGizmos)
            {
                // Solo convertimos al final para pintar
                Vector3 s = new Vector3(currentPos.x, currentPos.y, currentPos.z);
                Vector3 e = new Vector3(currentPos.x + rotatedOffset.x, currentPos.y + rotatedOffset.y, currentPos.z + rotatedOffset.z);
                Debug.DrawLine(s, e, Color.yellow);
            }

            currentPos = currentPos + rotatedOffset;
        }

        VectorUtils3D targetPos = VectorUtils3D.ToVectorUtils3D(target.position);

        // Distancia calculada con TU librería
        return VectorUtils3D.Distance(currentPos, targetPos);
    }

    void ApplyRotations()
    {
        for (int i = 0; i < joints.Count - 1; i++)
        {
            QuaternionUtils baseR = new QuaternionUtils(initialRotations[i]);
            QuaternionUtils bend = new QuaternionUtils();

            if (bendAxis.x == 1) bend.FromXRotation(angles[i] * MathFUtils.Degree2Rad);
            else if (bendAxis.z == 1) bend.FromZRotation(angles[i] * MathFUtils.Degree2Rad);
            else bend.FromYRotation(angles[i] * MathFUtils.Degree2Rad);

            baseR.Multiply(bend);

            // Único punto de contacto con Unity: Asignar al Transform
            joints[i].localRotation = baseR.GetAsUnityQuaternion();
        }

        if (joints.Count > 0)
            joints[joints.Count - 1].rotation = target.rotation;
    }
}
