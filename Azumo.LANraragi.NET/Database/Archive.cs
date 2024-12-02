using System.ComponentModel.DataAnnotations;

namespace Azumo.LANraragi.NET.Database
{
    public class Archive
    {
        [Key]
        public Guid Key { get; set; }

        /// <summary>
        /// 漫画标题
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// 漫画路径
        /// </summary>
        public string? Path { get; set; }

        /// <summary>
        /// 漫画SHA1值
        /// </summary>
        public string? SHA1 { get; set; }

        /// <summary>
        /// 漫画封面缩略图
        /// </summary>
        public string? Thumbnails { get; set; }
    }
}