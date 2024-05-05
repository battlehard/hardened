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
    private static void CheckContractAuthorization()
    {
      Assert(IsAdmin(), $"{CONTRACT_NAME}: No admin authorization");
    }

    private static bool IsAdmin()
    {
      if (IsOwner())
      {
        return true;
      }
      else
      {
        // tx.Sender is transaction signer
        var tx = (Transaction)Runtime.ScriptContainer;
        if (AdminHashesStorage.IsExist(tx.Sender))
        {
          return Runtime.CheckWitness(tx.Sender);
        }
        else
        {
          return false;
        }
      }
    }

    public static void SetAdmin(UInt160 contractHash)
    {
      CheckContractAuthorization();
      AdminHashesStorage.Put(contractHash);
    }

    public static void DeleteAdmin(UInt160 contractHash)
    {
      CheckContractAuthorization();
      AdminHashesStorage.Delete(contractHash);
    }

    public static void SetBlueprintImageUrl(string url)
    {
      CheckContractAuthorization();
      Storage.Put(Storage.CurrentContext, Prefix_Blueprint_Image_Url, url);
    }

    public static void FeeUpdate(BigInteger? bTokenMintCost, BigInteger? bTokenUpdateCost,
                                 BigInteger? gasMintCost, BigInteger? gasUpdateCost, UInt160? walletPoolHash)
    {
      CheckContractAuthorization();
      FeeStructure updatingFeeStructure = FeeStructureStorage.Get();
      if (bTokenMintCost != null) updatingFeeStructure.bTokenMintCost = (BigInteger)bTokenMintCost;
      if (bTokenUpdateCost != null) updatingFeeStructure.bTokenUpdateCost = (BigInteger)bTokenUpdateCost;
      if (gasMintCost != null) updatingFeeStructure.gasMintCost = (BigInteger)gasMintCost;
      if (gasUpdateCost != null) updatingFeeStructure.gasUpdateCost = (BigInteger)gasUpdateCost;
      if (walletPoolHash != null)
      {
        ValidateScriptHash(walletPoolHash);
        updatingFeeStructure.walletPoolHash = walletPoolHash;
      }
      FeeStructureStorage.Put(updatingFeeStructure);
    }
    public static void InfusionMint(string clientPubKey, string contractPubKey, UInt160 contractHash, UInt160 payTokenHash, BigInteger payTokenAmount, string base58Properties)
    {
      CheckReEntrancy();
      CheckContractAuthorization();
      PendingObject pending = PendingStorage.Get(clientPubKey, contractPubKey);
      Assert(IsEmpty(pending.bhNftId), E_16);
      // Pending object existing, check for matched pay token hash and amount
      Assert(pending.payTokenHash == payTokenHash && pending.payTokenAmount == payTokenAmount, E_06);
      PendingStorage.Delete(clientPubKey, contractPubKey); // Modify state before external call to prevent re-entrancy attack.
      // Transfer pay token from BH contract to provide contract hash
      Safe17Transfer(pending.payTokenHash, Runtime.ExecutingScriptHash, contractHash, pending.payTokenAmount);
      // Transfer Gas from BH contract to wallet pool
      Safe17Transfer(GAS.Hash, Runtime.ExecutingScriptHash, FeeStructureStorage.Get().walletPoolHash!, pending.gasAmount);

      string jsonString = StdLib.Base58Decode(base58Properties);
      Map<string, object> map = (Map<string, object>)StdLib.JsonDeserialize(jsonString);
      List<UInt160> contractList = new List<UInt160>();
      string[] contracts = (string[])map["Contract"];
      for (int i = 0; i < contracts.Length; i++)
      {
        contractList.Add(contracts[i].HexStringToUInt160());
      }
      int level = GetLevel(pending);
      HardenedState mintingNft = new HardenedState()
      {
        Owner = pending.userWalletHash,
        Name = (string)map["Name"],
        image = (string)map["Image"],
        state = State.Ready, // Minted state is always "Ready".
        level = level,
        project = ((string)map["Project"]).HexStringToUInt160(),
        contract = contractList,
        slotNftHashes = pending.slotNftHashes,
        slotNftIds = pending.slotNftIds,
        meta = (Map<string, object>)map["Meta"],
        attributes = (Map<string, object>)map["Attributes"]
      };

      // Checking unique tokenId
      string tokenId = mintingNft.Name;
      StorageMap tokenMap = new(Storage.CurrentContext, Prefix_Token);
      // If tokenId existing, update to unique one.
      if (tokenMap.Get(tokenId) != null)
      {
        mintingNft.Name = $"{mintingNft.Name}#{UniqueHeightStorage.Next()}";
      }
      Mint(mintingNft.Name, mintingNft);
    }
    public static void InfusionUpdate(string clientPubKey, string contractPubKey, UInt160 userWalletHash, UInt160 payTokenHash, BigInteger payTokenAmount, string base58Properties)
    {
      CheckReEntrancy();
      CheckContractAuthorization();
      PendingObject pending = PendingStorage.Get(clientPubKey, contractPubKey);
      // Pending object existing, check for matched user wallet hash
      Assert(pending.userWalletHash == userWalletHash, E_07);
      PendingStorage.Delete(clientPubKey, contractPubKey); // Modify state before external call to prevent re-entrancy attack.

      UInt160 walletPoolHash = FeeStructureStorage.Get().walletPoolHash!;
      // Transfer pay token from BH contract to wallet pool
      Safe17Transfer(pending.payTokenHash, Runtime.ExecutingScriptHash, walletPoolHash, pending.payTokenAmount);
      // Transfer Gas from BH contract to wallet pool
      Safe17Transfer(GAS.Hash, Runtime.ExecutingScriptHash, walletPoolHash, pending.gasAmount);

      string jsonString = StdLib.Base58Decode(base58Properties);
      Map<string, object> map = (Map<string, object>)StdLib.JsonDeserialize(jsonString);

      string bhNftId = pending.bhNftId;
      HardenedState nftState = GetState(bhNftId);
      nftState.image = (string)map["Image"];
      nftState.state = State.Ready; // Updated successfully turn state into "Ready"
      nftState.slotNftHashes = nftState.slotNftHashes.Concat(pending.slotNftHashes);
      nftState.slotNftIds = nftState.slotNftIds.Concat(pending.slotNftIds);
      nftState.meta = (Map<string, object>)map["Meta"];
      nftState.attributes = (Map<string, object>)map["Attributes"];

      UpdateState(nftState.Name, nftState); // NFT Name and ID are identical

      // Return BH NFT to owner
      Safe11Transfer(Runtime.ExecutingScriptHash, userWalletHash, bhNftId);
    }
    private static int GetLevel(PendingObject pending)
    {
      int level = 0;
      for (int i = 0; i < pending.slotNftIds.Length; i++)
      {
        if (pending.slotNftIds[i] != null) level += 1;
      }
      Assert(level <= MAX_SLOTS, E_12);
      return level;
    }
  }
}

