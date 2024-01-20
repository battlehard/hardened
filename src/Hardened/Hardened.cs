using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System.Numerics;
using static Hardened.Helpers;
using static Hardened.Transfer;

namespace Hardened
{
  [ManifestExtra("Author", "BATTLE STUDIO")]
  [ManifestExtra("Email", "admin@battlehard.studio")]
  [ManifestExtra("Description", "A soft locker system to support wrapping NFTs with battle traits.")]
  [SupportedStandards("NEP-11")]
  [ContractPermission("*", "*")]
  public partial class Hardened : Nep11Token<HardenedState>
  {
    public static string[] PreInfusion(string clientPubKey, UInt160 payTokenHash, BigInteger payTokenAmount, string bhNftId,
                                  UInt160 slot1NftHash, string slot1NftId, UInt160 slot2NftHash, string slot2NftId,
                                  UInt160 slot3NftHash, string slot3NftId, UInt160 slot4NftHash, string slot4NftId)
    {
      // Get user wallet information
      UInt160 userWalletHash = ((Transaction)Runtime.ScriptContainer).Sender;
      string userWalletAddress = userWalletHash.ToAddress();

      // Validate clientPubKey agaist user wallet address
      // Simple string comparison like a == b, or a.Equals(b) yield False result. Need to compare by char instead
      bool isLast4DigitsMatch = true;
      for (int i = 0; i < 4; i++)
      {
        if (clientPubKey.Substring(clientPubKey.Length - 4)[i] != userWalletAddress.Substring(userWalletAddress.Length - 4)[i])
        {
          isLast4DigitsMatch = false;
          break;
        }
      }
      Assert(isLast4DigitsMatch, E_01);

      bool isMintRequest = true;
      FeeStructure feeStructure = FeeStructureStorage.Get();
      // Destination of locking tokens and NFTs.
      UInt160 lockedDestinationHash = Runtime.ExecutingScriptHash; // This contract address

      if (!IsEmpty(bhNftId)) // Update
      {
        isMintRequest = false;
        HardenedState bhNftState = GetState(bhNftId);
        Assert(bhNftState.Owner == userWalletHash, $"Error: No ownership on {bhNftId}");
        Safe11Transfer(Runtime.ExecutingScriptHash, lockedDestinationHash, bhNftId); // transfer to lock
      }
      Safe17Transfer(payTokenHash, userWalletHash, lockedDestinationHash, payTokenAmount);
      // Transfer gas with amount set by admin
      BigInteger gasAmount = feeStructure.gasMintCost;
      if (!isMintRequest)
        gasAmount = feeStructure.gasUpdateCost;
      Safe17Transfer(GAS.Hash, userWalletHash, lockedDestinationHash, gasAmount);
      // Transfer NFTs to lock
      Assert(!IsEmpty(slot1NftId) || !IsEmpty(slot2NftId) || !IsEmpty(slot3NftId) || !IsEmpty(slot4NftId), E_02);
      if (!IsEmpty(slot1NftId)) Safe11Transfer(slot1NftHash, lockedDestinationHash, slot1NftId);
      if (!IsEmpty(slot2NftId)) Safe11Transfer(slot2NftHash, lockedDestinationHash, slot2NftId);
      if (!IsEmpty(slot3NftId)) Safe11Transfer(slot3NftHash, lockedDestinationHash, slot3NftId);
      if (!IsEmpty(slot4NftId)) Safe11Transfer(slot4NftHash, lockedDestinationHash, slot4NftId);

      // Generate contractPubKey (PubKey2)
      string contractPubKey = GenerateIdBase64(16);

      // Write pending storage
      PendingObject pending = new PendingObject()
      {
        clientPubKey = clientPubKey,
        contractPubKey = contractPubKey,
        userWalletHash = userWalletHash,
        payTokenHash = payTokenHash,
        payTokenAmount = payTokenAmount,
        gasAmount = gasAmount,
        bhNftId = bhNftId,
        slot1NftHash = slot1NftHash,
        slot1NftId = slot1NftId,
        slot2NftHash = slot2NftHash,
        slot2NftId = slot2NftId,
        slot3NftHash = slot3NftHash,
        slot3NftId = slot3NftId,
        slot4NftHash = slot4NftHash,
        slot4NftId = slot4NftId
      };
      PendingStorage.Put(pending);

      // return clientPubKey and ContractPubKey
      return new string[2] { clientPubKey, contractPubKey };
    }

    public static void CancelInfusion(string clientPubKey, string contractPubKey)
    {
      // get pending storage
      if (!IsAdmin())
      {
        // validate the ownership of this pending storage
      }
      // delete pending storage

      // send back nft
      // send back pay token
      // if user invoke this tx, send back full gas
      // if admin invoke this tx, send back (full gas - this tx gas fee)
    }

    public static void Unfuse(string bhNftId)
    {
      // validate bh nft ownership
      // get BH NFT information
      // validate BH NFT status, it must ready

      // transfer all locked NFT back
      // update BH NFT
      //  - status to blueprint
      //  - image to blueprint
      //  - strip stat in properties Meta.sync, Attributes.nature
    }

    public static void BurnInfusion(string bhNftId)
    {
      // validate nft ownership
      // get BH NFT information
      // validate BH NFT status whether it is ready or not. 

      // if ready, transfer all locked NFT back
      // burn BH NFT
    }

    public static void OnNEP11Payment(UInt160 from, BigInteger amount, ByteString tokenId, object[] data)
    {
    }

    public static void OnNEP17Payment(UInt160 from, BigInteger amount, object data)
    {
    }
  }
}
