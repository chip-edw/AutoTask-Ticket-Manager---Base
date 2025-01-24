using AutoTaskTicketManager_Base.AutoTaskAPI;
using Serilog;
using System.Text.Json.Nodes;

namespace AutoTaskTicketManager_Base.MSGraphAPI
{
    internal class ProcessEmail
    {/// <summary>
     /// Iterates through the TO: and CC: recipients of the email message. Returns AT Company ID if found else returns -1
     /// Please Note: Only Support Distros that have the EnableEmail bit set to '1' are loaded into memory.
     /// Therefore a match to a Support Distro is by default an avtive distro.
     /// </summary>
     /// <returns></returns>

        public static Int64 CheckSupportEmail()

        {
            string subject = null;
            string body = null;
            string ticketNumber = null;
            string addressCc = null;



            string rawSubject = EmailManager.GetField("Subject");
            string rawSender = EmailManager.GetField("From");


            //Check for empty subject. If empty return a -1 indicating do not process.
            if (String.IsNullOrEmpty(rawSubject) is true | rawSubject.Length < 4)
            {
                Log.Debug("...");
                Log.Warning("Detected empty Subject in inbound e-mail. Ignoring e-mail");
                return -1;
            }
            else
            {
                Log.Debug("...");
                Log.Debug($"Inbound E-mail Subject:  {rawSubject}");
            }

            //Cleanup the Subject for furthur processing.
            subject = EmailManager.SubjectCleanup(rawSubject);

            //Check for Subject Exclusion Key Words and if excluded KeyWord found return -1 indicating do not process.
            var isExclusion = EmailManager.SubjectExclusionKeyWords(EmailManager.GetField("Subject"));

            if (isExclusion.Result == true)
            {
                Log.Debug("Detected Subject Exclusion Keyword. Ignoring e-mail");
                return -1;
            }

            //Cleanup From e-mail address for futhur processing
            var senderAddress = EmailManager.SenderCleanup(rawSender, 0);



            //Check for Sender Exclusion From Email Addressess and if excluded email address found return -1 indicating do not process.
            var isSenderExclusion = EmailManager.SenderExclusionEmailAddresses(senderAddress);

            if (isSenderExclusion.Result == true)
            {
                Log.Debug("Detected Sender E-mail Exclusion. Ignoring e-mail");
                return -1;
            }

            //Check if Sender is an internal Sender Assignment Resource. If True and the Resource is marked active then update
            //the Email class SenderAssignment bool as True. It will be used later in the TicketHandler to determine if the ticket should be assigned to the sender.
            bool senderAssignmentExists = StartupConfiguration.autoAssignSenders.Values
                .Any(objects =>
                {
                    // Extract resourceEmail and resourceActive from the object array
                    var resourceEmail = objects.OfType<string>().ElementAtOrDefault(1); // Email is the second element
                    var resourceActive = objects.OfType<bool>().FirstOrDefault();      // Active is the last element

                    // Check if the resourceEmail matches and resourceActive is true
                    return resourceEmail?.Equals(senderAddress, StringComparison.OrdinalIgnoreCase) == true
                           && resourceActive == true;
                });



            if (senderAssignmentExists == true)
            {
                //Get the SenderAssignment status

                Log.Debug($"Sender Assign = {senderAssignmentExists} will attempt assigning ticket to sender \n ");

                EmailManager.SetBool("SenderAssignment", senderAssignmentExists);
            }
            else
            {
                //if senderAssignmentExists is not true let's go ahead and update the Email Static class bool to false 
                EmailManager.SetBool("SenderAssignment", senderAssignmentExists);
            }


            //Check for Ticket Number "Completed" Status in Subject

            //Get ticketNumber if it exists. A result of "No Match" means there was no ticket number
            ticketNumber = EmailManager.ReturnTicketNumberFromEmailSubject(rawSubject);

            if (ticketNumber != null && ticketNumber != "No Match")
            {
                Log.Debug($"Found Ticket number in Subject:  {ticketNumber}");
                //Check for Ticket Number "Complete" Status in Subject
                //##
                // The second parameter being passed is to bypass the need to compare the AT CompanyID against the support distro
                // which is not needed in this case and would not be successful since I dont have the company ID in this case
                //My easy way of repurposing an existing Method that had wider scope without needing to change much.

                string ticketStatus = TicketHandler.GetTicketByNumberUpdateTicketClass(ticketNumber, true);

                if (ticketStatus == "Complete")
                {
                    Log.Debug("Detected ticket exists and is in Complete Status. Ignoring e-mail");
                    return -1;
                }

            }


            //Get the Recipients in the TO:
            string toRecipients = EmailManager.GetField("ToRecipients");

            Log.Debug($"\nThe TO: recipients are:  {toRecipients}");

            JsonNode nodes = JsonArray.Parse(toRecipients);

            foreach (JsonObject aNode in nodes.AsArray())
            {
                foreach (var property in aNode.ToArray())
                {
                    string fieldValue = property.Value.ToString();

                    var emailDict = JsonValue.Parse(fieldValue);

                    // Check the TO: for a Support Distro. If we find a Support Distro dont bother checking the CC:
                    if (emailDict != null)
                    {
                        //parse e-mail address for a support distro


                        string address = (string)emailDict["address"];

                        if (EmailManager.GetDistros(address) >= 0)
                        {
                            //Found Match.  Need to add something to check if the distro is enabled...........
                            //################################################################################
                            Log.Debug("...");
                            Log.Debug($"Process Email Found match of TO: {address} in supportDistro Dictionary\n");

                            //Write support distro to Email class for later reference when looking up logo
                            EmailManager.SetField("SupportDistro", address);

                            //Get AT Id
                            Int64 ATID = EmailManager.GetDistros(address);


                            // Check for Duplicate Ticket before creating
                            //  Compare title to the dictionary 'companiesTicketsNotCompleted' and make sure title not already exists
                            bool cleanSubjctExists = StartupConfiguration.companiesTicketsNotCompleted.ContainsKey(subject);
                            bool rawSubjectExists = StartupConfiguration.companiesTicketsNotCompleted.ContainsKey(rawSubject);

                            if (cleanSubjctExists || rawSubjectExists)
                            {
                                //if cleaned email rawSubject or cleanSubjects exists as an open ticket title then set ATID to -1 so e-mail does not get processed
                                ATID = -1;
                                string existingTicketNumber = StartupConfiguration.companiesTicketsNotCompleted.GetValueOrDefault(subject);
                                Log.Information($"Ticket Generation skipped. Email with Subject: '{rawSubject}' already has a ticket:" +
                                    $" {existingTicketNumber}");


                            }

                            // if the ticket title of an open ticket does not exist then ATID stays the same and processes as normal

                            //We also need to update the 'StartupConfiguration.companiesTicketsNotCompleted' with the new subject soon as a new ticket is created
                            //that way we catch duplicates right away if a customer or employee quickly replies the first e-mail without the new ticket number.


                            return ATID;
                        }

                    }

                }

            }
            Log.Debug("...");
            Log.Debug("Process E-mail No TO: address match found in supportDistro Dictionary\n");

            // If did not find a "Support" Distro in the TO: then Check the CC:
            //Get the Recipients in the CC:
            string CcRecipients = EmailManager.GetField("CcRecipients");

            Log.Debug("Moving on to check the CC: addresses for a match in the supportDistro Dictionary");

            Log.Debug($"The CC: recipients are:  {CcRecipients}");

            JsonNode nodesCc = JsonArray.Parse(CcRecipients)!;

            foreach (JsonObject aNodeCc in nodesCc.AsArray())
            {
                foreach (var propertyCc in aNodeCc!.ToArray())
                {
                    string fieldValueCc = propertyCc.Value!.ToString();

                    var emailDictCc = JsonValue.Parse(fieldValueCc);

                    // Check the CC: for a Support Distro. If we do not find a Support Distro return -1
                    if (emailDictCc != null)
                    {
                        //parse e-mail address for a support distro
                        addressCc = (string)emailDictCc["address"]!;

                        Int64 foundAddress = EmailManager.GetDistros(addressCc);
                        if (foundAddress >= 0)
                        {

                            //Found Match now do something
                            Log.Verbose($" \nProcess Email Found match of CC: {addressCc} in supportDistro Dictionary \n");

                            //Write support distro to Email class for later reference when looking up logo                            
                            EmailManager.SetField("SupportDistro", addressCc);

                            //Get AT Id
                            Int64 ATID = EmailManager.GetDistros(addressCc);

                            // Check for Duplicate Ticket before creating
                            //  Compare title to the dictionary 'companiesTicketsNotCompleted' and make sure title not already exists
                            bool cleanSubjctExists = StartupConfiguration.companiesTicketsNotCompleted.ContainsKey(subject);
                            bool rawSubjectExists = StartupConfiguration.companiesTicketsNotCompleted.ContainsKey(rawSubject);

                            if (cleanSubjctExists || rawSubjectExists)
                            {
                                //if cleaned email rawSubject or cleanSubjects exists as an open ticket title then set ATID to -1 so e-mail does not get processed
                                ATID = -1;
                                string existingTicketNumber = StartupConfiguration.companiesTicketsNotCompleted.GetValueOrDefault(subject);
                                Log.Information($"Ticket Generation skipped. Email with Subject: '{rawSubject}' already has a ticket:" +
                                    $" {existingTicketNumber}");

                                // if the ticket title of an open ticket does not exist then ATID stays the same and processes as normal

                                //We also need to update the 'AppConfig.companiesTicketsNotCompleted' with the new subject soon as a new ticket is created
                                //that way we catch duplicates right away if a customer or employee quickly replies the first e-mail without the new ticket number.
                            }

                            return ATID;
                        }


                    }


                }


            }
            Log.Debug($"\n Process E-mail No CC: match for {addressCc} in supportDistro Dictionary \n");
            return -1;
        }

    }
}
