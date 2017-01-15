using System.Collections.Generic;
using System.Linq;
using SignalXLib.Lib;

namespace AkkaBootCampThings
{
    public class DataService : IDataService<object, object>
    {
        private static DataRepo _dataRepo;

        public DataService()
        {
            _dataRepo = _dataRepo ?? new DataRepo();
        }

        public object DoSomething(object message)
        {
            SignalX.RespondToAll("Update", message);
            return message;
        }

        public List<DataRepo.Client> GetAll(object message)
        {
            SignalX.RespondToAll("GetAll", message);
            return _dataRepo.Result.Select(x => x.Value).ToList();
        }

        public DataRepo.Client Get(object message, string id)
        {
            SignalX.RespondToAll("Get", message);
            return _dataRepo.Result[id];
        }

        public bool Post(object message, DataRepo.Client data)
        {
            SignalX.RespondToAll("Post", message);
            _dataRepo.Result.Add(data.ID, data);
            return true;
        }

        public bool Put(object message, DataRepo.Client data, string id)
        {
            SignalX.RespondToAll("Put", message);
            data.ID = id;
            _dataRepo.Result[id] = data;

            return true;
        }

        public bool Delete(object message, string id)
        {
            SignalX.RespondToAll("Delete", message);
            _dataRepo.Result.Remove(id);
            return true;
        }
    }
}