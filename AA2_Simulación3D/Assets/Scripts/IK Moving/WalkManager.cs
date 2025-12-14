using UnityEngine;

public class WalkManager : MonoBehaviour
{
    public static WalkManager Instance;

    private bool isAnyLegMoving = false;
    private int lastLegID = -1; // -1 significa que nadie se ha movido aún

    void Awake()
    {
        Instance = this;
    }

    // Ahora pedimos permiso con el ID de la pierna
    public bool RequestStep(int legID)
    {
        // Si alguien se está moviendo no se pueden mover mas piernas
        if (isAnyLegMoving) return false;

        // Si esta pierna fue la ultima en moverse no puede repetir
        if (legID == lastLegID) return false;

        isAnyLegMoving = true;
        lastLegID = legID;
        return true;
    }

    public void FinishStep()
    {
        isAnyLegMoving = false;
    }
}
