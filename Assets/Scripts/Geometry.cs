using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets
{
    internal static class Geometry
    {
        /// <summary>
        /// Вычисляет угол треугольника по трём данным сторонам
        /// </summary>
        /// <param name="a">Прилежащая сторона</param>
        /// <param name="b">Прилежащая сторона</param>
        /// <param name="c">Противолежащая сторона</param>
        /// <returns></returns>
        public static float CalculateAngle(float a, float b, float c)
        {
            return Mathf.Acos((Mathf.Pow(a, 2) + Mathf.Pow(b, 2) - Mathf.Pow(c, 2)) / (2 * a * b)) * Mathf.Rad2Deg;
        }

        public static bool IsPointInCircle(Vector3 point, Vector3 center, float radius)
        {
            return Mathf.Pow(point.x - center.x, 2) + Mathf.Pow(point.z - center.z, 2) <= Mathf.Pow(radius, 2);
        }

        public static bool IsPointInRectangle(Vector3 point, float xMin, float xMax, float zMin, float zMax)
        {
            return point.x > xMin && point.x < xMax && point.z > zMin && point.z < zMax;
        }
    }
}
