using Neo;
using Neo.SmartContract.Framework;
using System.Numerics;

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
}
