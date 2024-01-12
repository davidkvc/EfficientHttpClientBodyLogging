using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Diagnostics;
using System.Text;

namespace DavidKvc.Extensions.Http.BodyLogging;

internal class LoggingStream : Stream, IBufferWriter<byte>
{
    private const int MinimumBufferSize = 4096;

    private readonly Stream _target;
    private readonly ILogger _logger;
    private readonly int _limit;
    private readonly Content _content;
    private readonly Encoding _encoding;
    private readonly BodyLoggingContext _loggingContext;

    private bool hasLogged = false;
    private int bytesBuffered = 0;
    private bool isTruncated = false;
    private BufferSegment? head;
    private BufferSegment? tail;
    protected Memory<byte> tailMemory; // remainder of tail memory
    protected int tailBytesBuffered;

    public LoggingStream(Stream target,
        Encoding encoding,
        int limit,
        Content content,
        ILogger logger,
        BodyLoggingContext loggingContext)
    {
        _target = target;
        _encoding = encoding;
        _logger = logger;

        _limit = limit;
        _content = content;
        _loggingContext = loggingContext;
    }

    public override bool CanSeek => false;
    public override bool CanRead => _target.CanRead;
    public override bool CanWrite => _target.CanWrite;
    public override long Length => _target.Length;
    public override long Position { get => _target.Position; set => throw new NotImplementedException(); }

    public override bool CanTimeout => _target.CanTimeout;
    public override int ReadTimeout { get => _target.ReadTimeout; set => _target.ReadTimeout = value; }
    public override int WriteTimeout { get => _target.WriteTimeout; set => _target.WriteTimeout = value; }

    public override void Flush()
    {
        _target.Flush();
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        return _target.FlushAsync(cancellationToken);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var read = _target.Read(buffer);

        if (read == 0)
        {
            Log();
        }
        else
        {
            WriteToBuffer(buffer.AsSpan().Slice(0, read));
        }

        return read;
    }

    public override int Read(Span<byte> buffer)
    {
        var read = _target.Read(buffer);

        if (read == 0)
        {
            Log();
        }
        else
        {
            WriteToBuffer(buffer.Slice(0, read));
        }

        return read;
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var read = await _target.ReadAsync(buffer, offset, count, cancellationToken);

        if (read == 0)
        {
            Log();
        }
        else
        {
            WriteToBuffer(buffer.AsSpan().Slice(0, read));
        }

        return read;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var read = await _target.ReadAsync(buffer, cancellationToken);

        if (read == 0)
        {
            Log();
        }
        else
        {
            WriteToBuffer(buffer.Span.Slice(0, read));
        }

        return read;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        WriteToBuffer(buffer.AsSpan().Slice(offset, count));

        _target.Write(buffer, offset, count);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        WriteToBuffer(buffer);

        _target.Write(buffer);
    }

    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        WriteToBuffer(buffer.AsSpan().Slice(offset, count));

