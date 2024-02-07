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
      if (!IsEmpty(nftState.slot1NftId)) Safe11Transfer(nftState.slot1NftHash, nftState.Owner, nftState.slot1NftId); // Slot 1 NFT
      if (!IsEmpty(nftState.slot2NftId)) Safe11Transfer(nftState.slot2NftHash, nftState.Owner, nftState.slot2NftId); // Slot 2 NFT
      if (!IsEmpty(nftState.slot3NftId)) Safe11Transfer(nftState.slot3NftHash, nftState.Owner, nftState.slot3NftId); // Slot 3 NFT
      if (!IsEmpty(nftState.slot4NftId)) Safe11Transfer(nftState.slot4NftHash, nftState.Owner, nftState.slot4NftId); // Slot 4 NFT
    }
  }
}
