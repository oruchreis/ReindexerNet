# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.2.21-Embedded] - 2021-01-27
### Fixed
- Set minimal glibc version for xenial and bionic

## [0.2.20-Embedded] - 2021-01-27
### Added
- Updated Reindexer to 3.0.1
### Changed
- Removed Tcmalloc.

## [0.2.19-Embedded] - 2020-08-07
### Added
- Updated Reindexer to 2.11.1

## [0.2.17-Embedded] - 2020-08-07
### Fixed
- Bug in "IN" (Set) conditions for small namespaces that exists after Reindexer v2.9.2

## [0.2.16-Embedded] - 2020-08-05
### Added
- Updated Reindexer to 2.11.0

## [0.2.15-Embedded] - 2020-07-28
### Changed
- Default allocator as tcmalloc for linux and osx.
### Fixed
- Malloc override issues with static linking.
- Tcmalloc and jemalloc dlopen crashes.

## [0.2.14-Embedded] - 2020-06-26
### Added
- Jemalloc as default allocator.
- RocksDb
- RocksDb tests
- Updated Reindexer to 2.10.0
### Changed
- Removed Tcmalloc because of dlopen crash.
- Staticly linked all dependencies except glibc.

## [0.2.13-Embedded] - 2020-05-21
### Added
- Cross platform tests.

## [0.2.12-Embedded] - 2020-05-21
### Added
- Updated Reindexer to 2.9.0
### Fixed
- IIS .Net 4.7.2 library loading
- OSX Library loading
- Fixed a bug at server startup and stop
### Changed
- Native Library loading

## [0.2.11-Embedded] - 2020-04-29
### Fixed
- Added missing native function binding

## [0.2.10-Embedded] - 2020-04-29
### Added
- Updated Reindexer to 2.8.0
- Added utf-8 test.
### Changed
- All string operations converted to utf-8 char. 

## [0.2.9-Embedded] - 2020-04-23
### Added
- Added unload cleaning for .net core

## [0.2.2-Core] - 2020-04-23
### Changed
- Changed numeric field types to long

## [0.2.8-Embedded] - 2020-04-21
### Changed
- Increased the wait timeout for server to 60sec.

## [0.2.7-Embedded] - 2020-04-21
### Fixed
- Fixed iis overlapped recycle for rx server.

## [0.2.6-Embedded] - 2020-04-21
### Fixed
- Fixed reindexer server yaml config file.

## [0.2.5-Embedded] - 2020-04-21
### Fixed
- Fixed dll unload for iis overlapped recycle.

## [0.2.4-Embedded] - 2020-04-20
### Fixed
- Closed ns on openning errors.

## [0.2.3-Embedded] - 2020-04-20
### Added
- Added more native library search paths for .net core because of GetCurrenctDirectory bug.

## [0.2.2-Embedded] - 2020-04-14
### Added
- Added Windows-x86 and OSX version of Reindexer

## [0.2.1-Embedded] - 2020-04-13
### Fixed
- Fixed Logging

## [0.2.0-Embedded] - 2020-04-13
### Added
- Updated Reindexer to 2.7.0
- Added multiple server instance support of 2.7.0

## [0.2.0-Core] - 2020-04-13
### Changed
- Changed versioning of project

## [0.1.0] - 2020-04-2
- Initial Release