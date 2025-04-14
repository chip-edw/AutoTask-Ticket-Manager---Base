# Changelog

All notable changes to this project will be documented here.

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
