using System;
using System.Drawing;

namespace CornishRoom
{
    public class Ray
    {
        //откуда исходит луч
        public Point3D start;
        //направление луча
        public Point3D direction;
        
        public Ray(Point3D st, Point3D end)
        {
            start = st;
            direction = Point3D.Normalize(end - st);
        }

        public Ray() { }

        public Ray(Ray r)
        {
            start = r.start;
            direction = r.direction;
        }

        //Луч отражения
        /*
         R = I - 2 * N * (N * I)
         R - отраженный луч
         I - падающий первичный луч - direction
         N - вектор нормали - normal
             */
        public Ray Reflect(Point3D reachPoint, Point3D normal)
        {
            Point3D reflectionRay = direction - 2 * normal * Point3D.Scalar(direction, normal);
            return new Ray( reachPoint, reachPoint + reflectionRay);
        }
        
        /// <summary>
        /// луч преломления
        /// T = n1 / n2 * I - (cos(teta) + n1 / n2 * (N * I)) * N
        /// cos(teta) = sqrt(1 - (n1 / n2) ^ 2 * (1 - (N * I) ^ 2)) 
        /// I - direction
        /// </summary>
        /// <param name="reachPoint"></param>
        /// <param name="normal">нормаль</param>
        /// <param name="n1">коэффициент рефракции для первой среды</param>
        /// <param name="n2">коэффициент рефракции для 2ой среды</param>
        /// <returns></returns>
        public Ray Refract(Point3D reachPoint, Point3D normal, float n1 ,float n2)
        {
            //луч преломления
            Ray transparencyRay = new Ray();
            float NI = Point3D.Scalar(direction, normal);
            float refractRatio = n1 / n2;
            float theta = 1 - refractRatio * refractRatio * (1 - NI * NI);
            //так как у нас корень, то на всякий случай проверим, чтоб потом не стрессовать
            if (theta >= 0)
            {
                float cos = (float)Math.Sqrt(theta);
                transparencyRay.start = reachPoint;
                transparencyRay.direction = Point3D.Normalize(refractRatio * direction - (cos + refractRatio * NI) * normal);
                return transparencyRay;
            }
            else
                return null;
        }
    }
    
    public class RayTracing
    {

        /// <summary>
        /// Алгоритм обратной трассировки лучей
        /// </summary>
        /// <param name="pixels">все пиксели сцены</param>
        /// <param name="scene">сцена</param>
        /// <param name="w">ширина пикчербокса</param>
        /// <param name="h">высота пикчербокса</param>
        /// <returns>Битмапу</returns>
        public static Bitmap BackwardRayTracing(Point3D[,] pixels, Scene scene, int width, int height)
        {
            Bitmap res = new Bitmap(width, height);
            for (int i = 0; i < width; ++i)
            {
                for (int j = 0; j < height; ++j)
                {
                    //Задаем первичный луч наблюдатель -> пиксель сцены 
                    Ray ray = new Ray(scene.camera, pixels[i, j]);
                    //затем меняем стартовую точку луча
                    ray.start = pixels[i, j];
                    Point3D color = RayTracingOn(ray, 5, scene);
                    res.SetPixel(i, j, 
                        Color.FromArgb((int) (255 * color.X), (int) (255 * color.Y), (int) (255 * color.Z)));
                }
            }

            return res;
        }
        
        //видима ли точка пересечения луча с фигурой из источника света
        public static bool IsVisible(Point3D light_point, Point3D hit_point, Scene scene)
        {
            float max_t = (light_point - hit_point).Length();
            Ray r = new Ray(hit_point, light_point);
            foreach (Figure fig in scene.figures)
                if (fig.Intersection(r, out float t, out Point3D n))
                    if (t < max_t && t > Figure.eps)
                        return false;
            return true;
        }
        
        public 
        
        /// <summary>
        /// Сам алгоритм трассировки
        /// </summary>
        /// <param name="ray">луч</param>
        /// <param name="iter">номер итерации</param>
        /// <param name="scene">сцена с фигурками</param>
        /// <returns>цвет пикселя</returns>
        public static Point3D RayTracingOn(Ray ray, int iter, Scene scene)
        {
            if (iter <= 0)
                return new Point3D(0, 0);
            //
            float intersectionPoint = 0;
            //нормаль грани фигуры, с которым произошло пересечение луча
            Point3D normal = null;
            Material material = new Material();
            Point3D color = new Point3D(0, 0);
          
            //угол падения острый
            bool refractFigure = false;

            foreach (Figure fig in scene.figures)
            {
                if (fig.Intersection(ray, out float intersect, out Point3D norm))
                    //ищем ближайшую фигуру к точке наблюдения
                    if (intersect < intersectionPoint || intersectionPoint == 0)// нужна ближайшая фигура к точке наблюдения
                    {
                        intersectionPoint = intersect;
                        normal = norm;
                        material = new Material(fig.figureMaterial);
                    }
            }

            if (intersectionPoint == 0)//если не пересекается с фигурой
                return new Point3D(0, 0, 0);//Луч уходит в свободное пространство .Возвращаем значение по умолчанию

            
            if (Point3D.Scalar(ray.direction, normal) > 0)
            {
                normal *= -1;
                refractFigure = true;
            }


            //Точка пересечения луча с фигурой
            Point3D hit_point = ray.start + ray.direction * intersectionPoint;
            
            Point3D ambient_coef = scene.Light.intensive * material.ambient;
            ambient_coef.X = (ambient_coef.X * material.color.X);
            ambient_coef.Y = (ambient_coef.Y * material.color.Y);
            ambient_coef.Z = (ambient_coef.Z * material.color.Z);
            color += ambient_coef;
            // диффузное освещение
            if (IsVisible(scene.Light.position, hit_point, scene))//если точка пересечения луча с объектом видна из источника света
                color += scene.Light.Shade(hit_point, normal, material.color, material.diffuse);

            if (material.reflection > 0)
            {
                Ray reflected_ray = ray.Reflect(hit_point, normal);
                color += material.reflection * RayTracingOn(reflected_ray, iter - 1, scene);
            }


            if (material.refraction > 0)
            {
                
                float refract_coef;
                if (refractFigure)
                    refract_coef = material.environment;
                else
                    refract_coef = 1 / material.environment;

                Ray refracted_ray = ray.Refract(hit_point, normal, material.refraction, refract_coef);//создаем приломленный луч

                
                if (refracted_ray != null)
                    color += material.refraction * RayTracingOn(refracted_ray, iter - 1, scene);
            }
            if (color.X > 1.0f || color.Y > 1.0f || color.Z > 1.0f)
                color = Point3D.Normalize(color);
            return color;
        }
    }
}