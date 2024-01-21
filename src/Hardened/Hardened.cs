﻿using Neo;
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
      // Transfer NFTs to lock
      Assert(!IsEmpty(slot1NftId) || !IsEmpty(slot2NftId) || !IsEmpty(slot3NftId) || !IsEmpty(slot4NftId), E_02);
      if (!IsEmpty(slot1NftId)) Safe11Transfer(slot1NftHash, bhContractHash, slot1NftId);
      if (!IsEmpty(slot2NftId)) Safe11Transfer(slot2NftHash, bhContractHash, slot2NftId);
      if (!IsEmpty(slot3NftId)) Safe11Transfer(slot3NftHash, bhContractHash, slot3NftId);
      if (!IsEmpty(slot4NftId)) Safe11Transfer(slot4NftHash, bhContractHash, slot4NftId);

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
      if (!IsEmpty(pending.slot1NftId)) Safe11Transfer(pending.slot1NftHash, pending.userWalletHash, pending.slot1NftId); // Slot 1 NFT
      if (!IsEmpty(pending.slot2NftId)) Safe11Transfer(pending.slot2NftHash, pending.userWalletHash, pending.slot2NftId); // Slot 2 NFT
      if (!IsEmpty(pending.slot3NftId)) Safe11Transfer(pending.slot3NftHash, pending.userWalletHash, pending.slot3NftId); // Slot 3 NFT
      if (!IsEmpty(pending.slot4NftId)) Safe11Transfer(pending.slot4NftHash, pending.userWalletHash, pending.slot4NftId); // Slot 4 NFT
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
