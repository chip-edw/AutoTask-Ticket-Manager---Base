# Email Processing Overview - ATTMS

## Overview
This document explains how email processing works inside the ATTMS Worker Service, specifically focusing on how support emails are parsed, how ticket creation is triggered, and which configuration settings and in-memory dictionaries are involved. It also documents how the `EnableEmail` flag is enforced and when memory needs to be refreshed after admin maintenance.

---

## Worker Execution Loop
The Worker service runs an infinite loop inside `ExecuteAsync()`.

```csharp
var tD = StartupConfiguration.GetProtectedSetting("TimeDelay");
int timeDelay = Int32.Parse(tD) * 1000;
```

- The loop waits for `timeDelay` (loaded from `ConfigStore` in SQL at startup).
- After each interval, it calls:

```csharp
EmailManager.CheckEmail();
```

---

## Step 1: Checking for Unread Emails
In `CheckEmail()`, the following is executed:

```csharp
secureEmailApiHelper.FetchAndProcessUnreadEmailsAsync();
```

- This retrieves unread messages from the connected mailbox.
- For each message, `EmailManager.ProcessAddresses()` is called.

---

## Step 2: Email Routing and Validation
The first step in `ProcessAddresses()` is to validate the recipients:

```csharp
CheckSupportEmail(emailApiHelper);
```

If this returns `-1`, the email is dropped and marked as read. This happens under several conditions:

- Subject is fewer than 4 characters
- Subject contains words from `StartupConfiguration.subjectExclusions`
- Sender matches `StartupConfiguration.senderExclusions`
- **Support address is not in** `StartupConfiguration.supportDistros`

---

## Step 3: Role of `EnableEmail`
- `supportDistros` is loaded from the `CustomerSettings` SQL table at startup or refresh time.
- Only rows where `EnableEmail == true` are included.

### ✅ Therefore:
If `EnableEmail` is unchecked (set to `false`), the corresponding support address is excluded from the memory dictionary, and any emails to that address will not be processed.

### ✅ Logging behavior confirms this:
```
No TO: address match found in supportDistro Dictionary
No CC: match for support-test2@domain.com in supportDistro Dictionary
```

---

## Step 4: Ticket Creation or Update
If a valid Support Distro is found:
- If the subject contains an open AutoTask ticket number → it is updated
- If no ticket number → a new ticket is created with:
  - Subject → Ticket Title
  - Body → Ticket Description

Duplicate subject logic prevents re-creation.

---

## Step 5: AutoAssign
AutoAssignment is controlled via `StartupConfiguration.autoAssignCompanies`:
- Loaded from SQL where `AutoAssign == true`
- Used to decide whether the ticket should be auto-assigned

---

## Memory Dictionaries and Refresh Requirements

| Dictionary                        | Loaded From                 | Needs Refresh After                 |
|----------------------------------|-----------------------------|-------------------------------------|
| `supportDistros`                 | SQL `CustomerSettings`      | ✅ `SupportEmail`, `EnableEmail`    |
| `autoAssignCompanies`            | SQL `CustomerSettings`      | ✅ `AutoAssign`                     |
| `Companies.companies`            | AutoTask API                | ✅ AutoTask company sync only       |

---

## Final Notes
- `EnableEmail` **is enforced** by excluding companies at load time from `supportDistros`
- `CheckSupportEmail()` never checks EnableEmail directly — it is pre-filtered
- Any admin changes to customer config should trigger a refresh of the appropriate dictionaries