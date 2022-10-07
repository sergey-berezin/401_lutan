using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.ComponentModel;

namespace EmotionsLib
{
    public class EmotionFerPlus : IDisposable
    {
        private InferenceSession session;
        public string[] keys = { "neutral", "happiness", "surprise", "sadness", "anger", "disgust", "fear", "contempt" };

        public EmotionFerPlus()
        {
            using var modelStream = typeof(EmotionFerPlus).Assembly.GetManifestResourceStream("EmotionsLib.emotion-ferplus-7.onnx");
            using var memoryStream = new MemoryStream();
            modelStream.CopyTo(memoryStream);
            this.session = new InferenceSession(memoryStream.ToArray());
        }

        public async Task<List<Tuple<string, float>>> EmotionRecognition(Image<Rgb24> image, CancellationToken token)
        {
            return await Task<List<Tuple<string, float>>>.Factory.StartNew(() =>
            {
                //token.ThrowIfCancellationRequested();

                image.Mutate(ctx => ctx.Resize(new Size(64, 64)));
                var inputs = new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("Input3", GrayscaleImageToTensor(image)) };

                IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs); ;

                var emotions = Softmax(results.First(v => v.Name == "Plus692_Output_0").AsEnumerable<float>().ToArray());

                List<Tuple<string, float>> result = new();

                foreach (var i in keys.Zip(emotions))
                {
                    if (token.IsCancellationRequested) break;
                    result.Add(Tuple.Create(i.First, i.Second));
                }
                return result;

            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        DenseTensor<float> GrayscaleImageToTensor(Image<Rgb24> img)
        {
            var w = img.Width;
            var h = img.Height;
            var t = new DenseTensor<float>(new[] { 1, 1, h, w });

            img.ProcessPixelRows(pa =>
            {
                for (int y = 0; y < h; y++)
                {
                    Span<Rgb24> pixelSpan = pa.GetRowSpan(y);
                    for (int x = 0; x < w; x++)
                    {
                        t[0, 0, y, x] = pixelSpan[x].R; 
                    }
                }
            });

            return t;
        }
        static float[] Softmax(float[] z)
        {
            var exps = z.Select(x => Math.Exp(x)).ToArray();
            var sum = exps.Sum();
            return exps.Select(x => (float)(x / sum)).ToArray();
        }

        public void Dispose()
        {
            session.Dispose();
        }

        ~EmotionFerPlus()
        {
            session.Dispose();
        }
    }

}