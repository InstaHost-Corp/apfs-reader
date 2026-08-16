# Changelog

All notable changes to APFS Reader for Windows.

## 1.2.2 — Unreleased

### Fixed

- Replaced the 1.2.0 hand-rounded palette stub with the canonical InstaHost
  desktop theme ported verbatim from `user-account-migrator`'s `Ui/Theme.cs`
  (identical to the theme already shared by Simple Scanner and PackPilot).
  The window chrome Windows draws itself — scroll bars, the list view header,
  and the title bar — is now switched to dark via the same `DarkMode` Win32
  hooks the other tools use; previously only the flat control colours were
  themed and the native chrome stayed light. `MainForm` and `AboutDialog` keep
  their existing field names (`Theme.Background`, `Theme.Accent`, etc.), which
  are now aliases onto the shared token set rather than a second palette.
- Fixed squished spacing in the About window: increased dialog height and the
  gaps between the version line, licence text, publisher block, checksum
  field, executable path and link row so text no longer crowds against the
  row below it.
- `APFSReader.csproj` now pins `LangVersion` to 8.0 so the ported theme file's
  pattern-matching switch and null-coalescing assignment compile against the
  project's existing .NET Framework 4.0 target.

## 1.2.1 — 2026-08-16

### Security

- Both the GUI (`APFSReader.exe`) and the native read-only backend
  (`apfs_backend.exe`) are now Authenticode signed during the GitHub Actions
  release build, using the same Azure Artifact Signing (Trusted Signing)
  certificate used for the other InstaHost desktop tools: `CN=MyTechie`,
  issued by Microsoft ID Verified CS AOC CA 04, RFC 3161 timestamped. The
  About window's own signature check now reports both files as signed rather
  than unsigned, and Windows SmartScreen no longer needs to be dismissed on
  first run.
- `AssemblyCompany` changed from "Patrick Hamid" to "MyTechie" to match the
  signing certificate's subject; Windows shows this value as the file's
  publisher, so it has to agree with the certificate or the signature reads
  as a mismatch. Product branding is unaffected: the application still
  presents itself as "APFS Reader for Windows".

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

## 1.0.1 — 2026-08-09

- Corrected the Windows parser test paths and the .NET 4 reference assemblies
  used by the release build.

## 1.0.0 — 2026-08-09

- First public release: portable, read-only APFS browsing and extraction for
  Windows 7 SP1 and newer, in 32-bit and 64-bit builds.
