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

        public string[] task_questions;
        public string[] demo_questions;

        public Uri[] round_image;
        public Uri[] hint_images;
        public Uri[] demo_hint_images;


        public int task = 0;
        public int round = 0;

        public Image1()
        {
            InitializeComponent();

            demo_questions = new string[3] {
                "On the map provided, name the capital city of France.",
                "Based on the visualization provided, what is the population of the Ethiopian Wolf and what is its Endangered status?",
                "In the image provided, find the milk jug."
            };

            demo_hint_images = new Uri[2] {
                new Uri(@"\images\wolf.png", UriKind.Relative),
                new Uri(@"\images\milk_jug.png", UriKind.Relative)
            };

            task_questions = new string[15] {
                "On the map provided, name the capital city of Poland",
                "On the map provided, name all the major cities in Germany",
                "On the map provided, name the countries that surround Luxembourg",
                "On the map provided, name the country with the capital city of Podgorica",
                "On the map provided, name the country that borders the South of the Black Sea",
                "Based on the visualization provided, what is the Endangered status of the Greater Kudu?",
                "Based on the visualization provided, what is the population of the Walia Ibex?",
                "Based on the visualization provided, what is the endangered status of the African Golden Cat and what is its population? ",
                "Based on the visualization provided, what is the Endangered status of the Dryad Money?",
                "Based on the visualization provided, what is the population of the Eastern Gorilla and what is its Endangered status?",
                "In the image provided, find a rolling pin.",
                "In the image provided, find a bundle of carrots.",
                "In the image provided, find a pair of scissors.",
                "In the image provided, find a ring of keys.",
                "In the image provided, find 3 stars."
            };

            round_image = new Uri[3] {
                new Uri(@"\map.png", UriKind.Relative),
                new Uri(@"\ENDANGERED-SAFARI-SQUARE.png", UriKind.Relative),
                new Uri(@"\vissearch1.png", UriKind.Relative)
            };

            hint_images = new Uri[10] {
                new Uri(@"\images\greater_kudu.png", UriKind.Relative),
                new Uri(@"\images\walia_ibex.png", UriKind.Relative),
                new Uri(@"\images\golden_cat.png", UriKind.Relative),
                new Uri(@"\images\dryad_monkey.png", UriKind.Relative),
                new Uri(@"\images\gorilla.png", UriKind.Relative),
                new Uri(@"\images\rolling_pin.png", UriKind.Relative),
                new Uri(@"\images\carrots.png", UriKind.Relative),
                new Uri(@"\images\scissors.png", UriKind.Relative),
                new Uri(@"\images\keys.png", UriKind.Relative),
                new Uri(@"\images\stars.png", UriKind.Relative)
            };


            if (isDemo.IsChecked == true)
            {
                text_block.Text = demo_questions[task];
            }
            else
            {
                text_block.Text = task_questions[task];
            }

            image.Source = new BitmapImage(round_image[round]);

            TransformGroup group = new TransformGroup();
            TranslateTransform tt = new TranslateTransform();
            group.Children.Add(tt);
            image.RenderTransform = group;

            image.MouseLeftButtonDown += image_MouseLeftButtonDown;
            image.MouseLeftButtonUp += image_MouseLeftButtonUp;
            image.MouseMove += image_MouseMove;


           
        }

        private void nextlevel_button_Click(object sender, RoutedEventArgs e)
        {
            // go to next task
            task++;

            if (isDemo.IsChecked == true)
            {
                round++;
                if (task >= 3 || round >= 3)
                {
                    task = 0;
                    round = 0;
                }

                text_block.Text = demo_questions[task];
                if (round > 0) {
                    hint_img.Source = new BitmapImage(demo_hint_images[task - 1]);
                }
                else
                {
                    hint_img.Source = null;
                }

                image.Source = new BitmapImage(round_image[round]);

                Console.WriteLine("\n\n\n\nHeight: " + image.ActualHeight + " Width: " + image.ActualWidth);

                Dictionary<string, string> dataToSendBack = new Dictionary<string, string>(){
                    { "width", image.ActualWidth.ToString() },
                    { "height", image.ActualHeight.ToString() },
                    {"round", round.ToString()}
                };
                this.SOD.SendToDevices.All("setImgSize", dataToSendBack);

            }
            else
            {
                // end ;loop back
                if (task >= 15)
                {
                    round = 0;
                    task = 0;
                    image.Source = new BitmapImage(round_image[round]);
                }
                // switch image for new round
                if (task == 5 || task == 10)
                {
                    round++;
                    image.Source = new BitmapImage(round_image[round]);
                    Dictionary<string, string> dataToSendBack = new Dictionary<string, string>(){
                        { "width", image.ActualWidth.ToString() },
                        { "height", image.ActualHeight.ToString() },
                        {"round", round.ToString()}
                    };
                        this.SOD.SendToDevices.All("setImgSize", dataToSendBack);
                }

                text_block.Text = task_questions[task];

                if (round > 0)
                {
                    hint_img.Source = new BitmapImage(hint_images[task - 5]);
                }
                else
                {
                    hint_img.Source = null;
                }

                
            }
        
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