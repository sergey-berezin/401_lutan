using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Server.Database
{
    public interface IImageDB
    {

        public Task<List<Picture>> GetImagesByEmotion(string emotion);

        public Task<List<Picture>> GetAllImages();

        public Task<Picture> PostImage(byte[] img_byte, string path, CancellationToken ct);

        public int DeleteAll();
    }

    class ImageContext : DbContext
    {
        public DbSet<Picture> Pictures { get; set; }
        public DbSet<PictureHash> PicturesHash { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder o)
        {
            o.UseSqlite("Data Source=DatabaseEm.db");
        }
    }

    public class InMemoryImage : IImageDB
    {

        EmotionsLib.EmotionFerPlus EmotionCounter = new();

        static string ComputeSha256Hash(byte[] rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(rawData);
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private async Task<string> AnalyzeImage(Picture image, CancellationToken ct)
        {
            var Emotions = await Task.Run(async () =>
            {
                Image<Rgb24> img_rgb = SixLabors.ImageSharp.Image.Load<Rgb24>(image.FilePath);
                var emotions = EmotionCounter.EmotionRecognition(img_rgb, ct).Result;
                emotions.Sort((a, b) => -(a.Item2.CompareTo(b.Item2)));
                image.AssignInfo(emotions);
                return emotions;
            }, ct);
            string field = Emotions[0].Item1;
            image.Section = field;
            return image.Section;
        }

        private bool ShouldAddNewimage(string img_hash, byte[] img_byte)
        {
            using (var db = new ImageContext())
            {
                var img_right_hash = db.PicturesHash.Where(i => i.Hash.Equals(img_hash));

                if (img_right_hash.Count() == 0)
                {
                    return true;
                }
                else
                {
                    var img_right_image = img_right_hash.Where(i => i.Image.Equals(img_byte));

                    if (img_right_image.Count() == 0)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public async Task<Picture> PostImage(byte[] img_byte, string path, CancellationToken ct)
        {
            try
            {
                var img_hash = ComputeSha256Hash(img_byte);

                if (ShouldAddNewimage(img_hash, img_byte))
                {
                    using (var db = new ImageContext())
                    {
                        Picture newPic = new Picture
                        {
                            FilePath = path,
                            Section = "",
                            Info = ""
                        };
                        string section = await AnalyzeImage(newPic, ct);
                        db.Pictures.Add(newPic);
                        db.SaveChanges();

                        var newId = db.Pictures.OrderBy(i => i.ImageId).Last().ImageId;
                        PictureHash newPicHash = new PictureHash
                        {
                            Hash = img_hash,
                            Image = img_byte,
                            ImageId = newId
                        };

                        db.PicturesHash.Add(newPicHash);
                        db.SaveChanges();

                        return newPic;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }

        public int DeleteAll()
        {
            using (var db = new ImageContext())
            {
                if (db.Pictures.Any())
                {
                    foreach (var pic in db.Pictures)
                    {
                        db.Pictures.Remove(pic);
                    }
                }
                if (db.PicturesHash.Any())
                {
                    foreach (var pic in db.PicturesHash)
                    {
                        db.PicturesHash.Remove(pic);
                    }
                }

                db.SaveChanges();
            }
            return 0;
        }

        public Task<List<Picture>> GetImagesByEmotion(string emotion)
        {
            var emotion_list = new List<Picture>();
            using (var db = new ImageContext())
            {
                if (db.Pictures.Any())
                {
                    emotion_list = db.Pictures.Where(i => i.Section == emotion).ToList();
                }
            }
            return Task.FromResult(emotion_list);
        }

        public Task<List<Picture>> GetAllImages()
        {
            var images = new List<Picture>();
            using (var db = new ImageContext())
            {
                if (db.Pictures.Any())
                {
                    images = db.Pictures.ToList();
                }
            }
            return Task.FromResult(images);
        }
    }
}
