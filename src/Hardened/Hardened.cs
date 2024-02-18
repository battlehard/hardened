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
                                  UInt160[] slotNftHashes, string[] slotNftIds)
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
      UInt160 bhContractHash = Runtime.ExecutingScriptHash; // This contract address

      if (!IsEmpty(bhNftId)) // Update
      {
        isMintRequest = false;
        Safe11Transfer(bhContractHash, bhContractHash, bhNftId); // transfer to lock
      }
      Safe17Transfer(payTokenHash, userWalletHash, bhContractHash, payTokenAmount);
      // Transfer gas with amount set by admin
      BigInteger gasAmount = feeStructure.gasMintCost;
      if (!isMintRequest)
        gasAmount = feeStructure.gasUpdateCost;
      Safe17Transfer(GAS.Hash, userWalletHash, bhContractHash, gasAmount);
      // Validate NFT slots
      Assert(slotNftIds != null && slotNftIds.Length > 0 && slotNftIds.Length <= MAX_SLOTS, E_02);
      bool isNftProvided = false;
      for (int i = 0; i < slotNftIds!.Length; i++)
      {
        if (!IsEmpty(slotNftIds[i]))
        {
          isNftProvided = true;
          break;
        }
      }
      Assert(isNftProvided, E_02);

      // Validate provided NFT only for Update case
      if (!isMintRequest)
      {
        HardenedState nft = GetState(bhNftId);
        Assert(nft.state == State.Ready || nft.state == State.Blueprint, E_08);

        int[] updatingIndices = GetUpdateValuesIndex(nft.slotNftIds, slotNftIds);
        if (nft.state == State.Ready)
        {
          // If BH NFT is READY, transfer only new NFT.
          for (int i = 0; i < updatingIndices.Length; i++)
          {
            Safe11Transfer(slotNftHashes[updatingIndices[i]], bhContractHash, slotNftIds[updatingIndices[i]]);
          }
        }
        else
        {
          // If BH NFT is BLUEPRINT, allow transfer NFT pieces up to level.
          Assert(updatingIndices.Length <= nft.level, E_13);
          for (int i = 0; i < updatingIndices.Length; i++)
          {
            Safe11Transfer(slotNftHashes[updatingIndices[i]], bhContractHash, slotNftIds[updatingIndices[i]]);
          }
        }
      }
      else
      {
        for (int i = 0; i < slotNftIds.Length; i++)
        {
          if (!IsEmpty(slotNftIds[i])) Safe11Transfer(slotNftHashes[i], bhContractHash, slotNftIds[i]);
        }
      }

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
        slotNftHashes = slotNftHashes,
        slotNftIds = slotNftIds
      };
      PendingStorage.Put(pending);

      // return clientPubKey and ContractPubKey
      return new string[2] { clientPubKey, contractPubKey };
    }

    public static void CancelInfusion(string clientPubKey, string contractPubKey)
    {
      long transactionFee = Runtime.GasLeft;
      bool isAdmin = true;
      // get pending storage
      PendingObject pending = PendingStorage.Get(clientPubKey, contractPubKey);
      if (!IsAdmin())
      {
        isAdmin = false;
        Assert(Runtime.CheckWitness(pending.userWalletHash), E_03);
      }
      // Destination of locking tokens and NFTs.
      UInt160 bhContrachHash = Runtime.ExecutingScriptHash; // This contract address
      // Returning of locked assets.
      if (!IsEmpty(pending.bhNftId)) Safe11Transfer(bhContrachHash, pending.userWalletHash, pending.bhNftId); // BH NFT
      for (int i = 0; i < pending.slotNftIds.Length; i++)
      {
        if (!IsEmpty(pending.slotNftIds[i])) Safe11Transfer(pending.slotNftHashes[i], pending.userWalletHash, pending.slotNftIds[i]);
      }
      Safe17Transfer(pending.payTokenHash, bhContrachHash, pending.userWalletHash, pending.payTokenAmount); // Pay Token
      BigInteger refundGas = pending.gasAmount; // Prepare full refund amount

      // Delete pending storage
      PendingStorage.Delete(clientPubKey, contractPubKey);

      // Refund GAS 
      if (isAdmin)
      {
        refundGas -= transactionFee;
        // Case admin invoke this tx, refun partial (full gas - this tx gas fee) GAS.
        if (refundGas > 0)
          Safe17Transfer(GAS.Hash, bhContrachHash, pending.userWalletHash, refundGas);
      }
      else
      {
        // Case user invoke this tx, refund full GAS.
        Safe17Transfer(GAS.Hash, bhContrachHash, pending.userWalletHash, refundGas);
      }
    }

    public static void Unfuse(string bhNftId)
    {
      HardenedState nftState = ValidateOwnership(bhNftId);
      Assert(nftState.state == State.Ready, E_05);
      ReturnLockNFTs(nftState);

      nftState.state = State.Blueprint;
      nftState.image = GetBlueprintImageUrl();
      nftState.meta.Remove("Sync");
      nftState.attributes.Remove("Nature");
      UpdateState(bhNftId, nftState);
    }

    public static void BurnInfusion(string bhNftId)
    {
      HardenedState nftState = ValidateOwnership(bhNftId);
      // If ready return locked NFTs
      if (nftState.state == State.Ready)
      {
        ReturnLockNFTs(nftState);
      }
      Burn(bhNftId);
    }

    public static void OnNEP11Payment(UInt160 from, BigInteger amount, ByteString tokenId, object[] data)
    {
    }

    public static void OnNEP17Payment(UInt160 from, BigInteger amount, object data)
    {
    }
  }
}
