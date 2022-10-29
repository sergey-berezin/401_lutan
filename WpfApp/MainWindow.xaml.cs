using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        private int id;
        public int Id
        {
            get { return this.id; }
            set { this.id = value; }
        }

        public static int IdCount = 0;

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
            this.Id = IdCount;
            IdCount++;
        }

        public ImageInfo(ImageInfo obj)
        {
            this.Info = obj.Info;
            this.FilePath = obj.FilePath;
            this.Image = obj.Image;
            this.ImageBmp = obj.ImageBmp;
            this.Id = obj.Id;
        }

        public string AssignInfo(List<Tuple<string, float>> emotions_list)
        {
            string _info = "";
            foreach (var emotion in emotions_list) _info += $"{emotion.Item1} : {emotion.Item2}\n";
            this.Info = _info;
            return _info;
        }
    }



    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static List<string> emotions_list = new() { "neutral", "happiness", "surprise", "sadness", "anger", "disgust", "fear", "contempt" };
        static ObservableCollection<ImageInfo> list_neutral = new();
        static ObservableCollection<ImageInfo> list_happiness = new();
        static ObservableCollection<ImageInfo> list_surprise = new();
        static ObservableCollection<ImageInfo> list_sadness = new();
        static ObservableCollection<ImageInfo> list_anger = new();
        static ObservableCollection<ImageInfo> list_disgust = new();
        static ObservableCollection<ImageInfo> list_fear = new() ;
        static ObservableCollection<ImageInfo> list_contempt = new();
        static ObservableCollection<ImageInfo> list_new = new();
        static ObservableCollection<ImageInfo> list_all = new();
        Dictionary<string, ObservableCollection<ImageInfo>> dict_all = new Dictionary<string, ObservableCollection<ImageInfo>>()
        {
            { "neutral", list_neutral },
            { "happiness", list_happiness },
            { "surprise", list_surprise },
            { "sadness", list_sadness },
            { "anger", list_anger },
            { "disgust", list_disgust },
            { "fear", list_fear },
            { "contempt", list_contempt },
            { "all", list_all },
            { "new", list_new }
        };

        Dictionary<string, ObservableCollection<ImageInfo>> dict_new = new Dictionary<string, ObservableCollection<ImageInfo>>()
        {
            { "neutral", new ObservableCollection<ImageInfo>() },
            { "happiness", new ObservableCollection<ImageInfo>() },
            { "surprise", new ObservableCollection<ImageInfo>() },
            { "sadness", new ObservableCollection<ImageInfo>() },
            { "anger", new ObservableCollection<ImageInfo>() },
            { "disgust", new ObservableCollection<ImageInfo>() },
            { "fear", new ObservableCollection<ImageInfo>() },
            { "contempt", new ObservableCollection<ImageInfo>() },
            { "all", new ObservableCollection<ImageInfo>() },
        };

        EmotionsLib.EmotionFerPlus EmotionCounter = new();
        CancellationTokenSource cts;
        bool TaskInProcess = false;

        public MainWindow()
        {
            InitializeComponent();
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
            else return;

            foreach (string file_name in file_names)
            {
                Image<Rgb24> image = SixLabors.ImageSharp.Image.Load<Rgb24>(file_name);
                BitmapImage imgBmi = new BitmapImage(new Uri(file_name));
                list_new.Add(new ImageInfo(file_name.Remove(0, file_name.LastIndexOf('\\') + 1), image, imgBmi));
            }
            listNew.ItemsSource = list_new;
        }

        private async Task<ImageInfo> AnalyzeImage(ImageInfo image, CancellationTokenSource cts)
        {
            //CancellationTokenSource cts = new CancellationTokenSource();
            //CancellationToken token = cts.Token;

            var Emotions = await Task.Run(async () =>
            {
                var emotions = EmotionCounter.EmotionRecognition(image.Image, cts.Token).Result;
                emotions.Sort((a, b) => -(a.Item2.CompareTo(b.Item2)));
                image.AssignInfo(emotions);
                return emotions;
            }, cts.Token);
            string field = Emotions[0].Item1;
            dict_new[field].Add(image);
            dict_new["all"].Add(image);

            return image;
        }

        private void CancelCmd(object sender, RoutedEventArgs e)
        {
            if (!TaskInProcess)
            {
                MessageBox.Show("Analysis wasn't started");
            }
            else
            {
                cts.Cancel();
                foreach (var pair in dict_all)
                {
                    pair.Value.Clear();
                }

                progress_bar.Visibility = Visibility.Hidden;
                txt_progress.Visibility = Visibility.Visible;

                TaskInProcess = false;
            }
        }

        private void ClearCmd(object sender, RoutedEventArgs e)
        {
            if (((TabItem)TabCtrl.SelectedItem).HasHeader)
            {
                var i = ((TabItem)TabCtrl.SelectedItem).Header.ToString().ToLower();
                MessageBox.Show($"{i /*dict_all[i].ToString()*/}");
                dict_all[i].Clear();
                //MessageBox.Show($"{dict_all[i].Count.ToString()}");
            } else
            {
                MessageBox.Show($"Header not found in this tab");
                var ind = TabCtrl.SelectedIndex;
                dict_all[emotions_list[ind]].Clear();
                //MessageBox.Show($"{dict_all[emotions_list[ind]].Count.ToString()}");
            }
        }

        private void ClearAllCmd(object sender, RoutedEventArgs e)
        {
            foreach(var pair in dict_all)
            {
                pair.Value.Clear();
            }
        }

        private void AnalyzeImagesCmd(object sender, RoutedEventArgs e)
        {
            if (list_new.Count == 0)
            {
                MessageBox.Show("No images to analyze");
            }
            else
            {
                try
                {
                    TaskInProcess = true;

                    progress_bar.Visibility = Visibility.Visible;
                    progress_bar.Maximum = list_new.Count;
                    txt_progress.Visibility = Visibility.Hidden;

                    cts = new CancellationTokenSource();
                    
                    foreach (var image in list_new)
                    {
                        //MessageBox.Show($"{image.Id}, {image.Info}");
                        _ = AnalyzeImage(image, cts);
                        progress_bar.Value += 1;
                    }

                    foreach (var i in dict_new.Values) MessageBox.Show($"{i.Count}");

                    progress_bar.Visibility = Visibility.Hidden;
                    txt_progress.Visibility = Visibility.Visible;

                    //MessageBox.Show($"{dict_new.Keys.ToList()}");

                    foreach (var state in dict_new.Keys)
                    {
                        foreach (var item in dict_new[state])
                        {
                            dict_all[state].Add(new ImageInfo(item));
                        }
                    }

                    //foreach (var i in dict_all.Values) MessageBox.Show($"{i.Count}");

                    foreach (var val in dict_new.Values)
                    {
                        val.Clear();
                    }

                    //foreach (var i in dict_new.Values) MessageBox.Show($"{i.Count}");

                    dict_all["new"].Clear();

                    listAll.ItemsSource = dict_all["all"];
                    listNeutral.ItemsSource = dict_all["neutral"];
                    listHappiness.ItemsSource = dict_all["happiness"];
                    listSurprise.ItemsSource = dict_all["surprise"];
                    listSadness.ItemsSource = dict_all["sadness"];
                    listAnger.ItemsSource = dict_all["anger"];
                    listDisgust.ItemsSource = dict_all["disgust"];
                    listFear.ItemsSource = dict_all["fear"];
                    listContempt.ItemsSource = dict_all["contempt"];

                    //foreach (var i in dict_all.Values) MessageBox.Show($"{i.Count}");
                }
                catch
                {
                    MessageBox.Show("Analysis was canceled");
                }

                TaskInProcess = false;
            }
        }
    }
}
