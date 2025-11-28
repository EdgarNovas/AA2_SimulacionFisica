using QuaternionUtility;
using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting.Antlr3.Runtime;
using Unity.VisualScripting;
using UnityEditor.Search;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEngine.InputSystem.Controls.AxisControl;
using static UnityEngine.UI.Image;
using UnityEngine.UIElements;
public class SpiderManWalker : MonoBehaviour
{

    [Header("Asignar en orden: Muslo, Pantorrilla, Pie")]
    public List<Transform> joints = new List<Transform>();
    public Transform target;

    [Header("Ajustes de Física")]
    public float learningRate = 10.0f; // Bajado para evitar rebotes
    public float samplingDistance = 0.05f;
    public float minDistance = 0.05f;
    [Range(0, 50)] public int iterations = 10;

    [Header("Correcciones de Modelo")]
    // Si tu pierna se dobla mal, cambia este eje. 
    // (1,0,0) es X, (0,0,1) es Z.
    public VectorUtils3D bendAxis = new VectorUtils3D(1, 0, 0);
    public bool invertKnee = false; // Marca esto si la rodilla se dobla al revés

    // --- DATOS INTERNOS ---
    private float[] angles;
    private QuaternionUtils[] initialRotations;
    private VectorUtils3D[] localBoneOffsets; // Dónde está el siguiente hueso (en local)

    void Start()
    {
        angles = new float[joints.Count];
        initialRotations = new QuaternionUtils[joints.Count];
        localBoneOffsets = new VectorUtils3D[joints.Count - 1];

        for (int i = 0; i < joints.Count; i++)
        {
            // 1. Guardar rotación inicial exacta
            QuaternionUtils q = new QuaternionUtils();
            q.AssignFromUnityQuaternion(joints[i].localRotation);
            initialRotations[i] = q;

            // 2. Calcular el offset LOCAL al siguiente hueso
            // Esto es clave: averiguamos la longitud y dirección real del hueso
            if (i < joints.Count - 1)
            {
                // Vector del padre al hijo en espacio MUNDIAL
                Vector3 worldDelta = joints[i + 1].position - joints[i].position;

                // Convertir al espacio LOCAL del padre (usando la rotación actual de Unity para setup)
                Vector3 localDelta = Quaternion.Inverse(joints[i].rotation) * worldDelta;

                // Guardarlo en nuestra estructura
                localBoneOffsets[i] = VectorUtils3D.ToVectorUtils3D(localDelta);
            }
        }
    }

    void Update()
    {
        // Debug: Dibuja dónde está el objetivo
        Debug.DrawLine(joints[0].position, target.position, Color.red);

        // Bucle de IK
        for (int k = 0; k < iterations; k++)
        {
            if (GetDistanceAndDebug(false) < minDistance) break;

            CalculateGradient();
        }

        ApplyRotations();

        // Debug Visual: Si ves la línea AMARILLA separada de la pierna, el cálculo está mal.
        GetDistanceAndDebug(true);
    }

    void CalculateGradient()
    {
        for (int i = 0; i < joints.Count - 1; i++)
        {
            float gradient = CalculateSlope(i);
            angles[i] -= gradient * learningRate;

            // --- LÍMITES (CLAMPS) ---
            // Rodilla (índice 1)
            if (i == 1)
            {
                if (invertKnee)
                    angles[i] = Mathf.Clamp(angles[i], -150f, -5f); // Doblar negativo
                else
                    angles[i] = Mathf.Clamp(angles[i], 5f, 150f);  // Doblar positivo
            }
            // Cadera (índice 0)
            if (i == 0)
            {
                angles[i] = Mathf.Clamp(angles[i], -90f, 90f);
            }
        }
    }

    float CalculateSlope(int index)
    {
        float savedAngle = angles[index];

        float distA = GetDistanceAndDebug(false); // f(x)

        angles[index] += samplingDistance;
        float distB = GetDistanceAndDebug(false); // f(x+h)

        angles[index] = savedAngle; // Restaurar

        return (distB - distA) / samplingDistance;
    }

    // FK SIMULADO: Calcula dónde estaría la punta del pie SIN mover los objetos reales
    float GetDistanceAndDebug(bool drawGizmos)
    {
        VectorUtils3D currentPos = VectorUtils3D.ToVectorUtils3D(joints[0].position);

        // Empezamos con la rotación global de la pelvis (padre del muslo)
        Quaternion globalRot = (joints[0].parent != null) ? joints[0].parent.rotation : Quaternion.identity;
        QuaternionUtils accRot = new QuaternionUtils();
        accRot.AssignFromUnityQuaternion(globalRot);

        for (int i = 0; i < joints.Count - 1; i++)
        {
            // 1. Reconstruir rotación local: Inicial * IK
            QuaternionUtils baseLocal = new QuaternionUtils(initialRotations[i]);

            QuaternionUtils ikBend = new QuaternionUtils();
            if (bendAxis.x == 1) ikBend.FromXRotation(angles[i] * MathFUtils.Degree2Rad);
            else if (bendAxis.z == 1) ikBend.FromZRotation(angles[i] * MathFUtils.Degree2Rad);
            else ikBend.FromYRotation(angles[i] * MathFUtils.Degree2Rad); // Fallback Y

            // Multiplicamos (Orden: Base * Bend)
            baseLocal.Multiply(ikBend);

            // 2. Acumular rotación global
            accRot.Multiply(baseLocal);

            // 3. Calcular posición del siguiente joint
            // Rotamos el offset local (que es fijo) por la rotación acumulada actual
            VectorUtils3D rotatedOffset = accRot.Rotate(localBoneOffsets[i]);

            // Dibujar hueso virtual
            if (drawGizmos)
            {
                Vector3 startLine = currentPos.GetAsUnityVector();
                Vector3 endLine = (currentPos + rotatedOffset).GetAsUnityVector();
                Debug.DrawLine(startLine, endLine, Color.yellow);
            }

            currentPos = currentPos + rotatedOffset;
        }

        VectorUtils3D targetPos = VectorUtils3D.ToVectorUtils3D(target.position);
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
            joints[i].localRotation = baseR.GetAsUnityQuaternion();
        }

        // Alinear pie con suelo
        if (joints.Count > 0)
            joints[joints.Count - 1].rotation = target.rotation;
    }
}
