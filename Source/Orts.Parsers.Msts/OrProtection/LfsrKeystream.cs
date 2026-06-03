// OR Route Protection — Layer 2: Inline LFSR stream transformation
// This file is intentionally kept internal to this assembly.

using System;

namespace Orts.Parsers.Msts.OrProtection
{
    /// <summary>
    /// 32-bit Galois LFSR (Linear Feedback Shift Register).
    /// Produces a deterministic keystream byte-by-byte from a fixed seed.
    /// Applied inline while reading — the "transformed" content never exists
    /// as a complete buffer on disk or in memory.
    /// </summary>
    internal sealed class LfsrKeystream
    {
        private uint _state;

        internal LfsrKeystream(uint seed)
        {
            // State must never be zero — LFSR would be stuck
            _state = (seed == 0) ? 0xA5F1C3B7u : seed;
        }

        /// <summary>Returns the next byte from the keystream.</summary>
        internal byte Next()
        {
            // Xorshift32 — fast, no library dependencies, same result on all platforms
            _state ^= _state << 13;
            _state ^= _state >> 17;
            _state ^= _state << 5;
            return (byte)(_state & 0xFF);
        }

        /// <summary>
        /// XOR-transforms a byte array in-place using this keystream.
        /// XOR is its own inverse — same call encodes and decodes.
        /// </summary>
        internal void Transform(byte[] data, int offset, int count)
        {
            int end = offset + count;
            for (int i = offset; i < end; i++)
                data[i] ^= Next();
        }
    }
}
