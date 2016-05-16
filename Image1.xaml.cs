using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using SOD_CS_Library;

namespace ClientApp {
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Image1 : Window
    {
        private Point start;
        private Point origin;
        private bool[] flags = new bool[2];
        public SOD_CS_Library.SOD SOD;
        public Image1()
        {
            InitializeComponent();


            image.Source = new BitmapImage(new Uri(@"\ENDANGERED-SAFARI-SQUARE.png", UriKind.Relative));

            TransformGroup group = new TransformGroup();
            TranslateTransform tt = new TranslateTransform();
            group.Children.Add(tt);
            image.RenderTransform = group;

            image.MouseLeftButtonDown += image_MouseLeftButtonDown;
            image.MouseLeftButtonUp += image_MouseLeftButtonUp;
            image.MouseMove += image_MouseMove;

        }

        public void setImageSize(int width, int height)
        {

        }

        private void image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            image.CaptureMouse();
            var tt = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
            start = e.GetPosition(border);
            origin = new Point(tt.X, tt.Y);
            Console.WriteLine("ORIGINAL TTX {0} TTY {1}", tt.X, tt.Y);
        }
        private void image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            flags[0] = false;
            sendNewPoint();
            //skipTo(origin);
            image.ReleaseMouseCapture();
        }
        private void image_MouseMove(object sender, MouseEventArgs e)
        {
            if (!image.IsMouseCaptured) return;

            var tt = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
            Vector v = start - e.GetPosition(border);
            if (!flags[0])
            {
                flags[0] = true;

                if (Math.Sqrt(Math.Pow(tt.X - origin.X + v.X, 2)) > Math.Sqrt(Math.Pow(tt.Y - origin.Y + v.Y, 2)))
                {
                    flags[1] = true;
                }
                else
                {
                    flags[1] = false;
                }
            }
            else
            {
                if (flags[1])
                {
                    tt.X = origin.X - v.X;
                    Console.WriteLine("NEW TTX {0}", tt.X);
                }
                else
                {
                    tt.Y = origin.Y - v.Y;
                    Console.WriteLine("NEW TTY {0}", tt.Y);
                }
            }
        }

        private void sendNewPoint()
        {
            var tt = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
            Point p = new Point(tt.X, tt.Y);

            string s = "pointtag;" + p.X + ";" + p.Y;
            SOD.SendToDevices.All("string", s);

            //SOD.SendStringToDevices(stringToSend, new string[3]{"all", "all", "all"});
            filterDefinition filterList = new filterDefinition();
            filterList.AddFilter.All();
            filterList.AddFilter.All();
            filterList.AddFilter.All();
            //SOD.SendToDevices.CustomFilter(filterList, "string", stringToSend);

        }

        private void skipTo(Point p)
        {
            var tt = (TranslateTransform)((TransformGroup)image.RenderTransform).Children.First(tr => tr is TranslateTransform);
            tt.X = p.X;
            tt.Y = p.Y;
        }
        public void setNewPoint(Point p)
        {
            skipTo(p);
        }
    }
}