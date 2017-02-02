using System.Collections.Generic;
using System.Linq;

namespace AkkaBootCampThings
{
    public class DataRepo
    {
        public DataRepo()
        {
            foreach (var x in Enumerable.Range(1, 1000))
            {
                Result.Add(x.ToString(), new Client
                {
                    Name = "Otto Clay " + x,
                    Age = 61 + x,
                    Country = Country.Canada,
                    Address = "Ap #" + x + "97-1459 Quam Avenue",
                    Married = false,
                    ID = x.ToString()
                });
            }
        }

        public IDictionary<string, Client> Result = new Dictionary<string, Client>();
    }
}