using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace MicroGraph.Editor
{
    public readonly struct Rem
    {
        public float RemValue { get; }

        public float PixelValue => RemValue * 16;

        public Rem(float value)
        {
            RemValue = value;
        }

        public static implicit operator int(Rem unit)
        {
            return Convert.ToInt32(unit.PixelValue);
        }

        public static implicit operator float(Rem unit)
        {
            return unit.PixelValue;
        }

        public static implicit operator StyleFloat(Rem unit)
        {
            return unit.PixelValue;
        }

        public static implicit operator StyleLength(Rem unit)
        {
            return unit.PixelValue;
        }
    }
    public static class RemExtensions
    {
        public static Rem Rem(this int value)
        {
            return new Rem(value);
        }

        public static Rem Rem(this float value)
        {
            return new Rem(value);
        }

        public static Rem Rem(this double value)
        {
            return new Rem(Convert.ToSingle(value));
        }
    }
}
