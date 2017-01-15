using System.Collections.Generic;

namespace AkkaBootCampThings
{
    public interface IDataService<T, TR>
    {
        TR DoSomething(T message);
        List<DataRepo.Client> GetAll(object message);
        DataRepo.Client Get(object message, string id);
        bool Post(object message, DataRepo.Client data);
        bool Put(object message, DataRepo.Client data, string id);
        bool Delete(object message, string id);
    }
}