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

    public static void TestSuite()
    {
      CheckContractAuthorization();
      Debug_Helpers();
      Debug_Transfer();
      Debug_ManageAdmin();
      Debug_FeeUpdate();
      List<string[]> pendingMintList = Debug_PreInfusion_Mint();
      Debug_PendingInfusion();
      Debug_InfusionMint(pendingMintList);
      // TODO: Debug Unfuse and BurnInfusion
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
        PreInfusion(GenerateIdBase64(16), NEO.Hash, 10, null, null, null, null, null, null, null, null, null);
      }
      catch (Exception e)
      {
        Assert(GetExceptionMessage(e) == E_01, "Expected: " + E_01);
      }

      // Prepare PreInfusion clientPubKey to be matched with wallet address
      string clientPubKey = GenerateIdBase64(16);
      clientPubKey = clientPubKey.Substring(0, clientPubKey.Length - 4);
      string userWalletAddress = owner.ToAddress();
      clientPubKey += userWalletAddress.Substring(userWalletAddress.Length - 4);

      // Mint 5 NFTs for use in the mint scenarios
      for (int i = 1; i <= 5; i++)
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
        clientAndContractPubKey = PreInfusion(clientPubKey, NEO.Hash, 10, null, null, null, null, null, null, null, null, null);
      }
      catch (Exception e)
      {
        Assert(GetExceptionMessage(e) == E_02, "Expected: " + E_02);
      }

      // Case 3: Mint with full NFT slots and one slot
      clientAndContractPubKey = PreInfusion(clientPubKey, NEO.Hash, 20, null, Runtime.ExecutingScriptHash, "testNft_1", Runtime.ExecutingScriptHash, "testNft_2", Runtime.ExecutingScriptHash, "testNft_3", Runtime.ExecutingScriptHash, "testNft_4");
      pendingMintList.Add(clientAndContractPubKey);
      PendingObject fullNftObject = PendingStorage.Get(clientAndContractPubKey[0], clientAndContractPubKey[1]);
      Runtime.Notify("fullNftObject", new object[] { fullNftObject });

      clientAndContractPubKey = PreInfusion(clientPubKey, NEO.Hash, 30, null, Runtime.ExecutingScriptHash, "testNft_5", null, null, null, null, null, null);
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
      clientAndContractPubKey = PreInfusion(clientPubKey, NEO.Hash, 30, null, Runtime.ExecutingScriptHash, "testNft_5", null, null, null, null, null, null);
      pendingMintList.Add(clientAndContractPubKey);

      return pendingMintList;
    }
    private static void Debug_PendingInfusion()
    {
      PendingInfusion(0, 10, null);
      PendingInfusion(0, 10, new UInt160[] { owner });
      PendingInfusion(0, 10, new UInt160[] { admin1, admin2, owner });
    }
    private static void Debug_InfusionMint(List<string[]> pendingMintList)
    {
      // {"name":"hello","image":"url","state":"Ready","contract":["0xb3ccfca2bf5bab9cfae1c6c5b5df072579c4e138","0xb3ccfca2bf5bab9cfae1c6c5b5df072579c4e140"],"meta":{"seed":"s1","skill":[1,2,3],"sync":[7,8,9]},"attributes":{"primary":"main","skill":[1,2,3],"nature":["dragon","flying"],"stats":{"attack":1}}}
      string base58Properties = "UyFyBjMcoxkd92LLGqjJm7rh3hgzvzoWbRCjuJm1DhapkZg5DT5Ruz8a4fmL44vSrUAJa5BYuZaquKoMq7n8qBHjponS9pU7XXCuH7Y9DpdkpNrjCTb4ufPCJh6NnnJcTwnZvkLEoPQfKK2eka48P2Gmi6qyL4cmdbg3GbZwDK1RCDSgpaEfnWFhwLaQAbdCBif9fRLGty1GYFzsk9RMDg16dYEieer1KYBUjF3T1LeUD6jNFL34oNtVoCEfB6cxwa36r11Wdb6MrEfKsbZgiWTqw3eGDq9ZMu42YEJXMeEqEUcPgurpyAdmNZdqzczxLRoWnqzSy7VaaaVkhc8F4TDuAXQuALuFVZEfozpAA7HFdGMoBKsCEzrpWZ8Q3GAvuqXswwmnFvGpcRr95kPoYaRGVdzyws8hzcXpakSQoPrTSy3gkjzPakxGJ66cj6sttUZ9znoo88qckkc8wQkqG3phtsmPvbRFetmGxT7i8EDYrHV8YyzhVep9whbHsvijJvhCawieiAUdoCUoyov5wqPtKcg5hvc78QEjLo9sDWNT1tA3BfdcWEsZoqKYCyDoNCQxyiNXcbGqonDpwp6EqhFLeZAUw3By7P5axdfGeSL68arRzabBb4RTUs4MRNhBwkrSrqEgPX3eK6GAjf41CvaR4kKMngTfJ9kQ";
      for (int i = 0; i < pendingMintList.Count; i++)
      {
        string clientPubKey = pendingMintList[i][0];
        string contractPubKey = pendingMintList[i][1];
        PendingObject pending = PendingStorage.Get(clientPubKey, contractPubKey);
        InfusionMint(clientPubKey, contractPubKey, Runtime.ExecutingScriptHash, pending.payTokenHash, pending.payTokenAmount, base58Properties);
      }
      HardenedState firstMinted = GetState("hello");
      HardenedState secondMinted = GetState("hello#1");
      Runtime.Notify("NFT States", new object[] { firstMinted, secondMinted });
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
      return (string)StdLib.Deserialize(StdLib.Serialize(e));
    }
  }
}