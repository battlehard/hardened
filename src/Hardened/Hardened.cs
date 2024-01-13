using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;
using System.Numerics;
using static Hardened.Helpers;

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
      // Get user wallet address
      // Validate that last 4 digits of user address are match with last 4 digits of clientPubKey

      // Check bhNftId whether blank or not, if blank = mint, else = update
      // If update validate nft ownership

      // Transfer payToken to BH contract (this contract)
      // Transfer all NFT to BH contract for locking.

      // Transfer gas to BH contract with amount set by admin, different between mint and update

      // Generate contractPubKey (PubKey2)
      string contractPubKey = GenerateUuidV4();

      // Write pending storage

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
