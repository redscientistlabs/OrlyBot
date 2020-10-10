using System.Threading.Tasks;

namespace OrlyBot
{
    class Bot
    {
        public static Task Main(string[] args)
            => Startup.RunAsync(args);
    }
}
