# APFS Reader for Windows

A simple, read-only GUI for browsing and extracting files from APFS volumes on
Windows 7 SP1 or newer. Portable releases are provided separately for 32-bit
and 64-bit Windows.

## Features

- Opens APFS partition images and Windows device paths
- Browses folders and extracts selected files or directories
- Reads software-encrypted APFS volumes using a password
- Supports APFS compressed files, cloned files, and sub-volumes through the
  Paragon APFS SDK Community Edition
- Never writes to the APFS source

## Use

1. Extract the entire release ZIP to a folder.
2. Start `APFSReader.exe`.
3. Browse to an APFS partition image, or enter a device path such as
   `\\.\PhysicalDrive1`.
4. If the volume is encrypted, enter its password and volume number.
5. Select **Open**, browse the volume, then select items and choose
   **Extract selected**.

Reading a physical device normally requires running the application as an
administrator. Use the correct APFS partition device or a partition image;
an image of an entire GPT disk is not automatically partition-scanned.

## Requirements and limitations

- Windows 7 SP1 or newer with .NET Framework 4.0
- x86 releases can access only the first APFS sub-volume, an upstream SDK
  limitation
- Hardware-encrypted/T2 APFS volumes are not supported
- APFS write operations are deliberately not implemented
- FileVault credentials are kept out of process command-line arguments, but
  remain in the GUI process memory while the password field is populated
- Source and destination paths containing characters unavailable in the
  active Windows ANSI code page may not work in the native backend

## Free build and deployment

GitHub Actions builds both architectures with MSVC, static OpenSSL, and the
.NET Framework toolchain. A tag such as `v1.0.0` creates versioned portable
packages:

```text
APFSReader-1.0.0-win-x86.zip
APFSReader-1.0.0-win-x64.zip
```

This matches the portable, versioned ZIP approach used by User Account
Migrator. No paid compiler, installer, hosting service, or runtime library is
required.

To build only the GUI locally:

```powershell
.\build-gui.ps1 -Architecture x64
.\build-gui.ps1 -Architecture x86
```

The complete native build is reproducible through
`.github/workflows/build-release.yml`.

## Safety

The backend is compiled with `UFSD_APFS_RO` and opens APFS sources read-only.
Extracted files are first written to a uniquely named partial file, then moved
into place only after a successful read. Existing destination files are never
overwritten.

