using Avalonia;
using Espuma.Presentation;

namespace Espuma.Desktop
{
    internal static class Program
    {
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp(args).StartWithClassicDesktopLifetime(args);

        /// <summary>Software rendering is the default because it uses less memory; --gpu enables OpenGL.</summary>
        public static AppBuilder BuildAvaloniaApp(string[] args)
        {
            var useGpu = args.Contains("--gpu");

            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .With(new X11PlatformOptions
                {
                    RenderingMode = useGpu ? [X11RenderingMode.Glx, X11RenderingMode.Software] : [X11RenderingMode.Software]
                })
                .With(new Win32PlatformOptions
                {
                    RenderingMode = useGpu ? [Win32RenderingMode.AngleEgl, Win32RenderingMode.Software] : [Win32RenderingMode.Software]
                })
                .LogToTrace();
        }
    }
}
