using System.Collections.Generic;
using System.Linq;

namespace GreetingsBot.QueryFactory
{
    public interface IQueryFactory
    {
        List<T> GetData<T>()
            where T : class;

        T GetRandomEl<T>()
            where T : class;

        Greeting GetRandomGreetingEl(string holiday = null, string recipient = null);

        void DisposeContext();
    }
}
