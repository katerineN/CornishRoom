using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;

namespace CornishRoom
{
    public class Scene
    {
        public List<Figure> figures = new List<Figure>();
        public Light Light;
        public Point3D camera;

        public Scene() {
        }

        public Scene(List<Figure> fig)
        {
            figures = fig;
        }

        //поиск нормали к грани
        public static Point3D GetNormal(List<int> face, Figure f)
        {
            if (face.Count < 3)
                return new Point3D(0, 0, 0);
            Point3D e1 = f.points[face[1]] - f.points[face[0]];
            Point3D e2 = f.points[face[face.Count-1]] - f.points[face[0]];
            Point3D normal = e1 * e2;
            return Point3D.Normalize(normal);
        }
        
        //находит центр стороны (только для комнаты!!!)
        public static Point3D GetCenter(List<int> face, Figure f)
        {
            return f.points[face[0]] + f.points[face[1]] + f.points[face[2]] + f.points[face[3]];
        }
        
        public void addFigure(Figure f)
        {
            figures.Add(f);
            //если мы создаем комнату, то задаем положение камеры
            if (f.type == Type.Room)
            {
                //находим нормали
                Point3D normal = GetNormal(f.faces[0].facePoints, f);
                Point3D center = GetCenter(f.faces[0].facePoints, f);
                camera = center + normal * 11;
            }
        }
    }
}