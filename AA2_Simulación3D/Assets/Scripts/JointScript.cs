using UnityEngine;

public class JointScript : MonoBehaviour
{
    public Transform[] Joints;
    private float[] ls;

    public Transform target;

    public float alpha = 0.1f;
    private float tolerance = 1f;
    private float costFunction;

    private float[] theta;
    private float[] gradient;

    private VectorUtils3D[] limits;

    GradientMethod gradientMethod;

    void Start()
    {
        ls = new float[Joints.Length - 1];
        theta = new float[Joints.Length - 1];
        limits = new VectorUtils3D[Joints.Length];


        gradientMethod = GetComponent<GradientMethod>();
    }

    // Update is called once per frame
    void Update()
    {
        if (costFunction > tolerance)
        {

            //    gradient = gradientMethod.CalculateGradient(theta, target, ls);

            //    Vector3 newAlpha = AdaptativeLearningRate(gradient); //adaptative alpha * gradient
            //    theta += -newAlpha;

            //    theta = ApplyingConstraints(theta); //angle constraints

            //    ForwardKinematics(theta); //update position

        }
        //costFunction = Vector3.Distance(endEffector.position, target.position) * Vector3.Distance(endEffector.position, target.position);
    }
}
