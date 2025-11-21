using UnityEngine;
[RequireComponent (typeof(Rigidbody))]
public class MoveVectorWithRB : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    VectorUtils3D myVector;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        myVector = VectorUtils3D.right;
    }
   
    void FixedUpdate()
    {
        Vector3 movement = myVector.GetAsUnityVector();
        rb.AddForce(movement * 5);
    }
}
