using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using EmotionsLib;


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

cts.Cancel();

await emotions;
await emotions2;

foreach (var emotion in emotions.Result) Console.WriteLine($"{emotion.Item1} : {emotion.Item2}");
Console.WriteLine("\n");
foreach (var emotion in emotions2.Result) Console.WriteLine($"{emotion.Item1} : {emotion.Item2}");




