using UnityEngine;

public class ChaseManager : MonoBehaviour
{
    public Transform spiderman;
    public Transform idleTarget;

    private Transform currentTarget;

    public GradientMethod arm;

    bool isFollowingSpiderman = false;

    void Start()
    {
        currentTarget = idleTarget;
    }


    void Update()
    {
        SetTarget();
    }

    private void SetTarget()
    {
        if (isFollowingSpiderman)
        {
            currentTarget = spiderman;
        }
        else
        {
            currentTarget = idleTarget;
        }

        arm.target = currentTarget;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "spiderman")
        {
            isFollowingSpiderman = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "spiderman")
        {
            isFollowingSpiderman = false;
        }
    }
}
