using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using EmotionsLib;

namespace ConsoleApp {

    public class ImageInfo
    {
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
            set { this.Image = value; }
        }

        public string AssignInfo(Task<List<Tuple<string, float>>> emotions_list)
        {
            string _info = "";
            foreach (var emotion in emotions_list.Result) _info += $"{emotion.Item1} : {emotion.Item2}\n";
            this.Info = _info;
            return _info;
        }
    }


    public class Program
    {
        public async void OneImage()
        {
            var counter = new EmotionFerPlus();

            CancellationTokenSource cts = new CancellationTokenSource();
            CancellationToken token = cts.Token;


            using Image<Rgb24> image = Image.Load<Rgb24>(
                //"face1.png"       //сходится
                //"anger.jpg"       //сходится
                "disgust.jpg"     //нет
                                  //"surprise.jpg"    //нет
                );
            using Image<Rgb24> image2 = Image.Load<Rgb24>(
                //"face1.png"       //сходится
                "anger.jpg"       //сходится
                                  //"disgust.jpg"     //нет
                                  //"surprise.jpg"    //нет
                );

            var emotions = counter.EmotionRecognition(image, token);
            var emotions2 = counter.EmotionRecognition(image2, CancellationToken.None);

            //cts.Cancel();

            await emotions;
            await emotions2;

            foreach (var emotion in emotions.Result) Console.WriteLine($"{emotion.Item1} : {emotion.Item2}");
            Console.WriteLine("\n");
            foreach (var emotion in emotions2.Result) Console.WriteLine($"{emotion.Item1} : {emotion.Item2}");
        }

        public static void Main()
        {

        }
    }

}

