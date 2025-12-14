using UnityEngine;
using QuaternionUtility;

public class FollowSpiderman : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] Transform bodyPivot;

    [SerializeField] float desiredDistance;
    [SerializeField] float slowingRadius;
    [SerializeField] float maxSpeed;
    [SerializeField] float maxForce;
    [SerializeField] float mass;

    [SerializeField] float maxTiltAngle = 15f;
    [SerializeField] float tiltResponsiveness = 5f;

    private VectorUtils3D position;
    private VectorUtils3D velocity;
    private VectorUtils3D acceleration;

    QuaternionUtils currentRotation;

    private void Start()
    {
        // Inicializar posición, velocidad y aceleración
        position = VectorUtils3D.ToVectorUtils3D(transform.position);
        velocity = VectorUtils3D.zero;
        acceleration = VectorUtils3D.zero;

        // Inicializar rotación actual del pivot
        currentRotation = new QuaternionUtils();
        currentRotation.AssignFromUnityQuaternion(bodyPivot.rotation);
    }

    private void Update()
    {
        // Cálculos físicos de MRUA para el movimiento
        VectorUtils3D steeringForce = CalculateSteeringForce();
        VectorUtils3D calculatedAcceleration = steeringForce * (1f / mass);

        acceleration.x = calculatedAcceleration.x;
        acceleration.z = calculatedAcceleration.z;

        velocity = velocity + acceleration * Time.deltaTime;
        position = position + velocity * Time.deltaTime;

        transform.position = position.GetAsUnityVector();

        // Actualizar la inclinación y orientación del cuerpo
        UpdateBodyTilt();
    }

    // Calcular una steering force de arrive
    private VectorUtils3D CalculateSteeringForce()
    {
        // Vector y distancia hacia el target
        VectorUtils3D toTarget = VectorUtils3D.ToVectorUtils3D(target.position) - position;
        float distance = toTarget.Magnitude();

        // Si ya estamos dentro de la distancia deseada, detener movimiento
        if (distance <= desiredDistance)
        {
            velocity = VectorUtils3D.zero;
            return VectorUtils3D.zero;
        }

        // Distancia efectiva desde el anillo deseado
        float effectiveDistance = distance - desiredDistance;

        // Velocidad objetivo
        float targetSpeed = maxSpeed;
        if (effectiveDistance < slowingRadius)
        {
            targetSpeed = maxSpeed * (effectiveDistance / slowingRadius);
        }

        // Limitar velocidad para no pasar del target
        targetSpeed = Mathf.Min(targetSpeed, effectiveDistance / Time.deltaTime);

        VectorUtils3D desiredVelocity = toTarget.Normalize() * targetSpeed;

        // Calcular fuerza de steering
        VectorUtils3D steeringForce = desiredVelocity - velocity;
        if (steeringForce.Magnitude() > maxForce)
        {
            steeringForce = steeringForce.Normalize() * maxForce;
        }

        return steeringForce;
    }

    // Actualizar inclinación y yaw del cuerpo
    private void UpdateBodyTilt()
    {
        // CALCULAR ROTACIÓN YAW

        // Vector hacia el target en XZ
        VectorUtils3D toTarget = VectorUtils3D.ToVectorUtils3D(target.position) - position;
        toTarget.y = 0f;

        // Quaternion de yaw mirando al target
        QuaternionUtils yawRotation = new QuaternionUtils();
        if (toTarget.Magnitude() > 0.001f)
        {
            toTarget = toTarget.Normalize();
            float yaw = System.MathF.Atan2(toTarget.x, toTarget.z);
            yawRotation.FromYRotation(yaw);
        }

        // CALCULAR ROTACIÓN TILT

        // Inclinación por velocidad en espacio local del pivot
        QuaternionUtils tiltRotation = new QuaternionUtils();
        float speed = velocity.Magnitude();
        float speedFactor = Mathf.Clamp01(speed / maxSpeed);

        if (speedFactor > 0.01f)
        {
            // Convertir velocidad al espacio local
            Vector3 localVel = bodyPivot.InverseTransformDirection(velocity.GetAsUnityVector());

            // Calculamos inclinación proporcional a la velocidad
            float tiltAmount = maxTiltAngle * speedFactor * Mathf.Deg2Rad;

            float forwardTilt = localVel.z * tiltAmount;  // pitch
            float sideTilt = -localVel.x * tiltAmount;    // roll

            VectorUtils3D tiltEuler = new VectorUtils3D(forwardTilt, 0f, sideTilt);
            tiltRotation = QuaternionUtils.FromEulerZYX(tiltEuler);
        }

        // COMBINACIÓN FINAL DE ROTACIONES

        // Combinar yaw y tilt
        QuaternionUtils targetRotation = new QuaternionUtils(yawRotation);
        targetRotation.Multiply(tiltRotation);

        // Interpolación suave hacia la rotación objetivo
        currentRotation = currentRotation.Slerp(
            targetRotation,
            Time.deltaTime * tiltResponsiveness
        );

        // Aplicar rotación solo al pivot
        bodyPivot.rotation = currentRotation.GetAsUnityQuaternion();
    }

    public VectorUtils3D GetVelocity()
    {
        return velocity;
    }

    public float GetMaxSpeed()
    {
        return maxSpeed;
    }
}
