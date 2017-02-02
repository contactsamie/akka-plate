using SignalXLib.Lib;

namespace AkkaBootCampThings
{
    public class SignalXNotificationService : IUiNotificationService
    {
        public void Notify(string name, object message)
        {
            SignalX.RespondToAll(name, message);
        }
    }
}