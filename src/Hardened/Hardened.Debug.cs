using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;
using System;
using System.Numerics;
using static Hardened.Helpers;
using static Hardened.Transfer;

#pragma warning disable CS8625 
namespace Hardened
{
  public partial class Hardened
  {
    private static UInt160 owner = "NbMFc2fEpoF68KMRoJEhxQGTJUUVA5vqoe".AddressToScriptHash();
    private static UInt160 admin1 = "Nh9XK6sZ6vkPu9N2L9GxnkogxXKVCgDMws".AddressToScriptHash();
    private static UInt160 admin2 = "NQbHe4RTkzwGD3tVYsTcHEhxWpN4ZEuMuG".AddressToScriptHash();

    public static void TestNonCore()
    {
      CheckContractAuthorization();
      EnableDebugStorage();
      Debug_Helpers();
      Debug_Transfer();
      Debug_ManageAdmin();
      Debug_FeeUpdate();
    }
    public static void TestUserOperation()
    {
      CheckContractAuthorization();
      EnableDebugStorage();
      Debug_PreInfusion_Mint();
      Debug_PendingInfusion();
    }
    public static void TestAdminOperation()
    {
      CheckContractAuthorization();
      EnableDebugStorage();
      Debug_InfusionMintAndInfusionUpdate();
      Debug_UnfuseAndBurnInfusion();
    }
    private static void Debug_Helpers()
    {
      try
      {
        GenerateIdBase64(0);
      }
      catch (Exception e)
      {
        Runtime.Notify("Expected error", new object[] { e });
      }
      Runtime.Log(GenerateIdBase64(16));
      for (int i = 0; i < 5; i++)
      {
        string uuid = GenerateUuidV4();
        ValidateUuid(uuid);
      }

      string[] existing = new string[] { "a", "b" };
      string[] updating = new string[] { "c" };
      CheckUpdateNft(existing, updating);

      existing = new string[] { "a", "b" };
      updating = new string[] { "d", "c" };
      CheckUpdateNft(existing, updating);

      existing = new string[] { "a", "b", "c" };
      updating = new string[] { "d" };
      CheckUpdateNft(existing, updating);

      existing = new string[] { "a" };
      updating = new string[] { "b", "c", "d" };
      CheckUpdateNft(existing, updating);


      existing = new string[] { "a", "b", "c" };
      updating = new string[] { "d", "e" };
      try
      {
        CheckUpdateNft(existing, updating);
      }
      catch (Exception e)
      {
        Assert(GetExceptionMessage(e) == E_09, "Expected: " + E_09);
      }

      existing = new string[] { "a", "b", "c" };
      updating = new string[] { "a" };
      try
      {
        CheckUpdateNft(existing, updating);
      }
      catch (Exception e)
      {
        Assert(GetExceptionMessage(e) == E_10, "Expected: " + E_10);
      }
    }

