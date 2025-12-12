using QuaternionUtility;
using System.Collections.Generic;
using UnityEngine;

public class IKGradientWalk : MonoBehaviour
{

    [Header("Joints (Orden: Thigh, Calf, Foot)")]
    public List<Transform> joints = new List<Transform>();
    public Transform target;

    [Header("Configuración")]
    public float learningRate = 50.0f; // Más alto para reaccionar rápido
    public float samplingDistance = 0.1f;
    public float distanceThreshold = 0.05f;

    // IMPORTANTE: Si la pierna se dobla al revés, cambia esto a TRUE
    public bool inverseKneeBend = false;

    // Guardamos los ángulos actuales (en grados)
    private float[] angles;
    private VectorUtils3D[] startOffsets;

    void Start()
    {
        angles = new float[joints.Count];
        startOffsets = new VectorUtils3D[joints.Count - 1];

        // Calculamos las distancias (huesos) iniciales
        for (int i = 0; i < joints.Count - 1; i++)
        {
            VectorUtils3D p1 = VectorUtils3D.ToVectorUtils3D(joints[i].position);
            VectorUtils3D p2 = VectorUtils3D.ToVectorUtils3D(joints[i + 1].position);
            startOffsets[i] = p2 - p1; // Offset local inicial en coordenadas globales

            // Inicializamos ángulos en 0
            angles[i] = 0f;
        }
    }

    void Update()
    {
        // 1. Ejecutar el Gradiente Descendente (IK)
        // Hacemos varias iteraciones por frame para que sea sólido
        for (int k = 0; k < 10; k++)
        {
            if (GetDistanceToTarget() < distanceThreshold) break;

            CalculateSlopeAndMove();
        }

        // 2. Aplicar los ángulos calculados a los huesos
        ApplyRotations();

        // 3. ARREGLO BAILARINA: Forzar la rotación del pie final
        // Esto sobrescribe lo que el IK decidió para el tobillo y lo alinea con el target
        if (joints.Count >= 3)
        {
            int footIndex = joints.Count - 1; // El último hueso (el pie)
            joints[footIndex].rotation = target.rotation;
        }
    }

    void CalculateSlopeAndMove()
    {
        // Solo rotamos Thigh (0) y Calf (1). El Foot (2) lo ignoramos aquí porque lo forzamos al final.
        for (int i = 0; i < joints.Count - 1; i++)
        {
            float slope = CalculateGradient(i);
            angles[i] -= slope * learningRate;

            // --- RESTRICCIONES (Constraints) ---

            // Si es la rodilla (índice 1 usualmente), limitamos el ángulo
            if (i == 1)
            {
                if (inverseKneeBend)
                    angles[i] = Mathf.Clamp(angles[i], -160f, -5f); // Solo dobla hacia atrás negativo
                else
                    angles[i] = Mathf.Clamp(angles[i], 5f, 160f);  // Solo dobla hacia atrás positivo
            }

            // Si es la cadera (índice 0), damos libertad pero evitamos giros locos
            if (i == 0)
            {
                angles[i] = Mathf.Clamp(angles[i], -90f, 90f);
            }
        }
    }

    float CalculateGradient(int index)
    {
        float angleSaved = angles[index];

        float f_x = GetDistanceToTarget(); // Distancia actual

        angles[index] += samplingDistance; // Pequeño cambio
        float f_x_plus_h = GetDistanceToTarget(); // Nueva distancia

        angles[index] = angleSaved; // Restaurar

        return (f_x_plus_h - f_x) / samplingDistance; // Pendiente
    }

    // Forward Kinematics simple: Calcula dónde estaría el pie con los ángulos actuales
    float GetDistanceToTarget()
    {
        VectorUtils3D currentPos = VectorUtils3D.ToVectorUtils3D(joints[0].position);
        QuaternionUtils accRot = new QuaternionUtils();

        // Empezamos con la rotación global del padre de la pierna (Pelvis)
        if (joints[0].parent != null)
            accRot.AssignFromUnityQuaternion(joints[0].parent.rotation);

        for (int i = 0; i < joints.Count - 1; i++)
        {
            // Rotación local (Eje X es el estándar para doblar rodillas)
            QuaternionUtils localRot = new QuaternionUtils();
            localRot.FromXRotation(angles[i] * MathFUtils.Degree2Rad);

            // Acumulamos rotación
            accRot.Multiply(localRot);

            // Rotamos el vector del hueso original
            VectorUtils3D boneVector = accRot.Rotate(startOffsets[i]);

            // Sumamos posición
            currentPos = currentPos + boneVector;
        }

        VectorUtils3D targetPos = VectorUtils3D.ToVectorUtils3D(target.position);
        return VectorUtils3D.Distance(currentPos, targetPos);
    }

    void ApplyRotations()
    {
        for (int i = 0; i < joints.Count - 1; i++)
        {
            QuaternionUtils rot = new QuaternionUtils();
            rot.FromXRotation(angles[i] * MathFUtils.Degree2Rad);
            joints[i].localRotation = rot.GetAsUnityQuaternion();
        }
    }
}
