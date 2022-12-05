using System;
using System.Collections.Generic;
//using System.Windows.Forms;
using System.Windows.Media.Imaging;

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

        private string section;
        public string Section
        {
            get { return this.section; }
            set { this.section = value; }
        }

        private string info;
        public string Info
        {
            get { return this.info; }
            set { this.info = value; }
        }

        private BitmapImage imageBmp;
        public BitmapImage ImageBmp
        {
            get { return this.imageBmp; }
            set { this.imageBmp = value; }
        }
        public ImageInfo(string fp, BitmapImage bmi, string info, string sect)
        {
            if (info == "") this.Info = "Waiting for analysis";
            else this.Info = info;
            this.FilePath = fp;
            this.ImageBmp = bmi;
            this.Id = IdCount;
            IdCount++;
            this.Section = sect;
        }

        public ImageInfo(ImageInfo obj)
        {
            this.Info = obj.Info;
            this.FilePath = obj.FilePath;
            this.ImageBmp = obj.ImageBmp;
            this.Id = obj.Id;
            this.Section = obj.Section;
        }

        public string AssignInfo(List<Tuple<string, float>> emotions_list)
        {
            string _info = "";
            foreach (var emotion in emotions_list) _info += $"{emotion.Item1} : {emotion.Item2}\n";
            this.Info = _info;
            return _info;
        }
    }
}
