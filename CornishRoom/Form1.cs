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
        public Color[,] pixelsColor;
        public int width, height;
        //перемножение матриц
        public static float[,] MultMatrix(float[,] m1, float[,] m2)
        {
            float[,] res = new float[m1.GetLength(0), m2.GetLength(1)];

            for (int i = 0; i < m1.GetLength(0); ++i)
            for (int j = 0; j < m2.GetLength(1); ++j)
            for (int k = 0; k < m2.GetLength(0); k++)
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
            /*
             Учитывая разницу между размером комнаты и экранным отображение приводим координаты к пикселям
             */
            Point3D up_left = room.points[room.faces[0].facePoints[0]];
            Point3D up_right = room.points[room.faces[0].facePoints[1]];
            Point3D down_right = room.points[room.faces[0].facePoints[2]];
            Point3D down_left = room.points[room.faces[0].facePoints[3]];
            Point3D [,] pixels = new Point3D[width, height];
            pixelsColor = new Color[width, height];
            Point3D step_up = (up_right - up_left) / (width - 1);//отношение ширины комнаты к ширине экрана
            Point3D step_down = (down_right - down_left) / (width - 1);//отношение высоты комнаты к высоте экрана
            Point3D up = up_left;
            Point3D down = down_left;
            for (int i = 0; i < width; ++i)
            {
                Point3D step_y = (up - down) / (height - 1);
                Point3D d = down;
                for (int j = 0; j < height; ++j)
                {
                    pixels[i, j] = d;
                    d += step_y;
                }
                up += step_up;
                down += step_down;
            }

            return pixels;
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            //сама комната
            Figure room = new Cube(10, new Material(0, 0, 0.05f, 0.7f), Type.Room);
            room.SetPen(new Pen(Color.Gray));
            room.faces[0].pen = new Pen(Color.Green);
            room.faces[1].pen = new Pen(Color.Yellow);
            room.faces[2].pen = new Pen(Color.Red);
            room.faces[3].pen = new Pen(Color.Blue);
            
            Figure bigCube = new Cube(2.8f,new Material(0f, 0f, 0.1f, 0.7f, 1.5f), Type.Cube);
            bigCube.Offset(-1.5f, 1.5f, -3.9f);//сдвиг по осям 
            bigCube.SetPen(new Pen(Color.Magenta));
            
            //добавляем источники света
            Light l1 = new Light(new Point3D(0f, 2f, 4.9f), new Point3D(1f, 1f, 1f));//белый, посреди комнаты,как люстра

            Scene scene = new Scene();
            scene.addFigure(room);
            scene.addFigure(bigCube);
            scene.Light = l1;

            pixelsColor = RayTracing.BackwardRayTracing(GetPixels(room), pixelsColor, scene, width, height);
            
            for (int i = 0; i < width; ++i){
                for (int j = 0; j < height; ++j)
                {
                    (pictureBox1.Image as Bitmap).SetPixel(i, j, pixelsColor[i, j]);
                }
                pictureBox1.Invalidate();
            }
        }
    }
}