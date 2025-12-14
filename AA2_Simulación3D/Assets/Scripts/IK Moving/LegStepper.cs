using System.Collections;
using UnityEngine;

public class LegStepper : MonoBehaviour
{
    [Header("Asignaciones")]
    public Transform ikTarget;    
    public Transform body;       

    [Header("Configuración fácil")]
    public float stepDistance = 0.6f; // Distancia para dar el paso
    public float stepHeight = 0.3f;   // Altura que levanta el pie
    public float stepSpeed = 3.0f;    // Velocidad del paso

    [Header("Posición Ideal")]
    // Ajusta esto: X es lado, Z es adelante/atrás
    public Vector3 offsetFromBody = new Vector3(0.3f, 0, 0.2f);
    public float floorY = 0.0f; // La altura del suelo

    private bool isMoving = false;

    void Update()
    {
        // 1. Calcular dónde debería estar el pie (Home Position)
        // Usamos body.position y sumamos los vectores de dirección locales
        // Esto es más seguro que TransformPoint si hay escalas raras
        Vector3 idealPos = body.position
                           + (body.right * offsetFromBody.x)
                           + (body.forward * offsetFromBody.z);

        // Forzamos la altura al suelo
        idealPos.y = floorY;

        // 2. Si el pie ya se está moviendo, no hacemos nada
        if (isMoving) return;

        // 3. Comprobar distancia: ¿El pie real se ha alejado del ideal?
        float dist = Vector3.Distance(new Vector3(ikTarget.position.x, floorY, ikTarget.position.z), idealPos);

        if (dist > stepDistance)
        {
            // Predicción simple: Damos el paso un poco por delante del punto ideal
            // para que cuando el pie aterrice, el cuerpo no lo haya dejado atrás ya.
            Vector3 targetPos = idealPos + (body.forward * (stepDistance * 0.5f));
            targetPos.y = floorY;

            StartCoroutine(MoveLeg(targetPos));
        }
    }

    IEnumerator MoveLeg(Vector3 destination)
    {
        isMoving = true;
        Vector3 startPos = ikTarget.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * stepSpeed;

            // Interpolación Lineal (Movimiento al frente)
            Vector3 currentPos = Vector3.Lerp(startPos, destination, t);

            // Interpolación Seno (Arco hacia arriba)
            // Solo sumamos altura si estamos en medio del paso (t entre 0 y 1)
            float height = Mathf.Sin(t * Mathf.PI) * stepHeight;
            currentPos.y = floorY + height;

            ikTarget.position = currentPos;
            yield return null;
        }

        // Asegurar posición final exacta
        ikTarget.position = destination;
        isMoving = false;
    }

    // --- ESTO ES LO QUE TE AYUDARÁ A VER EL ERROR ---
    void OnDrawGizmos()
    {
        if (body == null) return;

        // Calcular punto ideal visualmente
        Vector3 idealPos = body.position
                           + (body.right * offsetFromBody.x)
                           + (body.forward * offsetFromBody.z);
        idealPos.y = floorY;

        // Dibuja una esfera AZUL donde el código cree que debe ir el pie
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(idealPos, 0.1f);

        // Dibuja una línea ROJA desde el pie actual hasta el ideal
        if (ikTarget != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(ikTarget.position, idealPos);
        }
    }
}
