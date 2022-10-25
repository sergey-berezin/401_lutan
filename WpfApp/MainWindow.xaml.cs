//using ConsoleApp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp
{
    public class ImageInfo
    {
        private string filePath;
        public string FilePath
        {
            get { return this.filePath; }
            set { this.filePath = value; }
        }

        private string info;
        public string Info
        {
            get { return this.info; }
            set { this.info = value; }
        }

        private Image<Rgb24> image;
        public Image<Rgb24> Image
        {
            get { return this.image; }
            set { this.image = value; }
        }

        private BitmapImage imageBmp;
        public BitmapImage ImageBmp
        {
            get { return this.imageBmp; }
            set { this.imageBmp = value; }
        }
        public ImageInfo(string fp, Image<Rgb24> img, BitmapImage bmi)
        {
            this.Info = "Waiting for analysis";
            this.FilePath = fp;
            this.Image = img;
            this.ImageBmp = bmi;
        }

        public string AssignInfo(Task<List<Tuple<string, float>>> emotions_list)
        {
            string _info = "";
            foreach (var emotion in emotions_list.Result) _info += $"{emotion.Item1} : {emotion.Item2}\n";
            this.Info = _info;
            return _info;
        }

        //public BitmapSource ConvertRgb24ToBitmapSource(Image<Rgb24> image)
        //{
        //    PixelFormat pf = PixelFormats.Rgb24;
        //    int width = image.Width;
        //    int height = image.Height;
        //    int rawStride = (width * pf.BitsPerPixel + 7) / 8;
        //    byte[] rawImage = new byte[rawStride * height];

        //    // Initialize the image with data.
        //    //Random value = new Random();
        //    //value.NextBytes(rawImage);
        //    //using (MemoryStream stream = new MemoryStream())
        //    //{
        //        //encoder.Save(stream);
        //        //byte[] bytes = stream.ToArray();
        //        // Use the raw binary data elsewhere, or use the MemoryStream directly after seeking back to the beginning
        //    //}

        //    // Create a BitmapSource.
        //    BitmapSource bitmap = BitmapSource.Create(width, height,
        //        96, 96, pf, null,
        //        rawImage, rawStride);
        //    return bitmap;
        //}
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ObservableCollection<ImageInfo> list_neutral = new();
        ObservableCollection<ImageInfo> list_new = new();
        EmotionsLib.EmotionFerPlus EmotionCounter;
        public MainWindow()
        {
            InitializeComponent();
            EmotionCounter = new EmotionsLib.EmotionFerPlus();
        }

        private void LoadImagesCmd(object sender, RoutedEventArgs e)
        {
            var ofd = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            ofd.Multiselect = true;
            string[] file_names;

            if (ofd.ShowDialog() == true)
            {
                file_names = ofd.FileNames;
            }
            //else throw new Exception("OpenFileDialog not working");
            else return;

            foreach(string file_name in file_names)
            {
                using Image<Rgb24> image = SixLabors.ImageSharp.Image.Load<Rgb24>(file_name);
                BitmapImage imgBmi = new BitmapImage(new Uri(file_name));
                imgBmi.DecodePixelHeight = 10;
                //string fileName = file_name.Remove(0, file_name.LastIndexOf('\\') + 1);
                list_new.Add(new ImageInfo(file_name.Remove(0, file_name.LastIndexOf('\\') + 1), image, imgBmi));
            }
            Console.WriteLine($"New images: {list_new.Count}");
            listNew.ItemsSource = list_new;
        }
    }
}
