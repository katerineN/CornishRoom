using System;
using System.Collections.Generic;
using System.Drawing;

namespace CornishRoom
{
    public class Material
    {
        ///отражение
        public float reflection;   
        ///преломление
        public float refraction;    
        ///фоновое освещение
        public float ambient;     
        ///диффузное освещение
        public float diffuse;    
        ///преломление среды
        public float environment;   
        ///цвет
        public Point3D color;

        public Material(float refl, float refr, float amb, float dif, float env = 1)
        {
            reflection = refl;
            refraction = refr;
            ambient = amb;
            diffuse = dif;
            environment = env;
        }
        
        public Material(float refl, float refr, float amb, float dif, Point3D p, float env = 1)
        {
            reflection = refl;
            refraction = refr;
            ambient = amb;
            diffuse = dif;
            environment = env;
            color = p;
        }
        
        public Material()
        {
            
        }
        
        public Material(Material m)
        {
            reflection = m.reflection;
            refraction = m.refraction;
            environment = m.environment;
            ambient = m.ambient;
            diffuse = m.diffuse;
            color = m.color;
        }
    }

    public enum Type
    {
        Cube = 0,
        Room,
        Sphere
    };

    //потому что у комнаты могут быть разные цвета
    public class Face
    {
        //точки
        public List<int> facePoints = new List<int>();
        //какой ручкой рисуем
        public Pen pen = new Pen(Color.Violet);

        public Face(List<int> faces)
        {
            facePoints = faces;
        }
    }
    
    public class Figure
    {
        public static float eps = 0.0001f;
        //Тип фигуры
        public Type type;
        //точки фигуры
        public List<Point3D> points = new List<Point3D>();
        //список граней
        public List<Face> faces = new List<Face>();
        //материал фигуры
        public Material figureMaterial;
        //радиус для сферы
        public float rad = 0;
        //какой ручкой рисуем для сферы
        public Pen pen = new Pen(Color.Violet);
        
        
        
        /// <summary>
        /// Пересечение луча с треугольником
        /// https://question-it.com/questions/717402/kak-najti-tochku-peresechenija-lucha-i-treugolnika
        /// </summary>
        /// <param name="r">Луч</param>
        /// <param name="p0">Точка треугольника</param>
        /// <param name="p1">Еще точка</param>
        /// <param name="p2">Еще точка</param>
        /// <param name="intersect">Пересечение</param>
        /// <returns>Есть ли пересечение???</returns>
        public bool RayTriangleIntersection(Ray r, Point3D p0, Point3D p1, Point3D p2, out float intersect)
        {
            intersect = -1;
            Point3D edge1 = p1 - p0;
            Point3D edge2 = p2 - p0;
            Point3D pVec = r.direction * edge2;
            float det = Point3D.Scalar(edge1, pVec);
            
            //если условие выполняется, то луч параллелен треугольнику
            if (det > -eps && det < eps)
                return false;       
            
            float invDet = 1.0f / det;
            Point3D s = r.start - p0;
            float u = invDet * Point3D.Scalar(s, pVec);
            
            //тут я тупанула
            if (u < 0 || u > 1)
                return false;
            
            Point3D q = s * edge1;
            float v = invDet * Point3D.Scalar(r.direction, q);
            
            if (v < 0 || u + v > 1)
                return false;
            
            // пытаемся узнать, где находится точка пересечения на линии
            float t = invDet * Point3D.Scalar(edge2, q);
            if (t > eps)
            {
                intersect = t;
                return true;
            }
            //Если мы тут, то значит есть пересечение линий, а не лучей
            else      
                return false;
        }

        /// <summary>
        /// Ищет пересечение луча с фигурой
        /// </summary>
        /// <param name="r">луч</param>
        /// <param name="intersect">точка пересечения</param>
        /// <param name="normal">нормаль стороны</param>
        /// <returns>Есть ли пересечение</returns>
        public bool Intersection(Ray r, out float intersect, out Point3D normal)
        {
            intersect = 0;
            normal = null;
            switch (type)
            {
                case (Type.Cube):
                case (Type.Room):
                    Face f = null;
                    //просматриваем каждую сторону
                    foreach (Face face in faces)
                    {
                        switch (face.facePoints.Count)
                        {
                            case 3:
                                if (RayTriangleIntersection(r, points[face.facePoints[0]], points[face.facePoints[1]],
                                    points[face.facePoints[2]], out float t) && (intersect == 0 || t < intersect))
                                {
                                    intersect = t;
                                    f = face;
                                }

                                break;
                            case 4:
                                //разбиваем на 2 треугольника четырехугольник
                                if (RayTriangleIntersection(r, points[face.facePoints[0]], points[face.facePoints[1]],
                                    points[face.facePoints[3]], out float t1) && (intersect == 0 || t1 < intersect))
                                {
                                    intersect = t1;
                                    f = face;
                                }
                                else if (RayTriangleIntersection(r, points[face.facePoints[1]],
                                             points[face.facePoints[2]], points[face.facePoints[3]], out float t2) &&
                                         (intersect == 0 || t2 < intersect))
                                {
                                    intersect = t2;
                                    f = face;
                                }

                                break;
                        }
                    }

                    //если нашли пересечение
                    if (intersect != 0)
                    {
                        normal = Scene.GetNormal(f.facePoints, this);
                        figureMaterial.color = new Point3D(f.pen.Color.R / 255f, f.pen.Color.G / 255f,
                            f.pen.Color.B / 255f);
                        return true;
                    }

                    return false;
                    
                case (Type.Sphere):
                    
                    if (Sphere.RaySphereIntersection(r, points[0], this.rad, out intersect) && (intersect > eps))
                    {
                        normal = (r.start + r.direction * intersect) - points[0];
                        normal = Point3D.Normalize(normal);
                        figureMaterial.color = new Point3D(this.pen.Color.R / 255f, pen.Color.G / 255f,
                            pen.Color.B / 255f);
                        return true;
                    }

                    return false;
            }

            return false;
        }
        
