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

    // Ahora pedimos permiso con el DNI (ID) de la pierna
    public bool RequestStep(int legID)
    {
        // 1. Si alguien se está moviendo, NADIE puede moverse
        if (isAnyLegMoving) return false;

        // 2. Si TÚ fuiste el último en moverte, NO puedes repetir. Deja al otro.
        if (legID == lastLegID) return false;

        // Si pasamos los filtros, concedemos el permiso
        isAnyLegMoving = true;
        lastLegID = legID;
        return true;
    }

    public void FinishStep()
    {
        isAnyLegMoving = false;
    }
}
