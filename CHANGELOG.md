# Changelog

All notable changes to APFS Reader for Windows.

## 1.2.0 — 2026-08-10

### What's new

- An **About** window reporting the product version, build date, publisher and
  the SHA-256 of the running executable, so a download can be checked against
  the published checksum without a network connection.
- Author, free tools, privacy and donation links in the About window, matching
  Simple Scanner.
- A permanent footer strip on the main window showing the freeware notice with
  the author and donation links.

### Behaviour changes

- The window header carries an **About** button beside the read-only badge.
- Copyright and publisher are attributed to Patrick Hamid.

### Fixed

- The assembly version no longer reports 1.0.0 for a tagged release; the
  informational version is stamped from the tag alongside the file version.

## 1.1.0 — 2026-08-09

- Applied the InstaHost desktop theme to the browser window.
- Shipped the complete dependency licences with every portable package.

## 1.0.1 — 2026-08-08

- Corrected the Windows parser test paths and the .NET 4 reference assemblies
  used by the release build.

## 1.0.0 — 2026-08-08

- First public release: portable, read-only APFS browsing and extraction for
  Windows 7 SP1 and newer, in 32-bit and 64-bit builds.
