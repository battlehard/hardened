using Neo;
using Neo.SmartContract.Framework;
using System.Numerics;

#pragma warning disable CS8618, CS8625 
namespace Hardened
{
  /// <summary>
  /// Class <c>HardenedState</c>
  /// Inherit Owner, Name from Nep11TokenState
  /// </summary>
  public class HardenedState : Nep11TokenState
  {
    // TODO: add properties
  }

  public class FeeStructure
  {
    public BigInteger bTokenMintCost;
    public BigInteger bTokenUpdateCost;
    public BigInteger gasMintCost;
    public BigInteger gasUpdateCost;
    public UInt160? walletPoolHash;
  }

  public class PendingObject
  {
    public string clientPubKey;
    public string contractPubKey;
    public UInt160 userWalletHash;
    public UInt160 payTokenHash;
    public BigInteger payTokenAmount;
    public BigInteger gasAmount;
    public string bhNftId;
    public UInt160 slot1NftHash;
    public string slot1NftId;
    public UInt160 slot2NftHash;
    public string slot2NftId;
    public UInt160 slot3NftHash;
    public string slot3NftId;
    public UInt160 slot4NftHash;
    public string slot4NftId;
  }
}
