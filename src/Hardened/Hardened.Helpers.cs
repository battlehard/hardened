using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using static Hardened.Helpers;
using static Hardened.Transfer;

namespace Hardened
{
  public partial class Hardened
  {
    private static HardenedState ValidateOwnership(string bhNftId)
    {
      HardenedState nftState = GetState(bhNftId);
      Assert(Runtime.CheckWitness(nftState.Owner), E_04);
      return nftState;
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

    private static int[] GetUpdateValuesIndex(string[] existingValues, string[] updateValues)
    {
      // Length check
      Assert(existingValues.Length == updateValues.Length, E_09);

      // Gather non empty values from existing.
      List<string> nonEmptyExistingValuesSet = new List<string>();
      for (int i = 0; i < existingValues.Length; i++)
      {
        if (!IsEmpty(existingValues[i]))
          nonEmptyExistingValuesSet.Add(existingValues[i]);
      }

      // Subset check
      foreach (string existingVal in existingValues)
      {
        if (IsEmpty(existingVal)) continue; // Skip empty slots

        bool isSubset = false;
        foreach (string newValue in updateValues)
        {
          if (existingVal == newValue)
          {
            isSubset = true;
          }
        }
        Assert(isSubset, E_10);
      }

      // Find new values and their indices
      List<int> newIndices = new List<int>();
      List<string> seenValues = new List<string>();

      for (int i = 0; i < updateValues.Length; i++)
      {
        string value = updateValues[i];
        if (value != null && !ContainsValue(nonEmptyExistingValuesSet, value))
        {
          Assert(!ContainsValue(seenValues, value), E_11);
          newIndices.Add(i);
          seenValues.Add(value);
        }
      }
      Assert(newIndices.Count > 0, E_12);
      return newIndices;
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
  }
}