        return _target.WriteAsync(buffer, offset, count, cancellationToken);
    }

    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        WriteToBuffer(buffer.Span);

        return _target.WriteAsync(buffer, cancellationToken);
    }

    private void WriteToBuffer(ReadOnlySpan<byte> span)
    {
        // get what was read into the buffer
        var remaining = _limit - bytesBuffered;

        if (remaining == 0)
        {
            return;
        }

        if (span.Length == 0)
        {
            return;
        }

        var innerCount = Math.Min(remaining, span.Length);

        if (span.Slice(0, innerCount).TryCopyTo(tailMemory.Span))
        {
            tailBytesBuffered += innerCount;
            bytesBuffered += innerCount;
            tailMemory = tailMemory.Slice(innerCount);
        }
        else
        {
            BuffersExtensions.Write(this, span.Slice(0, innerCount));
        }

        if (_limit - bytesBuffered == 0)
        {
            isTruncated = true;
            Log();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Reset();
        }
    }

    public void Log()
    {
        if (hasLogged)
        {
            return;
        }

        hasLogged = true;

        using (_loggingContext.Use(_logger))
        {
            switch (_content)
            {
                case Content.RequestBody:
                    _logger.RequestBody(GetString(_encoding), GetStatus(false));
                    break;
                case Content.ResponseBody:
                    _logger.ResponseBody(GetString(_encoding), GetStatus(false));
                    break;
            }
        }
    }

    public void Advance(int bytes)
    {
        if ((uint)bytes > (uint)tailMemory.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(bytes));
            //ThrowArgumentOutOfRangeException(nameof(bytes));
        }

        tailBytesBuffered += bytes;
        bytesBuffered += bytes;
        tailMemory = tailMemory.Slice(bytes);
    }

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        AllocateMemory(sizeHint);
        return tailMemory;
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        AllocateMemory(sizeHint);
        return tailMemory.Span;
    }

    private void AllocateMemory(int sizeHint)
    {
        if (head is null)
        {
            // We need to allocate memory to write since nobody has written before
            var newSegment = AllocateSegment(sizeHint);

            // Set all the pointers
            head = tail = newSegment;
            tailBytesBuffered = 0;
        }
        else
        {
            var bytesLeftInBuffer = tailMemory.Length;

            if (bytesLeftInBuffer == 0 || bytesLeftInBuffer < sizeHint)
            {
                Debug.Assert(tail != null);

                if (tailBytesBuffered > 0)
                {
                    // Flush buffered data to the segment
                    tail.End += tailBytesBuffered;
                    tailBytesBuffered = 0;
                }

                var newSegment = AllocateSegment(sizeHint);

                tail.SetNext(newSegment);
                tail = newSegment;
            }
        }
    }

    private BufferSegment AllocateSegment(int sizeHint)
    {
        var newSegment = CreateSegment();

        // We can't use the recommended pool so use the ArrayPool
        newSegment.SetOwnedMemory(ArrayPool<byte>.Shared.Rent(GetSegmentSize(sizeHint)));

        tailMemory = newSegment.AvailableMemory;

        return newSegment;
    }

    private static BufferSegment CreateSegment()
    {
        return new BufferSegment();
    }

    private static int GetSegmentSize(int sizeHint, int maxBufferSize = int.MaxValue)
    {
        // First we need to handle case where hint is smaller than minimum segment size
        sizeHint = Math.Max(MinimumBufferSize, sizeHint);
        // After that adjust it to fit into pools max buffer size
        var adjustedToMaximumSize = Math.Min(maxBufferSize, sizeHint);
        return adjustedToMaximumSize;
    }

    public string GetString(Encoding encoding)
    {
        try
        {
            if (head == null || tail == null)
            {
                // nothing written
                return "";
            }

            // Only place where we are actually using the buffered data.
            // update tail here.
            tail.End = tailBytesBuffered;

            var ros = new ReadOnlySequence<byte>(head, 0, tail, tailBytesBuffered);

            var bufferWriter = new ArrayBufferWriter<char>();

            var decoder = encoding.GetDecoder();
            // First calls convert on the entire ReadOnlySequence, with flush: false.
            // flush: false is required as we don't want to write invalid characters that
            // are spliced due to truncation. If we set flush: true, if effectively means
            // we expect EOF in this array, meaning it will try to write any bytes at the end of it.
            EncodingExtensions.Convert(decoder, ros, bufferWriter, flush: false, out var charUsed, out var completed);

            // Afterwards, we need to call convert in a loop until complete is true.
            // The first call to convert many return true, but if it doesn't, we call
            // Convert with a empty ReadOnlySequence and flush: true until we get completed: true.

            // This should never infinite due to the contract for decoders.
            // But for safety, call this only 10 times, throwing a decode failure if it fails.
            for (var i = 0; i < 10; i++)
            {
                if (completed)
                {
                    return new string(bufferWriter.WrittenSpan);
                }
                else
                {
                    EncodingExtensions.Convert(decoder, ReadOnlySequence<byte>.Empty, bufferWriter, flush: true, out charUsed, out completed);
                }
            }

            throw new DecoderFallbackException("Failed to decode after 10 calls to Decoder.Convert");
        }
        catch (DecoderFallbackException ex)
        {
            _logger.DecodeFailure(ex);
            return "<Decoder failure>";
        }
        finally
        {
            Reset();
        }
    }

    private string GetStatus(bool showCompleted) => (_content, isTruncated) switch
    {
        (_, false) => showCompleted ? "[Completed]" : "",
        (Content.RequestBody, true) => "[Truncated by RequestBodyLogLimit]",
        (Content.ResponseBody, true) => "[Truncated by ResponseBodyLogLimit]",
        (_, true) => "[Truncated]",
    };

    private void Reset()
    {
        var segment = head;
        while (segment != null)
        {
            var returnSegment = segment;
            segment = segment.NextSegment;

            // We haven't reached the tail of the linked list yet, so we can always return the returnSegment.
            returnSegment.ResetMemory();
        }

        head = tail = null;

        bytesBuffered = 0;
        tailBytesBuffered = 0;
    }

    public enum Content
    {
        RequestBody,
        ResponseBody
    }
}
