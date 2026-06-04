// OR Route Protection — File-level helpers used by STFReader

using System;
using System.IO;

namespace Orts.Parsers.Msts.OrProtection
{
    /// <summary>
    /// Static helpers for detecting and streaming-decoding OR-protected route files.
    ///
    /// Protected file layout:
    ///   [0-7]  Magic bytes (8 bytes) — identifies this as a protected file
    ///   [8+]   LFSR-XOR payload — token-remapped STF text, XOR'd byte-by-byte
    ///
    /// Decoding is STREAMING — only ~4 KB (StreamReader's buffer) is ever decoded
    /// in RAM at any moment.  A full RAM dump never contains the entire plain text.
    ///
    /// The LFSR seed is a compile-time constant stored only in this binary.
    /// Obfuscate this binary with ConfuserEx before distribution.
    /// </summary>
    internal static class OrFileProtection
    {
        // ── Change BOTH values before building your protected OR ─────────────
        // SEED  : any non-zero 32-bit hex value  — keep it secret
        // MAGIC : any 8 bytes of your choice     — keep them secret
        // These same values MUST be copied into RoutePackager/Program.cs
        internal static readonly uint   SEED  = 0xB3C7A1F5u;
        internal static readonly byte[] MAGIC = { 0x4F, 0x52, 0x53, 0x45, 0x43, 0x01, 0x4B, 0x00 };
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Returns true for file extensions that may be protected.</summary>
        internal static bool IsProtectedExtension(string filePath)
        {
            string ext = Path.GetExtension(filePath);
            return ext.Equals(".trk", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".tit", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".tdb", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".rdb", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".rit", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Reads only the first 8 bytes of the file to check for the magic header.
        /// Does NOT read or decode the rest of the file.
        /// </summary>
        internal static bool HasMagicHeader(string filePath)
        {
            byte[] header = new byte[MAGIC.Length];
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read,
                                          FileShare.Read, bufferSize: MAGIC.Length);
            int read = fs.Read(header, 0, MAGIC.Length);
            if (read < MAGIC.Length) return false;
            for (int i = 0; i < MAGIC.Length; i++)
                if (header[i] != MAGIC[i]) return false;
            return true;
        }

        /// <summary>
        /// Opens the protected file and returns a streaming LfsrStream positioned
        /// just after the magic header.  The caller wraps this in a StreamReader.
        ///
        /// At any moment, only the StreamReader's ~4 KB read buffer is decoded
        /// in RAM — the entire file is never decrypted at once.
        /// The stream (and the underlying FileStream) is owned by the caller.
        /// </summary>
        internal static Stream OpenDecodeStream(string filePath)
        {
            var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read,
                                    FileShare.Read, bufferSize: 4096);
            fs.Seek(MAGIC.Length, SeekOrigin.Begin);   // skip past magic header
            return new LfsrStream(fs, SEED);            // LFSR starts at byte 8
        }
    }
}
