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
    // ERROR MESSAGE
    private const string E_01 = "ERROR: The last 4 digits of wallet address and clientPubKey is not match";
    private const string E_02 = "ERROR: PreInfusion for Mint must have at least one NFT in any slots";
    private const string E_03 = "ERROR: No ownership on pending infusion";
    private const string E_04 = "ERROR: No ownership on NFT";
    private const string E_05 = "ERROR: NFT State must be Ready";
    private const string E_06 = "ERROR: Provided pay token hash and amount must match data in the pending storage";
    private const string E_07 = "ERROR: Provided user wallet hash must match data in the pending storage";
  }
}
