using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Input;

namespace WpfApp
{
    class Picture
    {
        [Key]
        public int ImageId { get; set; }
        public string Path { get; set; }
        public string Section { get; set; }
        public string Info { get; set; }
        //public byte[] Image { get; set; }
    }

    class PictureHash
    {
        [Key]
        [ForeignKey(nameof(Picture))]
        public int ImageId { get; set; }
        public string Hash { get; set; }
        public byte[]? Image { get; set; }
    }
}
