
using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System.Numerics;
using static Hardened.Helpers;

namespace Hardened
{
  partial class Hardened
  {
    private static readonly byte[] Prefix_Owner = new byte[] { 0x01, 0x00 };
    private static readonly byte[] Prefix_Admin_Hashes = new byte[] { 0x01, 0x01 };
    private static readonly byte[] Prefix_Pending = new byte[] { 0x01, 0x02 };
    private static readonly byte[] Prefix_Wallet_Filter = new byte[] { 0x01, 0x03 };
    private static readonly byte[] Prefix_Fee_Structure = new byte[] { 0x02, 0x00 };

    /// <summary>
    /// Class <c>AdminHashesStorage</c>
    /// Storage of address hash that can perform admin tasks.
    /// </summary>
    public static class AdminHashesStorage
    {
      internal static void Put(UInt160 addressHash)
      {
        StorageMap adminHashesMap = new(Storage.CurrentContext, Prefix_Admin_Hashes);
        adminHashesMap.Put(addressHash, 1);
      }

      internal static void Delete(UInt160 addressHash)
      {
        StorageMap adminHashesMap = new(Storage.CurrentContext, Prefix_Admin_Hashes);
        adminHashesMap.Delete(addressHash);
      }

      internal static List<UInt160> List()
      {
        StorageMap adminHashesMap = new(Storage.CurrentContext, Prefix_Admin_Hashes);
        Iterator addressHashes = adminHashesMap.Find(FindOptions.KeysOnly | FindOptions.RemovePrefix);
        List<UInt160> addressHashesList = new List<UInt160>();
        while (addressHashes.Next())
        {
          addressHashesList.Add((UInt160)addressHashes.Value);
        }
        return addressHashesList;
      }

      internal static bool IsExist(UInt160 addressHash)
      {
        StorageMap adminHashesMap = new(Storage.CurrentContext, Prefix_Admin_Hashes);
        if (adminHashesMap.Get(addressHash) != null) return true;
        else return false;
      }
    }

    public static class FeeStructureStorage
    {
      internal static void Put(FeeStructure feeStructure)
      {
        Storage.Put(Storage.CurrentContext, Prefix_Fee_Structure, StdLib.Serialize(feeStructure));
      }

      internal static FeeStructure Get()
      {
        ByteString feeStructureStorage = Storage.Get(Storage.CurrentContext, Prefix_Fee_Structure);
        if (feeStructureStorage == null)
        {
          FeeStructure feeStructure = new FeeStructure
          {
            bTokenMintCost = defaultBTokenMintCost,
            bTokenUpdateCost = defaultBTokenUpdateCost,
            gasMintCost = defaultGasMintCost,
            gasUpdateCost = defaultGasUpdateCost,
            walletPoolHash = Runtime.ExecutingScriptHash // If no pool set, it will default as this contract address.
          };
          return feeStructure;
        }
        else
        {
          return (FeeStructure)StdLib.Deserialize(feeStructureStorage);
        }
      }
    }

    public static class PendingStorage
    {
      private static StorageMap pendingMap = new StorageMap(Storage.CurrentContext, Prefix_Pending);
      private static StorageMap walletFilterMap = new StorageMap(Storage.CurrentContext, Prefix_Wallet_Filter);
      internal static void Put(PendingObject pendingObject)
      {
        byte[] key = Helper.Concat(pendingObject.clientPubKey.ToByteArray(), pendingObject.contractPubKey.ToByteArray());
        pendingMap.Put(key, StdLib.Serialize(pendingObject));
        // Write data for wallet filter
        byte[] walletFilterKey = Helper.Concat(ShortenUserWalletHash(pendingObject.userWalletHash), key);
        walletFilterMap.Put(walletFilterKey, 1);
      }
      internal static PendingObject Get(string clientPubKey, string contractPubKey)
      {
        byte[] key = Helper.Concat(clientPubKey.ToByteArray(), contractPubKey.ToByteArray());
        return Get(key);
      }
      internal static PendingObject Get(byte[] compositeKey)
      {
        ByteString pendingObject = pendingMap.Get(compositeKey);
        Assert(pendingObject != null, "ERROR: Provided clientPubKey and contractPubkey not existed");
        return (PendingObject)StdLib.Deserialize(pendingObject);
      }
      internal static void Delete(string clientPubKey, string contractPubKey)
      {
        PendingObject pendingObject = Get(clientPubKey, contractPubKey);
        byte[] key = Helper.Concat(clientPubKey.ToByteArray(), contractPubKey.ToByteArray());
        pendingMap.Delete(key);
        // Delete data from wallet filter
        byte[] walletFilterKey = Helper.Concat(ShortenUserWalletHash(pendingObject.userWalletHash), key);
        walletFilterMap.Delete(walletFilterKey);
      }
      internal static List<Map<string, object>> ListAll(BigInteger skipCount, BigInteger pageSize)
      {
        Iterator keys = pendingMap.Find(FindOptions.KeysOnly | FindOptions.RemovePrefix);
        return GetListByKeysIterator(skipCount, pageSize, keys);
      }
      internal static List<Map<string, object>> ListByWallet(UInt160 userWalletHash, BigInteger skipCount, BigInteger pageSize)
      {
        Iterator keys = walletFilterMap.Find(ShortenUserWalletHash(userWalletHash), FindOptions.KeysOnly | FindOptions.RemovePrefix);
        return GetListByKeysIterator(skipCount, pageSize, keys);
      }
      private static List<Map<string, object>> GetListByKeysIterator(BigInteger skipCount, BigInteger pageSize, Iterator keys)
      {
        List<Map<string, object>> returnListData = new();
        BigInteger foundKeySeq = 0;
        while (keys.Next())
        {
          if (foundKeySeq >= skipCount && foundKeySeq < (skipCount + pageSize))
          {
            byte[] key = (byte[])keys.Value;
            PendingObject pendingObject = Get(key);
            Map<string, object> pendingMapData = new();
            pendingMapData["clientPubKey"] = pendingObject.clientPubKey;
            pendingMapData["contractPubKey"] = pendingObject.contractPubKey;
            pendingMapData["userWalletAddress"] = pendingObject.userWalletHash.ToAddress();
            pendingMapData["bhNftId"] = pendingObject.bhNftId;
            pendingMapData["slot1NftHash"] = pendingObject.slot1NftHash;
            pendingMapData["slot1NftId"] = pendingObject.slot1NftId;
            pendingMapData["slot2NftHash"] = pendingObject.slot2NftHash;
            pendingMapData["slot2NftId"] = pendingObject.slot2NftId;
            pendingMapData["slot3NftHash"] = pendingObject.slot3NftHash;
            pendingMapData["slot3NftId"] = pendingObject.slot3NftId;
            pendingMapData["slot4NftHash"] = pendingObject.slot4NftHash;
            pendingMapData["slot4NftId"] = pendingObject.slot4NftId;
            returnListData.Add(pendingMapData);
          }
          if (returnListData.Count >= pageSize)
            break;
          foundKeySeq++;
        }
        return returnListData;
      }
      private static byte[] ShortenUserWalletHash(UInt160 userWalletHash)
      {
        return ((byte[])userWalletHash).Range(0, 10);
      }
    }
  }
}