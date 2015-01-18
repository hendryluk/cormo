using Alpaca.Injects;
using Alpaca.Web.WebApi;

namespace MyTest
{
    [Configuration]
    [EnableWebApi]
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

    public class HendersRepository: IRepository<string>
    {
        
    }
}