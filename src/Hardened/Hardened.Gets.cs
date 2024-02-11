using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using static Hardened.Helpers;
using System.Numerics;

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

    [Safe]
    public static List<Map<string, object>> PendingInfusion(BigInteger skipCount, BigInteger pageSize, UInt160[] walletHashesList)
    {
      if (IsAdmin())
      {
        if (walletHashesList == null || walletHashesList.Length == 0)
        {
          return PendingStorage.ListAll(skipCount, pageSize);
        }
        else
        {
          return PendingStorage.ListByWallets(walletHashesList, skipCount, pageSize);
        }
      }
      else
      {
        UInt160 userWallet = ((Transaction)Runtime.ScriptContainer).Sender;
        return PendingStorage.ListByWallet(userWallet, skipCount, pageSize);
      }
    }

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
      map["state"] = token.state;
      map["slot1NftHash"] = token.slot1NftHash;
      map["slot1NftId"] = token.slot1NftId;
      map["slot2NftHash"] = token.slot2NftHash;
      map["slot2NftId"] = token.slot2NftId;
      map["slot3NftHash"] = token.slot3NftHash;
      map["slot3NftId"] = token.slot3NftId;
      map["slot4NftHash"] = token.slot4NftHash;
      map["slot4NftId"] = token.slot4NftId;
      map["meta"] = token.meta;
      map["attributes"] = token.attributes;
      return map;
    }

    [Safe]
    private static string GetBlueprintImageUrl()
    {
      string url = (string)Storage.Get(Storage.CurrentContext, Prefix_Blueprint_Image_Url);
      return url == null ? "/blueprint/" : url;
    }
  }
}