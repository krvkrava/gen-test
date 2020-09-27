using UnityEngine;

namespace Helpers
{
    public static class Vector3Utils
    {
        public static Vector3 RotateAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
        {
            var direction = point - pivot;
            direction = Quaternion.Euler(angles) * direction;
            return direction + pivot;
        }
    }
}