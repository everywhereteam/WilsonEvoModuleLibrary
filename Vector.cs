using System;

namespace WilsonPluginModels
{
    [Serializable]
    public sealed class Vector
    {
        public Vector()
        {
        }

        public Vector(double x = 0, double y = 0)
        {
            X = x;
            Y = y;
        }

        public double X { set; get; }

        public double Y { set; get; }
    }
}