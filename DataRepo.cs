using System.Collections.Generic;
using System.Linq;

namespace AsyncTaskPatternsPerformanceComparisonInWebApi
{
    public class DataRepo
    {
        public enum Country
        {
            UnitedStates = 1,
            Canada = 2,
            UnitedKingdom = 3,
            France = 4,
            Brazil = 5,
            China = 6,
            Russia = 7
        }

        public class Client
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
            public Country? Country { get; set; }
            public string Address { get; set; }
            public bool Married { get; set; }
        }

        public DataRepo()
        {
            Result = Enumerable.Range(1, 1000).Select(x => new Client
            {
                Name = "Otto Clay " + x,
                Age = 61 + x,
                Country = Country.Canada,
                Address = "Ap #" + x + "97-1459 Quam Avenue",
                Married = false,
                ID = x
            }).ToList();
        }

        public List<Client> Result;
    }
}