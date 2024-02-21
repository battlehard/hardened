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
      Debug_Helpers();
      Debug_Transfer();
      Debug_ManageAdmin();
      Debug_FeeUpdate();
    }
    public static void TestUserOperation()
    {
      CheckContractAuthorization();
      Debug_PreInfusion_Mint();
      Debug_PendingInfusion();
    }
    public static void TestAdminOperation()
    {
      CheckContractAuthorization();
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
      PendingInfusion(0, 10, null);
      PendingInfusion(0, 10, new UInt160[] { owner });
      PendingInfusion(0, 10, new UInt160[] { admin1, admin2, owner });
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

      // {"Name":"Flamefury","Image":"https://battle-hardened-cache.b-cdn.net/legends/Alchemist%20Reaver.png","State":"READY","Project":"0x0000000000000000000000000000000000000000","Contract":["0x5555555555555555555555555555555555555555","0x6666666666666666666666666666666666666666"],"Meta":{"Seed":"f83e1c3d-4465-4b29-a3bb-fa457e30d6c8","Skill":[[10,8],[6,12]],"Sync":[80,90,85,75],"Level":3,"Taste":"777","Rarity":"Legendary","Rank":5},"Attributes":{"Primary":"Fire","Secondary":"None","Aura":"Heat","Nature":["Dragon","Elemental"],"Stats":{"Attack":12,"Health":10,"Armor":8,"Speed":13,"Accuracy":11,"Luck":9}}}
      string base58Properties = "GeV17U2Mre7DzzGVYjbdyGVJjbP8NYr69JhwQMQA2zbHpBE5RSWMH3mC7KfWPoQZB7k9i3eJVxsfLGXVjKNAsk3H4Wbyq3CzZA95XhJoUKJuZM6ZainnBtnmnknoS6VxMgHnofwan5pAcz9uQe3Lfpfqp6Uvbn6H6s3wSH3nhiCvYKpc7SjtvwmuerK1yH3DAMX2wqBHNSe63ssppXmBj13qRAEvz3abxx1tcKjVT8MVTktXVvaJKfWh15Q2mHUwnBx6v1yb3Ca8QbKxbgxA83d94PDBkP5NpbimJLAYJAHAiWdpYDWSQzaHViU71xv6iRsqN2JdTSQCrDX4sH7up3nRnCqfAna7wN7g4R1cG8zGSZ35hnYJzdMAqxeA4bV2ivkUepi3Rp9aGb4BALPbZ7Tq2TNJGhAtCrrc6gmGGubJjDL21ziYVbsS521vDHM79joZRx1AaBm5BCxZjAQ9qZoNcTwx7JGRhbdeArsgSrtXdDhvC9DACUdydK7719bkRxo4r4b5Bz5RA4RJnV2yJwVXPh8URf2Fu4WnYqnqaabW1BcLmWTzj4H4zzXVpiQRpUHJ7hXpQVdHvk7DuFvWGKUcBekiWTDBZdok3G8utv39FZvuxF3c1Gbu3A7mQKRnKRxWn73GKP2PXwEZFFtb7f7kFSDmkGdPfgznYHef2YpmuzjHERSmuCMhC1EdyKYdQ3JUMkva4WEBqttCeMUUbYUHho9aEZxjDed9cMPBX27STLvsPjKnEjtuMcLEjHxuDgcQEYf4pej8UMReMJa4HdmwYUvw7aNrMv9qukMwLXw4LJZZEqHFECr2Q";
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
      string base58UpdProperties = "6FpByEy3dEgXhqrit7JittgsQZbuj6KyBpTPN6CpjRCu6Cxy4ErF42FhnPQBw1BYiDshDg5fZN5aAKkA4dTtBvv2PkHWTkzgVVYqkgiB39ypbjaHyPir2E7FCTLzJUiQjW33QF8TjK1fbBS2XRw22414TKAYfLs6iL7ioMhvDCzzTHbXBn82wnQS9io6cfMawnQ6iq7r1Dd3npi1h9j3itZ4E6MMmnfKVtczYRdXYN4Ti6LDqeQn4BS2nHifPmf4JKdDnRWMQFWHd289ztzxrudcuWMUsZS6jprAHBbBY5AbMB5WJdtXg6h3W5JvCMNKefkofjExN3a3LF3VvGHaXRyyrdzYBH2B7sgdZWpZBwVa9smPLVHRsQVBTQBf4zQaidjJLjysKUFrnaPQWkA7teEhEp2c81YYmvXW1PMJdXrXk2CGfWKQPi4P9wYWoecdbSpK9cptRUtLuKZGinBDRaagCrTZistxMAEvUeXzx27BU6Y3JWQBKMxfVgZEfXUXQkLJekbWaGiF7KpzKov3TvdQy91FpoU4sAFs4NDNgHsCqgZgogmzunPwqpZmCScNav9EbLLCcTT66Gu9FvE8PTRuaa5p6fwcXx5q8J5k6znj5cFZ6wcpkopwpRSUBcEQYCwxqKhYhG2pXv9VNJY746M9o9FN5tvUv7Da4FRDVGYuakQ4kKNKTzpfNacpgMiRuwNFEoXKfQzBuLrSxKwJtk5kUYfGqu4ETgX8gF9vzBaRPTuDagrSA2TGgEimrKY8tqFWjnU67r2M9LhJ3eKRhbP2DamfwMyP7b7c4msEbQybzGdsLNL53ia82Bac";
      InfusionUpdate(clientAndContractPubKey[0], clientAndContractPubKey[1], owner, NEO.Hash, 1, base58UpdProperties);
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
  }
}