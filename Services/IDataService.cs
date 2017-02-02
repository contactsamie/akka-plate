using System.Collections.Generic;

namespace AkkaBootCampThings
{
    public interface IDataService<T, TR>
    {
        TR DoSomething(T message);

        List<Client> GetAll(object message);

        Client Get(object message, string id);

        bool Post(object message, Client data);

        bool Put(object message, Client data, string id);

        bool Delete(object message, string id);
    }
}