using UnityEngine;

public class WalkManager : MonoBehaviour
{
    public static WalkManager Instance;

    private bool isAnyLegMoving = false;

    void Awake()
    {
        Instance = this;
    }

    
    public bool RequestStep()
    {
        // Si nadie se mueve, digo que SÍ y bloqueo a los demás
        if (!isAnyLegMoving)
        {
            isAnyLegMoving = true;
            return true;
        }
        // Si alguien se mueve, digo que NO
        return false;
    }

    // Avisar: ¡Ya he terminado!
    public void FinishStep()
    {
        isAnyLegMoving = false;
    }
}
