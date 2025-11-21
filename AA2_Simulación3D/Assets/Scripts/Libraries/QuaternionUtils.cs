
using Unity.VisualScripting;
using UnityEngine.UIElements;
using UnityEngine;
using System;

namespace QuaternionUtility
{


    public class QuaternionUtils
    {
        float w;
        float i;
        float j;
        float k;

        const float epsilon = 0.01f;

        public const float Degree2Rad = MathF.PI / 180f;

        public const float Rad2Deg = 57.29578f;

        public QuaternionUtils()
        {
            this.w = 1f;
            this.i = 0f;
            this.j = 0f;
            this.k = 0f;
        }

        public QuaternionUtils(float w, float i, float j, float k)
        {
            this.w = w;
            this.i = i;
            this.j = j;
            this.k = k;
        }

        public QuaternionUtils(QuaternionUtils q)
        {
            this.w = q.w;
            this.i = q.i;
            this.j = q.j;
            this.k = q.k;
        }




        /// <summary>
        /// THIS PASSES A CUATERNION AND A VECTOR AND RETURNS A QUATERNION ALREADY ANGLED
        /// </summary>
        /// <param name="v"></param>
        /// <param name="angle"></param>
        /// <returns></returns>

        public QuaternionUtils AngleToQuaternion(VectorUtils3D v, float angle)
        {
            QuaternionUtils a = new QuaternionUtils();

            v.Normalize();
            a.w = System.MathF.Cos(angle / 2);
            float c = System.MathF.Sin(angle / 2);

            a.i = c * v.x;
            a.j = c * v.y;
            a.k = c * v.z;



            return a;
        }

        public float ToAngle(QuaternionUtils a)
        {
            VectorUtils3D v = new VectorUtils3D(0f, 0f, 0f);
            float angle = 2.0f * System.MathF.Acos(a.w);
            float divider = System.MathF.Sqrt(1.0f - a.w * a.w);

            if (divider != 0.0)
            {
                // Calculate the axis
                v.x = a.i / divider;
                v.y = a.j / divider;
                v.z = a.k / divider;
            }
            else
            {
                // Arbitrary normalized axis
                v.x = 1;
                v.y = 0;
                v.z = 0;
            }

            return angle;

        }

        public void FromXRotation(float angle)
        {

            VectorUtils3D axis = new VectorUtils3D(1.0f, 0, 0);
            QuaternionUtils newQuad = AngleToQuaternion(axis, angle);

            w = newQuad.w;
            i = newQuad.i;
            j = newQuad.j;
            k = newQuad.k;
        }

        public void FromYRotation(float angle)
        {

            VectorUtils3D axis = new VectorUtils3D(0, 1.0f, 0);
            QuaternionUtils newQuad = AngleToQuaternion(axis, angle);

            w = newQuad.w;
            i = newQuad.i;
            j = newQuad.j;
            k = newQuad.k;
        }

        public void FromZRotation(float angle)
        {

            VectorUtils3D axis = new VectorUtils3D(0, 0, 1.0f);
            QuaternionUtils newQuad = AngleToQuaternion(axis, angle);

            w = newQuad.w;
            i = newQuad.i;
            j = newQuad.j;
            k = newQuad.k;
        }


        public static QuaternionUtils FromEulerZYX(VectorUtils3D eulerZYX)
        {


            // Based on https://en.wikipedia.org/wiki/Conversion_between_quaternions_and_Euler_angles
            float cy = System.MathF.Cos(eulerZYX.z * 0.5f);
            float sy = System.MathF.Sin(eulerZYX.z * 0.5f);
            float cr = System.MathF.Cos(eulerZYX.x * 0.5f);
            float sr = System.MathF.Sin(eulerZYX.x * 0.5f);
            float cp = System.MathF.Cos(eulerZYX.y * 0.5f);
            float sp = System.MathF.Sin(eulerZYX.y * 0.5f);
            QuaternionUtils output = new QuaternionUtils();
            output.w = cy * cr * cp + sy * sr * sp;
            output.i = cy * sr * cp - sy * cr * sp;
            output.j = cy * cr * sp + sy * sr * cp;
            output.k = sy * cr * cp - cy * sr * sp;

            return output;

        }

