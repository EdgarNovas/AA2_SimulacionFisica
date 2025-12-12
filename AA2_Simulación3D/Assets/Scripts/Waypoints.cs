using UnityEngine;

public class Waypoints : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Arrastra aquí los objetos que servirán como puntos de destino")]
    public Transform[] waypoints;

    [Tooltip("Velocidad de movimiento en unidades por segundo")]
    public float speed = 5f;

    [Tooltip("Distancia mínima para considerar que ha llegado al punto")]
    public float threshold = 0.1f;

    // Índice para saber hacia qué waypoint nos dirigimos actualmente
    private int currentWaypointIndex = 0;

    void Update()
    {
        // Seguridad: Si no hay waypoints asignados, no hacemos nada
        if (waypoints.Length == 0) return;

        // 1. Identificar el objetivo actual
        Transform targetWaypoint = waypoints[currentWaypointIndex];

        // 2. Mover el objeto hacia el objetivo (MoveTowards asegura velocidad constante)
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetWaypoint.position,
            speed * Time.deltaTime
        );

        // Opcional: Que el objeto mire hacia donde va
        transform.LookAt(targetWaypoint);

        // 3. Comprobar si hemos llegado (distancia muy pequeña)
        if (Vector3.Distance(transform.position, targetWaypoint.position) < threshold)
        {
            // Pasamos al siguiente índice
            currentWaypointIndex++;

            // 4. Lógica de LOOP: Si el índice supera el tamaño del array, volvemos a 0
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = 0;
            }
        }
    }
}
