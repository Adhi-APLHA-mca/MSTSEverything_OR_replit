// OR Route Protection — File-level helpers used by STFReader
// Detects magic header, decodes payload.

using System;
using System.IO;
using System.Text;

namespace Orts.Parsers.Msts.OrProtection
{
    /// <summary>
    /// Static helpers for detecting and decoding OR-protected route files.
    ///
    /// Protected file layout:
    ///   [0-7]  Magic bytes (8 bytes) — identifies this as a protected file
    ///   [8+]   LFSR-XOR payload — token-remapped STF text, XOR'd byte-by-byte
    ///
    /// The LFSR seed is a compile-time constant stored only in this binary.
    /// Obfuscate this binary with ConfuserEx before distribution to make
    /// static extraction of the constant impractical.
    /// </summary>
    internal static class OrFileProtection
    {
        // ── Customise these two values before building your protected OR ───────
        // Change SEED to any non-zero 32-bit value — keep it secret.
        // Change MAGIC to any 8 bytes you choose — don't publish them.
        private static readonly uint SEED = 0xB3C7A1F5u;
        private static readonly byte[] MAGIC = { 0x4F, 0x52, 0x53, 0x45, 0x43, 0x01, 0x4B, 0x00 };
        // ────────────────────────────────────────────────────────────────────────

        /// <summary>File extensions that may be protected.</summary>
        internal static bool IsProtectedExtension(string filePath)
        {
            string ext = Path.GetExtension(filePath);
            return ext.Equals(".trk", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".tit", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".tdb", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".rdb", StringComparison.OrdinalIgnoreCase)
                || ext.Equals(".rit", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Returns true when the first 8 bytes match the magic header.</summary>
        internal static bool HasMagic(byte[] rawBytes)
        {
            if (rawBytes.Length < MAGIC.Length) return false;
            for (int i = 0; i < MAGIC.Length; i++)
                if (rawBytes[i] != MAGIC[i]) return false;
            return true;
        }

        /// <summary>
        /// Strips the 8-byte magic header and applies the LFSR reverse transform
        /// to the payload, returning the decoded STF text bytes.
        /// Never writes to disk — result lives only in a MemoryStream.
        /// </summary>
        internal static byte[] Decode(byte[] rawBytes)
        {
            int payloadLen = rawBytes.Length - MAGIC.Length;
            byte[] payload = new byte[payloadLen];
            Array.Copy(rawBytes, MAGIC.Length, payload, 0, payloadLen);

            // XOR is its own inverse — same LFSR pass decodes and encodes
            var lfsr = new LfsrKeystream(SEED);
            lfsr.Transform(payload, 0, payloadLen);

            return payload;
        }

        /// <summary>
        /// Encodes STF text bytes: applies LFSR transform then prepends magic header.
        /// Used by the RoutePackager tool, not by OR itself.
        /// </summary>
        internal static byte[] Encode(byte[] plainBytes)
        {
            byte[] payload = (byte[])plainBytes.Clone();

            var lfsr = new LfsrKeystream(SEED);
            lfsr.Transform(payload, 0, payload.Length);

            byte[] result = new byte[MAGIC.Length + payload.Length];
            Array.Copy(MAGIC, 0, result, 0, MAGIC.Length);
            Array.Copy(payload, 0, result, MAGIC.Length, payload.Length);
            return result;
        }
    }
}
