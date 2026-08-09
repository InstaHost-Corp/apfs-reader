using System;
using System.Collections.Generic;
using System.Text;
using APFSReader;

internal static class ParserTests
{
    private static int Main()
    {
        string unicodeName = "Résumé.txt";
        string hex = BitConverter.ToString(Encoding.UTF8.GetBytes(unicodeName)).Replace("-", "");
        IList<ApfsEntry> entries = ApfsBackend.ParseEntries(
            "noise\nENTRY\tD\t0\t446f63756d656e7473\nENTRY\tF\t42\t" + hex + "\n");

        if (entries.Count != 2 || entries[0].Name != "Documents" ||
            !entries[0].IsDirectory || entries[1].Name != unicodeName ||
            entries[1].Size != 42)
            return 1;

        try
        {
            ApfsBackend.DecodeHexUtf8("0");
            return 2;
        }
        catch (FormatException)
        {
            return 0;
        }
    }
}
