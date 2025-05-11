# Admin Maintenance Checklist - ATTMS

This checklist outlines what memory structures need to be refreshed when specific fields in the Customer Settings are modified. This ensures that changes take effect in the Worker service without needing a full restart.

---

## ✅ When to Refresh In-Memory Dictionaries

| Edited Field           | Refresh Required? | Affected Dictionary             | Notes                                                                 |
|------------------------|-------------------|----------------------------------|-----------------------------------------------------------------------|
| `SupportEmail`         | ✅ Yes             | `supportDistros`                | Used to map incoming emails to AutoTask companies                     |
| `EnableEmail`          | ✅ Yes             | `supportDistros`                | Excluded from memory if false                                         |
| `AutoAssign`           | ✅ Yes             | `autoAssignCompanies`           | Controls auto-assignment of new tickets                               |
| `Enabled`              | ❌ No              | None                             | Not currently used during email processing                            |
| `AccountName`          | ❌ No              | None                             | Cosmetic, not used in runtime processing                              |
| `AutotaskId`           | ❌ No              | None                             | Key field, assumed stable post-sync                                   |
| AutoTask Company Sync  | ✅ Yes             | `Companies.companies`           | Required to reflect new or renamed companies in the Ticket UI         |

---

## 🛠 Recommended Web Admin Process

After updating customer configurations in the Web UI:

1. Save changes
2. Use the Maintenance Panel to manually trigger memory refreshes:
   - 🔁 Refresh Support Distros (after `SupportEmail` or `EnableEmail` changes)
   - 🔁 Refresh AutoAssign Companies (after `AutoAssign` changes)
   - 🔁 Refresh Companies from SQL (after a full AutoTask sync or new company creation)

> ⚠️ **Important:** If a new company is created in AutoTask and associated with a ticket,  
> you must run the **"Refresh Companies from SQL"** action.  
> This updates the in-memory dictionary (`Companies.companies`) used to match company names in ticket views.

This dictionary is not automatically updated during runtime — a maintenance refresh is required for new or renamed companies to appear correctly in the UI.

---

## 🔐 Security Reminder

These memory refresh actions should be restricted to Admins only in the Web UI, using role-based access controls.

---

## 💡 Tip

To ensure consistent system behavior after updates:

- Log memory refresh actions to the console and application logs
- Consider tracking last refresh timestamps in memory or in the Web UI
