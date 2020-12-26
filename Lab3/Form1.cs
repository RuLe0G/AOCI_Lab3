using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace lab3
{
    public partial class Form1 : Form
    {

        private UCDLRAOCI myclass = new UCDLRAOCI();
        public Form1()
        {
            InitializeComponent();
        }
        //открыть картинку

        private string OpenFile()
        {
            string fileName = null;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = ("Файлы изображений | *.jpg; *.jpeg; *.jpe; *.jfif; *.png");
            var result = openFileDialog.ShowDialog(); // открытие диалога выбора файла            
            if (result == DialogResult.OK) // открытие выбранного файла
            {
                fileName = openFileDialog.FileName;
            }
            return fileName;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            myclass.Source(OpenFile());
            imageBox1.Image = myclass.sourceImage;
            imageBox2.Image = myclass.sourceImage;
        }
        private void button16_Click(object sender, EventArgs e)
        {
            myclass.MainImage();
            imageBox2.Image = myclass.MainImageExp;
        }


        private void button17_Click(object sender, EventArgs e)
        {

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = ("Файлы изображений | *.jpg; *.jpeg; *.jpe; *.jfif; *.png");
            var result = saveFileDialog.ShowDialog(); // открытие диалога выбора файла            
            if (result == DialogResult.OK) // открытие выбранного файла
            {
                string fileName = saveFileDialog.FileName;
                myclass.saveJpeg(fileName);
            }
        }

        Point[] pts = new Point[4];
        private void Scale_Click(object sender, EventArgs e)
        {
            imageBox2.Image = myclass.Scale(double.Parse(textBox1.Text, CultureInfo.InvariantCulture), double.Parse(textBox2.Text, CultureInfo.InvariantCulture));
        }
        
        
        int c = 0;
        private void imageBox1_MouseClick(object sender, MouseEventArgs e)
        {
            var ingCopy = myclass.sourceImage.Copy();
            int x = (int)(e.Location.X / imageBox1.ZoomScale);
            int y = (int)(e.Location.Y / imageBox1.ZoomScale);

            pts[c] = new Point(x, y);
            c++;
            if (c>=4)
            {
                c = 0;
            }

            int radius = 2;
            int thickness = 2; 
            var color = new Bgr(Color.Blue).MCvScalar;
            // функция, рисующая на изображении круг с заданными параметрами
            foreach (Point po in pts)
            {
                CvInvoke.Circle(ingCopy, po, radius, color, thickness);
            }
            
            imageBox1.Image = ingCopy;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            
            imageBox2.Image = myclass.homography(pts);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            imageBox2.Image = myclass.rotate(double.Parse(textBox3.Text),pts[0]);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            imageBox2.Image = myclass.shearing(10, 10);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            double x = double.Parse(textBox4.Text);
            double y = double.Parse(textBox5.Text);

            imageBox2.Image = myclass.shearing(x, y);
        }

        private void button4_Click_1(object sender, EventArgs e)
        {
            int X = int.Parse(textBox6.Text);
            int Y = int.Parse(textBox7.Text);
            imageBox2.Image = myclass.reflection(X, Y);
        }
    }
}