        public VectorUtils3D _toEulerZYX(QuaternionUtils q)
        {
            VectorUtils3D output = new VectorUtils3D();
            // Roll (x-axis rotation)
            float sinr_cosp = +2.0f * (q.w * q.i + q.j * q.k);
            float cosr_cosp = +1.0f - 2.0f * (q.i * q.i + q.j * q.j);
            output.x = System.MathF.Atan2(sinr_cosp, cosr_cosp);

            // Pitch (y-axis rotation)
            float sinp = +2.0f * (q.w * q.j - q.k * q.i);
            if (System.MathF.Abs(sinp) >= 1)
                output.y = CopySign(VectorUtils3D.PI / 2, sinp); // use 90 degrees if out of range
            else
                output.y = System.MathF.Asin(sinp);

            // Yaw (z-axis rotation)
            float siny_cosp = +2.0f * (q.w * q.k + q.i * q.j);
            float cosy_cosp = +1.0f - 2.0f * (q.j * q.j + q.k * q.k);
            output.z = System.MathF.Atan2(siny_cosp, cosy_cosp);

            return output;
        }

        public float Dot(QuaternionUtils q)
        {
            return w * q.w + i * q.i + j * q.j + k * q.k;
        }
        public float Norm()
        {
            return System.MathF.Sqrt(w * w + i * i + j * j + k * k);
        }


        public void Normalize()
        {
            float len = Norm();

            w = w / len;
            i = i / len;
            j = j / len;
            k = k / len;

        }

        public static float AngleBetween(QuaternionUtils q1, QuaternionUtils q2)
        {
            float dot = q1.Dot(q2);

            dot = Min(1f, Abs(dot));

            return 2f * System.MathF.Acos(dot); 
        }

        public static float Abs(float x)
        {
            return x < 0f ? -x : x;
        }

        public static float Min(float a, float b)
        {
            return a < b ? a : b;
        }

        public void Multiply(QuaternionUtils q)
        {
            /*
            Formula from http://www.euclideanspace.com/maths/algebra/realNormedAlgebra/quaternions/arithmetic/index.htm
                     a*e - b*f - c*g - d*h
                + i (b*e + a*f + c*h- d*g)
                + j (a*g - b*h + c*e + d*f)
                + k (a*h + b*g - c*f + d*e)
            */
            float tw = w, ti = i, tj = j, tk = k;

            w = tw * q.w - ti * q.i - tj * q.j - tk * q.k;
            i = tw * q.i + ti * q.w + tj * q.k - tk * q.j;
            j = tw * q.j - ti * q.k + tj * q.w + tk * q.i;
            k = tw * q.k + ti * q.j - tj * q.i + tk * q.w;
        }

        public VectorUtils3D Rotate(VectorUtils3D v)
        {
            VectorUtils3D result = new VectorUtils3D();

            float ww = w * w;
            float xx = i * i;
            float yy = j * j;
            float zz = k * k;
            float wx = w * i;
            float wy = w * j;
            float wz = w * k;
            float xy = i * j;
            float xz = i * k;
            float yz = j * k;

            // Formula from http://www.euclideanspace.com/maths/algebra/realNormedAlgebra/quaternions/transforms/index.htm
            // p2.x = w*w*p1.x + 2*y*w*p1.z - 2*z*w*p1.y + x*x*p1.x + 2*y*x*p1.y + 2*z*x*p1.z - z*z*p1.x - y*y*p1.x;
            // p2.y = 2*x*y*p1.x + y*y*p1.y + 2*z*y*p1.z + 2*w*z*p1.x - z*z*p1.y + w*w*p1.y - 2*x*w*p1.z - x*x*p1.y;
            // p2.z = 2*x*z*p1.x + 2*y*z*p1.y + z*z*p1.z - 2*w*y*p1.x - y*y*p1.z + 2*w*x*p1.y - x*x*p1.z + w*w*p1.z;

            result.x = ww * v.x + 2f * wy * v.z - 2f * wz * v.y +
                        xx * v.x + 2 * xy * v.y + 2f * xz * v.z -
                        zz * v.x - yy * v.x;

            result.y = 2 * xy * v.x + yy * v.y + 2 * yz * v.z +
                        2 * wz * v.x - zz * v.y + ww * v.y -
                        2 * wx * v.z - xx * v.y;

            result.z = 2 * xz * v.x + 2 * yz * v.y + zz * v.z -
                        2 * wy * v.x - yy * v.z + 2 * wx * v.y -
                        xx * v.z + ww * v.z;


            return result;
        }



