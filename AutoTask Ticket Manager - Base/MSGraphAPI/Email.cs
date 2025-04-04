namespace AutoTaskTicketManager_Base.MSGraphAPI
{
    public static class Email
    {
        static string Id = ""; //MS Graph message ID for this e-mail
        static string LastModifiedDateTime = "";
        static string ChangeKey = "";
        static string Categories = "";
        static string ReceivedDateTime = "";
        static string SendDateTime = "";
        static string HasAttachments = "";
        static string InternetMessageId = "";
        static string Subject = "";
        static string BodyPreview = "";
        static string Importance = "";
        static string ParentFolderId = "";
        static string ConversationId = "";
        static string ConversationIndex = "";
        static string IsDeliveryRecieptRequested = "";
        static string IsRead = "";
        static string IsDraft = "";
        static string WebLink = "";
        static string InferenceClassification = "";
        static string Body = "";
        static string Sender = "";
        static bool SenderAssignment = false;
        static string From = "";
        static string ToRecipients = "";
        static string CcRecipients = "";
        static string BccRecipients = "";
        static string TokenId = "";
        static string AtTicketId = "";
        static string AtTicketNo = "";
        static int AtCompanyId = 0;  //The AT Company ID associated with the Support Email. Used to compare with the 'companyID' returned when we get a ticket to prevent accidential updating od the wrong Ticket.
        static int UnreadCount = 0;  //Used as a flag to detect when all messages have been processed and to allow a delay before next check
        static string DraftMsgId = "";  //This is the messgae ID we created with the Draft ReplyAll in response to the inbound Support email
        static string DraftMsgContent = ""; //This is the content from the body of the Draft Reply All message. It is used when we update a draft message so we maintain thread history.
        static string SupportDistro = ""; //This is the support distro found when parsing the e-mail addresses. It is used in determining what Logo to use in the reply e-mails when creating new tickets in AT.

        static Email()
        {
            Id = "";
            LastModifiedDateTime = "";
            ChangeKey = "";
            Categories = "";
            ReceivedDateTime = "";
            SendDateTime = "";
            HasAttachments = "";
            InternetMessageId = "";
            Subject = "";
            BodyPreview = "";
            Importance = "";
            ParentFolderId = "";
            ConversationId = "";
            ConversationIndex = "";
            IsDeliveryRecieptRequested = "";
            IsRead = "";
            IsDraft = "";
            WebLink = "";
            InferenceClassification = "";
            Body = "";
            Sender = "";
            SenderAssignment = false;
            From = "";
            ToRecipients = "";
            CcRecipients = "";
            BccRecipients = "";
            TokenId = "";
            AtTicketId = "";
            AtTicketNo = "";
            AtCompanyId = 0;
            UnreadCount = 0;
            DraftMsgId = "";
            DraftMsgContent = "";
            SupportDistro = "";

        }

    }
}
