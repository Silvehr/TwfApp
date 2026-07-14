using TwfApp.Services;
using TwfApp.Types;

namespace TwfApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            
            var initializer = serviceProvider.GetRequiredService<AppInitializer>();
            if (!initializer.IsInitialized)
            {
                blazorWebView.StartPath = "/loading";
            }
        }

        protected override void OnAppearing()
        {
        }
    }
}
