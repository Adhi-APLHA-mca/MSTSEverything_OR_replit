// OR Route Packager
// Converts plain route files (.trk .tit .tdb .rdb .rit) into the
// two-layer protected format and saves them in a new folder next to
// the original route folder.
//
// Usage:
//   RoutePackager.exe "C:\MSTS\Routes\MyRoute"
//
// Output is written to:
//   "C:\MSTS\Routes\MyRoute_protected\"
//
// All non-route files (textures, sounds, shapes, etc.) are copied unchanged.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

class Program
{
    // ── Protected extensions ────────────────────────────────────────────────
    static readonly HashSet<string> ProtectedExts =
        new(StringComparer.OrdinalIgnoreCase) { ".trk", ".tit", ".tdb", ".rdb", ".rit" };

    // ── LFSR seed — MUST match OrFileProtection.SEED in your modified OR build
    const uint SEED = 0xB3C7A1F5u;

    // ── Magic header — MUST match OrFileProtection.MAGIC in your modified OR build
    static readonly byte[] MAGIC = { 0x4F, 0x52, 0x53, 0x45, 0x43, 0x01, 0x4B, 0x00 };

    // ── Token map — MUST match OrTokenMap in your modified OR build ─────────
    static readonly Dictionary<string, string> TokenMap =
        new(StringComparer.OrdinalIgnoreCase)
    {
        { "tr_routefile",                                "@3a7f" },
        { "_openrails",                                  "@c1e4" },
        { "routeid",                                     "@8b2d" },
        { "name",                                        "@4f9e" },
        { "filename",                                    "@7c3a" },
        { "description",                                 "@2e8b" },
        { "environment",                                 "@9d1c" },
        { "routestart",                                  "@6a4f" },
        { "maxlinevoltage",                              "@1b7e" },
        { "speedlimit",                                  "@e3c9" },
        { "electrified",                                 "@5f2a" },
        { "overheadwireheight",                          "@b4d6" },
        { "graphic",                                     "@d8e1" },
        { "loadingscreen",                               "@3c7b" },
        { "trackgauge",                                  "@a6f2" },
        { "milepostunitskilometers",                     "@7e1d" },
        { "temprestrictedspeed",                         "@c9b5" },
        { "defaultcrossingsms",                          "@4a8e" },
        { "defaultcoaltowersms",                         "@2f6c" },
        { "defaultdieseltowersms",                       "@8d3b" },
        { "defaultwatertowersms",                        "@1e7a" },
        { "defaultsignalsms",                            "@5c4f" },
        { "precision",                                   "@f1b3" },
        { "maxfreightunderbalance",                      "@6e9d" },
        { "maxpassengerunderbalance",                    "@3b5c" },
        { "minimumcant",                                 "@9a2e" },
        { "maximumcant",                                 "@c7f4" },
        { "minimumspeed",                                "@4d1b" },
        { "maximumspeed",                                "@8f6a" },
        { "maxrunoffslope",                              "@1c3e" },
        { "maxrunoffspeed",                              "@7a9f" },
        { "ortssingletunnelarea",                        "@b2e5" },
        { "ortssingletunnelperimeter",                   "@5d8c" },
        { "ortsdoubletunnelarea",                        "@e4b1" },
        { "ortsdoubletunnelperimeter",                   "@2a6f" },
        { "ortstracksuperelevation",                     "@9c7e" },
        { "ortssuperelevation",                          "@3f4d" },
        { "ortsforcesuperelevation",                     "@c5a8" },
        { "ortsmaxviewingdistance",                      "@6b1e" },
        { "ortsloadingscreenwide",                       "@d3f7" },
        { "ortsdoublewireenabled",                       "@4e9b" },
        { "ortsdoublewireheight",                        "@a1c6" },
        { "ortstriphaseenabled",                         "@7f2d" },
        { "ortstriphasewidth",                           "@1d8a" },
        { "ortsdefaultturntablesms",                     "@b9e3" },
        { "ortsswitchsmsnumber",                         "@5a7c" },
        { "ortscurvesmsnumber",                          "@2c4f" },
        { "ortscurveswitchsmsnumber",                    "@8e6b" },
        { "ortsopendoorsinaitrains",                     "@f4d2" },
        { "ortsplaytracksoundsbasecontinuous",           "@3d9a" },
        { "ortsdistancebetweentrackjoints",              "@c6f1" },
        { "ortsconcretesleepers",                        "@7b4e" },
        { "ortsuserpreferenceforestcleardistance",       "@1a5d" },
        { "ortsuserpreferenceremoveforesttreesfromroads","@9f3c" },
        { "trackdb",                                     "@6d8b" },
        { "tracknodes",                                  "@e2a4" },
        { "tracknode",                                   "@4b7f" },
        { "tritemtable",                                 "@c3e9" },
        { "crossoveritem",                               "@8a1d" },
        { "signalitem",                                  "@2f5c" },
        { "speedpostitem",                               "@b7e3" },
        { "platformitem",                                "@5d2a" },
        { "soundregionitem",                             "@1e9f" },
        { "emptyitem",                                   "@7c4b" },
        { "levelcritem",                                 "@3a8e" },
        { "sidingitem",                                  "@f6d1" },
        { "hazzarditem",                                 "@9b5c" },
        { "pickupitem",                                  "@4e7a" },
        { "uid",                                         "@c1f3" },
        { "trjunctionnode",                              "@8d6e" },
        { "trvectornode",                                "@2b9c" },
        { "trendnode",                                   "@6f4a" },
        { "trpins",                                      "@a3c7" },
        { "trpin",                                       "@1d5f" },
        { "trvectorsections",                            "@e8b2" },
        { "tritemsections",                              "@5c1a" },
        { "tritemrefs",                                  "@9e3d" },
        { "tritemref",                                   "@3b7c" },
        { "tritemid",                                    "@d4f8" },
        { "tritemrdata",                                 "@7a2e" },
        { "tritemsdata",                                 "@c9b1" },
        { "tritempdata",                                 "@4f6d" },
        { "crossovertritemdata",                         "@1c4b" },
        { "trsignaltype",                                "@8e7f" },
        { "trsignaldirs",                                "@5b9c" },
        { "trsignaldir",                                 "@2d6a" },
        { "speedposttritemdata",                         "@b1e4" },
        { "platformtritemdata",                          "@6c3f" },
        { "platformname",                                "@a9d2" },
        { "platformminwaitingtime",                      "@3e8b" },
        { "platformnumpassengerswaiting",                "@f7c5" },
        { "sidingname",                                  "@4a1d" },
        { "sidingtritemdata",                            "@9c6e" },
        { "tritemsrdata",                                "@7d4c" },
        { "trinodecount",                                "@c2a9" },
        { "trpincount",                                  "@1f7b" },
        { "carspawneritem",                              "@8b3d" },
    };

