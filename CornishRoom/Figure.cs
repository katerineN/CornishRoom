using System;
using System.Collections.Generic;
using System.Drawing;

namespace CornishRoom
{
    public class Material
    {
        //отражение
        public float reflection;   
        //преломление
        public float refraction;    
        //фоновое освещение
        public float ambient;     
        //диффузное освещение
        public float diffuse;    
        //преломление среды
        public float environment;   
        //цвет
        public Point3D color;

        public Material(float refl, float refr, float amb, float dif, float env = 1)
        {
            reflection = refl;
            refraction = refr;
            ambient = amb;
            diffuse = dif;
            environment = env;
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
        Room
    };

    //потому что у комнаты могут быть разные цвета
    public class Face
    {
        //точки
        public List<int> facePoints = new List<int>();
        //какой ручкой рисуем
        public Pen pen = new Pen(Color.Gray);

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
        
        //пересечение луча и треугольника
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
            
            //u > 1
            if (u < 0 || u > det)
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
            else      //Это означает, что есть пересечение линий, но не пересечение лучей
                return false;
        }
        
        // пересечение луча с фигурой
        public virtual bool Intersection(Ray r, out float intersect, out Point3D normal)
        {
            intersect = 0;
            normal = null;
            Face f = null;
            foreach (Face face in faces)
            {
                //треугольная сторона
                if (face.facePoints.Count == 3)
                {
                    if (RayTriangleIntersection(r, points[face.facePoints[0]], points[face.facePoints[1]], points[face.facePoints[2]], out float t) && (intersect == 0 || t < intersect))
                    {
                        intersect = t;
                        f = face;
                    }
                }

                //четырехугольная сторона
                else if (face.facePoints.Count == 4)
                {
                    if (RayTriangleIntersection(r, points[face.facePoints[0]], points[face.facePoints[1]], points[face.facePoints[2]], out float t) && (intersect == 0 || t < intersect))
                    {
                        intersect = t;
                        f = face;
                    }
                    else if (RayTriangleIntersection(r, points[face.facePoints[1]], points[face.facePoints[2]], points[face.facePoints[3]], out t) && (intersect == 0 || t < intersect))
                    {
                        intersect = t;
                        f = face;
                    }
                }
            }
            if (intersect != 0)
            {
                normal = Scene.GetNormal(f.facePoints, this);
                figureMaterial.color = new Point3D(f.pen.Color.R / 255f, f.pen.Color.G / 255f, f.pen.Color.B / 255f);
                return true;
            }
            return false;
        }
        
        public void ApplyMatrix(float[,] matrix)
        {
            for (int i = 0; i < points.Count; i++)
            {
                points[i].X = matrix[i, 0] / matrix[i, 3];
                points[i].Y = matrix[i, 1] / matrix[i, 3];
                points[i].Z = matrix[i, 2] / matrix[i, 3];
            }
        }
        
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

        public void SetPen(Pen dw)
        {
            foreach (Face s in faces)
                s.pen = dw;
        }
        
        private static float[,] ApplyOffset(float[,] transform_matrix, float offset_x, float offset_y, float offset_z)
        {
            float[,] translationMatrix = new float[,] { { 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 1, 0 }, { offset_x, offset_y, offset_z, 1 } };
            return Form1.MultMatrix(transform_matrix, translationMatrix);
        }
        
        public void Offset(float xs, float ys, float zs)
        {
            ApplyMatrix(ApplyOffset(GetMatrix(), xs, ys, zs));
        }
    }
    
    public class Cube:Figure
    {
        //задаем куб по размерам
        public Cube(float size, Material mat, Type t = Type.Cube)
        {
            points.Add(new Point3D(size / 2, size / 2, size / 2)); // 0 
            points.Add(new Point3D(-size / 2, size / 2, size / 2)); // 1
            points.Add(new Point3D(-size / 2, size / 2, -size / 2)); // 2
            points.Add(new Point3D(size / 2, size / 2, -size / 2)); //3

            points.Add(new Point3D(size / 2, -size / 2, size / 2)); // 4
            points.Add(new Point3D(-size / 2, -size / 2, size / 2)); //5
            points.Add(new Point3D(-size / 2, -size / 2, -size / 2)); // 6
            points.Add(new Point3D(size / 2, -size / 2, -size / 2)); // 7

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
}