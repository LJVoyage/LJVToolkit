using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LJVoyage.LJVToolkit.Runtime.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    public static class MathUtil
    {
        /// <summary>
        ///  
        /// </summary>
        /// <param name="value"></param>
        /// <param name="inMin"></param>
        /// <param name="inMax"></param>
        /// <param name="outMin"></param>
        /// <param name="outMax"></param>
        /// <returns></returns>
        public static float Remap(float value, float inMin, float inMax, float outMin, float outMax)
        {
            return (value - inMin) / (inMax - inMin) * (outMax - outMin) + outMin;
        }
    }
}