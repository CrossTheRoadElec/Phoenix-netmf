using System;
using Microsoft.SPOT;

namespace CTRE.Phoenix
{
    public static class Util
    {
        /** Returns the absolute value */
        public static float Abs(float f)
        {
            if (f >= 0)
                return f;
            return -f;
        }

        /** Bounds value within the cap of 1 */
        public static float Bound(float value, float capValue = 1f)
        {
            if (value > capValue)
                return capValue;
            if (value < -capValue)
                return -capValue;
            return value;
        }

        /*Value to cap at 
         * Peak positve float representing the maximum (peak) value
         */
        public  static float Cap(float Value, float Peak)
        {
            if (Value < -Peak)
                return -Peak;
            if (Value > +Peak)
                return +Peak;
            return Value;
        }

        public static bool Contains(char[] array, char item)
        {
            bool found = false;

            foreach (char element in array)
            {
                if (element == item)
                    found = true;
            }

            return found;
        }

        /** If value is within 10%, clear it */
        public static void Deadband(ref float value, float deadband = 0.10f)
        {
            if (value < -deadband)
            {
                /* outside of deadband */
            }
            else if (value > +deadband)
            {
                /* outside of deadband */
            }
            else
            {
                /* within 10% so zero it */
                value = 0;
            }
        }

        /** Not 100% sure what this is used for */
        public static bool IsWithin(float value, float compareTo, float allowDelta)
        {
            float f = value - compareTo;
            if (f < 0)
                f *= -1f;
            return (f < allowDelta);
        }

        /** Returns the smaller value of the two inputted */
        public static int SmallerOf(int value1, int value2)
        {
            if (value1 > value2)
                return value2;
            else
                return value1;
        }

        public static void Split_1(float forward, float turn, out float left, out float right)
        {
            left = forward + turn;
            right = forward - turn;
        }
        public static void Split_2(float left, float right, out float forward, out float turn)
        {
            forward = (left + right) * 0.5f;
            turn = (left - right) * 0.5f;
        }
    }
}
