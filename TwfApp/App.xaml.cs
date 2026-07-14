using TwfApp.Types;

namespace TwfApp
{
    public partial class App : Application
    {
        private readonly IServiceProvider _serviceProvider;

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            this._serviceProvider = serviceProvider;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new MainPage(_serviceProvider)) { Title = "TwfApp" };
        }
    }
}
