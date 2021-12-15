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

        //луч преломления
        //T = n1 / n2 * I - (cos(teta) + n1 / n2 * (N * I)) * N
        // cos(teta) = sqrt(1 - (n1 / n2) ^ 2 * (1 - (N * I) ^ 2)) 
        // I - direction
        // N - normal
        // n1 - коэффициент рефракции для первой среды
        // n2 - коэффициент рефракции для второй среды
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

        public static Color[,] BackwardRayTracing(Point3D[,] pixels, Color[,] pixelsColor, Scene scene, int w, int h)
        {
            /*
            Количество первичных лучей также известно – это общее
             количество пикселей видового окна
             */
            for (int i = 0; i < w; ++i)
            for (int j = 0; j < h; ++j)
            {
                Ray r = new Ray(scene.camera, pixels[i, j]);
                r.start = pixels[i, j];
                Point3D color = RayTrace(r, 10, 1, scene);//луч,кол-во итераций,коэфф
                if (color.X > 1.0f || color.Y > 1.0f || color.Z > 1.0f)
                    color = Point3D.Normalize(color);
                pixelsColor[i, j] = Color.FromArgb((int)(255 * color.X), (int)(255 * color.Y), (int)(255 * color.Z));
            }

            return pixelsColor;
        }
        
        //видима ли точка пересечения луча с фигурой из источника света
        public static bool IsVisible(Point3D light_point, Point3D hit_point, Scene scene)
        {
            float max_t = (light_point - hit_point).Length(); //позиция источника света на луче
            Ray r = new Ray(hit_point, light_point);
            foreach (Figure fig in scene.figures)
                if (fig.Intersection(r, out float t, out Point3D n))
                    if (t < max_t && t > Figure.eps)
                        return false;
            return true;
        }
        
        public static Point3D RayTrace(Ray r, int iter, float env, Scene scene)
        {
            if (iter <= 0)
                return new Point3D(0, 0, 0);
            float rey_fig_intersect = 0;// позиция точки пересечения луча с фигурой на луче
            //нормаль стороны фигуры,с которой пересекся луч
            Point3D normal = null;
            Material material = new Material();
            Point3D res_color = new Point3D(0, 0, 0);
            //угол падения острый
            bool refract_out_of_figure = false;

            foreach (Figure fig in scene.figures)
            {
                if (fig.Intersection(r, out float intersect, out Point3D norm))
                    if (intersect < rey_fig_intersect || rey_fig_intersect == 0)// нужна ближайшая фигура к точке наблюдения
                    {
                        rey_fig_intersect = intersect;
                        normal = norm;
                        material = new Material(fig.figureMaterial);
                    }
            }

            if (rey_fig_intersect == 0)//если не пересекается с фигурой
                return new Point3D(0, 0, 0);//Луч уходит в свободное пространство .Возвращаем значение по умолчанию

            //угол между направление луча и нормалью стороны острый
            //определяем из какой среды в какую
            //http://edu.glavsprav.ru/info/zakon-prelomleniya-sveta/
            if (Point3D.Scalar(r.direction, normal) > 0)
            {
                normal *= -1;
                refract_out_of_figure = true;
            }


            //Точка пересечения луча с фигурой
            Point3D hit_point = r.start + r.direction * rey_fig_intersect;
            /*В точке пересечения луча с объектом строится три вторичных
              луча – один в направлении отражения (1), второй – в направлении
              источника света (2), третий в направлении преломления
              прозрачной поверхностью (3).
             */

            //цвет коэффициент принятия фонового освещения
            Point3D ambient_coef = scene.Light.intensive * material.ambient;
            ambient_coef.X = (ambient_coef.X * material.color.X);
            ambient_coef.Y = (ambient_coef.Y * material.color.Y);
            ambient_coef.Z = (ambient_coef.Z * material.color.Z);
            res_color += ambient_coef;
            // диффузное освещение
            if (IsVisible(scene.Light.position, hit_point, scene))//если точка пересечения луча с объектом видна из источника света
                res_color += scene.Light.Shade(hit_point, normal, material.color, material.diffuse);


            /*Для отраженного луча
              проверяется возможность
              пересечения с другими
              объектами сцены.

                Если пересечений нет, то
                интенсивность и цвет
                отраженного луча равна
                интенсивности и цвету фона.

                Если пересечение есть, то в
                новой точке снова строится
                три типа лучей – теневые,
                отражения и преломления. 
              */
            if (material.reflection > 0)
            {
                Ray reflected_ray = r.Reflect(hit_point, normal);
                res_color += material.reflection * RayTrace(reflected_ray, iter - 1, env, scene);
            }


            if (material.refraction > 0)
            {
                //взависимости от того,из какой среды в какую,будет меняться коэффициент приломления
                float refract_coef;
                if (refract_out_of_figure)
                    refract_coef = material.environment;
                else
                    refract_coef = 1 / material.environment;

                Ray refracted_ray = r.Refract(hit_point, normal, material.refraction, refract_coef);//создаем приломленный луч

                /*
                 Как и в предыдущем случае,
                 проверяется пересечение вновь
                 построенного луча с объектами,
                 и, если они есть, в новой точке
                 строятся три луча, если нет – используется интенсивность и
                 цвет фона.
                 */
                if (refracted_ray != null)
                    res_color += material.refraction * RayTrace(refracted_ray, iter - 1, material.environment, scene);
            }
            return res_color;
        }
    }
}