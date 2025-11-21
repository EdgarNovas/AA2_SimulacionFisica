using UnityEngine;
using QuaternionUtility;
using System;

public class GradientMethod : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    VectorUtils3D GetJointPosition(VectorUtils3D oldJoint, VectorUtils3D rotationAxis, Transform[] joints, int index)
    {
        VectorUtils3D newPosition;

        QuaternionUtils rot1 = QuaternionUtils.FromEulerZYX(rotationAxis * QuaternionUtils.Rad2Deg);
        newPosition = VectorUtils3D.ToVectorUtils3D(joints[index].position) + rot1.Rotate(VectorUtils3D.ToVectorUtils3D(joints[index + 1].position) - oldJoint);

        return newPosition;
    }

    public float[] CalculateGradient(float[] thetas, Transform target, Transform[] joints, float[] ls)
    {
        float step = 0.0001f;

        int n = thetas.Length;
        float[] gradient = new float[n];


        float baseCost = Cost(thetas, target, joints, ls);

        for (int i = 0; i < n; i++) {
            float[] thetasPlus = new float[n];
            Array.Copy(thetas, thetasPlus, n);

            thetasPlus[i] += step;

            float costPlus = Cost(thetasPlus, target, joints, ls);

            gradient[i] = (costPlus - baseCost) / step;
        }

        return gradient;
    }

    float Cost(float[] thetas, Transform target, Transform[] joints, float[] ls) {
        VectorUtils3D endPos = ForwardKinematicsPosition(thetas, joints, ls);
        float dist = VectorUtils3D.Distance(endPos, VectorUtils3D.ToVectorUtils3D(target.position));
        return dist * dist;
    }

    VectorUtils3D ForwardKinematicsPosition(float[] thetas, Transform[] joints, float[] ls)
    {
        VectorUtils3D pos = VectorUtils3D.ToVectorUtils3D(joints[0].position);
        float accumulatedAngle = 0f;

        for (int i = 0; i < thetas.Length; i++) {
            accumulatedAngle += thetas[i];

            VectorUtils3D axis = new VectorUtils3D(0, 0, accumulatedAngle * QuaternionUtils.Rad2Deg);
            QuaternionUtils rot = QuaternionUtils.FromEulerZYX(axis);

            //ERROR 
            //pos += rot.Multiply(VectorUtils3D.right * ls[i]);
        }

        return pos;
    }

}
