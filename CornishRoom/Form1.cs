using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CornishRoom
{
    public partial class Form1 : Form
    {
       
        public int width, height;
        //перемножение матриц
        public static float[,] MultMatrix(float[,] m1, float[,] m2)
        {
            int r1 = m1.GetLength(0);
            int c1 = m1.GetLength(1);
            int r2 = m2.GetLength(0);
            int c2 = m2.GetLength(1);
            float[,] res = new float[r1, c2];

            for (int i = 0; i < r1; ++i)
                for (int j = 0; j < c2; ++j)
                    for (int k = 0; k < r2; k++)
                    {
                        res[i, j] += m1[i, k] * m2[k, j];
                    }

            return res;
        }
        
        public Form1()
        {
            InitializeComponent();
            width = pictureBox1.Width;
            height = pictureBox1.Height;
            pictureBox1.Image = new Bitmap(width, height);
        }

        // получение всех пикселей сцены
        public Point3D[,] GetPixels(Figure room)
        { 
            //Учитывая разницу между размером комнаты и экранным отображение приводим координаты к пикселям
            //точки стены у наблюдателя
            Point3D upLeft = room.points[room.faces[0].facePoints[0]];
            Point3D upRight = room.points[room.faces[0].facePoints[1]];
            Point3D downRight = room.points[room.faces[0].facePoints[2]];
            Point3D downLeft = room.points[room.faces[0].facePoints[3]];
            Point3D [,] pixels = new Point3D[width, height];
            //отношение ширины комнаты к ширине пикчербокса
            Point3D stepUp = (upRight - upLeft) / (width - 1);
            //отношение высоты комнаты к высоте пикчербокса
            Point3D stepDown = (downRight - downLeft) / (width - 1);
            Point3D up = upLeft;
            Point3D down = downLeft;
            for (int i = 0; i < width; ++i)
            {
                Point3D stepY = (up - down) / (height - 1);
                Point3D d = down;
                for (int j = 0; j < height; ++j)
                {
                    pixels[i, j] = d;
                    d += stepY;
                }
                up += stepUp;
                down += stepDown;
            }
            return pixels;
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            //сама комната
            Figure room = new Cube(10, new Material(0, 0, 0.05f, 0.7f), Type.Room);
            room.SetPen(new Pen(Color.Gray));
            room.faces[0].pen = new Pen(Color.Chartreuse);
            room.faces[1].pen = new Pen(Color.White);
            room.faces[2].pen = new Pen(Color.CornflowerBlue);
            room.faces[3].pen = new Pen(Color.Red);
            
            //маленький кубик
            Figure miniCube = new Cube(1.8f,new Material(0f, 0f, 0.1f, 0.7f, 1.5f), Type.Cube);
            miniCube.Offset(-3.7f, -2.5f, -4.1f);
            miniCube.SetPen(new Pen(Color.Magenta));
            
            //кубик побольше
            //Figure bigCube = new Cube(3f,new Material(0f, 0f, 0.1f, 0.7f, 1.5f), Type.Cube);
            //кубик побольше зеркальный
            Figure bigCube = new Cube(3f,new Material(0.9f, 0f, 0f, 0.1f, 1f), Type.Cube);
            //типа прозрачный
            //Figure bigCube = new Cube(3f,new Material(0f, 1f, 0f, 0.1f, 0.5f), Type.Cube);
            bigCube.Offset(2f, 1.5f, -4.1f);
            //bigCube.RotateArondRad(70);
            bigCube.SetPen(new Pen(Color.Aqua));
            
            //шарик
            //Figure sphere = new Sphere(new Point3D(-2.5f, -2, 2.5f), 2.5f,
              //  new Material(0.9f, 0.1f, 0.7f, 0.1f, new Point3D(0f, 0f, 0f), 1f),
              //  Type.Sphere);
           //шар над кубом   
           //Figure sphere = new Sphere(new Point3D(2f, 1.5f, -1.5f), 1.5f,
             //   new Material(0f, 0f, 0.1f, 0.7f, 1.5f),
             //   Type.Sphere);
             //Сфера справа от большого куба на полу
           Figure sphere = new Sphere(new Point3D(-1.8f, 1.5f, -3.6f), 1.5f,
               new Material(0f, 0f, 0.1f, 0.7f, 1.5f),
               Type.Sphere);
            sphere.SetPenSphere(new Pen(Color.Violet));
            
            //добавляем источники света
            Light l1 = new Light(new Point3D(0f, 2f, 4.9f), new Point3D(1f, 1f, 1f));
            //Light l1 = new Light(new Point3D(-1.2f, 4f, 4.9f), new Point3D(1f, 1f, 1f));
            
            Scene scene = new Scene();
            scene.addFigure(room);
            scene.addFigure(miniCube);
            scene.addFigure(bigCube);
            scene.addFigure(sphere);
            scene.Light = l1;

            pictureBox1.Image = RayTracing.BackwardRayTracing(GetPixels(room), scene, width, height);
            pictureBox1.Invalidate();
        }
    }
}