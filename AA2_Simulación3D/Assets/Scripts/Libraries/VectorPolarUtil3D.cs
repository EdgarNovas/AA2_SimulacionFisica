using UnityEngine;

public class VectorPolarUtils3D : MonoBehaviour
{
    public float p, theta, phi;
    private float PI = 3.1416f;

    public VectorPolarUtils3D(float p, float theta, float phi)
    {
        this.p = p;
        this.theta = theta;
        this.phi = phi;
    }

    public VectorPolarUtils3D()
    {
        this.p = 0;
        this.theta = 0;
        this.phi = 0;
    }

    public static VectorPolarUtils3D operator +(VectorPolarUtils3D a, VectorPolarUtils3D b)
    {
        VectorUtils3D tempVect = a.ConvertToCartesian() + b.ConvertToCartesian();

        return tempVect.ConvertToSpherical();
    }
    public static VectorPolarUtils3D operator -(VectorPolarUtils3D a, VectorPolarUtils3D b)
    {

        VectorUtils3D tempVect = a.ConvertToCartesian() - b.ConvertToCartesian();

        return tempVect.ConvertToSpherical();
    }

    public VectorUtils3D ConvertToCartesian()
    {
        float newX = p * System.MathF.Sin(phi) * System.MathF.Cos(theta);
        float newY = p * System.MathF.Sin(phi) * System.MathF.Sin(theta);
        float newZ = p * System.MathF.Cos(phi);

        return new VectorUtils3D(newX, newY, newZ);
    }

    public VectorPolarUtils3D EscalarByProduct(float a)
    {
        VectorUtils3D tempVect = ConvertToCartesian();
        tempVect = tempVect.EscalarByProduct(a);
        return tempVect.ConvertToSpherical();
    }

    public float Magnitud()
    {
        VectorUtils3D tempVect = ConvertToCartesian();
        float magnitude = tempVect.Magnitude();
        return magnitude;
    }

    public VectorPolarUtils3D Normalize()
    {
        VectorUtils3D tempVect = ConvertToCartesian();
        tempVect = tempVect.Normalize();

        return tempVect.ConvertToSpherical();
    }

    public float DotProduct(VectorPolarUtils3D b)
    {
        VectorUtils3D tempVect = ConvertToCartesian();
        VectorUtils3D cartesianB = ConvertToCartesian();

        return tempVect.DotProduct(cartesianB);
    }

    public VectorPolarUtils3D CrossProduct3D(VectorPolarUtils3D b)
    {
        VectorUtils3D tempVect = ConvertToCartesian();
        VectorUtils3D cartesianB = ConvertToCartesian();

        tempVect = tempVect.CrossProduct3D(cartesianB);

        return tempVect.ConvertToSpherical(); ;
    }

    public float Angle(VectorPolarUtils3D b)
    {
        VectorUtils3D tempVect = ConvertToCartesian();
        VectorUtils3D cartesianB = ConvertToCartesian();

        return tempVect.Angle(cartesianB);
    }

    public VectorPolarUtils3D LERP(VectorPolarUtils3D a, float t)
    {
        if (t < 0 || t > 1)
        {
            UnityEngine.Debug.Log("Error: t debe ser entre 0 y 1");
            return new VectorPolarUtils3D();
        }

        float newP = p + (a.p - p) * t;

        float deltaTheta = ShortestAngleDifference(theta, a.theta);
        float newTheta = Mod2PI(theta + t * deltaTheta);

        float deltaPhi = ShortestAngleDifference(phi, a.phi);
        float newPhi = Clamp(phi + deltaPhi * t, 0, PI);

        return new VectorPolarUtils3D(newP, newTheta, newPhi);
    }

    private float Mod2PI(float angle)
    {
        float result = angle % (PI * 2);
        return result < 0 ? result + (PI * 2) : result;
    }

    private float ShortestAngleDifference(float from, float to)
    {
        float difference = (to - from + PI) % (2 * PI) - PI;
        return difference < -PI ? difference + (2 * PI) : difference;
    }

    private float Clamp(float value, float min, float max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    public string ToString()
    {
        return "(" + p + ", " + theta + "," + phi + ")";
    }
}
