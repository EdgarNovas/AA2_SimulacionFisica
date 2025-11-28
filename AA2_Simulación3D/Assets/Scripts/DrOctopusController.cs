using System.Collections.Generic;
using UnityEngine;

public class DrOctopusController : MonoBehaviour
{
    List<Transform> armJoints;
    public Transform mainRoot;

    private const int JOINTS_COUNT = 39;

    void Start()
    {
        armJoints = new List<Transform>();
        Transform currentParent = mainRoot;
        for (int i = 0; i < JOINTS_COUNT; i++)
        {
            Transform newJoint = currentParent.GetChild(0).transform;
            armJoints.Add(newJoint);
            currentParent = newJoint;
        }

        for (int i = 0; i < armJoints.Count; i++)
        {
            armJoints[i].localEulerAngles = Vector3.zero;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
