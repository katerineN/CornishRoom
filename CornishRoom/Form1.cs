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
        }
    }
}