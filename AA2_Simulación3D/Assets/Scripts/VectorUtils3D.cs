
using QuaternionUtility;
using System.Diagnostics;
using System.Numerics;

public class VectorUtils3D
{
    public float x, y, z;
    

    public VectorUtils3D(float x, float y, float z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public VectorUtils3D()
    {
        this.x = 0;
        this.y = 0;
        this.z = 0;
    }

    public static VectorUtils3D operator + (VectorUtils3D a , VectorUtils3D b){
        
        return new VectorUtils3D(a.x + b.x , a.y + b.y, a.z + b.z);
    }
    public static VectorUtils3D operator - (VectorUtils3D a , VectorUtils3D b){

        return new VectorUtils3D(a.x - b.x, a.y - b.y, a.z - b.z);
    }

    public static VectorUtils3D operator *(VectorUtils3D a, VectorUtils3D b)
    {
        return new VectorUtils3D(a.x * b.x, a.y * b.y, a.z * b.z);
    }
    public static VectorUtils3D operator *(VectorUtils3D a, float b)
    {
        return new VectorUtils3D(a.x * b, a.y * b, a.z * b);
    }

    public static VectorUtils3D forward => new VectorUtils3D(0, 0, 1);
    public static VectorUtils3D back => new VectorUtils3D(0, 0, -1);
    public static VectorUtils3D up => new VectorUtils3D(0, 1, 0);
    public static VectorUtils3D down => new VectorUtils3D(0, -1, 0);
    public static VectorUtils3D right => new VectorUtils3D(1, 0, 0);
    public static VectorUtils3D left => new VectorUtils3D(-1, 0, 0);
    public static VectorUtils3D zero => new VectorUtils3D(0, 0, 0);
    public static VectorUtils3D one => new VectorUtils3D(1, 1, 1);

    public VectorUtils3D EscalarByProduct(float a)
    {
       
       return new VectorUtils3D(this.x * a, this.y * a, this.z * a);
    }

    public float Magnitude()
    {
        return (float)System.MathF.Sqrt(x * x + y * y + z * z);
    }

    public VectorUtils3D Normalize()
    {
        float magnitude = Magnitude();
        if (magnitude == 0)
        {
            return new VectorUtils3D(0, 0, 0);
        }

        return new VectorUtils3D(x / magnitude, y / magnitude, z / magnitude);
    }

    public float DotProduct(VectorUtils3D b)
    {
        return x * b.x + y * b.y + z * b.z;
    }

    public VectorUtils3D CrossProduct3D(VectorUtils3D b)
    {
        float newX = y * b.z - z * b.y;
        float newY = z * b.x - x * b.z;
        float newZ = x * b.y - y * b.x;

        return new VectorUtils3D(newX, newY, newZ);
    }

    public float Angle(VectorUtils3D b)
    {
        float dot = DotProduct(b);
        float magnitudA = Magnitude();
        float magnitudB = b.Magnitude();

        if (magnitudA == 0 || magnitudB == 0)
        {
            return 0;
        }

        return (float)System.MathF.Acos(dot / (magnitudA * magnitudB)) * (180f / MathFUtils.PI);
    }

    public VectorPolarUtils3D ConvertToSpherical()
    {
        float newP = System.MathF.Sqrt(x * x + y * y + z * z);
        float newTheta = System.MathF.Atan(y / x);
        float newPhi = System.MathF.Acos(z / newP);

        return new VectorPolarUtils3D(newP, newTheta, newPhi);
    }

    public static VectorUtils3D ToVectorUtils3D(UnityEngine.Vector3 v)
    {
        VectorUtils3D retVec = new VectorUtils3D();
        retVec.x = v.x;
        retVec.y = v.y;
        retVec.z = v.z;
        return retVec;
    }

    /// <summary>
    /// t tiene que ser entre (0 <= t <= 1)
    /// </summary>
    /// <param name="a"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public VectorUtils3D LERP(VectorUtils3D a,  float t)
    {
        if(t < 0 || t > 1)
        {
            UnityEngine.Debug.Log("Error: t debe ser entre 0 y 1");
            return new VectorUtils3D();
        }

        float newX = (1 - t) * x + t * a.x;
        float newY = (1 - t) * y + t * a.y;
        float newZ = (1 - t) * z + t * a.z;

        return new VectorUtils3D(newX, newY, newZ);
    }

    public static float Distance(VectorUtils3D firstV, VectorUtils3D secondV)
    {
        float dx = firstV.x - secondV.x;
        float dy = firstV.y - secondV.y;
        float dz = firstV.z - secondV.z;
        return System.MathF.Sqrt(dx * dx + dy * dy + dz * dz);
    }

    public string ToString()
    {
        return "(" + x + ", " + y + ", " + z + ")";
    }

    public void AssignFromUnityVector(UnityEngine.Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }

    public UnityEngine.Vector3 GetAsUnityVector()
    {
        return new UnityEngine.Vector3(x, y, z);
    }
}