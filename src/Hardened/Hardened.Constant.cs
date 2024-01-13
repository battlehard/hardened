using System.Numerics;

namespace Hardened
{
  public partial class Hardened
  {
    private const string CONTRACT_NAME = "Hardened";
    // TODO: Update default cost
    private static readonly BigInteger defaultBTokenMintCost = 1_00000000;
    private static readonly BigInteger defaultBTokenUpdateCost = 0;
    private static readonly BigInteger defaultGasMintCost = 1_00000000;
    private static readonly BigInteger defaultGasUpdateCost = 0_50000000;
  }
}
