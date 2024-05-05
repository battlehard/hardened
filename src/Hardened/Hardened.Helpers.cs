using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System.Numerics;
using static Hardened.Helpers;
using static Hardened.Transfer;

namespace Hardened
{
  public partial class Hardened
  {
    private static HardenedState ValidateHardenedOwnership(string bhNftId)
    {
      HardenedState nftState = GetState(bhNftId);
      Assert(Runtime.CheckWitness(nftState.Owner), E_04);
      return nftState;
    }
    private static void ValidateExternalNftOwnership(UInt160 contractHash, string nftId)
    {
      UInt160 nftOwner = (UInt160)((Map<string, object>)Contract.Call(contractHash, "properties", CallFlags.All, new object[] { nftId }))["owner"];
      Assert(Runtime.CheckWitness(nftOwner), E_04);
    }
    private static void UpdateState(string bhNftId, HardenedState nftState)
    {
      StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
      tokenMap.Put(bhNftId, StdLib.Serialize(nftState));
    }

    private static void ReturnLockNFTs(HardenedState nftState)
    {
      for (int i = 0; i < nftState.slotNftIds.Length; i++)
      {
        if (!IsEmpty(nftState.slotNftIds[i])) Safe11Transfer(nftState.slotNftHashes[i], nftState.Owner, nftState.slotNftIds[i]);
      }
    }

    private static void CheckUpdateNft(string[] existingValues, string[] updateValues)
    {
      Assert(existingValues.Length + updateValues.Length <= MAX_SLOTS, E_09);

      // Duplication check
      foreach (string existingVal in existingValues)
      {
        if (IsEmpty(existingVal)) continue; // Skip empty slots

        bool isDuplicate = false;
        foreach (string newValue in updateValues)
        {
          if (existingVal == newValue)
          {
            isDuplicate = true;
          }
        }
        Assert(!isDuplicate, E_10);
      }
    }

    private static bool ContainsValue(List<string> list, string value)
    {
      foreach (string item in list)
      {
        if (item == value)
        {
          return true;
        }
      }
      return false;
    }

    private static void CheckReEntrancy()
    {
      // Debug always default as false.
      // Will be change to true in the debug mode to skip ReEntrancy Check where same methods call multiples time in a transaction.
      if ((BigInteger)Storage.Get(Storage.CurrentContext, Prefix_Debug) == 0)
      {
        Assert(Runtime.InvocationCounter == 1, "Re-Entrancy Not Allowed");
      }
    }
  }
}
