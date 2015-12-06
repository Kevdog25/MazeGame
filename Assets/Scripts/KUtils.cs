using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

static class KUtils
{
    public static System.Random rand = new System.Random();

    /// <summary>
    /// Returns a gaussian random variable with mean mu and std std
    /// </summary>
    /// <param name="mu"></param>
    /// <param name="std"></param>
    /// <returns></returns>
    public static double RandGauss(double mu, double std)
    {
        double u1 = rand.NextDouble();
        double u2 = rand.NextDouble();
        double x = Math.Sqrt(-2 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        return mu + x * std;
    }

    public static Vector2 Rotate2D(Vector2 start, float angle)
    {
        Vector2 end = new Vector2();
        end.x = (float)(Math.Sin(angle) * start.y + Math.Cos(angle) * start.x);
        end.y = (float)(Math.Cos(angle) * start.y - Math.Sin(angle) * start.x);
        return end;
    }

    public static bool CheckRange(int x, int l, int r)
    {
        return l <= x && x < r;
    }
}
