using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SS14.Launcher.Utility;

public sealed class BandwidthStream : Stream
{
    private const int NumSeconds = 8;
    private const int BucketDivisor = 2;
    private const int BucketsPerSecond = 2 << BucketDivisor;

    // TotalBuckets MUST be power of two!
    private const int TotalBuckets = NumSeconds * BucketsPerSecond;

    private readonly Stopwatch _stopwatch;
    private readonly Stream _baseStream;
    private readonly long[] _buckets;

    private long _bucketIndex;

    public BandwidthStream(Stream baseStream)
    {
        _stopwatch = Stopwatch.StartNew();
        _baseStream = baseStream;
        _buckets = new long[TotalBuckets];
    }

    private void TrackBandwidth(long value)
    {
        const int bucketMask = TotalBuckets - 1;

        var bucketIdx = CurBucketIdx();

        // Increment to bucket idx, clearing along the way.
        if (bucketIdx != _bucketIndex)
        {
            var diff = bucketIdx - _bucketIndex;
            if (diff > TotalBuckets)
            {
                for (var i = _bucketIndex; i < bucketIdx; i++)
                {
                    _buckets[i & bucketMask] = 0;
                }
            }
            else
            {
                // We managed to skip so much time the whole buffer is empty.
                Array.Clear(_buckets);
            }

            _bucketIndex = bucketIdx;
        }

        // Write value.
        _buckets[bucketIdx & bucketMask] += value;
    }

    private long CurBucketIdx()
    {
        var elapsed = _stopwatch.Elapsed.TotalSeconds;
        return (long)(elapsed / BucketsPerSecond);
    }

    public long CalcCurrentAvg()
    {
        var sum = 0L;

        for (var i = 0; i < TotalBuckets; i++)
        {
            sum += _buckets[i];
        }

        return sum >> BucketDivisor;
    }

    public override void Flush()
    {
        _baseStream.Flush();
    }

    public override Task FlushAsync(CancellationToken cancellationToken)
    {
        return _baseStream.FlushAsync(cancellationToken);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _baseStream.Dispose();
    }

    public override ValueTask DisposeAsync()
    {
        return _baseStream.DisposeAsync();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var read = _baseStream.Read(buffer, offset, count);
        TrackBandwidth(read);
        return read;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var read = await base.ReadAsync(buffer, cancellationToken);
        TrackBandwidth(read);
        return read;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return _baseStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        _baseStream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        _baseStream.Write(buffer, offset, count);
        TrackBandwidth(count);
    }

    public override async ValueTask WriteAsync(
        ReadOnlyMemory<byte> buffer,
        CancellationToken cancellationToken = default)
    {
        await _baseStream.WriteAsync(buffer, cancellationToken);
        TrackBandwidth(buffer.Length);
    }

    public override bool CanRead => _baseStream.CanRead;

    public override bool CanSeek => _baseStream.CanSeek;

    public override bool CanWrite => _baseStream.CanWrite;

    public override long Length => _baseStream.Length;

    public override long Position
    {
        get => _baseStream.Position;
        set => _baseStream.Position = value;
    }
}
