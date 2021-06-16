using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WallPaper
{
    class DrawString
    {
        public bool isEnded = false;
        string str;
        Random rand;
        Point point;
        Color Color;
        public DrawString(string str, int formSize)
        {
            rand = new Random();
            this.str = str;
            Color = Color.FromArgb(rand.Next(255), rand.Next(255), rand.Next(255));
            point.X = rand.Next(formSize);
        }

        public void Process(int formSize)
        {
            point.Y += 10;
            if (point.Y >= formSize + 50) isEnded = true;
        }
        public void Draw(PaintEventArgs e)
        {
            e.Graphics.DrawString(str,Form1.MyFont,new SolidBrush(Color), point);
        }
    }
}
