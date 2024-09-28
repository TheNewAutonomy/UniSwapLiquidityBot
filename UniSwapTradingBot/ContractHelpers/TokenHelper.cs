using Nethereum.Web3;
using Nethereum.Contracts;
using System.Numerics;
using System.Threading.Tasks;
using System;
using Nethereum.Hex.HexTypes;

public static class TokenHelper
{
    private static string abi = @"[
  {
    ""anonymous"": false,
    ""inputs"": [
      { ""indexed"": true, ""internalType"": ""address"", ""name"": ""owner"", ""type"": ""address"" },
      { ""indexed"": true, ""internalType"": ""address"", ""name"": ""spender"", ""type"": ""address"" },
      { ""indexed"": false, ""internalType"": ""uint256"", ""name"": ""value"", ""type"": ""uint256"" }
    ],
    ""name"": ""Approval"",
    ""type"": ""event""
  },
  {
    ""anonymous"": false,
    ""inputs"": [
      { ""indexed"": true, ""internalType"": ""address"", ""name"": ""authorizer"", ""type"": ""address"" },
      { ""indexed"": true, ""internalType"": ""bytes32"", ""name"": ""nonce"", ""type"": ""bytes32"" }
    ],
    ""name"": ""AuthorizationCanceled"",
    ""type"": ""event""
  },
  {
    ""anonymous"": false,
    ""inputs"": [
      { ""indexed"": true, ""internalType"": ""address"", ""name"": ""authorizer"", ""type"": ""address"" },
      { ""indexed"": true, ""internalType"": ""bytes32"", ""name"": ""nonce"", ""type"": ""bytes32"" }
    ],
    ""name"": ""AuthorizationUsed"",
    ""type"": ""event""
  },
  {
    ""anonymous"": false,
    ""inputs"": [
      { ""indexed"": true, ""internalType"": ""address"", ""name"": ""account"", ""type"": ""address"" }
    ],
    ""name"": ""Blacklisted"",
    ""type"": ""event""
  },
  {
    ""anonymous"": false,
    ""inputs"": [
      { ""indexed"": false, ""internalType"": ""address"", ""name"": ""userAddress"", ""type"": ""address"" },
      { ""indexed"": false, ""internalType"": ""address payable"", ""name"": ""relayerAddress"", ""type"": ""address"" },
      { ""indexed"": false, ""internalType"": ""bytes"", ""name"": ""functionSignature"", ""type"": ""bytes"" }
    ],
    ""name"": ""MetaTransactionExecuted"",
    ""type"": ""event""
  },
  {
    ""anonymous"": false,
    ""inputs"": [],
    ""name"": ""Pause"",
    ""type"": ""event""
  },
  {
    ""anonymous"": false,
    ""inputs"": [
      { ""indexed"": true, ""internalType"": ""address"", ""name"": ""newRescuer"", ""type"": ""address"" }
    ],
    ""name"": ""RescuerChanged"",
    ""type"": ""event""
  },
  {
    ""anonymous"": false,
    ""inputs"": [
      { ""indexed"": true, ""internalType"": ""bytes32"", ""name"": ""role"", ""type"": ""bytes32"" },
      { ""indexed"": true, ""internalType"": ""bytes32"", ""name"": ""previousAdminRole"", ""type"": ""bytes32"" },
      { ""indexed"": true, ""internalType"": ""bytes32"", ""name"": ""newAdminRole"", ""type"": ""bytes32"" }
    ],
    ""name"": ""RoleAdminChanged"",
    ""type"": ""event""
  },
  {
    ""anonymous"": false,
    ""inputs"": [
      { ""indexed"": true, ""internalType"": ""bytes32"", ""name"": ""role"", ""type"": ""bytes32"" },
      { ""indexed"": true, ""internalType"": ""address"", ""name"": ""account"", ""type"": ""address"" },
      { ""indexed"": true, ""internalType"": ""address"", ""name"": ""sender"", ""type"": ""address"" }
    ],
    ""name"": ""RoleGranted"",
    ""type"": ""event""
  },
  {
    ""anonymous"": false,
    ""inputs"": [
      { ""indexed"": true, ""internalType"": ""bytes32"", ""name"": ""role"", ""type"": ""bytes32"" },
      { ""indexed"": true, ""internalType"": ""address"", ""name"": ""account"", ""type"": ""address"" },
      { ""indexed"": true, ""internalType"": ""address"", ""name"": ""sender"", ""type"": ""address"" }
    ],
    ""name"": ""RoleRevoked"",
    ""type"": ""event""
  },
  {
    ""anonymous"": false,
    ""inputs"": [
      { ""indexed"": true, ""internalType"": ""address"", ""name"": ""from"", ""type"": ""address"" },
      { ""indexed"": true, ""internalType"": ""address"", ""name"": ""to"", ""type"": ""address"" },
      { ""indexed"": false, ""internalType"": ""uint256"", ""name"": ""value"", ""type"": ""uint256"" }
    ],
    ""name"": ""Transfer"",
    ""type"": ""event""
  },
  {
    ""anonymous"": false,
    ""inputs"": [
      { ""indexed"": true, ""internalType"": ""address"", ""name"": ""account"", ""type"": ""address"" }
    ],
    ""name"": ""UnBlacklisted"",
    ""type"": ""event""
  },
  {
    ""anonymous"": false,
    ""inputs"": [],
    ""name"": ""Unpause"",
    ""type"": ""event""
  },
  {
    ""inputs"": [],
    ""name"": ""APPROVE_WITH_AUTHORIZATION_TYPEHASH"",
    ""outputs"": [{ ""internalType"": ""bytes32"", ""name"": """", ""type"": ""bytes32"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""BLACKLISTER_ROLE"",
    ""outputs"": [{ ""internalType"": ""bytes32"", ""name"": """", ""type"": ""bytes32"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""CANCEL_AUTHORIZATION_TYPEHASH"",
    ""outputs"": [{ ""internalType"": ""bytes32"", ""name"": """", ""type"": ""bytes32"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""DECREASE_ALLOWANCE_WITH_AUTHORIZATION_TYPEHASH"",
    ""outputs"": [{ ""internalType"": ""bytes32"", ""name"": """", ""type"": ""bytes32"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""DEFAULT_ADMIN_ROLE"",
    ""outputs"": [{ ""internalType"": ""bytes32"", ""name"": """", ""type"": ""bytes32"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""DEPOSITOR_ROLE"",
    ""outputs"": [{ ""internalType"": ""bytes32"", ""name"": """", ""type"": ""bytes32"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""DOMAIN_SEPARATOR"",
    ""outputs"": [{ ""internalType"": ""bytes32"", ""name"": """", ""type"": ""bytes32"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""EIP712_VERSION"",
    ""outputs"": [{ ""internalType"": ""string"", ""name"": """", ""type"": ""string"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""INCREASE_ALLOWANCE_WITH_AUTHORIZATION_TYPEHASH"",
    ""outputs"": [{ ""internalType"": ""bytes32"", ""name"": """", ""type"": ""bytes32"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""META_TRANSACTION_TYPEHASH"",
    ""outputs"": [{ ""internalType"": ""bytes32"", ""name"": """", ""type"": ""bytes32"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""PAUSER_ROLE"",
    ""outputs"": [{ ""internalType"": ""bytes32"", ""name"": """", ""type"": ""bytes32"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""PERMIT_TYPEHASH"",
    ""outputs"": [{ ""internalType"": ""bytes32"", ""name"": """", ""type"": ""bytes32"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""RESCUER_ROLE"",
    ""outputs"": [{ ""internalType"": ""bytes32"", ""name"": """", ""type"": ""bytes32"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""TRANSFER_WITH_AUTHORIZATION_TYPEHASH"",
    ""outputs"": [{ ""internalType"": ""bytes32"", ""name"": """", ""type"": ""bytes32"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""WITHDRAW_WITH_AUTHORIZATION_TYPEHASH"",
    ""outputs"": [{ ""internalType"": ""bytes32"", ""name"": """", ""type"": ""bytes32"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""address"", ""name"": ""owner"", ""type"": ""address"" },
      { ""internalType"": ""address"", ""name"": ""spender"", ""type"": ""address"" }
    ],
    ""name"": ""allowance"",
    ""outputs"": [{ ""internalType"": ""uint256"", ""name"": """", ""type"": ""uint256"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""address"", ""name"": ""spender"", ""type"": ""address"" },
      { ""internalType"": ""uint256"", ""name"": ""amount"", ""type"": ""uint256"" }
    ],
    ""name"": ""approve"",
    ""outputs"": [{ ""internalType"": ""bool"", ""name"": """", ""type"": ""bool"" }],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""address"", ""name"": ""owner"", ""type"": ""address"" },
      { ""internalType"": ""address"", ""name"": ""spender"", ""type"": ""address"" },
      { ""internalType"": ""uint256"", ""name"": ""value"", ""type"": ""uint256"" },
      { ""internalType"": ""uint256"", ""name"": ""validAfter"", ""type"": ""uint256"" },
      { ""internalType"": ""uint256"", ""name"": ""validBefore"", ""type"": ""uint256"" },
      { ""internalType"": ""bytes32"", ""name"": ""nonce"", ""type"": ""bytes32"" },
      { ""internalType"": ""uint8"", ""name"": ""v"", ""type"": ""uint8"" },
      { ""internalType"": ""bytes32"", ""name"": ""r"", ""type"": ""bytes32"" },
      { ""internalType"": ""bytes32"", ""name"": ""s"", ""type"": ""bytes32"" }
    ],
    ""name"": ""approveWithAuthorization"",
    ""outputs"": [],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""address"", ""name"": ""authorizer"", ""type"": ""address"" },
      { ""internalType"": ""bytes32"", ""name"": ""nonce"", ""type"": ""bytes32"" }
    ],
    ""name"": ""authorizationState"",
    ""outputs"": [{ ""internalType"": ""enum GasAbstraction.AuthorizationState"", ""name"": """", ""type"": ""uint8"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""address"", ""name"": ""account"", ""type"": ""address"" }
    ],
    ""name"": ""balanceOf"",
    ""outputs"": [{ ""internalType"": ""uint256"", ""name"": """", ""type"": ""uint256"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""address"", ""name"": ""account"", ""type"": ""address"" }
    ],
    ""name"": ""blacklist"",
    ""outputs"": [],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""blacklisters"",
    ""outputs"": [{ ""internalType"": ""address[]"", ""name"": """", ""type"": ""address[]"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""address"", ""name"": ""authorizer"", ""type"": ""address"" },
      { ""internalType"": ""bytes32"", ""name"": ""nonce"", ""type"": ""bytes32"" },
      { ""internalType"": ""uint8"", ""name"": ""v"", ""type"": ""uint8"" },
      { ""internalType"": ""bytes32"", ""name"": ""r"", ""type"": ""bytes32"" },
      { ""internalType"": ""bytes32"", ""name"": ""s"", ""type"": ""bytes32"" }
    ],
    ""name"": ""cancelAuthorization"",
    ""outputs"": [],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""decimals"",
    ""outputs"": [{ ""internalType"": ""uint8"", ""name"": """", ""type"": ""uint8"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""address"", ""name"": ""spender"", ""type"": ""address"" },
      { ""internalType"": ""uint256"", ""name"": ""subtractedValue"", ""type"": ""uint256"" }
    ],
    ""name"": ""decreaseAllowance"",
    ""outputs"": [{ ""internalType"": ""bool"", ""name"": """", ""type"": ""bool"" }],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""address"", ""name"": ""owner"", ""type"": ""address"" },
      { ""internalType"": ""address"", ""name"": ""spender"", ""type"": ""address"" },
      { ""internalType"": ""uint256"", ""name"": ""decrement"", ""type"": ""uint256"" },
      { ""internalType"": ""uint256"", ""name"": ""validAfter"", ""type"": ""uint256"" },
      { ""internalType"": ""uint256"", ""name"": ""validBefore"", ""type"": ""uint256"" },
      { ""internalType"": ""bytes32"", ""name"": ""nonce"", ""type"": ""bytes32"" },
      { ""internalType"": ""uint8"", ""name"": ""v"", ""type"": ""uint8"" },
      { ""internalType"": ""bytes32"", ""name"": ""r"", ""type"": ""bytes32"" },
      { ""internalType"": ""bytes32"", ""name"": ""s"", ""type"": ""bytes32"" }
    ],
    ""name"": ""decreaseAllowanceWithAuthorization"",
    ""outputs"": [],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""address"", ""name"": ""user"", ""type"": ""address"" },
      { ""internalType"": ""bytes"", ""name"": ""depositData"", ""type"": ""bytes"" }
    ],
    ""name"": ""deposit"",
    ""outputs"": [],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""address"", ""name"": ""userAddress"", ""type"": ""address"" },
      { ""internalType"": ""bytes"", ""name"": ""functionSignature"", ""type"": ""bytes"" },
      { ""internalType"": ""bytes32"", ""name"": ""sigR"", ""type"": ""bytes32"" },
      { ""internalType"": ""bytes32"", ""name"": ""sigS"", ""type"": ""bytes32"" },
      { ""internalType"": ""uint8"", ""name"": ""sigV"", ""type"": ""uint8"" }
    ],
    ""name"": ""executeMetaTransaction"",
    ""outputs"": [{ ""internalType"": ""bytes"", ""name"": """", ""type"": ""bytes"" }],
    ""stateMutability"": ""payable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""bytes32"", ""name"": ""role"", ""type"": ""bytes32"" }
    ],
    ""name"": ""getRoleAdmin"",
    ""outputs"": [{ ""internalType"": ""bytes32"", ""name"": """", ""type"": ""bytes32"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""bytes32"", ""name"": ""role"", ""type"": ""bytes32"" },
      { ""internalType"": ""uint256"", ""name"": ""index"", ""type"": ""uint256"" }
    ],
    ""name"": ""getRoleMember"",
    ""outputs"": [{ ""internalType"": ""address"", ""name"": """", ""type"": ""address"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""bytes32"", ""name"": ""role"", ""type"": ""bytes32"" }
    ],
    ""name"": ""getRoleMemberCount"",
    ""outputs"": [{ ""internalType"": ""uint256"", ""name"": """", ""type"": ""uint256"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""bytes32"", ""name"": ""role"", ""type"": ""bytes32"" },
      { ""internalType"": ""address"", ""name"": ""account"", ""type"": ""address"" }
    ],
    ""name"": ""grantRole"",
    ""outputs"": [],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""bytes32"", ""name"": ""role"", ""type"": ""bytes32"" },
      { ""internalType"": ""address"", ""name"": ""account"", ""type"": ""address"" }
    ],
    ""name"": ""hasRole"",
    ""outputs"": [{ ""internalType"": ""bool"", ""name"": """", ""type"": ""bool"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""address"", ""name"": ""spender"", ""type"": ""address"" },
      { ""internalType"": ""uint256"", ""name"": ""addedValue"", ""type"": ""uint256"" }
    ],
    ""name"": ""increaseAllowance"",
    ""outputs"": [{ ""internalType"": ""bool"", ""name"": """", ""type"": ""bool"" }],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""address"", ""name"": ""owner"", ""type"": ""address"" },
      { ""internalType"": ""address"", ""name"": ""spender"", ""type"": ""address"" },
      { ""internalType"": ""uint256"", ""name"": ""increment"", ""type"": ""uint256"" },
      { ""internalType"": ""uint256"", ""name"": ""validAfter"", ""type"": ""uint256"" },
      { ""internalType"": ""uint256"", ""name"": ""validBefore"", ""type"": ""uint256"" },
      { ""internalType"": ""bytes32"", ""name"": ""nonce"", ""type"": ""bytes32"" },
      { ""internalType"": ""uint8"", ""name"": ""v"", ""type"": ""uint8"" },
      { ""internalType"": ""bytes32"", ""name"": ""r"", ""type"": ""bytes32"" },
      { ""internalType"": ""bytes32"", ""name"": ""s"", ""type"": ""bytes32"" }
    ],
    ""name"": ""increaseAllowanceWithAuthorization"",
    ""outputs"": [],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""string"", ""name"": ""newName"", ""type"": ""string"" },
      { ""internalType"": ""string"", ""name"": ""newSymbol"", ""type"": ""string"" },
      { ""internalType"": ""uint8"", ""name"": ""newDecimals"", ""type"": ""uint8"" },
      { ""internalType"": ""address"", ""name"": ""childChainManager"", ""type"": ""address"" }
    ],
    ""name"": ""initialize"",
    ""outputs"": [],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""initialized"",
    ""outputs"": [{ ""internalType"": ""bool"", ""name"": """", ""type"": ""bool"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""address"", ""name"": ""account"", ""type"": ""address"" }
    ],
    ""name"": ""isBlacklisted"",
    ""outputs"": [{ ""internalType"": ""bool"", ""name"": """", ""type"": ""bool"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""name"",
    ""outputs"": [{ ""internalType"": ""string"", ""name"": """", ""type"": ""string"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""address"", ""name"": ""owner"", ""type"": ""address"" }
    ],
    ""name"": ""nonces"",
    ""outputs"": [{ ""internalType"": ""uint256"", ""name"": """", ""type"": ""uint256"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""pause"",
    ""outputs"": [],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""paused"",
    ""outputs"": [{ ""internalType"": ""bool"", ""name"": """", ""type"": ""bool"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""pausers"",
    ""outputs"": [{ ""internalType"": ""address[]"", ""name"": """", ""type"": ""address[]"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""address"", ""name"": ""owner"", ""type"": ""address"" },
      { ""internalType"": ""address"", ""name"": ""spender"", ""type"": ""address"" },
      { ""internalType"": ""uint256"", ""name"": ""value"", ""type"": ""uint256"" },
      { ""internalType"": ""uint256"", ""name"": ""deadline"", ""type"": ""uint256"" },
      { ""internalType"": ""uint8"", ""name"": ""v"", ""type"": ""uint8"" },
      { ""internalType"": ""bytes32"", ""name"": ""r"", ""type"": ""bytes32"" },
      { ""internalType"": ""bytes32"", ""name"": ""s"", ""type"": ""bytes32"" }
    ],
    ""name"": ""permit"",
    ""outputs"": [],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""bytes32"", ""name"": ""role"", ""type"": ""bytes32"" },
      { ""internalType"": ""address"", ""name"": ""account"", ""type"": ""address"" }
    ],
    ""name"": ""renounceRole"",
    ""outputs"": [],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""contract IERC20"", ""name"": ""tokenContract"", ""type"": ""address"" },
      { ""internalType"": ""address"", ""name"": ""to"", ""type"": ""address"" },
      { ""internalType"": ""uint256"", ""name"": ""amount"", ""type"": ""uint256"" }
    ],
    ""name"": ""rescueERC20"",
    ""outputs"": [],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""rescuers"",
    ""outputs"": [{ ""internalType"": ""address[]"", ""name"": """", ""type"": ""address[]"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""bytes32"", ""name"": ""role"", ""type"": ""bytes32"" },
      { ""internalType"": ""address"", ""name"": ""account"", ""type"": ""address"" }
    ],
    ""name"": ""revokeRole"",
    ""outputs"": [],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""symbol"",
    ""outputs"": [{ ""internalType"": ""string"", ""name"": """", ""type"": ""string"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""totalSupply"",
    ""outputs"": [{ ""internalType"": ""uint256"", ""name"": """", ""type"": ""uint256"" }],
    ""stateMutability"": ""view"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""address"", ""name"": ""recipient"", ""type"": ""address"" },
      { ""internalType"": ""uint256"", ""name"": ""amount"", ""type"": ""uint256"" }
    ],
    ""name"": ""transfer"",
    ""outputs"": [{ ""internalType"": ""bool"", ""name"": """", ""type"": ""bool"" }],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""address"", ""name"": ""sender"", ""type"": ""address"" },
      { ""internalType"": ""address"", ""name"": ""recipient"", ""type"": ""address"" },
      { ""internalType"": ""uint256"", ""name"": ""amount"", ""type"": ""uint256"" }
    ],
    ""name"": ""transferFrom"",
    ""outputs"": [{ ""internalType"": ""bool"", ""name"": """", ""type"": ""bool"" }],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""address"", ""name"": ""from"", ""type"": ""address"" },
      { ""internalType"": ""address"", ""name"": ""to"", ""type"": ""address"" },
      { ""internalType"": ""uint256"", ""name"": ""value"", ""type"": ""uint256"" },
      { ""internalType"": ""uint256"", ""name"": ""validAfter"", ""type"": ""uint256"" },
      { ""internalType"": ""uint256"", ""name"": ""validBefore"", ""type"": ""uint256"" },
      { ""internalType"": ""bytes32"", ""name"": ""nonce"", ""type"": ""bytes32"" },
      { ""internalType"": ""uint8"", ""name"": ""v"", ""type"": ""uint8"" },
      { ""internalType"": ""bytes32"", ""name"": ""r"", ""type"": ""bytes32"" },
      { ""internalType"": ""bytes32"", ""name"": ""s"", ""type"": ""bytes32"" }
    ],
    ""name"": ""transferWithAuthorization"",
    ""outputs"": [],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""address"", ""name"": ""account"", ""type"": ""address"" }
    ],
    ""name"": ""unBlacklist"",
    ""outputs"": [],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [],
    ""name"": ""unpause"",
    ""outputs"": [],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""string"", ""name"": ""newName"", ""type"": ""string"" },
      { ""internalType"": ""string"", ""name"": ""newSymbol"", ""type"": ""string"" }
    ],
    ""name"": ""updateMetadata"",
    ""outputs"": [],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""uint256"", ""name"": ""amount"", ""type"": ""uint256"" }
    ],
    ""name"": ""withdraw"",
    ""outputs"": [],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  },
  {
    ""inputs"": [
      { ""internalType"": ""address"", ""name"": ""owner"", ""type"": ""address"" },
      { ""internalType"": ""uint256"", ""name"": ""value"", ""type"": ""uint256"" },
      { ""internalType"": ""uint256"", ""name"": ""validAfter"", ""type"": ""uint256"" },
      { ""internalType"": ""uint256"", ""name"": ""validBefore"", ""type"": ""uint256"" },
      { ""internalType"": ""bytes32"", ""name"": ""nonce"", ""type"": ""bytes32"" },
      { ""internalType"": ""uint8"", ""name"": ""v"", ""type"": ""uint8"" },
      { ""internalType"": ""bytes32"", ""name"": ""r"", ""type"": ""bytes32"" },
      { ""internalType"": ""bytes32"", ""name"": ""s"", ""type"": ""bytes32"" }
    ],
    ""name"": ""withdrawWithAuthorization"",
    ""outputs"": [],
    ""stateMutability"": ""nonpayable"",
    ""type"": ""function""
  }
]";

    public static async Task<int> GetTokenDecimals(Web3 web3, string tokenAddress)
    {
        var contract = web3.Eth.GetContract(abi, tokenAddress);
        var decimalsFunction = contract.GetFunction("decimals");
        int decimals = await decimalsFunction.CallAsync<int>();

        return decimals;
    }

    public static async Task<decimal> GetAvailableTokenBalance(Web3 web3, string tokenAddress, string proxyAddress, string walletAddress)
    {
        var decimals = await GetTokenDecimals(web3, proxyAddress);

        var contract = web3.Eth.GetContract(abi, proxyAddress);
        var balanceOfFunction = contract.GetFunction("balanceOf");
        var balance = await balanceOfFunction.CallAsync<BigInteger>(walletAddress);

        // Convert the balance using the retrieved decimals
        return Web3.Convert.FromWei(balance, decimals);
    }

    /// <summary>
    /// Retrieves the current allowance of a token for a given owner and spender.
    /// </summary>
    /// <param name="web3">Web3 instance for blockchain interaction.</param>
    /// <param name="tokenAddress">The address of the ERC20 token contract.</param>
    /// <param name="ownerAddress">The address of the token owner.</param>
    /// <param name="spenderAddress">The address of the spender (e.g., Uniswap Router).</param>
    /// <returns>The allowance amount in Wei.</returns>
    public static async Task<BigInteger> GetAllowance(Web3 web3, string tokenAddress, string ownerAddress, string spenderAddress)
    {
        try
        {
            var contract = web3.Eth.GetContract(abi, tokenAddress);
            var allowanceFunction = contract.GetFunction("allowance");

            // Call the allowance function with owner and spender addresses
            var allowance = await allowanceFunction.CallAsync<BigInteger>(ownerAddress, spenderAddress);

            return allowance;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching allowance for token at {tokenAddress}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Approves the spender to spend a specified amount of tokens on behalf of the owner.
    /// </summary>
    /// <param name="web3">Web3 instance for blockchain interaction.</param>
    /// <param name="tokenAddress">The address of the ERC20 token contract.</param>
    /// <param name="spenderAddress">The address of the spender (e.g., Uniswap Router).</param>
    /// <param name="amount">The amount of tokens to approve (in Wei).</param>
    /// <returns>The transaction hash of the approval transaction.</returns>
    public static async Task<string> ApproveToken(Web3 web3, string tokenAddress, string spenderAddress, BigInteger amount)
    {
        try
        {
            var contract = web3.Eth.GetContract(abi, tokenAddress);
            var approveFunction = contract.GetFunction("approve");

            // Estimate gas for the approval transaction
            var gasEstimate = await approveFunction.EstimateGasAsync(
                web3.TransactionManager.Account.Address,
                new HexBigInteger(0), // No value sent in the transaction
                new HexBigInteger(0), // Use default gas price
                spenderAddress,
                amount
            );

            // Send the approval transaction
            var transactionHash = await approveFunction.SendTransactionAsync(
                web3.TransactionManager.Account.Address, // From address
                new HexBigInteger(gasEstimate), // Estimated gas
                new HexBigInteger(0), // Use default gas price
                spenderAddress,
                amount
            );

            Console.WriteLine($"Approval transaction sent. Hash: {transactionHash}");
            return transactionHash;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error approving tokens for spender at {spenderAddress}: {ex.Message}");
            throw;
        }
    }

    public static string getTokenFromProxy(string proxyAddress)
    {
        string tokenAddress = string.Empty;

        switch (proxyAddress)
        {
            case "0x1BFD67037B42Cf73acF2047067bd4F2C47D9BfD6":
                tokenAddress = "0x7FFB3d637014488b63fb9858E279385685AFc1e2";
                break;
            case "0x2791Bca1f2de4661ED88A30C99A7a9449Aa84174":
                tokenAddress = "0xDD9185DB084f5C4fFf3b4f70E7bA62123b812226";
                break;
            default:
                break;
        }
        return tokenAddress;
    }
}
