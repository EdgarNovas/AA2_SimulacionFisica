using System.Collections;
using UnityEngine;

public class SmartLegStepper : MonoBehaviour
{
    [Header("Referencias")]
    public Transform ikTarget;    // La esfera roja
    public Transform body;        // La pelvis

    [Header("Configuración")]
    public float stepDistance = 0.5f;
    public float stepHeight = 0.3f;
    public float stepSpeed = 4.0f;

    [Header("Posición Relativa")]
    // X: Lado (0.3 Derecha, -0.3 Izquierda)
    // Z: Adelante (0 normalmente)
    public Vector3 defaultOffset = new Vector3(0.3f, 0, 0);
    public float floorY = 0.0f;

    private Vector3 lastBodyPos;
    private Vector3 currentVelocity;
    private bool isMoving = false;

    void Start()
    {
        lastBodyPos = body.position;
    }

    void Update()
    {
        // 1. Calcular Velocidad Real (Hacia donde se mueve el cuerpo)
        Vector3 displacement = body.position - lastBodyPos;
        currentVelocity = displacement / Time.deltaTime;
        lastBodyPos = body.position;

        if (isMoving) return;

        // 2. Calcular dónde debería estar el pie
        Vector3 idealPos = CalculateIdealPosition();

        // 3. Comprobar distancia
        float dist = Vector3.Distance(new Vector3(ikTarget.position.x, floorY, ikTarget.position.z), idealPos);

        if (dist > stepDistance)
        {
            // 4. PREGUNTAR AL MANAGER: ¿Puedo moverme?
            // Si el Manager no existe (olvidaste ponerlo), se mueve igual.
            if (WalkManager.Instance == null || WalkManager.Instance.RequestStep())
            {
                // Predicción: Lanzar el pie un poco más lejos en la dirección del movimiento
                Vector3 targetPos = idealPos + (currentVelocity.normalized * (stepDistance * 0.4f));
                targetPos.y = floorY;

                StartCoroutine(MoveLeg(targetPos));
            }
        }
    }

    Vector3 CalculateIdealPosition()
    {
        // Posición base al lado del cuerpo
        Vector3 sidePos = body.position + (body.right * defaultOffset.x);

        // Si nos movemos, alineamos el "ideal" con la dirección del movimiento
        // Si estamos quietos, usamos la rotación del cuerpo
        if (currentVelocity.magnitude > 0.1f)
        {
            // Opcional: Girar un poco el offset basándose en la velocidad
            // Pero para simplificar, mantenemos el offset lateral relativo al cuerpo
            // y la posición Z relativa a la velocidad no afecta tanto aquí.
        }

        Vector3 finalPos = sidePos;
        finalPos.y = floorY;
        return finalPos;
    }

    IEnumerator MoveLeg(Vector3 destination)
    {
        isMoving = true;
        Vector3 startPos = ikTarget.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * stepSpeed;

            Vector3 currentPos = Vector3.Lerp(startPos, destination, t);
            float height = Mathf.Sin(t * Mathf.PI) * stepHeight;
            currentPos.y = floorY + height;

            ikTarget.position = currentPos;
            yield return null;
        }

        ikTarget.position = destination;

        isMoving = false;

        // 5. AVISAR AL MANAGER: He terminado, que pase el siguiente
        if (WalkManager.Instance != null)
        {
            WalkManager.Instance.FinishStep();
        }
    }

    void OnDrawGizmos()
    {
        if (body == null) return;
        Gizmos.color = Color.blue;
        Vector3 ideal = body.position + (body.right * defaultOffset.x);
        ideal.y = floorY;
        Gizmos.DrawWireSphere(ideal, 0.1f);
    }
}
