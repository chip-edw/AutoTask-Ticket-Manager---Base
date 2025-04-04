using Newtonsoft.Json;
using Serilog;

namespace AutoTaskTicketManager_Base.AutoTaskAPI
{
    public static class Ticket
    {
        //Dictionary holding all Ticket Entity Picklists
        public static Dictionary<string, Array> pickLists = new Dictionary<string, Array>();


        //This is the AutoTask Company ID. it is set to a -1 as the first company setup in AT has an ID = 0
        static string autoTaskId = null;
        static string apiVendorID = null;
        static string assignedResourceID = null;
        static string assignedResourceRoleID = null;
        static string billingCodeID = null;


        //The Customer Company name associated with the AT Company ID
        static string company = "";

        //The Ticket Category if we do not want to accept the Default. Will be used for the Managed Services Tickets
        static string ticketCategory = "";
        static string templateSpeedCode = "";


        // The Ticket Type if we do not want to accept the default.
        static string ticketType = "";

        static string title = "";
        static string description = "";
        static string status = "";
        static string priority = "";

        //Ticket Information
        static string ticketNumber = "";
        static string issueType = "";
        static string subIssueType = "";
        static string source = "";
        static string estimatedHours = "";

        //Assignment
        static string queue = "";
        static string primaryResource = "";
        static string secondaryResource = "";

        //Billing
        static string contract = "";
        static string workType = "";


        static Ticket()
        {
            pickLists = new Dictionary<string, Array>();
            autoTaskId = "";
            apiVendorID = "";
            assignedResourceID = "";
            assignedResourceRoleID = "";
            billingCodeID = "";
            company = "";
            ticketCategory = "";
            templateSpeedCode = "";
            ticketType = "";
            title = "";
            description = "";
            status = "";
            priority = "";
            ticketNumber = "";
            issueType = "";
            subIssueType = "";
            source = "";
            estimatedHours = "";
            queue = "";
            primaryResource = "";
            secondaryResource = "";
            contract = "";
            workType = "";

        }

        public static void SetPickLists(string Tkey, Array Tvalue)
        {
            pickLists.Add(Tkey, Tvalue);
        }

        public static Array GetPickLists(string Tkey)
        {
            try
            {
                Array value = pickLists[Tkey];
                return value;
            }
            catch
            {
                string[] errMsg = { $"There was an error. Please verify {Tkey} exists" };
                return errMsg;
            }

        }


        public static string GetTicketPicklistLableValue(string name, string label)
        {
            string value = "";
            var arrayOfLists = GetPickLists(name);

            // Serialize the array to a JSON string
            string json = JsonConvert.SerializeObject(arrayOfLists);

            // Deserialize the JSON string into a List<Dictionary<string, object>>
            List<Dictionary<string, object>> listOfDictionaries =
                JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);

            int foundIndex = -1; // Initialize to -1 to indicate not found

            for (int i = 0; i < listOfDictionaries.Count; i++)
            {
                Dictionary<string, object> kvpDictionary = listOfDictionaries[i];
                if (kvpDictionary.ContainsValue(label))
                {
                    foundIndex = i;
                    value = kvpDictionary["value"].ToString();
                    break;
                }
            }

            if (foundIndex != -1)
            {
                Log.Verbose($"Found the value '{label}' in the Ticket picklist dictionary at index {foundIndex}");
                return value;
            }
            else
            {
                Log.Verbose($"Value '{label}' not found in the Ticket picklist dictionary... Ticket.GetPicklistLableValue");
            }
            return value;

        }

    }
}
