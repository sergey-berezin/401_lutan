using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Sqlite;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.ComponentModel.DataAnnotations;
using Microsoft.Win32;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Polly;
using System.Net.Http;
using Polly.Retry;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Net.Http.Json;
using Contracts;
using System.Net.Http.Headers;
using System.Net;

namespace WpfApp
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static List<string> emotions_list = new() { "neutral", "happiness", "surprise", "sadness", "anger", "disgust", "fear", "contempt" };
        Dictionary<string, ObservableCollection<Picture>> dict_all = new Dictionary<string, ObservableCollection<Picture>>()
        {
            { "neutral", new ObservableCollection<Picture>() },
            { "happiness", new ObservableCollection<Picture>() },
            { "surprise", new ObservableCollection<Picture>() },
            { "sadness", new ObservableCollection<Picture>() },
            { "anger", new ObservableCollection<Picture>() },
            { "disgust", new ObservableCollection<Picture>() },
            { "fear", new ObservableCollection<Picture>() },
            { "contempt", new ObservableCollection<Picture>() },
            { "all", new ObservableCollection<Picture>() },
            { "new", new ObservableCollection<Picture>() }
        };

        Dictionary<string, ObservableCollection<Picture>> dict_new = new Dictionary<string, ObservableCollection<Picture>>()
        {
            { "neutral", new ObservableCollection<Picture>() },
            { "happiness", new ObservableCollection<Picture>() },
            { "surprise", new ObservableCollection<Picture>() },
            { "sadness", new ObservableCollection<Picture>() },
            { "anger", new ObservableCollection<Picture>() },
            { "disgust", new ObservableCollection<Picture>() },
            { "fear", new ObservableCollection<Picture>() },
            { "contempt", new ObservableCollection<Picture>() },
            { "all", new ObservableCollection<Picture>() },
        };

        CancellationTokenSource cts = new();
        bool TaskInProcess = false;
        private AsyncRetryPolicy retryPolicy;
        private int MAX_RETRIES = 3;
        private string URL = "https://localhost:7156/api/images";

        public MainWindow()
        {
            InitializeComponent();

            retryPolicy = Policy.Handle<HttpRequestException>().WaitAndRetryAsync(MAX_RETRIES, times =>
                   TimeSpan.FromMilliseconds(times * 200));

            loadExistingImagesAsync();
            DataContext = dict_all;
        }

        private async Task loadExistingImagesAsync()
        {
            try
            {
                await retryPolicy.ExecuteAsync(async () =>
                {
                    HttpClient client = new HttpClient();
                    var result = await client.GetAsync(URL); //, cts.Token);

                    if (result.IsSuccessStatusCode)
                    {
                        var existingImages = await result.Content.ReadFromJsonAsync<ObservableCollection<Picture>>();
                        //MessageBox.Show($"{existingImages.Count}");
                        foreach (var img in existingImages)
                        {
                            dict_all["all"].Add(img);
                            dict_all[img.Section].Add(img);
                        }
                    }
                    else
                    {

                        MessageBox.Show($"Error: code {result.StatusCode}, message {result.Content}");
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void LoadImagesCmd(object sender, RoutedEventArgs e)
        {
            var fbd = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            //var ofd = new FolderBrowserDialog();
            //ofd.Filter = "Image Files(*.BMP;*.JPG;*.GIF;*.PNG)|*.BMP;*.JPG;*.GIF;*.PNG";  
            string path;
            string[] file_names;

            if (fbd.ShowDialog() == true)
            {
                path = fbd.SelectedPath;
                file_names = Directory.GetFiles(path);
            }
            else return;

            foreach (string file_name in file_names)
            {
                //string short_name = file_name.Remove(0, file_name.LastIndexOf('\\') + 1);
                dict_all["new"].Add(new Picture(file_name, "", ""));
            }
            listNew.ItemsSource = dict_all["new"];
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
                foreach (var pair in dict_new)
                {
                    pair.Value.Clear();
                }

                progress_bar.Visibility = Visibility.Hidden;
                txt_progress.Visibility = Visibility.Visible;

                TaskInProcess = false;
            }
        }

        private async void ClearAllCmd(object sender, RoutedEventArgs e)
        {
            try
            {
                HttpClient client = new HttpClient();
                await retryPolicy.ExecuteAsync(async () =>
                {
                    var status = await client.DeleteAsync(URL);
                    foreach (var pair in dict_all)
                    {
                        pair.Value.Clear();
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private async void AnalyzeImagesCmd(object sender, RoutedEventArgs e)
        {
            if (dict_all["new"].Count == 0)
            {
                MessageBox.Show("No images to analyze");
            }
            else
            {
                TaskInProcess = true;
                mi_analyze.IsEnabled = false;
                mi_load.IsEnabled = false;

                try
                {

                    progress_bar.Visibility = Visibility.Visible;
                    progress_bar.Maximum = dict_all["new"].Count;
                    progress_bar.Value = 0;
                    var step = progress_bar.Maximum / dict_all["new"].Count;
                    txt_progress.Visibility = Visibility.Hidden;

                    cts = new CancellationTokenSource();

                    foreach (var image in dict_all["new"])
                    {
                        await retryPolicy.ExecuteAsync(async () =>
                        {
                            var client = new HttpClient();
                            var img_bytes = await File.ReadAllBytesAsync(image.FilePath, cts.Token);
                            var new_img = new NewPicture(image.FilePath, img_bytes);
                            //MessageBox.Show("all going good");

                            var response = await client.PostAsJsonAsync(URL, new_img, cts.Token);

                            if (response.IsSuccessStatusCode)
                            {
                                //MessageBox.Show($"here, {response.IsSuccessStatusCode}, {response.StatusCode}");

                                if (response.StatusCode == HttpStatusCode.OK)
                                {
                                    var to_add = await response.Content.ReadFromJsonAsync<Picture>();
                                    //MessageBox.Show("norm");
                                    if (to_add != null)
                                    {
                                        dict_new["all"].Add(to_add);
                                        dict_new[to_add.Section].Add(to_add);
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show($"code {(int)response.StatusCode}, \n message:{await response.Content.ReadAsStringAsync()}");
                            }
                        });
                        progress_bar.Value += step;
                    }

                    progress_bar.Visibility = Visibility.Hidden;
                    txt_progress.Visibility = Visibility.Visible;

                    foreach (var state in dict_new.Keys)
                    {
                        foreach (var item in dict_new[state])
                        {
                            dict_all[state].Add(new Picture(item));
                        }
                    }

                    foreach (var val in dict_new.Values)
                    {
                        val.Clear();
                    }

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
                }
                catch
                {
                    MessageBox.Show("Analysis was canceled");
                }

                TaskInProcess = false;
                mi_load.IsEnabled = true;
                mi_analyze.IsEnabled = true;
            }
        }


        /*private void DeleteImgCmd(object sender, RoutedEventArgs e)
        {
            string section = "list" + ((TabItem)TabCtrl.SelectedItem).Header.ToString();
            var lv_curr = TabCtrl.FindName(section) as ListView;
            var img_ii = lv_curr.SelectedItem as Picture;
            
            if (img_ii.Section == "new")
            {
                dict_all["new"].Remove(img_ii);
            }
            else{
                dict_all[img_ii.Section].Remove(img_ii);
                dict_all["all"].Remove(img_ii);

                using (var db = new ImageContext())
                {
                    var image_delete = db.Pictures.Where(i => i.FilePath.Equals(img_ii.FilePath)).FirstOrDefault();
                    var image_hash_delete = db.PicturesHash.Where(i => i.ImageId.Equals(image_delete.ImageId)).FirstOrDefault();
                    db.Pictures.Remove(image_delete);
                    db.PicturesHash.Remove(image_hash_delete);
                    db.SaveChanges();
                }

                MessageBox.Show("Deleted from db");
            }
        }*/
    }
}
