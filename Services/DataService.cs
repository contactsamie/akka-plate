using System.Collections.Generic;
using System.Linq;

namespace AkkaBootCampThings
{
    public class DataService : IDataService<object, object>
    {
        private static DataRepo _dataRepo;
        private readonly IUiNotificationService _notificationService;

        public DataService(IUiNotificationService notificationService)
        {
            _notificationService = notificationService;
            _dataRepo = _dataRepo ?? new DataRepo();
        }

        public object DoSomething(object message)
        {
            _notificationService.Notify("Update", message);
            return message;
        }

        public List<Client> GetAll(object message)
        {
            _notificationService.Notify("GetAll", message);
            return _dataRepo.Result.Select(x => x.Value).ToList();
        }

        public Client Get(object message, string id)
        {
            _notificationService.Notify("Get", message);
            return _dataRepo.Result[id];
        }

        public bool Post(object message, Client data)
        {
            _notificationService.Notify("Post", message);
            _dataRepo.Result.Add(data.ID, data);
            return true;
        }

        public bool Put(object message, Client data, string id)
        {
            _notificationService.Notify("Put", message);
            data.ID = id;
            _dataRepo.Result[id] = data;

            return true;
        }

        public bool Delete(object message, string id)
        {
            _notificationService.Notify("Delete", message);
            _dataRepo.Result.Remove(id);
            return true;
        }
    }
}