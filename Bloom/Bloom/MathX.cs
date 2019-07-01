using System;

namespace Bloom
{
    /// <summary>
    /// Provides extra math functions
    /// </summary>
    public static class MathX
    {
        /// <summary>
        /// Convert degrees to radians
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static double DegToRad(double degrees)
        {
            return degrees * 0.01745329252;
        }

        /// <summary>
        /// Convert radians to degrees
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        public static double RadToDeg(double radians)
        {
            return radians / 0.01745329252;
        }

        /// <summary>
        /// Convert degrees to radians
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static float DegToRad(float degrees)
        {
            return degrees * 0.01745329252f;
        }

        /// <summary>
        /// Convert radians to degrees
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        public static float RadToDeg(float radians)
        {
            return radians / 0.01745329252f;
        }

        /// <summary>
        /// Get the cosine of an angle in degrees
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static double DCos(double degrees)
        {
            return Math.Cos(DegToRad(degrees));
        }

        /// <summary>
        /// Get the sine of an angle in degrees
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static double DSin(double degrees)
        {
            return Math.Sin(DegToRad(degrees));
        }

        /// <summary>
        /// Get the cosine of an angle in degrees
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static float DCos(float degrees)
        {
            return MathF.Cos(DegToRad(degrees));
        }

        /// <summary>
        /// Get the sine of an angle in degrees
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static float DSin(float degrees)
        {
            return MathF.Sin(DegToRad(degrees));
        }
    }
}
