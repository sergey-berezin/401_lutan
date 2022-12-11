using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Input;

namespace Contracts
{
    public class Picture
    {
        [Key]
        public int ImageId { get; set; }
        public string FilePath { get; set; }
        public string Section { get; set; }
        public string Info { get; set; }

        public Picture() { }
        public Picture(/*int imageId, */string path, string section, string info)
        {
            //ImageId = imageId;
            FilePath = path;
            Section = section;
            Info = info;
        }

        public Picture(Picture picture)
        {
            ImageId = picture.ImageId;
            FilePath = picture.FilePath;
            Section = picture.Section;
            Info = picture.Info;
        }

        public string AssignInfo(List<Tuple<string, float>> emotions_list)
        {
            string _info = "";
            foreach (var emotion in emotions_list) _info += $"{emotion.Item1} : {emotion.Item2}\n";
            this.Info = _info;
            return _info;
        }
    }

    public class PictureHash
    {
        [Key]
        [ForeignKey(nameof(Picture))]
        public int ImageId { get; set; }
        public string Hash { get; set; }
        public byte[]? Image { get; set; }
    }

    public class NewPicture
    {
        public string FilePath { get; set; }
        public byte[] Bytes { get; set; }
        public NewPicture() { }
        public NewPicture(string path, byte[] bytes)
        {
            FilePath = path;
            Bytes = bytes;
        }
    }
}
