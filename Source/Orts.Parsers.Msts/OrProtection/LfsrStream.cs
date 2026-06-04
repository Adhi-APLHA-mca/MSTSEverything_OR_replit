// OR Route Protection — Streaming LFSR XOR wrapper
// Wraps any readable Stream and decodes bytes on-the-fly.
// At any moment, only the StreamReader's internal buffer (~4 KB) is ever
// decoded in RAM — the entire file is never decrypted at once.

using System;
using System.IO;

namespace Orts.Parsers.Msts.OrProtection
{
    /// <summary>
    /// A read-only Stream decorator that applies a Xorshift32 LFSR keystream
    /// to every byte it delivers — chunk by chunk, as the consumer asks for data.
    ///
    /// Memory at any point: only the caller's read buffer (StreamReader uses 4 KB).
    /// The raw file on disk is never fully decrypted into a single buffer.
    /// </summary>
    internal sealed class LfsrStream : Stream
    {
        private readonly Stream       _inner;   // raw file stream (positioned past magic)
        private readonly LfsrKeystream _lfsr;   // stateful keystream — must advance in order

        internal LfsrStream(Stream inner, uint seed)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _lfsr  = new LfsrKeystream(seed);
        }

        // ── Core: decode each byte as it arrives ────────────────────────────
        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = _inner.Read(buffer, offset, count);
            int end       = offset + bytesRead;
            for (int i = offset; i < end; i++)
                buffer[i] ^= _lfsr.Next();          // decode in-place, chunk only
            return bytesRead;
        }

        // ── Stream contract ──────────────────────────────────────────────────
        public override bool CanRead  => true;
        public override bool CanSeek  => false;     // seeking would desync the LFSR state
        public override bool CanWrite => false;
        public override long Length   => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }
        public override void  Flush()                                      { }
        public override long  Seek(long offset, SeekOrigin origin)         => throw new NotSupportedException();
        public override void  SetLength(long value)                        => throw new NotSupportedException();
        public override void  Write(byte[] buffer, int offset, int count)  => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing) _inner.Dispose();
            base.Dispose(disposing);
        }
    }
}
