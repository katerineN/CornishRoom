using System;
using System.Collections.Generic;
using System.Drawing;

namespace CornishRoom
{
    public class Light
    {
        //позиция источника света
        public Point3D position;       
        public Point3D intensive; 

        public Light(Point3D p, Point3D i)
        {
            position = p;
            intensive = i;
        }
        
        
        /*private static double recalcLight(Point3D point, Point3D normal, List<Light> lights, Scene scene)
        {
            double i = 0;
            foreach (var light in lights)
            {
                Point3D l = new Point3D(light.position.X - point.X, light.position.Y - point.Y, light.position.Z - point.Z);

                Figure shadowElem = null;
                double shadowT = 0;
                closestElem(point, l, scene, out shadowElem, out shadowT);

                if (shadowElem != null && shadowElem.Type != ElemType.Plain)
                {
                    continue;
                }

                double normalMult = Point3D.Scalar(normal, l);
                if (normalMult > 0)
                {
                    i += light.Intens * normalMult / (normal.Length() * l.Length());
                }
            }
            return i;
        }
        */
        //вычисление локальной модели освещения
        public Point3D Shade(Point3D reachPoint, Point3D normal, Point3D material, float diffuseRatio)
        {
            //направление луча
            Point3D dir = Point3D.Normalize(position - reachPoint);
            //если угол между нормалью и направлением луча больше 90 градусов,то диффузное  освещение равно 0
            Point3D diff = diffuseRatio * intensive * Math.Max(Point3D.Scalar(normal, dir), 0);
            return new Point3D(diff.X * material.X, diff.Y * material.Y, diff.Z * material.Z);
        }
    }
}