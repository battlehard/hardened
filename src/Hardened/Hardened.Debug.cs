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
    private static UInt160 owner = ToScriptHash("NbMFc2fEpoF68KMRoJEhxQGTJUUVA5vqoe");
    private static UInt160 admin1 = ToScriptHash("Nh9XK6sZ6vkPu9N2L9GxnkogxXKVCgDMws");
    private static UInt160 admin2 = ToScriptHash("NQbHe4RTkzwGD3tVYsTcHEhxWpN4ZEuMuG");

    public static void TestSuite()
    {
      CheckContractAuthorization();
      Debug_Helpers();
      Debug_Transfer();
      Debug_ManageAdmin();
      Debug_FeeUpdate();
      Debug_PreInfusion_Mint();
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
      UInt160 walletPoolHash = ToScriptHash("Nh9XK6sZ6vkPu9N2L9GxnkogxXKVCgDMws");
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
          State = State.Ready,
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

      // Case 6: Delete one pending
      CancelInfusion(clientAndContractPubKey[0], clientAndContractPubKey[1]); // Cancel oneNftObject infusion
      pendingListAll = PendingStorage.ListAll(0, 5);
      Runtime.Notify("pendingListAll", new object[] { pendingListAll });
      Assert(pendingListAll.Count == 1, $"ERROR: Expected 1 all pending but got {pendingListAll.Count}");

      return pendingMintList;
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