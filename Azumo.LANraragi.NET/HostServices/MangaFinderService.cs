
using Azumo.LANraragi.NET.ConfigModels;
using Azumo.LANraragi.NET.Database;
using System.IO.Compression;
using System.IO.MemoryMappedFiles;
using System.Security.Cryptography;
using System.Threading.Tasks.Dataflow;

namespace Azumo.LANraragi.NET.HostServices
{
    /// <summary>
    /// 漫画后台服务
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="logger"></param>
    public class MangaFinderService : IHostedService, IAsyncDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<MangaFinderService> _logger;
        private readonly MangaFinderConfig _mangaFinderConfig;
        private readonly LANraragiContext _LANraragiContext;

        private readonly FileSystemWatcher _fileSystemWatcher;

        private delegate void MangaPathEventHandler(RenamedEventArgs renamedEventArgs);
        private readonly ActionBlock<RenamedEventArgs> _mangaPathEventHandler;
        private readonly ActionBlock<Archive> _MangaInfoUpdate;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="logger"></param>
        /// <param name="configuration"></param>
        public MangaFinderService(IServiceProvider serviceProvider, ILogger<MangaFinderService> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _LANraragiContext = _serviceProvider.GetRequiredService<LANraragiContext>();

            MangaFinderConfig? mangaFinderConfig = configuration.Get<MangaFinderConfig>();
            this._mangaFinderConfig = mangaFinderConfig ?? new MangaFinderConfig
            {
                UseAllCores = false,
                UsedCores = Environment.ProcessorCount / 2
            };

            _fileSystemWatcher = new FileSystemWatcher();
            _MangaInfoUpdate = new ActionBlock<Archive>((archive) =>
            {
                if (string.IsNullOrEmpty(archive.Path))
                {
                    if (File.Exists(archive.Path)) File.Delete(archive.Path);
                    _LANraragiContext.Remove(archive);
                    _LANraragiContext.SaveChanges();
                    return;
                }
                SHA1 sha1 = SHA1.Create();
                using (FileStream fileStream = new(archive.Path ?? string.Empty, FileMode.Open, FileAccess.Read))
                {
                    // 计算文件的SHA1值
                    string sha1Str = Convert.ToBase64String(sha1.ComputeHash(fileStream));
                    if (!string.IsNullOrEmpty(archive.SHA1) && sha1Str.Equals(archive.SHA1))
                        return;
                    // 文件内容改变，开始更新数据库
                    archive.SHA1 = sha1Str;
                    archive.Thumbnails = GenerateMangaThumbnails(fileStream);

                    // 更新数据库
                    _LANraragiContext.Update(archive);
                    _LANraragiContext.SaveChanges();
                }
                sha1.Dispose();
            });
            _mangaPathEventHandler = new ActionBlock<RenamedEventArgs>((args) =>
            {
                Archive? archive = _LANraragiContext.Archives.Where(x => x.Path == args.OldFullPath).FirstOrDefault();
                switch (args.ChangeType)
                {
                    case WatcherChangeTypes.Created:
                        if (archive == null)
                            archive = new Archive
                            {

                            };
                        _LANraragiContext.Archives.Add(archive);
                        _LANraragiContext.SaveChanges();
                        _MangaInfoUpdate.Post(archive);
                        break;
                    case WatcherChangeTypes.Deleted:

                        break;
                    case WatcherChangeTypes.Changed:
                        break;
                    case WatcherChangeTypes.Renamed:
                        if (archive == null)
                            return;
                        archive.Path = args.FullPath;
                        _LANraragiContext.SaveChanges();
                        break;
                    default:
                        break;
                }
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (!Directory.Exists(_mangaFinderConfig.MangaPath))
                return;

            MangaFinder();

            _fileSystemWatcher.Path = _mangaFinderConfig.MangaPath;
            _fileSystemWatcher.Created += this.MangaPathCreate;
            _fileSystemWatcher.Deleted += this.MangaPathDeleted;
            _fileSystemWatcher.Renamed += this.MangaPathRenamed;

            _fileSystemWatcher.EnableRaisingEvents = true;

            await Task.CompletedTask;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MangaPathRenamed(object sender, RenamedEventArgs e)
        {
            _mangaPathEventHandler.Post(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void MangaPathDeleted(object sender, FileSystemEventArgs e)
        {
            _mangaPathEventHandler.Post(new RenamedEventArgs(WatcherChangeTypes.Deleted, Path.GetDirectoryName(e.FullPath) ?? string.Empty, string.Empty, e.Name));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void MangaPathCreate(object sender, FileSystemEventArgs e)
        {
            _mangaPathEventHandler.Post(new RenamedEventArgs(WatcherChangeTypes.Created, Path.GetDirectoryName(e.FullPath) ?? string.Empty, e.Name, string.Empty));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await DisposeAsync();
        }

        /// <summary>
        /// 搜索指定目录下的漫画
        /// </summary>
        /// <returns></returns>
        private void MangaFinder()
        {

        }

        /// <summary>
        /// 生成漫画缩略图
        /// </summary>
        /// <returns></returns>
        private string GenerateMangaThumbnails(Stream file)
        {
            ZipArchive zipArchive = new ZipArchive(file, ZipArchiveMode.Read);
            return string.Empty;
        }

        private string ConvertToZip(Stream file)
        {
            SharpCompress.Archives.IArchive archive = SharpCompress.Archives.ArchiveFactory.Open(file);
            
            return string.Empty;
        }
    }
}
