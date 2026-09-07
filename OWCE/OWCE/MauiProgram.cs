using CommunityToolkit.Maui;
using Mopups.Hosting;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace OWCE
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            RegisterPlatformServices();

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureMopups()
                .UseSkiaSharp()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("SairaExtraCondensed-Black.ttf", "SairaExtraCondensed-Black");
                    fonts.AddFont("SairaExtraCondensed-Bold.ttf", "SairaExtraCondensed-Bold");
                    fonts.AddFont("SairaExtraCondensed-ExtraBold.ttf", "SairaExtraCondensed-ExtraBold");
                    fonts.AddFont("SairaExtraCondensed-ExtraLight.ttf", "SairaExtraCondensed-ExtraLight");
                    fonts.AddFont("SairaExtraCondensed-Light.ttf", "SairaExtraCondensed-Light");
                    fonts.AddFont("SairaExtraCondensed-Medium.ttf", "SairaExtraCondensed-Medium");
                    fonts.AddFont("SairaExtraCondensed-Regular.ttf", "SairaExtraCondensed-Regular");
                    fonts.AddFont("SairaExtraCondensed-SemiBold.ttf", "SairaExtraCondensed-SemiBold");
                    fonts.AddFont("SairaExtraCondensed-Thin.ttf", "SairaExtraCondensed-Thin");
                });

            return builder.Build();
        }

        static void RegisterPlatformServices()
        {
#if IOS
            DependencyService.Register<DependencyInterfaces.IOWBLE, OWCE.iOS.DependencyImplementations.OWBLE>();
            DependencyService.Register<DependencyInterfaces.IPermissionPrompt, OWCE.iOS.DependencyImplementations.PermissionPrompt>();
            DependencyService.Register<DependencyInterfaces.IUserAgent, OWCE.iOS.DependencyImplementations.UserAgent>();
            DependencyService.Register<DependencyInterfaces.IWatch, OWCE.iOS.DependencyImplementations.Watch>();
#elif ANDROID
            DependencyService.Register<DependencyInterfaces.IOWBLE, OWCE.Droid.DependencyImplementations.OWBLE>();
            DependencyService.Register<DependencyInterfaces.IPermissionPrompt, OWCE.Droid.DependencyImplementations.PermissionPrompt>();
            DependencyService.Register<DependencyInterfaces.IUserAgent, OWCE.Droid.DependencyImplementations.UserAgent>();
            DependencyService.Register<DependencyInterfaces.IWatch, OWCE.Droid.DependencyImplementations.Watch>();
#endif
        }
    }
}
