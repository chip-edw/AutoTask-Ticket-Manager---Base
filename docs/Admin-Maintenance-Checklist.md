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
| AutoTask Company Sync  | ✅ Yes             | `Companies.companies`           | Refresh only after pulling from the external AutoTask API             |

---

## 🛠 Recommended Admin Process

After updating customer configurations in the Admin UI:

1. Save changes
2. Use the Maintenance Panel to manually trigger memory refreshes:
   - 🔁 Refresh Support Distros (after SupportEmail or EnableEmail changes)
   - 🔁 Refresh AutoAssign Companies (after AutoAssign changes)
   - 🔁 Refresh Companies from SQL (only after a full AutoTask sync)

---

## 🔐 Security Reminder

These memory refresh actions should be restricted to Admins only in the UI, using role-based access controls.

---

## 💡 Tip

To ensure consistent system behavior after updates:
- Log memory refresh actions to the console and application logs
- Consider tracking last refresh timestamps in memory or in the Admin UI