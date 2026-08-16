using System.Reflection;
using System.Runtime.InteropServices;

// Windows shows Company as the file's Authenticode publisher. It has to match the
// certificate's common name exactly, or the signature reads as a spoof. The Azure
// Artifact Signing certificate subject is sourced from the billing account's registered
// legal name and cannot be chosen freely - see tools/SIGNING-SETUP.md in the sibling
// signed tools. Product branding remains "APFS Reader for Windows"; Company is the
// legal publisher, not the brand.
[assembly: AssemblyTitle("APFS Reader for Windows")]
[assembly: AssemblyDescription("Read-only APFS browser and extractor")]
[assembly: AssemblyCompany("MyTechie")]
[assembly: AssemblyProduct("APFS Reader for Windows")]
[assembly: AssemblyCopyright("Copyright (c) 2026 Patrick Hamid")]
[assembly: ComVisible(false)]
[assembly: Guid("AE41504B-A1E2-4FD5-A19E-FD84A28B79B3")]
[assembly: AssemblyVersion("1.2.1.0")]
[assembly: AssemblyFileVersion("1.2.1.0")]
[assembly: AssemblyInformationalVersion("1.2.1")]
