using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;

namespace Hardened
{
  public partial class Hardened
  {
    [Safe]
    public override string Symbol() => "BH";

    [Safe]
    public static UInt160 GetContractOwner()
    {
      return (UInt160)Storage.Get(Storage.CurrentContext, Prefix_Owner);
    }

    // [Safe]
    // public static List<object> PendingInfusion(UInt160[] walletHashesList)
    // {
    //   if (IsAdmin())
    //   {
    //     if (walletHashesList == null || walletHashesList.Length == 0)
    //     {
    //       // return all
    //     }
    //     else
    //     {
    //       // return filtered
    //     }
    //   }
    //   else
    //   {
    //     // return only user wallet data
    //   }
    // }
  }
}