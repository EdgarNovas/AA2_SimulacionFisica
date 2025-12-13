using System.Collections;
using UnityEngine;
using QuaternionUtility;

public class SmartLegStepper : MonoBehaviour
{
    [Header("Identificación (0:Izq, 1:Der)")]
    public int legID = 0;

    [Header("Referencias")]
    public Transform ikTarget;
    public Transform body;

    [Header("Configuración")]
    public float stepDistance = 0.5f;
    public float stepHeight = 0.3f;
    public float stepSpeed = 6.0f;    // Un poco más rápido para que no arrastre
    public bool dynamicSpeed = true;

    [Header("Posición Relativa (VectorUtils3D)")]
    // IMPORTANTE: X es el ancho. (0.3 Derecha, -0.3 Izquierda)
    public VectorUtils3D defaultOffset = new VectorUtils3D(0.3f, 0, 0);
    public float floorY = 0.0f;

    [Header("Corrección de Rotación para el pie")]
    // Si están del revés, prueba (0, 180, 0). Si miran de lado, (0, 90, 0).
    public Vector3 rotationOffset = new Vector3(0, 180, 0);

    // Internas
    private VectorUtils3D lastBodyPos;
    private VectorUtils3D currentVelocity;
    private bool isMoving = false;

    void Start()
    {
        // Auto-detectar suelo si se te olvida configurarlo
        if (floorY == 0.0f) floorY = ikTarget.position.y;

        lastBodyPos = VectorUtils3D.ToVectorUtils3D(body.position);
        currentVelocity = new VectorUtils3D(0, 0, 0);
    }

    void Update()
    {
        // 1. Calcular velocidad
        VectorUtils3D currentBodyPos = VectorUtils3D.ToVectorUtils3D(body.position);
        VectorUtils3D displacement = currentBodyPos - lastBodyPos;
        float invDt = 1.0f / Time.deltaTime;
        currentVelocity = displacement * invDt;
        lastBodyPos = currentBodyPos;

        if (isMoving) return;

        // 2. Calcular posición ideal (Raíles)
        VectorUtils3D idealPos = CalculateIdealPosition();

        // Posición actual proyectada al suelo
        VectorUtils3D currentTargetPos = VectorUtils3D.ToVectorUtils3D(ikTarget.position);
        currentTargetPos.y = floorY;

        // 3. Comprobar distancia
        float dist = VectorUtils3D.Distance(currentTargetPos, idealPos);

        if (dist > stepDistance)
        {
            // Pedir turno (Lógica que funcionaba bien)
            if (WalkManager.Instance == null || WalkManager.Instance.RequestStep(legID))
            {
                // --- EL ARREGLO PARA QUE NO VAYAN AL CENTRO ---

                // Antes calculábamos la predicción sumando velocidad pura.
                // Ahora forzamos que el destino respete el ancho (Offset X).

                // Paso 1: Predicción hacia ADELANTE (Dirección del cuerpo o velocidad)
                VectorUtils3D forwardPrediction = currentVelocity * (stepDistance * 0.5f);

                // Paso 2: Sumar la predicción al punto ideal (que ya tiene el ancho aplicado)
                VectorUtils3D targetPos = idealPos + forwardPrediction;

                // Paso 3: Asegurar suelo
                targetPos.y = floorY;

                StartCoroutine(MoveLeg(targetPos, dist));
            }
        }
    }

    VectorUtils3D CalculateIdealPosition()
    {
        // Calculamos el punto lateral exacto usando la derecha del cuerpo
        VectorUtils3D bodyRight = VectorUtils3D.ToVectorUtils3D(body.right);
        VectorUtils3D bodyPos = VectorUtils3D.ToVectorUtils3D(body.position);

        // Esto mantiene el "Raíl": Siempre a X distancia del centro del cuerpo
        VectorUtils3D lateralOffset = bodyRight * defaultOffset.x;

        // Sumamos un poco de offset Z para que el punto de reposo no sea tan atrás
        VectorUtils3D bodyFwd = VectorUtils3D.ToVectorUtils3D(body.forward);
        VectorUtils3D forwardOffset = bodyFwd * defaultOffset.z;

        VectorUtils3D finalPos = bodyPos + lateralOffset + forwardOffset;
        finalPos.y = floorY;
        return finalPos;
    }

    IEnumerator MoveLeg(VectorUtils3D destination, float distanceToTravel)
    {
        isMoving = true;
        VectorUtils3D startPos = VectorUtils3D.ToVectorUtils3D(ikTarget.position);

        // NUEVO: Guardamos la rotación inicial del pie y la final (la del cuerpo)
        QuaternionUtils startRot = new QuaternionUtils();
        startRot.AssignFromUnityQuaternion(ikTarget.rotation);

        QuaternionUtils bodyRot = new QuaternionUtils();
        bodyRot.AssignFromUnityQuaternion(body.rotation);

        // Convertimos el offset (0, 180, 0) a Quaternion
        QuaternionUtils correctionUnity = new QuaternionUtils();
        VectorUtils3D rotOffset = new VectorUtils3D();
        rotOffset.AssignFromUnityVector(rotationOffset);
        correctionUnity = correctionUnity.Euler(rotOffset);



        QuaternionUtils endRot = new QuaternionUtils(bodyRot);
        endRot.Multiply(correctionUnity);

        float t = 0f;

        // Velocidad
        float actualSpeed = stepSpeed;
        if (dynamicSpeed && distanceToTravel > stepDistance * 1.5f)
        {
            actualSpeed *= 1.5f;
        }

        while (t < 1f)
        {
            t += Time.deltaTime * actualSpeed;
            if (t > 1f) t = 1f;

            // Interpolación Lineal
            VectorUtils3D currentPos = startPos.LERP(destination, t);

            // Arco Seno
            float height = System.MathF.Sin(t * MathFUtils.PI) * stepHeight;
            currentPos.y = floorY + height;

            ikTarget.position = new Vector3(currentPos.x, currentPos.y, currentPos.z);

            // Usamos Slerp (Spherical Lerp) para que la rotación sea suave
            
            ikTarget.rotation = QuaternionUtils.Slerp(startRot, endRot, t).ToUnityQuaternion();

            yield return null;
        }

        // Aterrizaje
        ikTarget.position = new Vector3(destination.x, destination.y, destination.z);
        ikTarget.rotation = endRot.ToUnityQuaternion();
        isMoving = false;

        if (WalkManager.Instance != null)
        {
            WalkManager.Instance.FinishStep();
        }
    }


}