    private static void Debug_Transfer()
    {
      Safe17Transfer(GAS.Hash, owner, admin1, 1_00000000);
    }
    private static void Debug_ManageAdmin()
    {
      // Case 1: No admin
      Assert(GetAdmin().Count == 0, "Expected empty list");
      Runtime.Notify("adminList case 1", new object[] { GetAdmin() });

      // Case 2: Add the first admin
      SetAdmin(admin1);
      Assert(GetAdmin().Count == 1 && GetAdmin()[0] == admin1, "Expected admin1");
      Runtime.Notify("adminList case 2", new object[] { GetAdmin() });

      // Case 3: Add the second admin
      SetAdmin(admin2);
      Assert(GetAdmin().Count == 2, "Expected two admin");
      Assert(AdminHashesStorage.IsExist(admin1), "Expected admin1");
      Assert(AdminHashesStorage.IsExist(admin2), "Expected admin2");
      Runtime.Notify("adminList case 3", new object[] { GetAdmin() });

      // Case 4: Delete the first admin
      DeleteAdmin(admin1);
      Assert(AdminHashesStorage.IsExist(admin1) == false, "Expected no admin1");
      Assert(GetAdmin().Count == 1 && GetAdmin()[0] == admin2, "Expected admin2");
      Runtime.Notify("adminList case 4", new object[] { GetAdmin() });
    }
    private static void Debug_FeeUpdate()
    {
      // Case 1: Default fee
      Assert_DefaultFeeStructure();
      Runtime.Notify("Default Fee Structure: case 1", new object[] { FeeStructureStorage.Get() });
      // Case 2: Updating with null value, end with default fee
      FeeUpdate(null, null, null, null, null);
      Assert_DefaultFeeStructure();
      Runtime.Notify("Default Fee Structure: case 2", new object[] { FeeStructureStorage.Get() });

      BigInteger bTokenMintCost = 2_00000000;
      BigInteger bTokenUpdateCost = 1_00000000;
      BigInteger gasMintCost = 0_10000000;
      BigInteger gasUpdateCost = 0_05000000;
      UInt160 walletPoolHash = "Nh9XK6sZ6vkPu9N2L9GxnkogxXKVCgDMws".AddressToScriptHash();
      // Case 3: Updating with new fee structure
      FeeUpdate(bTokenMintCost, bTokenUpdateCost, gasMintCost, gasUpdateCost, walletPoolHash);
      Assert_UpdatedFeeStructure(bTokenMintCost, bTokenUpdateCost, gasMintCost, gasUpdateCost, walletPoolHash);
      Runtime.Notify("Updated Fee Structure: case 1", new object[] { FeeStructureStorage.Get() });
      // Case 4: Updating with null value, fee structure not changed.
      FeeUpdate(null, null, null, null, null);
      Assert_UpdatedFeeStructure(bTokenMintCost, bTokenUpdateCost, gasMintCost, gasUpdateCost, walletPoolHash);
      Runtime.Notify("Updated Fee Structure: case 2", new object[] { FeeStructureStorage.Get() });
    }
    private static List<string[]> Debug_PreInfusion_Mint()
    {
      List<string[]> pendingMintList = new List<string[]>();

      // Case 1: PreInfusion with unmatched last 4 digit of clientPubKey and wallet address
      try
      {
        PreInfusion(GenerateIdBase64(16), NEO.Hash, 10, null, null, null);
      }
      catch (Exception e)
      {
        Assert(GetExceptionMessage(e) == E_01, "Expected: " + E_01);
      }

      string clientPubKey = GetClientPubKey(owner.ToAddress());

      // Mint 6 NFTs for use in the mint and update scenarios
      int quantity = 6;
      for (int i = 1; i <= quantity; i++)
        Mint($"testNft_{i}", new HardenedState()
        {
          Owner = owner,
          state = State.Ready,
        });

      // Try checking NFT state
      HardenedState bhNft1 = GetState("testNft_1");
      Runtime.Notify("NFT 1 state", new object[] { bhNft1 });
      // Try checking NFT properties
      Hardened h = new Hardened();
      Map<string, object> bhNft1Properties = h.Properties("testNft_1");
      Runtime.Notify("NFT 1 properties", new object[] { bhNft1Properties });

      // Case 2: Mint with no NFTs in any slots
      string[] clientAndContractPubKey;
      try
      {
        clientAndContractPubKey = PreInfusion(clientPubKey, NEO.Hash, 10, null, null, null);
      }
      catch (Exception e)
      {
        Assert(GetExceptionMessage(e) == E_02, "Expected: " + E_02);
      }

      // Case 3: Mint with full NFT slots and one slot
      UInt160[] providingNftHashes = new UInt160[4];
      string[] providingNftIds = new string[4];
      for (int i = 0; i < 4; i++)
      {
        providingNftHashes[i] = Runtime.ExecutingScriptHash;
        providingNftIds[i] = $"testNft_{i + 1}";
      }
      clientAndContractPubKey = PreInfusion(clientPubKey, NEO.Hash, 20, null, providingNftHashes, providingNftIds);
      pendingMintList.Add(clientAndContractPubKey);
      PendingObject fullNftObject = PendingStorage.Get(clientAndContractPubKey[0], clientAndContractPubKey[1]);
      Runtime.Notify("fullNftObject", new object[] { fullNftObject });

      clientAndContractPubKey = PreInfusion(clientPubKey, NEO.Hash, 30, null, new UInt160[] { Runtime.ExecutingScriptHash }, new string[] { "testNft_5" });
      PendingObject oneNftObject = PendingStorage.Get(clientAndContractPubKey[0], clientAndContractPubKey[1]);
      Runtime.Notify("oneNftObject", new object[] { oneNftObject });

      List<Map<string, object>> pendingListAll = PendingStorage.ListAll(0, 5);
      Runtime.Notify("pendingListAll", new object[] { pendingListAll });
      Assert(pendingListAll.Count == 2, $"ERROR: Expected 2 all pending but got {pendingListAll.Count}");

      // Case 4: Get pending list with owner wallet
      List<Map<string, object>> pendingListByOwnerWallet = PendingStorage.ListByWallet(owner, 0, 5);
      Runtime.Notify("pendingListByOwnerWallet", new object[] { pendingListByOwnerWallet });
      Assert(pendingListByOwnerWallet.Count == 2, $"ERROR: Expected 2 owner pending but got {pendingListByOwnerWallet.Count}");

      // Case 5: Get pending list with admin wallet
      List<Map<string, object>> pendingListByAdmin1Wallet = PendingStorage.ListByWallet(admin1, 0, 5);
      Runtime.Notify("pendingListByAdmin1Wallet", new object[] { pendingListByAdmin1Wallet });
      Assert(pendingListByAdmin1Wallet.Count == 0, $"ERROR: Expected 0 admin pending but got {pendingListByAdmin1Wallet.Count}");

      // Case 6: Get pending list with filtered wallet
      List<Map<string, object>> pendingListByFilteredWallet = PendingStorage.ListByWallets(new UInt160[] { admin1, owner }, 0, 5);
      Runtime.Notify("pendingListByFilteredWallet", new object[] { pendingListByFilteredWallet });
      Assert(pendingListByFilteredWallet.Count == 2, $"ERROR: Expected 2 filtered pending but got {pendingListByFilteredWallet.Count}");

      // Case 7: Delete one pending
      CancelInfusion(clientAndContractPubKey[0], clientAndContractPubKey[1]); // Cancel oneNftObject infusion
      pendingListAll = PendingStorage.ListAll(0, 5);
      Runtime.Notify("pendingListAll", new object[] { pendingListAll });
      Assert(pendingListAll.Count == 1, $"ERROR: Expected 1 all pending but got {pendingListAll.Count}");

      // Preparation for InfusionMint
      clientAndContractPubKey = PreInfusion(clientPubKey, NEO.Hash, 30, null, new UInt160[] { Runtime.ExecutingScriptHash }, new string[] { "testNft_5" });
      pendingMintList.Add(clientAndContractPubKey);

      return pendingMintList;
    }

