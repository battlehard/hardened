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
    private static readonly int MAX_SLOTS = 4;
    // ERROR MESSAGE
    private const string E_01 = "ERROR: The last 4 digits of wallet address and clientPubKey is not match";
    private const string E_02 = "ERROR: PreInfusion for Mint must have at least one NFT in any slots";
    private const string E_03 = "ERROR: No ownership on pending infusion";
    private const string E_04 = "ERROR: No ownership on NFT";
    private const string E_05 = "ERROR: NFT State must be Ready";
    private const string E_06 = "ERROR: Provided pay token hash and amount must match data in the pending storage";
    private const string E_07 = "ERROR: Provided user wallet hash must match data in the pending storage";
    private const string E_08 = "ERROR: Invalid NFT state";
    private const string E_09 = "ERROR: Update NFTs is more than available slots";
    private const string E_10 = "ERROR: New NFTs must not have any duplication with existing NFTs";
    private const string E_11 = "ERROR: Quantity of update NFT for blueprint state cannot exceed the blueprint level";
    private const string E_12 = "ERROR: Level cannot exceed max allowd NFT slots";
  }
}
