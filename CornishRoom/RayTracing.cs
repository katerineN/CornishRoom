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
        
        
        /// <summary>
        /// Проверяем видна ли точка пересечения луча с фигурой из источника света
        /// </summary>
        /// <param name="light">источник освещения</param>
        /// <param name="reachPoint"></param>
        /// <param name="scene"></param>
        /// <returns></returns>
        public static bool IsVisible(Light light, Point3D reachPoint, Scene scene)
        {
            float length = (light.position - reachPoint).Length();
            Ray ray = new Ray(reachPoint, light.position);
            foreach (Figure fig in scene.figures)
            {
                if (fig.Intersection(ray, out float t, out Point3D n))
                    //смотрим на луч
                    //если точка t ближе к источнику освещения чем length, то точка не видна, ее закрывает фигура
                    if (t < length && t > Figure.eps)
                        return false;
            }

            return true;
        }

        /// <summary>
        /// Находит ближайшую фигуру
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="r"></param>
        /// <param name="normal"></param>
        /// <param name="material"></param>
        /// <returns>возвращает точку пересечения</returns>
        public static float FindClosestFigure(Scene scene, Ray r, out Point3D normal, out Material material)
        {
            float intersectionPoint = 0;
            normal = null;
            material = new Material();
            foreach (Figure f in scene.figures)
            {
                if (f.Intersection(r, out float intersect, out Point3D norm))
                    if (intersect < intersectionPoint || intersectionPoint == 0)
                    {
                        intersectionPoint = intersect;
                        normal = norm;
                        material = new Material(f.figureMaterial);
                    }
            }
            return intersectionPoint;
        }
        
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
            //позиция точки пересечения на луче
            float intersectionPoint = 0;
            //нормаль грани фигуры, с которым произошло пересечение луча
            Point3D normal = null;
            Material material = new Material();
            //итоговый цвет пикселя
            Point3D color = new Point3D(0, 0);
          
            //угол падения острый
            bool refractFigure = false;

            intersectionPoint = FindClosestFigure(scene, ray, out normal, out material);
            
            //если не пересекается с фигурой
            //значит луч ушел в свободное плавание
            if (intersectionPoint == 0)
                return new Point3D(0, 0);

            //если острый угол между направление луча и нормалью стороны
            //определяем из какой среды в какую
            if (Point3D.Scalar(ray.direction, normal) > 0)
            {
                normal *= -1;
                refractFigure = true;
            }

            //Точка пересечения луча с фигурой
            Point3D reachPoint = ray.start + ray.direction * intersectionPoint;
            
            //из презентации 
            //В точке пересечения луча с объектом строится три вторичных 
            //луча – один в направлении отражения (1), второй – в направлении 
            //источника света (2), третий в направлении преломления 
            //прозрачной поверхностью (3)
            
            //работаем с источником света
            //фоновый цвет
            Point3D ambientRatio= scene.Light.color * material.ambient;
            ambientRatio = new Point3D(ambientRatio.X * material.color.X, ambientRatio.Y * material.color.Y,
                ambientRatio.Z * material.color.Z);
            color += ambientRatio;
            // диффузное освещение
            //если точка пересечения луча с объектом видна из источника света
            if (IsVisible(scene.Light, reachPoint, scene))
                color += scene.Light.Shade(reachPoint, normal, material.color, material.diffuse);

            //Для отраженного луча проверяется возможность пересечения с другими объектами сцены.
            //Если пересечений нет, то интенсивность и цвет отраженного луча равна интенсивности и цвету фона.
            //Если пересечение есть, то в новой точке снова строится три типа лучей – теневые, отражения и преломления. 
            if (material.reflection > 0)
            {
                Ray reflectionRay = ray.Reflect(reachPoint, normal);
                color += material.reflection * RayTracingOn(reflectionRay, iter - 1, scene);
            }

            //проверяется пересечение вновь построенного луча с объектами, и, если они есть, в новой 
            //точке строятся три луча, если нет –используется интенсивность и цвет фона.
            if (material.refraction > 0)
            {
                //коэффициент преломления
                float refractRatio;
                //если угол острый получился, то
                if (refractFigure)
                    refractRatio = material.environment;
                else
                    refractRatio = 1 / material.environment;

                Ray transparencyRay = ray.Refract(reachPoint, normal, material.refraction, refractRatio);
                if (transparencyRay != null)
                    color += material.refraction * RayTracingOn(transparencyRay, iter - 1, scene);
            }
            if (color.X > 1.0f || color.Y > 1.0f || color.Z > 1.0f)
                color = Point3D.Normalize(color);
            return color;
        }
    }
}