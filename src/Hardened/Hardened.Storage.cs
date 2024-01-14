
using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace Hardened
{
  partial class Hardened
  {
    private static readonly byte[] Prefix_Owner = new byte[] { 0x01, 0x00 };
    private static readonly byte[] Prefix_Admin_Hashes = new byte[] { 0x01, 0x01 };
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
      // TODO:
      // get by wallet id
      // get all
    }
  }
}