using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Util
{
    public static class Math
    {
        public static float ComputeOrientation(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return (p2.x - p1.x) * (p3.y - p1.y) - (p2.y - p1.y) * (p3.x - p1.x);
        }

        public static bool IsPointInsideTriangle(Vector3 p, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float d1, d2, d3;

            d1 = ComputeOrientation(p, p1, p2);
            d2 = ComputeOrientation(p, p2, p3);
            d3 = ComputeOrientation(p, p3, p1);

            bool cw = d1 < 0 && d2 < 0 && d3 < 0;
            bool ccw = d1 > 0 && d2 > 0 && d3 > 0;

            return cw || ccw;
        }
    }
}
