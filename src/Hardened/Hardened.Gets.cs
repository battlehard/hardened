using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using static Hardened.Helpers;

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

    private static HardenedState GetState(string tokenId)
    {
      StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
      Assert(tokenMap[tokenId] != null, $"Error: token not found id: {tokenId}");

      return (HardenedState)StdLib.Deserialize(tokenMap[tokenId]);
    }
  }
}