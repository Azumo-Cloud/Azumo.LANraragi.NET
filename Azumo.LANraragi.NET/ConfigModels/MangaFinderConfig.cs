namespace Azumo.LANraragi.NET.ConfigModels
{
    public class MangaFinderConfig
    {
        /// <summary>
        /// 处理时使用的核心数
        /// </summary>
        public int UsedCores { get; set; }

        /// <summary>
        /// 指示是否使用全部的核心
        /// </summary>
        public bool UseAllCores { get; set; }

        /// <summary>
        /// 漫画目录
        /// </summary>
        public string MangaPath { get; set; } = string.Empty;
    }
}
