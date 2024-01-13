using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services;
using System.Numerics;
using static Hardened.Helpers;

namespace Hardened
{
  public partial class Hardened
  {
    private static void CheckContractAuthorization()
    {
      Assert(IsAdmin(), $"{CONTRACT_NAME}: No admin authorization");
    }

    private static bool IsAdmin()
    {
      if (IsOwner())
      {
        return true;
      }
      else
      {
        // tx.Sender is transaction signer
        var tx = (Transaction)Runtime.ScriptContainer;
        if (AdminHashesStorage.IsExist(tx.Sender))
        {
          return true;
        }
        else
        {
          return false;
        }
      }
    }

    public static void SetAdmin(UInt160 contractHash)
    {
      CheckContractAuthorization();
      AdminHashesStorage.Put(contractHash);
    }

    public static List<UInt160> GetAdmin()
    {
      CheckContractAuthorization();
      return AdminHashesStorage.List();
    }

    public static void DeleteAdmin(UInt160 contractHash)
    {
      CheckContractAuthorization();
      AdminHashesStorage.Delete(contractHash);
    }

    public static void FeeUpdate(BigInteger? bTokenMintCost, BigInteger? bTokenUpdateCost,
                                 BigInteger? gasMintCost, BigInteger? gasUpdateCost, UInt160? walletPoolHash)
    {
      CheckContractAuthorization();
      FeeStructure updatingFeeStructure = FeeStructureStorage.Get();
      if (bTokenMintCost != null) updatingFeeStructure.bTokenMintCost = (BigInteger)bTokenMintCost;
      if (bTokenUpdateCost != null) updatingFeeStructure.bTokenUpdateCost = (BigInteger)bTokenUpdateCost;
      if (gasMintCost != null) updatingFeeStructure.gasMintCost = (BigInteger)gasMintCost;
      if (gasUpdateCost != null) updatingFeeStructure.gasUpdateCost = (BigInteger)gasUpdateCost;
      if (walletPoolHash != null)
      {
        ValidateScriptHash(walletPoolHash);
        updatingFeeStructure.walletPoolHash = walletPoolHash;
      }
      FeeStructureStorage.Put(updatingFeeStructure);
    }
  }
}
