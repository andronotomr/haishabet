using System.Security.Cryptography;
using System.Text;

namespace Haishabet.GameEngine.Rng;

/// <summary>
/// Stream pseudo-aleatório criptograficamente seguro e determinístico baseado em
/// HMAC-SHA256(serverSeed, "{clientSeed}:{nonce}:{counter}").
/// Padrão "provably fair" usado em iGaming. Mesma seed/nonce → mesma sequência exata.
/// </summary>
public sealed class HmacRngStream : IRngStream
{
    private readonly byte[] _serverSeed;
    private readonly string _clientSeed;
    private readonly ulong _nonce;
    private uint _counter;
    private byte[] _buffer = [];
    private int _bufferPos;

    public HmacRngStream(byte[] serverSeed, string clientSeed, ulong nonce)
    {
        ArgumentNullException.ThrowIfNull(serverSeed);
        ArgumentNullException.ThrowIfNull(clientSeed);
        _serverSeed = serverSeed;
        _clientSeed = clientSeed;
        _nonce = nonce;
        _counter = 0;
    }

    public uint NextUInt32()
    {
        EnsureBuffer(4);
        uint result = (uint)_buffer[_bufferPos]
                    | (uint)_buffer[_bufferPos + 1] << 8
                    | (uint)_buffer[_bufferPos + 2] << 16
                    | (uint)_buffer[_bufferPos + 3] << 24;
        _bufferPos += 4;
        return result;
    }

    public int NextInt(int exclusiveMax)
    {
        if (exclusiveMax <= 0)
            throw new ArgumentOutOfRangeException(nameof(exclusiveMax), "Must be positive.");

        // Rejection sampling para evitar viés modular.
        ulong threshold = (1UL << 32) - ((1UL << 32) % (uint)exclusiveMax);
        uint v;
        do { v = NextUInt32(); } while (v >= threshold);
        return (int)(v % (uint)exclusiveMax);
    }

    public double NextDouble()
    {
        EnsureBuffer(8);
        ulong bits = BitConverter.ToUInt64(_buffer, _bufferPos);
        _bufferPos += 8;
        // 53 bits significativos da mantissa do double.
        return (bits >> 11) * (1.0 / (1UL << 53));
    }

    private void EnsureBuffer(int needed)
    {
        if (_bufferPos + needed > _buffer.Length)
        {
            var msg = Encoding.UTF8.GetBytes($"{_clientSeed}:{_nonce}:{_counter}");
            _counter++;
            _buffer = HMACSHA256.HashData(_serverSeed, msg);
            _bufferPos = 0;
        }
    }

    /// <summary>Gera 32 bytes criptograficamente aleatórios para usar como server seed.</summary>
    public static byte[] GenerateServerSeed()
    {
        var seed = new byte[32];
        RandomNumberGenerator.Fill(seed);
        return seed;
    }

    /// <summary>Hash do server seed para divulgar pré-jogo (usuário verifica depois).</summary>
    public static string HashServerSeed(byte[] serverSeed)
        => Convert.ToHexString(SHA256.HashData(serverSeed)).ToLowerInvariant();
}