        //меняет значения точек по матрице
        public void ApplyMatrix(float[,] matrix)
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i].X = matrix[i, 0] / matrix[i, 3];
                points[i].Y = matrix[i, 1] / matrix[i, 3];
                points[i].Z = matrix[i, 2] / matrix[i, 3];
            }
        }
        
        //из точек делает матрицу
        public float[,] GetMatrix()
        {
            var res = new float[points.Count, 4];
            for (int i = 0; i < points.Count; i++)
            {
                res[i, 0] = points[i].X;
                res[i, 1] = points[i].Y;
                res[i, 2] = points[i].Z;
                res[i, 3] = 1;
            }
            return res;
        }

        //устанавливает цвет для грани
        public void SetPen(Pen pen)
        {
            foreach (Face f in faces)
                f.pen = pen;
        }
        
        //смещение
        public void Offset(float x, float y, float z)
        {
            float[,] matrix = GetMatrix();
            float[,] translationMatrix = new float[,]
            {
                { 1, 0, 0, 0 }, 
                { 0, 1, 0, 0 }, 
                { 0, 0, 1, 0 }, 
                { x, y, z, 1 }
            };
            ApplyMatrix(Form1.MultMatrix(matrix, translationMatrix));
        }
        //смещение
        public float[,] Offset1(float[,] mat, float x, float y, float z)
        {
            float[,] translationMatrix = new float[,]
            {
                { 1, 0, 0, 0 }, 
                { 0, 1, 0, 0 }, 
                { 0, 0, 1, 0 }, 
                { x, y, z, 1 }
            };
            return Form1.MultMatrix(mat, translationMatrix);
        }
        
        private Point3D GetCenter()
        {
            Point3D res = new Point3D(0, 0, 0);
            foreach (Point3D p in points)
            {
                res.X += p.X;
                res.Y += p.Y;
                res.Z += p.Z;

            }
            res.X /= points.Count;
            res.Y /= points.Count;
            res.Z /= points.Count;
            return res;
        }
        
       //поворот
        public void RotateArondRad(float angle)
        {
            float[,] mt = GetMatrix();
            Point3D center = GetCenter();
            mt = Offset1(mt, -center.X, -center.Y, -center.Z);
            float[,] rotationMatrix = new float[,] { 
                { (float)Math.Cos(angle), (float)Math.Sin(angle), 0, 0 }, 
                { -(float)Math.Sin(angle), (float)Math.Cos(angle), 0, 0 },
                { 0, 0, 1, 0 }, 
                { 0, 0, 0, 1} };
            mt = Form1.MultMatrix(mt, rotationMatrix);
            mt = Offset1(mt, center.X, center.Y, center.Z);
            ApplyMatrix(mt);
        }
        
        //устанавливает цвет для сферы
        public void SetPenSphere(Pen p)
        {
            pen = p;
        }
    }
    
    public class Cube:Figure
    {
        //задаем куб по размерам
        public Cube(float size, Material mat, Type t = Type.Cube)
        {
            points.Add(new Point3D(size / 2, size / 2, size / 2)); 
            points.Add(new Point3D(-size / 2, size / 2, size / 2));
            points.Add(new Point3D(-size / 2, size / 2, -size / 2)); 
            points.Add(new Point3D(size / 2, size / 2, -size / 2)); 

            points.Add(new Point3D(size / 2, -size / 2, size / 2)); 
            points.Add(new Point3D(-size / 2, -size / 2, size / 2)); 
            points.Add(new Point3D(-size / 2, -size / 2, -size / 2)); 
            points.Add(new Point3D(size / 2, -size / 2, -size / 2)); 

            faces.Add(new Face(new List<int>() {3, 2, 1, 0}));
            faces.Add(new Face(new List<int>() {4, 5, 6, 7}));
            faces.Add(new Face(new List<int>() {2, 6, 5, 1}));
            faces.Add(new Face(new List<int>() {0, 4, 7, 3}));
            faces.Add(new Face(new List<int>() {1, 5, 4, 0}));
            faces.Add(new Face(new List<int>() {2, 3, 7, 6}));

            type = t;
            figureMaterial = mat;
        }
        
    }
    
    public class Sphere : Figure
    {
        public Sphere(Point3D p, float r, Material mat, Type t)
        {
            points.Add(p);
            rad = r;
            figureMaterial = mat;
            type = t;
        }

        /// <summary>
        /// Пересечение луча со сферой
        /// http://www.ray-tracing.ru/articles245.html
        /// http://netlib.narod.ru/library/book0032/ch15_04.htm
        /// </summary>
        /// <param name="r">луч</param>
        /// <param name="center">позиция сферы</param>
        /// <param name="rad">радиус</param>
        /// <param name="intersect"></param>
        /// <returns></returns>
        public static bool RaySphereIntersection(Ray r, Point3D center, float rad, out float intersect)
        {
            Point3D k = r.start - center;
            float b = Point3D.Scalar(k, r.direction);
            float c = Point3D.Scalar(k, k) - rad * rad;
            //дискриминант
            float d = b * b - c;
            intersect = 0;
            if (d >= 0)
            {
                float sqrtd = (float)Math.Sqrt(d);
                float t1 = -b + sqrtd;
                float t2 = -b - sqrtd;

                float min_t = Math.Min(t1, t2);
                float max_t = Math.Max(t1, t2);

                intersect = (min_t > eps) ? min_t : max_t;
                return intersect > eps;
            }
            return false;
        }

    }
}