    // STF directives that must never be remapped (OR needs these verbatim)
    static readonly HashSet<string> StfDirectives =
        new(StringComparer.OrdinalIgnoreCase) { "include", "comment", "skip" };

    // ════════════════════════════════════════════════════════════════════════
    static int Main(string[] args)
    {
        Console.WriteLine("╔══════════════════════════════════════════╗");
        Console.WriteLine("║       OR Route Packager  v1.0            ║");
        Console.WriteLine("╚══════════════════════════════════════════╝");
        Console.WriteLine();

        // ── Resolve input folder ────────────────────────────────────────────
        string inputDir;
        if (args.Length >= 1)
        {
            inputDir = args[0].Trim('"', '\'', ' ');
        }
        else
        {
            Console.Write("Enter route folder path: ");
            string? typed = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(typed))
            {
                Console.Error.WriteLine("No folder given. Exiting.");
                return 1;
            }
            inputDir = typed.Trim('"', '\'', ' ');
        }

        inputDir = Path.GetFullPath(inputDir);

        if (!Directory.Exists(inputDir))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"ERROR: Folder not found:\n  {inputDir}");
            Console.ResetColor();
            return 2;
        }

        // ── Auto-derive output folder (sibling, same parent, name + _protected)
        string routeName  = Path.GetFileName(inputDir.TrimEnd(Path.DirectorySeparatorChar,
                                                               Path.AltDirectorySeparatorChar));
        string parentDir  = Path.GetDirectoryName(inputDir)!;
        string outputDir  = Path.Combine(parentDir, routeName + "_protected");

        Console.WriteLine($"  Source : {inputDir}");
        Console.WriteLine($"  Output : {outputDir}");
        Console.WriteLine();

        // If output already exists, ask before overwriting
        if (Directory.Exists(outputDir))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"  Output folder already exists. Overwrite? (y/N): ");
            Console.ResetColor();
            string? answer = Console.ReadLine();
            if (!string.Equals(answer?.Trim(), "y", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Cancelled.");
                return 0;
            }
        }

        // ── Process all files ───────────────────────────────────────────────
        int totalFiles   = 0;
        int protectedCnt = 0;
        int copiedCnt    = 0;
        int errorCnt     = 0;

        string[] allFiles = Directory.GetFiles(inputDir, "*", SearchOption.AllDirectories);
        int total = allFiles.Length;

        Console.WriteLine($"  Processing {total} file(s)...");
        Console.WriteLine();

        foreach (string srcFile in allFiles)
        {
            string rel     = Path.GetRelativePath(inputDir, srcFile);
            string dstFile = Path.Combine(outputDir, rel);

            // Ensure destination sub-folder exists
            Directory.CreateDirectory(Path.GetDirectoryName(dstFile)!);

            totalFiles++;
            string ext = Path.GetExtension(srcFile);

            try
            {
                if (ProtectedExts.Contains(ext))
                {
                    ProtectFile(srcFile, dstFile);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"  [PROTECTED]  {rel}");
                    Console.ResetColor();
                    protectedCnt++;
                }
                else
                {
                    File.Copy(srcFile, dstFile, overwrite: true);
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"  [copied   ]  {rel}");
                    Console.ResetColor();
                    copiedCnt++;
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"  [ERROR    ]  {rel}");
                Console.WriteLine($"               {ex.Message}");
                Console.ResetColor();
                errorCnt++;
            }
        }

        // ── Summary ─────────────────────────────────────────────────────────
        Console.WriteLine();
        Console.WriteLine("─────────────────────────────────────────────");
        if (errorCnt == 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  Done!  {protectedCnt} protected,  {copiedCnt} copied.");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  Done with {errorCnt} error(s).  " +
                              $"{protectedCnt} protected,  {copiedCnt} copied.");
        }
        Console.ResetColor();
        Console.WriteLine($"  Output folder: {outputDir}");
        Console.WriteLine("─────────────────────────────────────────────");
        Console.WriteLine();
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey(intercept: true);

        return errorCnt > 0 ? 3 : 0;
    }

    // ════════════════════════════════════════════════════════════════════════
    // Core: read plain file → remap tokens → LFSR encode → write with header
    // ════════════════════════════════════════════════════════════════════════
    static void ProtectFile(string srcPath, string dstPath)
    {
        string originalText = File.ReadAllText(srcPath);

        // Layer 1 — replace known STF tokens with opaque codes
        string remappedText = RemapTokens(originalText);

        // Convert to UTF-8 bytes
        byte[] plain = Encoding.UTF8.GetBytes(remappedText);

        // Layer 2 — LFSR XOR transform
        byte[] encoded = LfsrTransform(plain);

        // Write: 8-byte magic header + encoded payload
        using var fs = File.Open(dstPath, FileMode.Create, FileAccess.Write, FileShare.None);
        fs.Write(MAGIC, 0, MAGIC.Length);
        fs.Write(encoded, 0, encoded.Length);
    }

    // ── Layer 1: Token remapping ─────────────────────────────────────────────
    // Character-by-character scanner.  Only bare identifiers outside quoted
    // strings and comments are eligible for remapping.
    static string RemapTokens(string text)
    {
        var sb  = new StringBuilder(text.Length + text.Length / 4);
        int len = text.Length;
        int i   = 0;

        while (i < len)
        {
            char c = text[i];

            // Quoted string — copy verbatim (including escape sequences)
            if (c == '"')
            {
                sb.Append(c); i++;
                while (i < len)
                {
                    char q = text[i]; sb.Append(q); i++;
                    if (q == '\\' && i < len) { sb.Append(text[i]); i++; continue; }
                    if (q == '"') break;
                }
                continue;
            }

            // # comment — copy to end of line verbatim
            if (c == '#')
            {
                while (i < len && text[i] != '\n') sb.Append(text[i++]);
                continue;
            }

            // Identifier candidate
            if (char.IsLetter(c) || c == '_')
            {
                int start = i;
                while (i < len && (char.IsLetterOrDigit(text[i]) || text[i] == '_')) i++;
                string token = text.Substring(start, i - start);

                if (!StfDirectives.Contains(token) &&
                    TokenMap.TryGetValue(token, out string? code))
                    sb.Append(code);
                else
                    sb.Append(token);
                continue;
            }

            // Everything else (parens, numbers, whitespace) — verbatim
            sb.Append(c); i++;
        }

        return sb.ToString();
    }

    // ── Layer 2: LFSR XOR (Xorshift32) ──────────────────────────────────────
    // XOR is self-inverse — encoding and decoding are the same operation.
    static byte[] LfsrTransform(byte[] data)
    {
        byte[] result = (byte[])data.Clone();
        uint state = SEED;
        for (int i = 0; i < result.Length; i++)
        {
            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            result[i] ^= (byte)(state & 0xFF);
        }
        return result;
    }
}
