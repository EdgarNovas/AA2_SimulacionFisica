
public class VectorUtils2D
{
    public float x, y, z;
    private float PI = 3.1416f;

    public VectorUtils2D(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public VectorUtils2D()
    {
        this.x = 0;
        this.y = 0;
    }

    public static VectorUtils2D operator + (VectorUtils2D a, VectorUtils2D b){
        
        return new VectorUtils2D(a.x + b.x , a.y + b.y);
    }
    public static VectorUtils2D operator - (VectorUtils2D a, VectorUtils2D b){

        return new VectorUtils2D(a.x - b.x, a.y - b.y);
    }

    public VectorUtils2D EscalarByProduct(float a)
    {        
        return new VectorUtils2D(this.x * a, this.y * a);
    }

    public float Magnitud()
    {
        return (float)System.MathF.Sqrt(x * x + y * y);
    }

    public VectorUtils2D Normalize()
    {
        float magnitude = Magnitud();
        if (magnitude == 0)
        {
            return new VectorUtils2D(0, 0);
        }
       
        return new VectorUtils2D(x / magnitude, y / magnitude);
    }

    public float DotProduct(VectorUtils2D b)
    {
        return x * b.x + y * b.y;
    }

    public float CrossProduct2D(VectorUtils2D b)
    {
        return x * b.y - y * b.x;
    }

    public float Angle(VectorUtils2D b)
    {
        float dot = DotProduct(b);
        float magnitudA = Magnitud();
        float magnitudB = b.Magnitud();

        if (magnitudA == 0 || magnitudB == 0)
        {
            return 0;
        }

        return (float)System.MathF.Acos(dot / (magnitudA * magnitudB)) * (180f / PI);
    }

    public VectorUtils2D LERP(VectorUtils2D a, float t)
    {
        if (t < 0 || t > 1)
        {
            UnityEngine.Debug.Log("Error: t debe ser entre 0 y 1");
            return new VectorUtils2D();
        }

        float newX = (1 - t) * x + t * a.x;
        float newY = (1 - t) * y + t * a.y;

        return new VectorUtils2D(newX, newY);
    }
    public VectorPolarUtils2D ConvertToPolar()
    {
        float newR = System.MathF.Sqrt(x * x + y * y);
        float newTheta = System.MathF.Atan(y / x);

        return new VectorPolarUtils2D(newR, newTheta);
    }

    public string ToString()
    {
        return "(" + x + ", " + y + ")";
    }
}