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
          return true;
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

    public static List<UInt160> GetAdmin()
    {
      CheckContractAuthorization();
      return AdminHashesStorage.List();
    }

    public static void DeleteAdmin(UInt160 contractHash)
    {
      CheckContractAuthorization();
      AdminHashesStorage.Delete(contractHash);
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
      CheckContractAuthorization();
      PendingObject pending = PendingStorage.Get(clientPubKey, contractPubKey);
      // Pending object existing, check for matched pay token hash and amount
      Assert(pending.payTokenHash == payTokenHash && pending.payTokenAmount == payTokenAmount, E_06);
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
        slot1NftHash = pending.slot1NftHash,
        slot1NftId = pending.slot1NftId!,
        slot2NftHash = pending.slot2NftHash,
        slot2NftId = pending.slot2NftId!,
        slot3NftHash = pending.slot3NftHash,
        slot3NftId = pending.slot3NftId!,
        slot4NftHash = pending.slot4NftHash,
        slot4NftId = pending.slot4NftId!,
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
      PendingStorage.Delete(clientPubKey, contractPubKey);
    }
    public static void InfusionUpdate(string clientPubKey, string contractPubKey, UInt160 userWalletHash, UInt160 payTokenHash, BigInteger payTokenAmount, string base58Properties)
    {
      CheckContractAuthorization();
      PendingObject pending = PendingStorage.Get(clientPubKey, contractPubKey);
      // Pending object existing, check for matched user wallet hash
      Assert(pending.userWalletHash == userWalletHash, E_07);

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
      nftState.slot1NftHash = pending.slot1NftHash;
      nftState.slot1NftId = pending.slot1NftId;
      nftState.slot2NftHash = pending.slot2NftHash;
      nftState.slot2NftId = pending.slot2NftId;
      nftState.slot3NftHash = pending.slot3NftHash;
      nftState.slot3NftId = pending.slot3NftId;
      nftState.slot4NftHash = pending.slot4NftHash;
      nftState.slot4NftId = pending.slot4NftId;
      nftState.meta = (Map<string, object>)map["Meta"];
      nftState.attributes = (Map<string, object>)map["Attributes"];

      UpdateState(nftState.Name, nftState); // NFT Name and ID are identical
      PendingStorage.Delete(clientPubKey, contractPubKey);

      // Return BH NFT to owner
      Safe11Transfer(Runtime.ExecutingScriptHash, userWalletHash, bhNftId);
    }
    private static int GetLevel(PendingObject pending)
    {
      int level = 0;
      if (pending.slot1NftId != null) level += 1;
      if (pending.slot2NftId != null) level += 1;
      if (pending.slot3NftId != null) level += 1;
      if (pending.slot4NftId != null) level += 1;
      return level;
    }
  }
}

