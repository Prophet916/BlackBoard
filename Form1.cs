using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.MonthCalendar;

namespace BlackBoard
{
    public partial class BlackBoard : Form
    {
        string filepath;
        public struct RGBAColor
        {
            // Properties for Red, Green, Blue, and Alpha components
            public byte Red { get; set; }
            public byte Green { get; set; }
            public byte Blue { get; set; }
            public float Alpha { get; set; }

            // Constructor to initialize the RGBAColor
            public RGBAColor(byte red, byte green, byte blue, float alpha)
            {
                Red = red;
                Green = green;
                Blue = blue;
                Alpha = alpha;
            }

            // Convenience constructor to use Alpha as percentage (0-100)
            public RGBAColor(byte red, byte green, byte blue, int alphaPercentage)
            {
                Red = red;
                Green = green;
                Blue = blue;
                Alpha = alphaPercentage / 100f; // Convert percentage to float (0-1)
            }
        }
        public BlackBoard()
        {
            InitializeComponent();

            RGBAColor color = new RGBAColor(30, 30, 30, 1);

            // Convert RGBAColor to Color
            Color backgroundColor = Color.FromArgb(
                alpha: (int)(color.Alpha * 255), // Alpha value should be in the range 0-255
                red: color.Red,
                green: color.Green,
                blue: color.Blue
            );

            bm = new Bitmap(pic.Width, pic.Height);
            g = Graphics.FromImage(bm);
            g.Clear(backgroundColor);
            pic.Image = bm;
        }
        Bitmap bm;
        Graphics g;
        bool paint = false;
        Point px, py;
        Pen p = new Pen(Color.WhiteSmoke, 1);
        int index;
        int x, y, sX, sY, cX, cY;

        ColorDialog cd = new ColorDialog();
        Color new_color;

        private void btnCircle_Click(object sender, EventArgs e)
        {
            index = 3;
        }

        private void btnBox_Click(object sender, EventArgs e)
        {
            index = 4;
        }

        private void btnLine_Click(object sender, EventArgs e)
        {
            index = 5;
        }

        private void pic_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            if(paint)
            {
                if (index == 3)
                {
                    g.DrawEllipse(p, cX, cY, sX, sY);
                }
                if (index == 4)
                {
                    g.DrawRectangle(p, cX, cY, sX, sY);
                }
                if (index == 5)
                {
                    g.DrawLine(p, cX, cY, x, y)
    ;
                }
            }
        }

        private void btnClearImage_Click(object sender, EventArgs e)
        {
            RGBAColor color = new RGBAColor(30, 30, 30, 1);

            // Convert RGBAColor to Color
            Color backgroundColor = Color.FromArgb(
                alpha: (int)(color.Alpha * 255), // Alpha value should be in the range 0-255
                red: color.Red,
                green: color.Green,
                blue: color.Blue
            );
            g.Clear(backgroundColor);
            pic.Image = bm;
            index = 0;
        }

        private void btnColor_Click(object sender, EventArgs e)
        {
            cd.ShowDialog();
            new_color = cd.Color;
            pic_color.BackColor = new_color;
            p.Color = cd.Color;
        }

        static Point set_point(PictureBox pb, Point pt)
        {
            float pX = 1f * pb.Image.Width / pb.Width;
            float pY = 1f * pb.Image.Height / pb.Height;
            return new Point((int)(pt.X * pX), (int)(pt.Y * pY));
        }

        private void colorPicker_MouseClick(object sender, MouseEventArgs e)
        {
            Point point = set_point(colorPicker, e.Location);
            pic_color.BackColor = ((Bitmap)colorPicker.Image).GetPixel(point.X, point.Y);
            new_color = pic_color.BackColor;
            p.Color = pic_color.BackColor;
        }

        private void validate(Bitmap bm, Stack<Point>sp, int x, int y, Color old_color, Color new_color)
        {
            Color cx = bm.GetPixel(x, y);
            if(cx == old_color)
            {
                sp.Push(new Point(x, y));
                bm.SetPixel(x, y, new_color);
            }
        }