        public void Print()
        {
            UnityEngine.Debug.Log("w : " + w + ", i : " + i + ", j : " + j + " , k : " + k);

        }


        public float CopySign(float value, float sign)
        {
            return (sign >= 0f) ? System.MathF.Abs(value) : -System.MathF.Abs(value);
        }

        public QuaternionUtils Slerp(QuaternionUtils q, float t)
        {
            QuaternionUtils result = new QuaternionUtils();

            // Based on http://www.euclideanspace.com/maths/algebra/realNormedAlgebra/quaternions/slerp/index.htm
            float cosHalfTheta = w * q.w + i * q.i + j * q.j + k * q.k;

            // if q1=q2 or qa=-q2 then theta = 0 and we can return qa
            if (System.MathF.Abs(cosHalfTheta) >= 1.0)
            {
                //Return this quaternion
                return this;
            }

            float halfTheta = System.MathF.Acos(cosHalfTheta);
            float sinHalfTheta = System.MathF.Sqrt(1.0f - cosHalfTheta * cosHalfTheta);
            // If theta = 180 degrees then result is not fully defined
            // We could rotate around any axis normal to q1 or q2
            if (System.MathF.Abs(sinHalfTheta) < epsilon)
            {
                result.w = (w * 0.5f + q.w * 0.5f);
                result.i = (i * 0.5f + q.i * 0.5f);
                result.j = (j * 0.5f + q.j * 0.5f);
                result.k = (k * 0.5f + q.k * 0.5f);
            }
            else
            {
                // Default quaternion calculation
                float ratioA = System.MathF.Sin((1f - t) * halfTheta) / sinHalfTheta;
                float ratioB = System.MathF.Sin(t * halfTheta) / sinHalfTheta;
                result.w = (w * ratioA + q.w * ratioB);
                result.i = (i * ratioA + q.i * ratioB);
                result.j = (j * ratioA + q.j * ratioB);
                result.k = (k * ratioA + q.k * ratioB);
            }
            return result;
        }

        public QuaternionUtils MultiplyWithInverse(QuaternionUtils other)
        {
            // Calcula el conjugado
            QuaternionUtils inverse = new QuaternionUtils(other.w, -other.i, -other.j, -other.k);

            // Multiplicamos este quaternon por el inverso
            float rw = w * inverse.w - i * inverse.i - j * inverse.j - k * inverse.k;
            float ri = w * inverse.i + i * inverse.w + j * inverse.k - k * inverse.j;
            float rj = w * inverse.j - i * inverse.k + j * inverse.w + k * inverse.i;
            float rk = w * inverse.k + i * inverse.j - j * inverse.i + k * inverse.w;

            return new QuaternionUtils(rw, ri, rj, rk);
        }

        public Quaternion ToUnityQuaternion()
        {
            Quaternion result;

            result.w = w;

            result.x = i;
            result.y = j;
            result.z = k;


            return result;
        }

        public string ToString()
        {
            return "(i:" + i + ", j:" + j + ", k:" + k + ", w:" + w + ")";
        }

        public void AssignFromUnityQuaternion(UnityEngine.Quaternion quaternion)
        {
            w = quaternion.w;
            i = quaternion.x;
            j = quaternion.y;
            k = quaternion.z;
        }

        public UnityEngine.Quaternion GetAsUnityQuaternion()
        {
            return new UnityEngine.Quaternion(i, j, k, w);
        }
    }
}

