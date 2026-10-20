using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mopups.Pages;
using Mopups.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace OWCE.Pages.Popup
{
    public partial class SideMenuPopup : PopupPage
    {
        static SideMenuPopup _instance;
        public static SideMenuPopup Instance => _instance ??= new SideMenuPopup();

        IAsyncRelayCommand _closeCommand;
        public IAsyncRelayCommand CloseCommand => _closeCommand ??= new AsyncRelayCommand(CloseCommand_Clicked);

        IAsyncRelayCommand<Grid> _aboutCommand;
        public IAsyncRelayCommand<Grid> AboutCommand => _aboutCommand ??= new AsyncRelayCommand<Grid>(async (sender) => await AboutCommand_Clicked(sender));

        IAsyncRelayCommand<Grid> _settingsCommand;
        public IAsyncRelayCommand<Grid> SettingsCommand => _settingsCommand ??= new AsyncRelayCommand<Grid>(async (sender) => await SettingsCommand_Clicked(sender));

        View _pageSpecificSideMenu = null;
        public View PageSpecificSideMenu {
            get
            {
                return _pageSpecificSideMenu;
            }
            set
            {
                if (_pageSpecificSideMenu != null)
                {
                    MainGrid.Children.Remove(_pageSpecificSideMenu);
                    _pageSpecificSideMenu = null;
                }

                if (value != null)
                {
                    _pageSpecificSideMenu = value;
                    Grid.SetRow(_pageSpecificSideMenu, 2);
                    MainGrid.Children.Add(_pageSpecificSideMenu);
                }
            }
        }

        private SideMenuPopup()
        {
            InitializeComponent();

            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                HasSystemPadding = false;
            }

            BindingContext = this;
        }

        internal async Task CloseCommand_Clicked()
        {
            await MopupService.Instance.PopAsync(true);
        }

        async Task AboutCommand_Clicked(Grid sender)
        {
            sender.Opacity = 0.6f;

            await Task.WhenAll(
                sender.FadeTo(1f),
                App.Current.MainPage.Navigation.PushModalAsync(new CustomNavigationPage(new AboutPage())),
                MopupService.Instance.RemovePageAsync(this)
            );
        }

        async Task SettingsCommand_Clicked(Grid sender)
        {
            sender.Opacity = 0.6f;

            await Task.WhenAll(
                sender.FadeTo(1f),
                App.Current.MainPage.Navigation.PushModalAsync(new CustomNavigationPage(new AppSettingsPage())),
                MopupService.Instance.RemovePageAsync(this)
            );
        }


        

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Content is Grid grid)
            {
                if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    var safeInsets = GetSafeAreaInsets();
                    grid.Padding = new Thickness(0, safeInsets.Top, 0, safeInsets.Bottom);
                }
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            // Leave a strip on the right so the page behind (and the close button) stays reachable.
            if (width > 0)
            {
                MainGrid.WidthRequest = Math.Min(500, width - MainGrid.Margin.Right);
            }
        }

        Thickness GetSafeAreaInsets()
        {
            var insets = On<iOS>().SafeAreaInsets();
#if IOS
            // MAUI only populates the page-specific insets when UseSafeArea is on, so fall back to the window.
            if (insets.Top == 0)
            {
                var window = UIKit.UIApplication.SharedApplication.ConnectedScenes
                    .OfType<UIKit.UIWindowScene>()
                    .SelectMany(scene => scene.Windows)
                    .FirstOrDefault(w => w.IsKeyWindow);
                if (window != null)
                {
                    insets = new Thickness(0, window.SafeAreaInsets.Top, 0, window.SafeAreaInsets.Bottom);
                }
            }
#endif
            return insets;
        }
    }
}
