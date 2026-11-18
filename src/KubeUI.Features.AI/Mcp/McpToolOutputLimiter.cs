using System.Buffers;
using System.Text.Json;

namespace KubeUI.Features.AI.Mcp;

/// <summary>Limits the serialized payload returned by an MCP tool.</summary>
public static class McpToolOutputLimiter
{
    /// <summary>Maximum UTF-8 output size accepted for one MCP tool call.</summary>
    public const int MaximumOutputBytes = 1024 * 1024;

    /// <summary>Checks a tool result without retaining a serialized copy.</summary>
    /// <typeparam name="T">Tool result type.</typeparam>
    /// <param name="value">Tool result to serialize for size validation.</param>
    /// <param name="options">Serializer options containing metadata for the result type.</param>
    /// <returns>The original result when its serialized form fits the output limit.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is null.</exception>
    /// <exception cref="McpToolOutputLimitExceededException">The serialized result exceeds the output limit.</exception>
    public static T EnsureWithinLimit<T>(T value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var buffer = new BoundedBufferWriter(MaximumOutputBytes);
        using var writer = new Utf8JsonWriter(buffer);
        var typeInfo = options.GetTypeInfo(value?.GetType() ?? typeof(T));
        JsonSerializer.Serialize(writer, value, typeInfo);
        writer.Flush();
        return value;
    }

    private sealed class BoundedBufferWriter(int maximumBytes) : IBufferWriter<byte>
    {
        private const int BufferOverheadAllowance = 64;
        private const int MaximumEscapeExpansion = 6;
        private readonly int _maximumBufferBytes = checked(maximumBytes * MaximumEscapeExpansion + BufferOverheadAllowance);
        private byte[] _buffer = new byte[256];
        private int _written;

        public void Advance(int count)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(count);
            if (count > _buffer.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (count > maximumBytes - _written)
            {
                throw new McpToolOutputLimitExceededException();
            }

            _written += count;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            EnsureCapacity(sizeHint);
            // Utf8JsonWriter may request worst-case escaped capacity; Advance counts only actual output bytes.
            return _buffer;
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            EnsureCapacity(sizeHint);
            return _buffer;
        }

        private void EnsureCapacity(int sizeHint)
        {
            sizeHint = Math.Max(sizeHint, 1);
            if (sizeHint > _maximumBufferBytes)
            {
                throw new McpToolOutputLimitExceededException();
            }

            if (sizeHint <= _buffer.Length)
            {
                return;
            }

            var doubledCapacity = Math.Min((long)_buffer.Length * 2, _maximumBufferBytes);
            Array.Resize(ref _buffer, (int)Math.Max(sizeHint, doubledCapacity));
        }

    }
}

/// <summary>Indicates that an MCP tool result exceeds the permitted output size.</summary>
public sealed class McpToolOutputLimitExceededException : InvalidOperationException
{
    /// <summary>Creates an exception for an oversized MCP tool result.</summary>
    public McpToolOutputLimitExceededException()
        : base("MCP tool output exceeds the 1 MiB limit.")
    {
    }
}
