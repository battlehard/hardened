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
    public static List<UInt160> GetAdmin()
    {
      return AdminHashesStorage.List();
    }

    [Safe]
    public static Map<string, object> PendingInfusion(BigInteger pageNumber, BigInteger pageSize, UInt160[] walletHashesList)
    {
      Assert(pageNumber > 0 && pageSize > 0, E_13);
      Assert(pageSize <= MAX_PAGE_LIMIT, E_14);

      BigInteger totalPending;
      if (walletHashesList == null || walletHashesList.Length == 0)
      {
        totalPending = PendingStorage.Count(new UInt160[] { });
      }
      else
      {
        totalPending = PendingStorage.Count(walletHashesList);
      }

      if (totalPending == 0) // Not found
      {
        return BuildReturnPendingInfusionObject(0, 0, new List<Map<string, object>>());
      }

      // Calculate the total number of pages based on the total trades and page size
      BigInteger totalPages = totalPending / pageSize;
      if (totalPending % pageSize > 0)
      {
        totalPages += 1;
      }
      Assert(pageNumber <= totalPages, E_15);

      // Calculate the number of items to skip based on the requested page and page size
      BigInteger skipCount = (pageNumber - 1) * pageSize;

      List<Map<string, object>> pendingList;
      if (walletHashesList == null || walletHashesList.Length == 0)
      {
        pendingList = PendingStorage.ListAll(skipCount, pageSize);
      }
      else
      {
        pendingList = PendingStorage.ListByWallets(walletHashesList, skipCount, pageSize);
      }

      return BuildReturnPendingInfusionObject(totalPending, totalPages, pendingList);
    }

    private static Map<string, object> BuildReturnPendingInfusionObject(BigInteger totalPending, BigInteger totalPages, List<Map<string, object>> pendingList)
    {
      // Initialize return variable
      Map<string, object> pendingPaginationData = new();
      pendingPaginationData["totalPages"] = totalPages;
      pendingPaginationData["totalPending"] = totalPending;
      pendingPaginationData["pendingList"] = pendingList; // Get list of active trades with pagination parameters
      return pendingPaginationData;
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
      //map["ownerAddress"] = token.Owner.ToAddress(); changed back to Hash format
      map["owner"] = token.Owner;
      map["name"] = tokenId;
      map["state"] = token.state;
      map["slotNftHashes"] = token.slotNftHashes;
      map["slotNftIds"] = token.slotNftIds;
      map["meta"] = token.meta;
      map["attributes"] = token.attributes;
      return map;
    }

    [Safe]
    public static string GetBlueprintImageUrl()
    {
      string url = (string)Storage.Get(Storage.CurrentContext, Prefix_Blueprint_Image_Url);
      return url == null ? "/blueprint/" : url;
    }
  }
}