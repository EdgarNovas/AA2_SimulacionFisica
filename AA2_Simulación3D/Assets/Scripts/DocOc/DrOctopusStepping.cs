using UnityEngine;

public class DrOctopusStepping : MonoBehaviour
{
    enum Side { Left, Right }

    [SerializeField] Transform drOctopusBodyCenter;
    [SerializeField] FollowSpiderman drOctopusFollowScript;
    [SerializeField] Transform leftPlacementObj;
    [SerializeField] Transform rightPlacementObj;

    [SerializeField] float sideDistanceToStep = 1f;     // distancia hacia los lados
    [SerializeField] float forwardDistanceToStep = 1f;  // distancia máxima hacia delante

    [SerializeField] float stepInterval = 0.4f; // tiempo entre pasos

    float FLOOR_HEIGHT = -0.5f;

    float stepTimer;
    float weightedForwardDistance;
    Side nextStepSide = Side.Left;

    void Start()
    {
        // Colocar los brazos inicialmente
        SetNewPlacement(Side.Left);
        SetNewPlacement(Side.Right);
    }

    void Update()
    {
        stepTimer += Time.deltaTime;

        if (stepTimer >= stepInterval)
        {
            stepTimer = 0f;

            // Teleport del target del brazo que toca
            SetNewPlacement(nextStepSide);

            // Alternar lado
            nextStepSide = (nextStepSide == Side.Left) ? Side.Right : Side.Left;
        }
    }

    private VectorUtils3D GetPlacementPos(VectorUtils3D bodyPosition, VectorUtils3D forwardDir, Side side)
    {
        // Ignorar Y y normalizar la dirección forward
        forwardDir.y = 0;
        forwardDir = forwardDir.Normalize();

        // Offset hacia delante
        VectorUtils3D forwardOffset = forwardDir * weightedForwardDistance;

        // Offset lateral, default es hacia la derecha e izquierda es invertido
        VectorUtils3D lateralDir = VectorUtils3D.up.CrossProduct3D(forwardDir).Normalize();
        if (side == Side.Left)
            lateralDir *= -1;
        VectorUtils3D lateralOffset = lateralDir * sideDistanceToStep;

        // Calcular posición del target sumando los offsets
        VectorUtils3D placement = bodyPosition + forwardOffset + lateralOffset;

        placement.y = FLOOR_HEIGHT;
        return placement;
    }

    private void SetNewPlacement(Side side)
    {
        UpdateWeightedForwardDistance();

        VectorUtils3D bodyPosition = VectorUtils3D.ToVectorUtils3D(drOctopusBodyCenter.position);
        VectorUtils3D forwardDir = VectorUtils3D.ToVectorUtils3D(drOctopusBodyCenter.forward);

        VectorUtils3D newPlacementPos = GetPlacementPos(bodyPosition, forwardDir, side);

        // Actualizar la posición del target en el objeto real
        if (side == Side.Left)
            leftPlacementObj.position = newPlacementPos.GetAsUnityVector();
        else
            rightPlacementObj.position = newPlacementPos.GetAsUnityVector();
    }

    // Actualizar la distancia hacia delante que se usará para el offset en base a la velocidad que lleva el dr octopus
    private void UpdateWeightedForwardDistance()
    {
        float velocityMagnitude = drOctopusFollowScript.GetVelocity().Magnitude();

        float distanceMultiplier = velocityMagnitude / drOctopusFollowScript.GetMaxSpeed();
        
        weightedForwardDistance = forwardDistanceToStep * distanceMultiplier;
        if (weightedForwardDistance < 0.01f)
            weightedForwardDistance = 0.01f;
    }
}
