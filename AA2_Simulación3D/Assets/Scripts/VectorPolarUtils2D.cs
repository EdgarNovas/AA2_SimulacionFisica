using UnityEngine;

public class VectorPolarUtils2D : MonoBehaviour
{
    public float r, theta;
    private float PI = 3.1416f;

    public VectorPolarUtils2D(float r, float theta)
    {
        this.r = r;
        this.theta = theta;
    }

    public VectorPolarUtils2D()
    {
        this.r = 0;
        this.theta = 0;
    }

    public static VectorPolarUtils2D operator +(VectorPolarUtils2D a, VectorPolarUtils2D b)
    {
        VectorUtils2D tempVect = a.ConvertToCartesian() + b.ConvertToCartesian();

        return tempVect.ConvertToPolar();
    }
    public static VectorPolarUtils2D operator -(VectorPolarUtils2D a, VectorPolarUtils2D b)
    {

        VectorUtils2D tempVect = a.ConvertToCartesian() - b.ConvertToCartesian();

        return tempVect.ConvertToPolar();
    }

    public VectorUtils2D ConvertToCartesian()
    {
        float newX = r * System.MathF.Cos(theta);
        float newY = r * System.MathF.Sin(theta);

        return new VectorUtils2D(newX, newY);
    }

    public VectorPolarUtils2D EscalarByProduct(float a)
    {
        VectorUtils2D tempVect = ConvertToCartesian();
        tempVect = tempVect.EscalarByProduct(a);
        return tempVect.ConvertToPolar();
    }

    public float Magnitud()
    {
        VectorUtils2D tempVect = ConvertToCartesian();
        float magnitude = tempVect.Magnitud();
        return magnitude;
    }

    public VectorPolarUtils2D Normalize()
    {
        VectorUtils2D tempVect = ConvertToCartesian();
        tempVect = tempVect.Normalize();

        return tempVect.ConvertToPolar();
    }

    public float DotProduct(VectorPolarUtils2D b)
    {
        VectorUtils2D tempVect = ConvertToCartesian();
        VectorUtils2D cartesianB = ConvertToCartesian();

        return tempVect.DotProduct(cartesianB);
    }

    public float CrossProduct2D(VectorPolarUtils2D b)
    {
        VectorUtils2D tempVect = ConvertToCartesian();
        VectorUtils2D cartesianB = ConvertToCartesian();

        tempVect.CrossProduct2D(cartesianB);

        return tempVect.CrossProduct2D(cartesianB); ;
    }

    public float Angle(VectorPolarUtils2D b)
    {
        VectorUtils2D tempVect = ConvertToCartesian();
        VectorUtils2D cartesianB = ConvertToCartesian();

        return tempVect.Angle(cartesianB);
    }

    public VectorPolarUtils2D LERP(VectorPolarUtils2D a, float t)
    {
        if (t < 0 || t > 1)
        {
            UnityEngine.Debug.Log("Error: t debe ser entre 0 y 1");
            return new VectorPolarUtils2D();
        }

        float newR = r + (a.r - r) * t;

        float deltaTheta = ShortestAngleDifference(theta, a.theta);
        float interpolatedTheta = theta + t * deltaTheta;
        
        float newTheta = Mod2PI(interpolatedTheta);

        return new VectorPolarUtils2D(newR, newTheta);
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

    public string ToString()
    {
        return "(" + r + ", " + theta + ")";
    }
}