    private static string GetClientPubKey(string userWalletAddress)
    {
      // Prepare PreInfusion clientPubKey to be matched with wallet address
      string clientPubKey = GenerateIdBase64(16);
      clientPubKey = clientPubKey.Substring(0, clientPubKey.Length - 4);
      clientPubKey += userWalletAddress.Substring(userWalletAddress.Length - 4);
      return clientPubKey;
    }

    private static void Debug_PendingInfusion()
    {
      try
      {
        PendingInfusion(0, 0, null);
      }
      catch (Exception e)
      {
        Assert(GetExceptionMessage(e) == E_13, "Expected: " + E_13);
      }
      try
      {
        PendingInfusion(1, 51, null);
      }
      catch (Exception e)
      {
        Assert(GetExceptionMessage(e) == E_14, "Expected: " + E_14);
      }
      try
      {
        PendingInfusion(10, 1, null);
      }
      catch (Exception e)
      {
        Assert(GetExceptionMessage(e) == E_15, "Expected: " + E_15);
      }
      Map<string, object> pendingListPagination = PendingInfusion(1, 10, null);
      Runtime.Notify("pendingListPagination 1", new object[] { pendingListPagination["totalPages"], pendingListPagination["totalPending"], pendingListPagination["pendingList"] });
      pendingListPagination = PendingInfusion(1, 1, new UInt160[] { owner });
      Runtime.Notify("pendingListPagination 2", new object[] { pendingListPagination["totalPages"], pendingListPagination["totalPending"], pendingListPagination["pendingList"] });
      pendingListPagination = PendingInfusion(2, 1, new UInt160[] { admin1, admin2, owner });
      Runtime.Notify("pendingListPagination 3", new object[] { pendingListPagination["totalPages"], pendingListPagination["totalPending"], pendingListPagination["pendingList"] });
    }
    private static void Debug_InfusionMintAndInfusionUpdate()
    {
      // Mint 6 NFTs for use in the mint and update scenarios
      int quantity = 6;
      for (int i = 1; i <= quantity; i++)
        Mint($"testNft_{i}", new HardenedState()
        {
          Owner = owner,
          state = State.Ready,
        });

      // Prepare data for PreInfusion
      string clientPubKey = GetClientPubKey(owner.ToAddress());
      List<string[]> pendingMintList = new List<string[]>();
      UInt160[] providingNftHashes = new UInt160[4];
      string[] providingNftIds = new string[4];
      for (int i = 0; i < 4; i++)
      {
        providingNftHashes[i] = Runtime.ExecutingScriptHash;
        providingNftIds[i] = $"testNft_{i + 1}";
      }
      // Invoke PreInfusion to mint 2 NFTs
      string[] clientAndContractPubKey = PreInfusion(clientPubKey, NEO.Hash, 20, null, providingNftHashes, providingNftIds);
      pendingMintList.Add(clientAndContractPubKey);
      clientAndContractPubKey = PreInfusion(clientPubKey, NEO.Hash, 30, null, new UInt160[] { Runtime.ExecutingScriptHash }, new string[] { "testNft_5" });
      pendingMintList.Add(clientAndContractPubKey);

      // {"Name":"Flamefury","Image":"https://battle-hardened-cache.b-cdn.net/legends/Alchemist%20Reaver.png","State":"READY","Type":"BlackMarket","Main":"0x9999999999999999999999999999999999999999","Project":"0x0000000000000000000000000000000000000000","Contract":["0x5555555555555555555555555555555555555555","0x6666666666666666666666666666666666666666"],"Meta":{"Seed":"f83e1c3d-4465-4b29-a3bb-fa457e30d6c8","Skill":[[10,8],[6,12]],"Sync":[80,90,85,75],"Level":3,"Taste":"777","Rarity":"Legendary","Rank":5},"Attributes":{"Primary":"Fire","Secondary":"None","Aura":"Heat","Nature":["Dragon","Elemental"],"Stats":{"Attack":12,"Health":10,"Armor":8,"Speed":13,"Accuracy":11,"Luck":9}}}
      string base58Properties = "5Vx5CdjTLyb26ptWU7cmHNLU4W8TkCv5o1JDrXPUqXya2Ug3KANZcuD4VKCnwReSoLEP5ej4t3p22UR2hzt15wZ3sQzzQcYJYbciQLEBZw7RHXtXEATnHSDRnUNntCeWv6XrKdShrMDR35PrHJ8PckQWdnkTYcZP4HVCXaZtWxzyn2EhiVsCc6cRL2J8Z9Z96xcV7uJD9qqWx9AARfZ46BGEzEC793Gcg1qAsQ4mRNKpLDoeNZtpsqPcisSftdUVBDgFg4Ys5A3WAKCcAfBfnExfjpjgUrh6ntKGG7i7rxzcoLQjfu9jsqr6maTA9tYGZB5yUi3eEhnvKc7aXhg4sXpr9gRejAqHaYowcnNHsWso6aZy5UkEzf3AybrwNDzDFVGYsNZNbcYHo7DhCeWHmVDYc3FrvGAQ8kiSFk31wCDtKvt4cAwG5qFtnUdVz9nUNWM5CmizW7ZNJongRXivVyRNauJHaNQ4x4YQ2AmsFj3Xjojbtmfod5JRZP5eMT3J3puzfmv4fhNbuMQYsf8AqTxwe7r99wWA8NYoEbbTEXen74jg7THaEMEsypd9bqHC6Zs8RVyTLWzcaAoRkK4jJshL426GdtwrPqPo8vZVjDUHujNzUR3EAoia6vqXRSgu3fSRdrzsVLsW9X12MLQ2SETDZv4fT3XNS339Foy3pqrBDDSWPPrf2czup8VPNjZzBNWdfnrBbhArMmXozb9bYtk4YBP85VsFDk5JZ51m8ym2biETmc2PEsMFkn2Zvx7tMdy34AqvjNhW4L7NU2D7dkVx4RZ65m8hGwygihd4v1maVK36yZoW7dUQj7zN63tf7AksRNTVBk5erBq4H2ABhQjjD4rqQQLGPEKXxB2n4N6DgA5Kz947fx8aNJosdR9ZaS82e9QP3MMHuj978ytw7eFdtPNsN";
      for (int i = 0; i < pendingMintList.Count; i++)
      {
        string contractPubKey = pendingMintList[i][1];
        PendingObject pending = PendingStorage.Get(clientPubKey, contractPubKey);
        InfusionMint(clientPubKey, contractPubKey, Runtime.ExecutingScriptHash, pending.payTokenHash, pending.payTokenAmount, base58Properties);
      }
      string nftId = "Flamefury";
      string nftId2 = $"{nftId}#1";
      HardenedState firstMinted = GetState(nftId);
      HardenedState secondMinted = GetState(nftId2);
      Runtime.Notify("firstMinted State", new object[] { firstMinted });
      Runtime.Notify("secondMinted State", new object[] { secondMinted });

      string updClientPubKey = GetClientPubKey(owner.ToAddress());
      clientAndContractPubKey = PreInfusion(updClientPubKey, NEO.Hash, 1, nftId2, new UInt160[] { Runtime.ExecutingScriptHash }, new string[] { "testNft_6" });
      string base58UpdProperties = "2Wehy3MqbLN5t6RbDZE3KGqYnsNGMYvANiycWEycsnf8zXW3hNgLT2BgN8PhVCBQq3tUNG5fyhJEqjd1Gbdpf9FVeh179T5nQfPERAhzVytxKhReeMNS7Q57P1DDfRNP7FP7fnXjH7dRURnTsZ5Zo7vC4jhnXB5bQVTyFdxsSRFxjADysJgX3dytgXvvezoSaeY4WPXQgaRrsBE8wztQpyebESz4TqCf4veVRxCtnAgn7rD8stcNSC6GHQGsevC3z49GtB1TYLWu8xSwsLkGDojrbte5FF1Cq4ZbH4SfdqzTgVFx6hvMfkbAE15eKXYWdtmHCzvmeHQnpfopjnqKKjE55GiWYKyf4fURMrNQgE1aZaWcyGPFYfL6hD4HWkkYjiQjzpBYDXs2A48ysM8u3eZfTsUYNXEj54U9GAURK4muG6FS7jz9T86qpsSADCUCK87G3W3Y2QEm9tTKjvBseRnDwr1aAjyaxMUCTLBLSzR7PgrZmsDhvecJAGAwxQputztJAtTPineu5WYxQMkG48Z13gPqvUcFJozK1ai3qimEuA3NA42g5GnQ1GWuz53D84guN9ontDS1Q4meqw7xR2KTDQW3bGkW5YUFnJBxQTV3rZB2XD9SjR8Vh4DUgM7nYFHhhDUZ2fiBZNvuPK5WFNVLr3QYhqkNWTGxbgy1kAuPiYGX5mGkrTxEmwZfWxtrij7NqNDa46BE9xDACRkL7XxtNxD7HN95vaHKrgayYsQGs79vRcAHYBez94sESTq9M5XnBAcZYSrpaKs5nmxgwQXx1hu8ToQVs8w1avhYtmaKSNGSjGeAj298VMzG7svyDrYf2JKMYJMqus8RYWJEJQ361N4s5PQoEYYX94YHhJavtbvieGEDTRjvfQMBBBBeEtigttaeNHrPcGFfx5rz9EjiTGtgE3Ek";
      try
      {
        InfusionMint(clientAndContractPubKey[0], clientAndContractPubKey[1], owner, NEO.Hash, 1, base58UpdProperties);
      }
      catch (Exception e)
      {
        Assert(GetExceptionMessage(e) == E_16, "Expected: " + E_16);
      }
      InfusionUpdate(clientAndContractPubKey[0], clientAndContractPubKey[1], owner, NEO.Hash, 1, Runtime.ExecutingScriptHash, base58UpdProperties);
      HardenedState updatedNft = GetState(nftId2);
      Runtime.Notify("Updated NFT State", new object[] { updatedNft });
    }
    private static void Debug_UnfuseAndBurnInfusion()
    {
      string nftId = "Flamefury";
      Runtime.Notify("Ready State", new object[] { GetState(nftId) });
      Unfuse(nftId);
      HardenedState unfusedState = GetState(nftId);
      Assert(unfusedState.meta.HasKey("Sync") == false, "Error: Meta.Sync must not existing");
      Assert(unfusedState.attributes.HasKey("Nature") == false, "Error: Attributes.Nature must not existing");
      Runtime.Notify("Blueprint State", new object[] { unfusedState });

      string burnId = "Flamefury#1";
      BurnInfusion(burnId);
      bool isNotFound = false;
      try
      {
        GetState(burnId);
      }
      catch (Exception)
      {
        isNotFound = true;
      }
      Assert(isNotFound, "Error: NFT has not been burnt");
    }
    private static void Assert_DefaultFeeStructure()
    {
      FeeStructure defaultFeeStructure = FeeStructureStorage.Get();
      Assert(defaultFeeStructure.bTokenMintCost == defaultBTokenMintCost, "Expected defaultBTokenMintCost");
      Assert(defaultFeeStructure.bTokenUpdateCost == defaultBTokenUpdateCost, "Expected defaultBTokenUpdateCost");
      Assert(defaultFeeStructure.gasMintCost == defaultGasMintCost, "Expected defaultBTokenMintCost");
      Assert(defaultFeeStructure.gasUpdateCost == defaultGasUpdateCost, "Expected defaultBTokenMintCost");
      Assert(defaultFeeStructure.walletPoolHash == Runtime.ExecutingScriptHash, "Expected Runtime.ExecutingScriptHash");
    }
    private static void Assert_UpdatedFeeStructure(BigInteger bTokenMintCost, BigInteger bTokenUpdateCost, BigInteger gasMintCost, BigInteger gasUpdateCost, UInt160 walletPoolHash)
    {
      FeeStructure updatedFeeStructure = FeeStructureStorage.Get();
      Assert(updatedFeeStructure.bTokenMintCost == bTokenMintCost, $"Expected {bTokenMintCost}");
      Assert(updatedFeeStructure.bTokenUpdateCost == bTokenUpdateCost, $"Expected {bTokenUpdateCost}");
      Assert(updatedFeeStructure.gasMintCost == gasMintCost, $"Expected {gasMintCost}");
      Assert(updatedFeeStructure.gasUpdateCost == gasUpdateCost, $"Expected {gasUpdateCost}");
      Assert(updatedFeeStructure.walletPoolHash == walletPoolHash, $"Expected {(ByteString)walletPoolHash}");
    }
    private static string GetExceptionMessage(Exception e)
    {
      string msg = (string)StdLib.Deserialize(StdLib.Serialize(e));
      return msg;
    }

    private static void EnableDebugStorage()
    {
      Storage.Put(Storage.CurrentContext, Prefix_Debug, 1);
    }
  }
}