using System.Collections.Specialized;

namespace AutoTaskTicketManager_Base.AutoTaskAPI
{
    public static class Companies
    {

        //Dictionary holding all Companies
        public static ObservableDictionary<Int64, object[]> companies = new ObservableDictionary<Int64, object[]>();


        //This is the AutoTask Company ID. it is set to a -1 as the first company setup in AT has an ID = 0
        static string id = string.Empty;
        static string companyName = string.Empty;
        static string isActive = string.Empty; //value to indicate if the company is active with FF


        static Companies()
        {
            companies = new ObservableDictionary<Int64, object[]>();
            id = "";
            companyName = "";
            isActive = "";

            companies.CollectionChanged += Companies_CollectionChanged;
        }

        private static void Companies_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            Console.WriteLine($"Companies count changed. New count: {Companies.GetCompanyCountFromMemory()}");
        }


        public static void SetCompanies(Int64 Tkey, object[] Tvalue)
        {
            companies.Add(Tkey, Tvalue);
        }

        public static Array GetCompanies(Int64 Tkey)
        {
            try
            {
                Array value = companies[Tkey];
                return value;
            }
            catch
            {
                string[] errMsg = { $"There was an error. Please verify {Tkey} exists" };
                return errMsg;
            }

        }


        public static List<KeyValuePair<Int64, object[]>> GetCompaniesListforManagementAPI()
        {
            return companies.ToList();
        }


        public static int GetCompanyCountFromMemory()
        {
            var cCount = companies.Count;
            return cCount;
        }

        public static void ClearCompaniesDictionary()
        {
            companies.Clear();
        }


    }
}
