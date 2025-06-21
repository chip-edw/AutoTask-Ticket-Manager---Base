# Changelog

All notable changes to this project will be documented here.

## [1.1.0-beta] - 2025-06-21
### Added - MAJOR MILESTONE: Complete Ticket CRUD System
- **Full Ticket Management WebUI**: Complete React-based interface for ticket operations
- **Ticket CRUD API**: RESTful endpoints at `/api/v1/tickets/*` with full AutoTask integration
  - `GET /api/v1/tickets` - Paginated ticket listing with filtering
  - `GET /api/v1/tickets/{ticketNumber}` - Individual ticket retrieval
  - `POST /api/v1/tickets` - Create new tickets with validation
  - `PATCH /api/v1/tickets/{ticketId}` - Efficient partial ticket updates
  - `GET /api/v1/tickets/metadata` - Real-time AutoTask picklist data
- **AutoTask Integration**: Real-time metadata loading for statuses, queues, and priorities
- **Edit Ticket Form**: Complete edit functionality with AutoTask picklist integration
- **Ticket Listing**: Material-UI DataGrid with sortable columns and company name resolution
- **PATCH-based Updates**: Efficient partial updates sending only changed fields to AutoTask
- **Error Handling**: Comprehensive error handling and logging throughout the ticket system

### Technical Improvements
- Implemented `ITicketUIService` with full async/await pattern
- Added `TicketCreateDto`, `TicketUpdateDto`, and `TicketMetadataDto` for clean API contracts
- Real-time picklist conversion from AutoTask JObject arrays to frontend-friendly options
- In-memory company name resolution using `Companies.companies` cache
- Proper HTTP status codes (200, 201, 404, 500) with meaningful error messages

### In Progress - Current Sprint: BASIC CRUD Completion
- 🔄 **IN PROGRESS:** Fix Create New Ticket form to use real metadata (remove hardcoded values)
- 🔄 **IN PROGRESS:** Add AutoTask Resources to backend metadata endpoint
- 🔄 **IN PROGRESS:** Add Resource assignment dropdown to Create and Edit forms
						
## [1.0.4-beta] - 2025-05-04
### Added
- Admin UI: Sender and Subject Exclusions grid (add/delete with reload)
- Snackbar feedback for exclusion changes
- Memory reload triggered on exclusion updates

### Fixed
- Corrected API payload field for sender exclusion (`email`)
- Resolved missing CreatedOn timestamp in SenderExclusionDto and SubjectExclusionKeywordDto
- Adjusted frontend fallback logic for null exclusion fields

## [1.0.4-beta] - 2025-05-03

### Notes
- Renamed development branch from `feature/sender-subject-autoassign` to `feature/sender-subject-Exclusions` to reflect current scope.
  - Auto-Assign functionality deferred to a future plugin-based rules engine.

### Added
- Completed implementation of Sender and Subject Exclusion Maintenance APIs:
  - `GET`, `POST`, and `DELETE` endpoints for both sender and subject exclusions
  - Fully documented `MaintenanceController` using XML-style summaries for consistency
- Introduced `ISubjectExclusionService` interface and wired it into the controller via DI
- Updated DTO references for exclusions to use consistent namespaces across all layers

### Fixed
- Resolved build error (`CS1503`) caused by ambiguous DTO type references in the controller
- Aligned `ISenderExclusionService` and `SenderExclusionService.cs` with the `Dtos` namespace
- Confirmed no remaining namespace or EF model mismatches across exclusions logic

### Changed
- Cleaned up `MaintenanceController.cs`:
  - Removed unused/deferred `Auto-Assign Rules` region
  - Ensured all exclusion endpoints follow consistent naming and route structure

---

## [1.0.3-beta] - 2025-04-30
### Added
- AdminUI Company Settings Management page
  - Editable grid for `supportEmail`, `enabled`, `enableEmail`, `autoAssign`
  - Save button for batching updates via API
  - Refresh button to reload memory after admin changes

---
 
## [1.0.2-beta] - 2025-04-28
### Added
- Added `MaintenanceController` for AdminUI-triggered dictionary and list reloads
- Added `ReloadRequest` DTO for selective reload flexibility (Exclusions, Companies, Resources, etc.)
- Created `StartupLoaderService` for centralized, scoped startup data loading
- Introduced `LoadAllStartupDataAsync` method for structured initialization flow

---

### Changed
- Refactored `ExclusionService` to use centralized master lists from `StartupConfiguration`
- Removed internal list duplication in `ExclusionService`
- Migrated `ManagementService` and `IManagementService` to `Services/` folder
- Updated namespaces and references after ManagementService refactor
- Delegated initial database loading in `Worker.StartAsync()` to `StartupLoaderService`
- Prepared project for future async/await adoption in all startup memory loading operations

---

## [1.0.1-beta] - 2025-04-28
### Added
- Created `ExclusionService` for centralized memory access to Sender and Subject Exclusion Lists
- Standardized API responses across Management and Setup controllers using `ApiResponse<T>`
- Implemented top-level inbound logging for all controller methods
- Added structured exception handling and uniform error responses across Management API
- Prepared Management API v1 for AdminUI integration

### Fixed
- Eliminated circular references between `EmailManager`, `ManagementApiController`, and exclusion logic
- Updated `EmailManager` to consume exclusions through `ExclusionService` instead of helper classes
- Updated `ManagementApiController` to use `ExclusionService` for memory-based exclusion retrieval
- Ensured consistent HTTP status codes (200, 400, 404, 500) across all API endpoints

### Changed
- Moved exclusion list logic out of controllers into service layer
- Marked helper-based exclusion retrieval (e.g., `ManagementApiHelper`) for deprecation
- Improved error logging with full exception capture in all service catch blocks

---

## [1.0.0-beta] - 2025-04-13
### Added
- Initial scheduler plugin architecture
- Plugin loading via `PluginManager`
- Email integration with MS Graph API
- Scoped DI resolution using `ServiceActivator`

### Fixed
- DI scope disposal issue in `RunSchedulerLoop`

### Changed
- Replaced all logging with Serilog

---

## [0.9.0-alpha] - 2025-03-30
### Added
- Worker architecture
- SQLite-based configuration loading
- Early REST API integrations with AutoTask
