using Alpaca.Injects;

namespace MyTest
{
    [Configuration]
    public class AlpacaConfig
    {
        [Inject]
        IRepository<string> Blah;

        [PostConstruct]
        public void blah()
        {

        }
    }

    public interface IRepository<T>
    {
        
    }

    public class HendersRepository: IRepository<int>
    {
        
    }
}