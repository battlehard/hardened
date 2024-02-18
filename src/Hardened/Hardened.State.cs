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
    public string image;
    public string state;
    public int level;
    public UInt160 project;
    public UInt160[] contract; // Contract origin
    public UInt160[] slotNftHashes;
    public string[] slotNftIds;
    public Map<string, object> meta;
    public Map<string, object> attributes;
  }

  public class State
  {
    public const string Ready = "READY";
    public const string Blueprint = "BLUEPRINT";
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
    public UInt160[] slotNftHashes;
    public string[] slotNftIds;
  }
}
