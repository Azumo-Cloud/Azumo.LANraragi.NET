using Azumo.LANraragi.NET.Database;
using Azumo.LANraragi.NET.HostServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace Azumo.LANraragi.NET
{
    /// <summary>
    /// 
    /// </summary>
    public class Program
    {
        private const string DBName = "lanraragi.db";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

            // 添加Session
            _ = builder.Services.AddSession();
            // 添加MVC
            _ = builder.Services.AddControllersWithViews();
            // 添加内存缓存
            _ = builder.Services.AddMemoryCache();
            // 添加数据库Context 
            _ = builder.Services.AddDbContext<LANraragiContext>((serivce, options) =>
            {
                _ = options.UseSqlite($"Data Source={DBName}")
                    .UseMemoryCache(serivce.GetRequiredService<IMemoryCache>());
            });
            // 添加认证
            _ = builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login";
                    options.LogoutPath = "/Logout";
                });
            // 添加后台服务
            //builder.Services.AddHostedService<MangaFinderService>();

            WebApplication app = builder.Build();

            if (CheckError(out string error, ref app))
            {
                app.Logger.LogError("(╯・_>・）╯︵ ┻━┻");
                app.Logger.LogError(error);
                await app.DisposeAsync();
                return;
            }

            app.Logger.LogInformation("ｷﾀ━━━━━━(ﾟ∀ﾟ)━━━━━━!!!!!");

            _ = app.MapGet("/", () => "Hello World!");
            _ = app.MapControllers();
            _ = app.UseAuthorization();

            SharpCompress.Archives.IArchive archive = SharpCompress.Archives.ArchiveFactory.Open(new FileStream("D:\\Download\\Input\\(C78) [芦間山道 (芦間たくみ)] 愛紫あい (東方Project) [广告组汉化].zip", FileMode.Open));
            var aa = archive.Entries;

            archive.Dispose();
            app.Run();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static bool CheckError(out string errorInfo, ref WebApplication app)
        {
            errorInfo = string.Empty;
            try
            {
#if DEBUG
                if (File.Exists(DBName))
                    File.Delete(DBName);
#endif
                // 尝试链接数据库
                using (IServiceScope serviceScope = app.Services.CreateScope())
                {
                    LANraragiContext LANraragiContext = serviceScope.ServiceProvider.GetRequiredService<LANraragiContext>();
                    bool createResult = LANraragiContext.Database.EnsureCreated();
                    if (!createResult)
                        throw new Exception("数据库创建失败");
                }
                
                // 尝试链接Redis


                return false;
            }
            catch (Exception ex)
            {
                errorInfo = $"发生严重错误: {Environment.NewLine}";
                errorInfo += ex.Message;
                return true;
            }
        }
    }
}
