# Battle Hard Contract (Locker)

## About
Battle Hardened is a soft locker system, where users can create lockers that get minted with stats by external server. Users can also unfuse items (soft burn) to obtain a blueprint of the locker, hard burn to return all relevant items, or cancel to return any pending operation.

### Storage:
- **Pending**
  - Pubkey1 (must be unique or we throw error/reject)
  - Pubkey2 (must be unique or we throw error/reject)
  - Wallet
  - Pay tokens
  - BH NFT (conditional to update, not mint)
  - Nft1
  - Nft2
  - Nft3
  - Nft4
- **Locker (BH NFT)**
  - Pubkey3 (represents BH NFT ID)
  - State (ready|blueprint)
  - Nfl properties
    - Nft1
    - Nft2
    - Nft3
    - Nft4

### PUBLIC METHODS
#### Method: PreInfusion
**Request:**
  - Pubkey1
  - String provided from dApp - used to identify the request to the server
  - Pay Token (B or other) Value and contract not determined by contract, will depend on server approval
  - BH NFT (Nullable - only for update)
  - NFT1, NFT2, NFT3, NFT4 (Any NFT or Null)
**Returns:**
  - Pubkey1 (Provided from pre-infusion queue)
  - Pubkey2 (Provided from contract as unique identifier - can be uuid, or simple counter)
**Details:**
  Items are placed in pending storage, tracking the pubkey1 (server-side key) and pubkey2 (smart contract key) - this links the two services together in an idiosyncratic way.
  The last 4 digits of the pubkey1 must match the user wallet’s last 4 digits.
  BH NFT is only present when dealing with an update/infusion.

#### Method: CancelInfusion
**Request:**
  - PubKey1
  - PubKey2
**Returns:**
  - Submitted Pay Token
  - Gas-fee
  - Unused gas for mint (minus fees for trxns to ensure no deficit on gas)
  - Pending NFTs
**Details:**
  Cancels the selected mint/update process and returns all relevant items pending in storage to the user minus gas fees.
  Must be owner of pending snapshot or admin to invoke.

#### Method: PendingInfusions
**Request:**
  - Filter (admin only)
**Return:**
  - Array[]
    - PubKey1
    - Pubkey2
    - Wallet
    - NFT items (All 4 NFTs slots in order, and the BH NFT if applicable to validate request holdings)
**Details:**
  Provides a list of pending Fuse events, mint and update. Admin can read all, or filter by wallet/s. Non-admin (public) will return filtered results for their wallet ID only, regardless of filter status.

#### Method: Unfuse
**Request:**
  - BH NFT (ready)
**Return:**
  - All locked NFTs
  - Updated BH NFT (blueprint)
**Details:**
  BH NFT’s image is edited to point to /ready/ => /blueprint/.
  BH’s stats are reset/stripped.
  Meta.sync 
  Attributes.nature.
  Return all Locked NFTs + BH NFT to wallet.

#### Method: BurnInfusion
**Request:**
  - BH NFT
**Return:**
  - All locked NFTs
**Details:**
  BH token is burned.

### ADMIN METHODS
#### Method: SetAdmin
**Details:**
  Standard SET for trusted wallets for admin actions.

#### Method: GetAdmin
**Details:**
  Standard GET for trusted wallets for admin actions.

#### Method: DeleteAdmin
**Details:**
  Standard DELETE for trusted wallets for admin actions.

#### Method: InfusionMint
**Request:**
  - Pubkey1
  - Pubkey2
  - Contract hash (used to identify what official contract it is and used for transfer PayToken to)
  - Pay ID (pay token hash)
  - Pay Amount (pay token amount) //both ID and Amount will be used to verify correctness against pending storage
  - NFT properties (base58)
**Details:**
  Pay Token needs to migrate to the contract owner's wallet for recycling (This will be Contract Hash)
  Gas transfer to pool wallet then return the BH NFT to the user.
  Delete pending storage reference.

#### Method: InfusionUpdate
**Request:**
  - Pubkey1
  - Pubkey2
  - User Wallet (for matching source)
  - Pay Balance (correct balance to verify Pay token)
  - Pay ID (contract hash to verify Pay token)
  - Pool Wallet (send pay tokens too)
  - NFT properties (base58)
**Details:**
  We update the graphic and new properties, then return the BH NFT to the user.
  Then clear the pending storage for the target NFT (PubKey).

#### Method: FeeUpdate
**Request**
  - B Token mint cost
  - B Token update cost
  - GAS mint cost (should be a multiplier to ensure fees are covered for minting)
  - GAS update cost (should have this also because of different fee for mint and update.)
  - Wallet Pool
**Details:**
  Any of the items can be null, so specific values can be set by the admin as desired.
  Wallet pool is where B tokens and excess gas will be stored, excess gas is used for server invoke methods for Minting and Updates.

### Notes:
- PubKey1: a UUID that associates the request from the client to the server. The server would typically generate this pubkey for the client to pass onto the blockchain, these allow the user and server to track the request from the client.
- PubKey2: a UUID generated on the blockchain that helps identify the entry for admin mint, and owner Cancel operations.

### NFT attributes:
- **Name**: string (GENERATED NAME) (provided name # token UID)
- **Image**: string (URL) (IPFS namespace URL)
- **State**: string (READY|BLUEPRINT) (flag for NFT state)
- **Contract**: string[] (contract origin) (contract origins for each NFT)
- **Meta**:
  - **Seed**: string (seed string used for algorithm)
  - **Skill**: number[] (2d array that shows attack/defense bias)
  - **Sync**: number[] (values for sync status between all fusions)
  - **Level**: number (evolution status, indicates max level of fusions)
  - **Taste**: string (seed formula for likes and hates)
  - **Rarity**: string (the quality of the seed bias)
  - **Rank**: number (seed string used for algorithm)
- **Attributes**:
  - **Primary**: string (Primary element, normal as default)
  - **Secondary**: string (Secondary