        public void Fill(Bitmap bm,int x, int y, Color new_clr)
        {
            Color old_color = bm.GetPixel(x, y);
            Stack<Point> pixel = new Stack<Point>();
            pixel.Push(new Point(x, y));
            bm.SetPixel(x,y,new_clr);
            if (old_color == new_clr) return;

            while(pixel.Count > 0)
            {
                Point pt = (Point)pixel.Pop();
                if(pt.X >= 0 && py.Y > 0 && pt.X < bm.Width - 1 && pt.Y < bm.Height - 1)
                {
                    validate(bm, pixel, pt.X - 1, pt.Y, old_color, new_clr);
                    validate(bm, pixel, pt.X, pt.Y - 1, old_color, new_clr);
                    validate(bm, pixel, pt.X + 1, pt.Y, old_color, new_clr);
                    validate(bm, pixel, pt.X, pt.Y + 1, old_color, new_clr);
                }
            }
        }

        private void pic_MouseClick(object sender, MouseEventArgs e)
        {
            if(index == 7)
            {
                Point point = set_point(pic, e.Location);
                Fill(bm, point.X, point.Y, new_color);
            }
        }

        private void btnFill_Click(object sender, EventArgs e)
        {
            index = 7;
        }

        private void btnSaveImage_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "Image(*.jpg)|*.jpg|(*.*|*.*";
            if(sfd.ShowDialog() == DialogResult.OK)
            {
                Bitmap btm = bm.Clone(new Rectangle(0,0, pic.Width, pic.Height),bm.PixelFormat);
                btm.Save(sfd.FileName, ImageFormat.Jpeg);
                MessageBox.Show("Image saved successfully");
            }
        }

        private void btnSaveTxt_Click(object sender, EventArgs e)
        {
            if (File.Exists(filepath))
            {
                File.WriteAllText(filepath, txtArea.Text);
            }
            else
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "Text Document(*.txt)|*.txt";
                var result = saveFileDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    filepath = saveFileDialog.FileName;
                    Stream stream = saveFileDialog.OpenFile();
                    StreamWriter streamWriter = new StreamWriter(stream);
                    streamWriter.WriteLine(txtArea.Text);
                    streamWriter.Close();
                    stream.Close();
                }
            }
        }

        private void btnLoadTxt_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text Files(*.txt)|*.txt";
            openFileDialog.Title = "Open text files only";
            openFileDialog.ShowDialog();
            if (openFileDialog.FileName != string.Empty)
            {
                filepath = openFileDialog.FileName;
                txtArea.Text = File.ReadAllText(filepath);
            }
        }

        private void btnClearTxt_Click(object sender, EventArgs e)
        {
            txtArea.Clear();
        }

        private void pic_MouseDown(object sender, MouseEventArgs e)
        {
            paint = true;
            py = e.Location;

            cX= e.X;
            cY = e.Y;
        }

        private void pic_MouseMove(object sender, MouseEventArgs e)
        {
            RGBAColor color = new RGBAColor(30, 30, 30, 1);

            // Convert RGBAColor to Color
            Color backgroundColor = Color.FromArgb(
                alpha: (int)(color.Alpha * 255), // Alpha value should be in the range 0-255
                red: color.Red,
                green: color.Green,
                blue: color.Blue
            );

            Pen erase = new Pen(backgroundColor, 10);
            if (paint)
            {
                if (index == 1)
                {
                    px = e.Location;
                    g.DrawLine(p, px, py);
                    py = px;
                }
                if (index == 2)
                {
                    px = e.Location;
                    g.DrawLine(erase, px, py);
                    py = px;
                }
            }
            //if (paint)
            //{
            //    if (index == 1)
            //    {
            //        px = e.Location;
            //        g.DrawLine(p, px, py);
            //        py = px;
            //    }
            //    else if (index == 2)
            //    {
            //        px = e.Location;
            //        // Erase by clearing the area with the background color
            //        g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
            //        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            //        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //        g.Clear(Color.FromArgb(255, 30, 30, 30)); // Use the background color to erase
            //        g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            //        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            //        py = px;
            //    }
            //}
            pic.Refresh();

            x = e.X;
            y = e.Y;
            sX = cX - cX;
            sY = e.Y - cY;
        }

        private void pic_MouseUp(object sender, MouseEventArgs e)
        {
            paint = false;

            sX = x - cX;
            sY = y - cY;

            if(index == 3)
            {
                g.DrawEllipse(p, cX, cY, sX, sY);
            }
            if(index == 4)
            {
                g.DrawRectangle(p, cX, cY, sX,sY);  
            }
            if(index == 5)
            {
                g.DrawLine(p, cX, cY, x, y)
;            }
        }

        private void btnPencil_Click(object sender, EventArgs e)
        {
            index = 1;
        }

        private void btnErase_Click(object sender, EventArgs e)
        {
            index = 2;
        }

        private void btnPic_Click(object sender, EventArgs e)
        {

        }
    }
}
