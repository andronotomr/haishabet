namespace Haishabet.GameEngine.Rng;

/// <summary>
/// Stream determinístico de números pseudo-aleatórios usado pelos motores de jogo.
/// Mesmo (seed, nonce) → mesma sequência exata, sempre. Permite "provably fair".
/// </summary>
public interface IRngStream
{
    /// <summary>Próximo uint32 do stream.</summary>
    uint NextUInt32();

    /// <summary>Inteiro em [0, exclusiveMax) com distribuição uniforme (rejection sampling).</summary>
    int NextInt(int exclusiveMax);

    /// <summary>Double em [0, 1) com 53 bits de precisão.</summary>
    double NextDouble();
}
