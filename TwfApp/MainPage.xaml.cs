using TwfApp.Types;

namespace TwfApp
{
    public partial class MainPage : ContentPage
    {
        private readonly IServiceProvider _serviceProvider;

        public MainPage(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await _serviceProvider.GetRequiredService<AppInitializer>().InitializeAsync();
        }
    }
}
