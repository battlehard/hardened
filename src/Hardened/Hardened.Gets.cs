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
      Assert(tokenMap[tokenId] != null, $"ERROR: token not found id: {tokenId}");

      return (HardenedState)StdLib.Deserialize(tokenMap[tokenId]);
    }

    [Safe]
    public override Map<string, object> Properties(ByteString tokenId) // Overriding, so leave ByteString as-is
    {
      HardenedState token = GetState(tokenId);
      Map<string, object> map = new();
      map["ownerAddress"] = token.Owner.ToAddress();
      map["name"] = tokenId;
      map["state"] = token.State;
      map["slot1NftHash"] = token.Slot1NftHash;
      map["slot1NftId"] = token.Slot1NftId;
      map["slot2NftHash"] = token.Slot2NftHash;
      map["slot2NftId"] = token.Slot2NftId;
      map["slot3NftHash"] = token.Slot3NftHash;
      map["slot3NftId"] = token.Slot3NftId;
      map["slot4NftHash"] = token.Slot4NftHash;
      map["slot4NftId"] = token.Slot4NftId;
      map["meta"] = token.Meta;
      map["attributes"] = token.Attributes;
      return map;
    }
  }
}