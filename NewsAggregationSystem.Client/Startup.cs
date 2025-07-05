using NewsAggregationSystem.Client.Handlers;

namespace NewsAggregationSystem.Client
{
    public class Startup
    {
        private readonly IMainMenuHandler mainMenuHandler;
        public Startup()
        {
            this.mainMenuHandler = new MainMenuHandler();
        }
        public static async Task Main(string[] args)
        {
            var startup = new Startup();
            await startup.mainMenuHandler.ShowWelcomeMenu();
        }
    }
}
