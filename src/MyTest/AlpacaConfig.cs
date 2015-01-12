using Alpaca.Weld;
using Alpaca.Weld.Attributes;

namespace MyTest
{
    [Configuration]
    public class AlpacaConfig
    {
        [Inject]
        IRepository<int> Blah;

        [PostConstruct]
        public void blah()
        {

        }
    }

    public interface IRepository<T>
    {
        
    }

    public class Repository<T>: IRepository<T>
    {
        
    }
}