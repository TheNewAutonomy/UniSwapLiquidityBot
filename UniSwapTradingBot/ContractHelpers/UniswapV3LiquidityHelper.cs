using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Hex.HexTypes;
using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Nethereum.Model;
using Account = Nethereum.Web3.Accounts.Account;
using Nethereum.Contracts.Standards.ERC20.TokenList;
using Nethereum.BlockchainProcessing.BlockStorage.Entities;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.RPC.Eth.DTOs;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Nethereum.ABI.FunctionEncoding;

public class LiquidityRemover
{
    private const string UniswapV3NFTPositionManagerAddress = "0xc36442b4a4522e871399cd717abdd847ab11fe88";

    private const string NONFUNGIBLE_POSITION_MANAGER_ABI = @"[
   {
      ""inputs"":[
         {
            ""internalType"":""address"",
            ""name"":""_factory"",
            ""type"":""address""
         },
         {
            ""internalType"":""address"",
            ""name"":""_WETH9"",
            ""type"":""address""
         },
         {
            ""internalType"":""address"",
            ""name"":""_tokenDescriptor_"",
            ""type"":""address""
         }
      ],
      ""stateMutability"":""nonpayable"",
      ""type"":""constructor""
   },
   {
      ""anonymous"":false,
      ""inputs"":[
         {
            ""indexed"":true,
            ""internalType"":""address"",
            ""name"":""owner"",
            ""type"":""address""
         },
         {
            ""indexed"":true,
            ""internalType"":""address"",
            ""name"":""approved"",
            ""type"":""address""
         },
         {
            ""indexed"":true,
            ""internalType"":""uint256"",
            ""name"":""tokenId"",
            ""type"":""uint256""
         }
      ],
      ""name"":""Approval"",
      ""type"":""event""
   },
   {
      ""anonymous"":false,
      ""inputs"":[
         {
            ""indexed"":true,
            ""internalType"":""address"",
            ""name"":""owner"",
            ""type"":""address""
         },
         {
            ""indexed"":true,
            ""internalType"":""address"",
            ""name"":""operator"",
            ""type"":""address""
         },
         {
            ""indexed"":false,
            ""internalType"":""bool"",
            ""name"":""approved"",
            ""type"":""bool""
         }
      ],
      ""name"":""ApprovalForAll"",
      ""type"":""event""
   },
   {
      ""anonymous"":false,
      ""inputs"":[
         {
            ""indexed"":true,
            ""internalType"":""uint256"",
            ""name"":""tokenId"",
            ""type"":""uint256""
         },
         {
            ""indexed"":false,
            ""internalType"":""address"",
            ""name"":""recipient"",
            ""type"":""address""
         },
         {
            ""indexed"":false,
            ""internalType"":""uint256"",
            ""name"":""amount0"",
            ""type"":""uint256""
         },
         {
            ""indexed"":false,
            ""internalType"":""uint256"",
            ""name"":""amount1"",
            ""type"":""uint256""
         }
      ],
      ""name"":""Collect"",
      ""type"":""event""
   },
   {
      ""anonymous"":false,
      ""inputs"":[
         {
            ""indexed"":true,
            ""internalType"":""uint256"",
            ""name"":""tokenId"",
            ""type"":""uint256""
         },
         {
            ""indexed"":false,
            ""internalType"":""uint128"",
            ""name"":""liquidity"",
            ""type"":""uint128""
         },
         {
            ""indexed"":false,
            ""internalType"":""uint256"",
            ""name"":""amount0"",
            ""type"":""uint256""
         },
         {
            ""indexed"":false,
            ""internalType"":""uint256"",
            ""name"":""amount1"",
            ""type"":""uint256""
         }
      ],
      ""name"":""DecreaseLiquidity"",
      ""type"":""event""
   },
   {
      ""anonymous"":false,
      ""inputs"":[
         {
            ""indexed"":true,
            ""internalType"":""uint256"",
            ""name"":""tokenId"",
            ""type"":""uint256""
         },
         {
            ""indexed"":false,
            ""internalType"":""uint128"",
            ""name"":""liquidity"",
            ""type"":""uint128""
         },
         {
            ""indexed"":false,
            ""internalType"":""uint256"",
            ""name"":""amount0"",
            ""type"":""uint256""
         },
         {
            ""indexed"":false,
            ""internalType"":""uint256"",
            ""name"":""amount1"",
            ""type"":""uint256""
         }
      ],
      ""name"":""IncreaseLiquidity"",
      ""type"":""event""
   },
   {
      ""anonymous"":false,
      ""inputs"":[
         {
            ""indexed"":true,
            ""internalType"":""address"",
            ""name"":""from"",
            ""type"":""address""
         },
         {
            ""indexed"":true,
            ""internalType"":""address"",
            ""name"":""to"",
            ""type"":""address""
         },
         {
            ""indexed"":true,
            ""internalType"":""uint256"",
            ""name"":""tokenId"",
            ""type"":""uint256""
         }
      ],
      ""name"":""Transfer"",
      ""type"":""event""
   },
   {
      ""inputs"":[
         
      ],
      ""name"":""DOMAIN_SEPARATOR"",
      ""outputs"":[
         {
            ""internalType"":""bytes32"",
            ""name"":"""",
            ""type"":""bytes32""
         }
      ],
      ""stateMutability"":""view"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         
      ],
      ""name"":""PERMIT_TYPEHASH"",
      ""outputs"":[
         {
            ""internalType"":""bytes32"",
            ""name"":"""",
            ""type"":""bytes32""
         }
      ],
      ""stateMutability"":""view"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         
      ],
      ""name"":""WETH9"",
      ""outputs"":[
         {
            ""internalType"":""address"",
            ""name"":"""",
            ""type"":""address""
         }
      ],
      ""stateMutability"":""view"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""address"",
            ""name"":""to"",
            ""type"":""address""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""tokenId"",
            ""type"":""uint256""
         }
      ],
      ""name"":""approve"",
      ""outputs"":[
         
      ],
      ""stateMutability"":""nonpayable"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""address"",
            ""name"":""owner"",
            ""type"":""address""
         }
      ],
      ""name"":""balanceOf"",
      ""outputs"":[
         {
            ""internalType"":""uint256"",
            ""name"":"""",
            ""type"":""uint256""
         }
      ],
      ""stateMutability"":""view"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         
      ],
      ""name"":""baseURI"",
      ""outputs"":[
         {
            ""internalType"":""string"",
            ""name"":"""",
            ""type"":""string""
         }
      ],
      ""stateMutability"":""pure"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""uint256"",
            ""name"":""tokenId"",
            ""type"":""uint256""
         }
      ],
      ""name"":""burn"",
      ""outputs"":[
         
      ],
      ""stateMutability"":""payable"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""components"":[
               {
                  ""internalType"":""uint256"",
                  ""name"":""tokenId"",
                  ""type"":""uint256""
               },
               {
                  ""internalType"":""address"",
                  ""name"":""recipient"",
                  ""type"":""address""
               },
               {
                  ""internalType"":""uint128"",
                  ""name"":""amount0Max"",
                  ""type"":""uint128""
               },
               {
                  ""internalType"":""uint128"",
                  ""name"":""amount1Max"",
                  ""type"":""uint128""
               }
            ],
            ""internalType"":""struct INonfungiblePositionManager.CollectParams"",
            ""name"":""params"",
            ""type"":""tuple""
         }
      ],
      ""name"":""collect"",
      ""outputs"":[
         {
            ""internalType"":""uint256"",
            ""name"":""amount0"",
            ""type"":""uint256""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""amount1"",
            ""type"":""uint256""
         }
      ],
      ""stateMutability"":""payable"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""address"",
            ""name"":""token0"",
            ""type"":""address""
         },
         {
            ""internalType"":""address"",
            ""name"":""token1"",
            ""type"":""address""
         },
         {
            ""internalType"":""uint24"",
            ""name"":""fee"",
            ""type"":""uint24""
         },
         {
            ""internalType"":""uint160"",
            ""name"":""sqrtPriceX96"",
            ""type"":""uint160""
         }
      ],
      ""name"":""createAndInitializePoolIfNecessary"",
      ""outputs"":[
         {
            ""internalType"":""address"",
            ""name"":""pool"",
            ""type"":""address""
         }
      ],
      ""stateMutability"":""payable"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""components"":[
               {
                  ""internalType"":""uint256"",
                  ""name"":""tokenId"",
                  ""type"":""uint256""
               },
               {
                  ""internalType"":""uint128"",
                  ""name"":""liquidity"",
                  ""type"":""uint128""
               },
               {
                  ""internalType"":""uint256"",
                  ""name"":""amount0Min"",
                  ""type"":""uint256""
               },
               {
                  ""internalType"":""uint256"",
                  ""name"":""amount1Min"",
                  ""type"":""uint256""
               },
               {
                  ""internalType"":""uint256"",
                  ""name"":""deadline"",
                  ""type"":""uint256""
               }
            ],
            ""internalType"":""struct INonfungiblePositionManager.DecreaseLiquidityParams"",
            ""name"":""params"",
            ""type"":""tuple""
         }
      ],
      ""name"":""decreaseLiquidity"",
      ""outputs"":[
         {
            ""internalType"":""uint256"",
            ""name"":""amount0"",
            ""type"":""uint256""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""amount1"",
            ""type"":""uint256""
         }
      ],
      ""stateMutability"":""payable"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         
      ],
      ""name"":""factory"",
      ""outputs"":[
         {
            ""internalType"":""address"",
            ""name"":"""",
            ""type"":""address""
         }
      ],
      ""stateMutability"":""view"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""uint256"",
            ""name"":""tokenId"",
            ""type"":""uint256""
         }
      ],
      ""name"":""getApproved"",
      ""outputs"":[
         {
            ""internalType"":""address"",
            ""name"":"""",
            ""type"":""address""
         }
      ],
      ""stateMutability"":""view"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""components"":[
               {
                  ""internalType"":""uint256"",
                  ""name"":""tokenId"",
                  ""type"":""uint256""
               },
               {
                  ""internalType"":""uint256"",
                  ""name"":""amount0Desired"",
                  ""type"":""uint256""
               },
               {
                  ""internalType"":""uint256"",
                  ""name"":""amount1Desired"",
                  ""type"":""uint256""
               },
               {
                  ""internalType"":""uint256"",
                  ""name"":""amount0Min"",
                  ""type"":""uint256""
               },
               {
                  ""internalType"":""uint256"",
                  ""name"":""amount1Min"",
                  ""type"":""uint256""
               },
               {
                  ""internalType"":""uint256"",
                  ""name"":""deadline"",
                  ""type"":""uint256""
               }
            ],
            ""internalType"":""struct INonfungiblePositionManager.IncreaseLiquidityParams"",
            ""name"":""params"",
            ""type"":""tuple""
         }
      ],
      ""name"":""increaseLiquidity"",
      ""outputs"":[
         {
            ""internalType"":""uint128"",
            ""name"":""liquidity"",
            ""type"":""uint128""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""amount0"",
            ""type"":""uint256""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""amount1"",
            ""type"":""uint256""
         }
      ],
      ""stateMutability"":""payable"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""address"",
            ""name"":""owner"",
            ""type"":""address""
         },
         {
            ""internalType"":""address"",
            ""name"":""operator"",
            ""type"":""address""
         }
      ],
      ""name"":""isApprovedForAll"",
      ""outputs"":[
         {
            ""internalType"":""bool"",
            ""name"":"""",
            ""type"":""bool""
         }
      ],
      ""stateMutability"":""view"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""components"":[
               {
                  ""internalType"":""address"",
                  ""name"":""token0"",
                  ""type"":""address""
               },
               {
                  ""internalType"":""address"",
                  ""name"":""token1"",
                  ""type"":""address""
               },
               {
                  ""internalType"":""uint24"",
                  ""name"":""fee"",
                  ""type"":""uint24""
               },
               {
                  ""internalType"":""int24"",
                  ""name"":""tickLower"",
                  ""type"":""int24""
               },
               {
                  ""internalType"":""int24"",
                  ""name"":""tickUpper"",
                  ""type"":""int24""
               },
               {
                  ""internalType"":""uint256"",
                  ""name"":""amount0Desired"",
                  ""type"":""uint256""
               },
               {
                  ""internalType"":""uint256"",
                  ""name"":""amount1Desired"",
                  ""type"":""uint256""
               },
               {
                  ""internalType"":""uint256"",
                  ""name"":""amount0Min"",
                  ""type"":""uint256""
               },
               {
                  ""internalType"":""uint256"",
                  ""name"":""amount1Min"",
                  ""type"":""uint256""
               },
               {
                  ""internalType"":""address"",
                  ""name"":""recipient"",
                  ""type"":""address""
               },
               {
                  ""internalType"":""uint256"",
                  ""name"":""deadline"",
                  ""type"":""uint256""
               }
            ],
            ""internalType"":""struct INonfungiblePositionManager.MintParams"",
            ""name"":""params"",
            ""type"":""tuple""
         }
      ],
      ""name"":""mint"",
      ""outputs"":[
         {
            ""internalType"":""uint256"",
            ""name"":""tokenId"",
            ""type"":""uint256""
         },
         {
            ""internalType"":""uint128"",
            ""name"":""liquidity"",
            ""type"":""uint128""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""amount0"",
            ""type"":""uint256""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""amount1"",
            ""type"":""uint256""
         }
      ],
      ""stateMutability"":""payable"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""bytes[]"",
            ""name"":""data"",
            ""type"":""bytes[]""
         }
      ],
      ""name"":""multicall"",
      ""outputs"":[
         {
            ""internalType"":""bytes[]"",
            ""name"":""results"",
            ""type"":""bytes[]""
         }
      ],
      ""stateMutability"":""payable"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         
      ],
      ""name"":""name"",
      ""outputs"":[
         {
            ""internalType"":""string"",
            ""name"":"""",
            ""type"":""string""
         }
      ],
      ""stateMutability"":""view"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""uint256"",
            ""name"":""tokenId"",
            ""type"":""uint256""
         }
      ],
      ""name"":""ownerOf"",
      ""outputs"":[
         {
            ""internalType"":""address"",
            ""name"":"""",
            ""type"":""address""
         }
      ],
      ""stateMutability"":""view"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""address"",
            ""name"":""spender"",
            ""type"":""address""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""tokenId"",
            ""type"":""uint256""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""deadline"",
            ""type"":""uint256""
         },
         {
            ""internalType"":""uint8"",
            ""name"":""v"",
            ""type"":""uint8""
         },
         {
            ""internalType"":""bytes32"",
            ""name"":""r"",
            ""type"":""bytes32""
         },
         {
            ""internalType"":""bytes32"",
            ""name"":""s"",
            ""type"":""bytes32""
         }
      ],
      ""name"":""permit"",
      ""outputs"":[
         
      ],
      ""stateMutability"":""payable"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""uint256"",
            ""name"":""tokenId"",
            ""type"":""uint256""
         }
      ],
      ""name"":""positions"",
      ""outputs"":[
         {
            ""internalType"":""uint96"",
            ""name"":""nonce"",
            ""type"":""uint96""
         },
         {
            ""internalType"":""address"",
            ""name"":""operator"",
            ""type"":""address""
         },
         {
            ""internalType"":""address"",
            ""name"":""token0"",
            ""type"":""address""
         },
         {
            ""internalType"":""address"",
            ""name"":""token1"",
            ""type"":""address""
         },
         {
            ""internalType"":""uint24"",
            ""name"":""fee"",
            ""type"":""uint24""
         },
         {
            ""internalType"":""int24"",
            ""name"":""tickLower"",
            ""type"":""int24""
         },
         {
            ""internalType"":""int24"",
            ""name"":""tickUpper"",
            ""type"":""int24""
         },
         {
            ""internalType"":""uint128"",
            ""name"":""liquidity"",
            ""type"":""uint128""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""feeGrowthInside0LastX128"",
            ""type"":""uint256""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""feeGrowthInside1LastX128"",
            ""type"":""uint256""
         },
         {
            ""internalType"":""uint128"",
            ""name"":""tokensOwed0"",
            ""type"":""uint128""
         },
         {
            ""internalType"":""uint128"",
            ""name"":""tokensOwed1"",
            ""type"":""uint128""
         }
      ],
      ""stateMutability"":""view"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         
      ],
      ""name"":""refundETH"",
      ""outputs"":[
         
      ],
      ""stateMutability"":""payable"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""address"",
            ""name"":""from"",
            ""type"":""address""
         },
         {
            ""internalType"":""address"",
            ""name"":""to"",
            ""type"":""address""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""tokenId"",
            ""type"":""uint256""
         }
      ],
      ""name"":""safeTransferFrom"",
      ""outputs"":[
         
      ],
      ""stateMutability"":""nonpayable"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""address"",
            ""name"":""from"",
            ""type"":""address""
         },
         {
            ""internalType"":""address"",
            ""name"":""to"",
            ""type"":""address""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""tokenId"",
            ""type"":""uint256""
         },
         {
            ""internalType"":""bytes"",
            ""name"":""_data"",
            ""type"":""bytes""
         }
      ],
      ""name"":""safeTransferFrom"",
      ""outputs"":[
         
      ],
      ""stateMutability"":""nonpayable"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""address"",
            ""name"":""token"",
            ""type"":""address""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""value"",
            ""type"":""uint256""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""deadline"",
            ""type"":""uint256""
         },
         {
            ""internalType"":""uint8"",
            ""name"":""v"",
            ""type"":""uint8""
         },
         {
            ""internalType"":""bytes32"",
            ""name"":""r"",
            ""type"":""bytes32""
         },
         {
            ""internalType"":""bytes32"",
            ""name"":""s"",
            ""type"":""bytes32""
         }
      ],
      ""name"":""selfPermit"",
      ""outputs"":[
         
      ],
      ""stateMutability"":""payable"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""address"",
            ""name"":""token"",
            ""type"":""address""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""nonce"",
            ""type"":""uint256""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""expiry"",
            ""type"":""uint256""
         },
         {
            ""internalType"":""uint8"",
            ""name"":""v"",
            ""type"":""uint8""
         },
         {
            ""internalType"":""bytes32"",
            ""name"":""r"",
            ""type"":""bytes32""
         },
         {
            ""internalType"":""bytes32"",
            ""name"":""s"",
            ""type"":""bytes32""
         }
      ],
      ""name"":""selfPermitAllowed"",
      ""outputs"":[
         
      ],
      ""stateMutability"":""payable"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""address"",
            ""name"":""token"",
            ""type"":""address""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""nonce"",
            ""type"":""uint256""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""expiry"",
            ""type"":""uint256""
         },
         {
            ""internalType"":""uint8"",
            ""name"":""v"",
            ""type"":""uint8""
         },
         {
            ""internalType"":""bytes32"",
            ""name"":""r"",
            ""type"":""bytes32""
         },
         {
            ""internalType"":""bytes32"",
            ""name"":""s"",
            ""type"":""bytes32""
         }
      ],
      ""name"":""selfPermitAllowedIfNecessary"",
      ""outputs"":[
         
      ],
      ""stateMutability"":""payable"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""address"",
            ""name"":""token"",
            ""type"":""address""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""value"",
            ""type"":""uint256""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""deadline"",
            ""type"":""uint256""
         },
         {
            ""internalType"":""uint8"",
            ""name"":""v"",
            ""type"":""uint8""
         },
         {
            ""internalType"":""bytes32"",
            ""name"":""r"",
            ""type"":""bytes32""
         },
         {
            ""internalType"":""bytes32"",
            ""name"":""s"",
            ""type"":""bytes32""
         }
      ],
      ""name"":""selfPermitIfNecessary"",
      ""outputs"":[
         
      ],
      ""stateMutability"":""payable"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""address"",
            ""name"":""operator"",
            ""type"":""address""
         },
         {
            ""internalType"":""bool"",
            ""name"":""approved"",
            ""type"":""bool""
         }
      ],
      ""name"":""setApprovalForAll"",
      ""outputs"":[
         
      ],
      ""stateMutability"":""nonpayable"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""bytes4"",
            ""name"":""interfaceId"",
            ""type"":""bytes4""
         }
      ],
      ""name"":""supportsInterface"",
      ""outputs"":[
         {
            ""internalType"":""bool"",
            ""name"":"""",
            ""type"":""bool""
         }
      ],
      ""stateMutability"":""view"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""address"",
            ""name"":""token"",
            ""type"":""address""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""amountMinimum"",
            ""type"":""uint256""
         },
         {
            ""internalType"":""address"",
            ""name"":""recipient"",
            ""type"":""address""
         }
      ],
      ""name"":""sweepToken"",
      ""outputs"":[
         
      ],
      ""stateMutability"":""payable"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         
      ],
      ""name"":""symbol"",
      ""outputs"":[
         {
            ""internalType"":""string"",
            ""name"":"""",
            ""type"":""string""
         }
      ],
      ""stateMutability"":""view"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""uint256"",
            ""name"":""index"",
            ""type"":""uint256""
         }
      ],
      ""name"":""tokenByIndex"",
      ""outputs"":[
         {
            ""internalType"":""uint256"",
            ""name"":"""",
            ""type"":""uint256""
         }
      ],
      ""stateMutability"":""view"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""address"",
            ""name"":""owner"",
            ""type"":""address""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""index"",
            ""type"":""uint256""
         }
      ],
      ""name"":""tokenOfOwnerByIndex"",
      ""outputs"":[
         {
            ""internalType"":""uint256"",
            ""name"":"""",
            ""type"":""uint256""
         }
      ],
      ""stateMutability"":""view"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""uint256"",
            ""name"":""tokenId"",
            ""type"":""uint256""
         }
      ],
      ""name"":""tokenURI"",
      ""outputs"":[
         {
            ""internalType"":""string"",
            ""name"":"""",
            ""type"":""string""
         }
      ],
      ""stateMutability"":""view"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         
      ],
      ""name"":""totalSupply"",
      ""outputs"":[
         {
            ""internalType"":""uint256"",
            ""name"":"""",
            ""type"":""uint256""
         }
      ],
      ""stateMutability"":""view"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""address"",
            ""name"":""from"",
            ""type"":""address""
         },
         {
            ""internalType"":""address"",
            ""name"":""to"",
            ""type"":""address""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""tokenId"",
            ""type"":""uint256""
         }
      ],
      ""name"":""transferFrom"",
      ""outputs"":[
         
      ],
      ""stateMutability"":""nonpayable"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""uint256"",
            ""name"":""amount0Owed"",
            ""type"":""uint256""
         },
         {
            ""internalType"":""uint256"",
            ""name"":""amount1Owed"",
            ""type"":""uint256""
         },
         {
            ""internalType"":""bytes"",
            ""name"":""data"",
            ""type"":""bytes""
         }
      ],
      ""name"":""uniswapV3MintCallback"",
      ""outputs"":[
         
      ],
      ""stateMutability"":""nonpayable"",
      ""type"":""function""
   },
   {
      ""inputs"":[
         {
            ""internalType"":""uint256"",
            ""name"":""amountMinimum"",
            ""type"":""uint256""
         },
         {
            ""internalType"":""address"",
            ""name"":""recipient"",
            ""type"":""address""
         }
      ],
      ""name"":""unwrapWETH9"",
      ""outputs"":[
         
      ],
      ""stateMutability"":""payable"",
      ""type"":""function""
   },
   {
      ""stateMutability"":""payable"",
      ""type"":""receive""
   }
]";

    private readonly Web3 _web3;
    private readonly ILogger _logger;

    public LiquidityRemover(Web3 web3, ILogger logger)
    {
        _web3 = web3 ?? throw new ArgumentNullException(nameof(web3));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<string> RemoveLiquidityAsync(string privateKey, Account account, ulong positionId, CancellationToken cancellationToken)
    {
        if (account == null) throw new ArgumentNullException(nameof(account));

        // 1. Decrease Liquidity to 0
        await DecreaseLiquidityToZeroAsync(account, positionId, cancellationToken);

        // 2. Collect all tokens
        await CollectAllTokensAsync(account, positionId, cancellationToken);

        // 3. Burn the NFT position
        return await BurnPositionAsync(account, positionId, cancellationToken);
    }

    private async Task<bool> IsApprovedForAllAsync(string owner, string operatorAddress)
    {
        var erc721Contract = _web3.Eth.GetContract(NONFUNGIBLE_POSITION_MANAGER_ABI, UniswapV3NFTPositionManagerAddress);
        var isApprovedForAllFunction = erc721Contract.GetFunction("isApprovedForAll");

        return await isApprovedForAllFunction.CallAsync<bool>(owner, operatorAddress);
    }

    private async Task ApprovePositionManagerAsync(Account account, ulong positionId)
    {
        var erc721Contract = _web3.Eth.GetContract(NONFUNGIBLE_POSITION_MANAGER_ABI, UniswapV3NFTPositionManagerAddress);
        var approveFunction = erc721Contract.GetFunction("approve");

        try
        {
            var gasEstimate = await approveFunction.EstimateGasAsync(account.Address, null, null, UniswapV3NFTPositionManagerAddress,positionId);
            var gasPrice = await _web3.Eth.GasPrice.SendRequestAsync();

            var transactionInput = approveFunction.CreateTransactionInput(
                account.Address,
                UniswapV3NFTPositionManagerAddress,
                positionId
            );

            transactionInput.Gas = gasEstimate;
            transactionInput.GasPrice = gasPrice;

            var signedTransaction = await _web3.Eth.TransactionManager.SignTransactionAsync(transactionInput);
            var transactionHash = await _web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedTransaction);

            _logger.LogInformation($"Approval Transaction hash: {transactionHash}");
            await WaitForTransactionReceiptAsync(transactionHash, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to approve position manager: {ex.Message}");
            throw;
        }
    }

    private async Task DecreaseLiquidityToZeroAsync(Account account, ulong positionId, CancellationToken cancellationToken)
    {
        var contract = _web3.Eth.GetContract(NONFUNGIBLE_POSITION_MANAGER_ABI, UniswapV3NFTPositionManagerAddress);
        var decreaseLiquidityFunction = contract.GetFunction("decreaseLiquidity");

        // Approve the position manager to operate the NFT if not already approved
        var isApproved = await IsApprovedForAllAsync(account.Address, UniswapV3NFTPositionManagerAddress);
        if (!isApproved)
        {
            await ApprovePositionManagerAsync(account, positionId);
        }

        try
        {
            // Fetch position details to get the current liquidity
            var positionsFunction = contract.GetFunction("positions");

            // Create the encoded function call data
            var encodedData = positionsFunction.GetData(new BigInteger(positionId));

            // Create a CallInput object
            var callInput = new CallInput
            {
                From = account.Address, // Optional: specify the sender address
                To = UniswapV3NFTPositionManagerAddress,
                Data = encodedData
            };

            // Send the raw call
            var rawResponse = await _web3.Eth.Transactions.Call.SendRequestAsync(callInput);

            var function = new FunctionCallDecoder();

            // Decode the raw response into the Position class
            var positionDetails = function.DecodeFunctionOutput<Position>(rawResponse);

            if (positionDetails.Liquidity == 0)
            {
                _logger.LogInformation("Position already has 0 liquidity.");
                return;
            }

            // Get the current block's timestamp
            var latestBlock = await _web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(BlockParameter.CreateLatest());
            var blockTimestamp = latestBlock.Timestamp.Value; // Block timestamp in seconds

            // Set the deadline to the block time plus 10 minutes (600 seconds)
            var deadline = new HexBigInteger(blockTimestamp + 600);

            var decreaseLiquidityParams = new
            {
                tokenId = new HexBigInteger(positionId),
                liquidity = new HexBigInteger(positionDetails.Liquidity), // Decrease all liquidity
                amount0Min = new HexBigInteger(0), // Setting min amount to 0 to ensure full liquidity removal
                amount1Min = new HexBigInteger(0),
                deadline = deadline // 10 minutes from now
            };

            var canCall = await decreaseLiquidityFunction.CallAsync<bool>(account.Address, null, null, decreaseLiquidityParams);
            if (!canCall)
            {
                throw new Exception("The transaction would revert. Check the parameters and contract state.");
            }

            var gasEstimate = await decreaseLiquidityFunction.EstimateGasAsync(account.Address, null, null, decreaseLiquidityParams);
            var gasPrice = await _web3.Eth.GasPrice.SendRequestAsync();

            var transactionInput = decreaseLiquidityFunction.CreateTransactionInput(
                account.Address,
                new HexBigInteger(gasEstimate),
                new HexBigInteger(gasPrice),
                new HexBigInteger(0), // No ETH value needs to be sent with the transaction
                decreaseLiquidityParams
            );

            var signedTransaction = await _web3.Eth.TransactionManager.SignTransactionAsync(transactionInput);
            var transactionHash = await _web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedTransaction);

            _logger.LogInformation($"Decrease Liquidity Transaction hash: {transactionHash}");
            await WaitForTransactionReceiptAsync(transactionHash, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to fetch position details: {ex.Message}");
            throw;
        }
    }

    private async Task CollectAllTokensAsync(Account account, ulong positionId, CancellationToken cancellationToken)
    {
        var contract = _web3.Eth.GetContract(NONFUNGIBLE_POSITION_MANAGER_ABI, UniswapV3NFTPositionManagerAddress);
        var collectFunction = contract.GetFunction("collect");

        var collectParams = new
        {
            tokenId = positionId,
            recipient = account.Address,
            amount0Max = BigInteger.Parse("0xffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff"), // Max possible value
            amount1Max = BigInteger.Parse("0xffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff")
        };

        var gasEstimate = await collectFunction.EstimateGasAsync(account.Address, null, null, collectParams);
        var gasPrice = await _web3.Eth.GasPrice.SendRequestAsync();

        var transactionInput = collectFunction.CreateTransactionInput(
            account.Address,
            new HexBigInteger(gasEstimate),
            new HexBigInteger(gasPrice),
            new HexBigInteger(0), // No ETH value needs to be sent with the transaction
            collectParams
        );

        var signedTransaction = await _web3.Eth.TransactionManager.SignTransactionAsync(transactionInput);
        var transactionHash = await _web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedTransaction);

        _logger.LogInformation($"Collect Tokens Transaction hash: {transactionHash}");
        await WaitForTransactionReceiptAsync(transactionHash, cancellationToken);
    }

    private async Task<string> BurnPositionAsync(Account account, ulong positionId, CancellationToken cancellationToken)
    {
        var contract = _web3.Eth.GetContract(NONFUNGIBLE_POSITION_MANAGER_ABI, UniswapV3NFTPositionManagerAddress);
        var burnFunction = contract.GetFunction("burn");

        string transactionHash;

        try
        {
            var gasEstimate = await burnFunction.EstimateGasAsync(account.Address, null, null, positionId);
            var gasPrice = await _web3.Eth.GasPrice.SendRequestAsync();

            var transactionInput = burnFunction.CreateTransactionInput(
                account.Address,
                new HexBigInteger(gasEstimate),
                new HexBigInteger(gasPrice),
                new HexBigInteger(0), // No ETH value needs to be sent with the burn transaction
                new HexBigInteger(positionId)
            );

            var signedTransaction = await _web3.Eth.TransactionManager.SignTransactionAsync(transactionInput);
            transactionHash = await _web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedTransaction);

            _logger.LogInformation($"Burn Position Transaction hash: {transactionHash}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send burn transaction: {ex.Message}");
            throw;
        }

        await WaitForTransactionReceiptAsync(transactionHash, cancellationToken);

        return transactionHash;
    }

    private async Task<Nethereum.RPC.Eth.DTOs.TransactionReceipt> WaitForTransactionReceiptAsync(string transactionHash, CancellationToken cancellationToken)
    {
        Nethereum.RPC.Eth.DTOs.TransactionReceipt receipt = null;

        while (receipt == null && !cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Waiting for transaction receipt...");
            await Task.Delay(1000, cancellationToken);
            receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
        }

        return receipt;
    }

    [FunctionOutputAttribute]
    public class Position
    {
        [Parameter("uint96", "nonce", 1)]
        public BigInteger Nonce { get; set; }

        [Parameter("address", "operator", 2)]
        public string Operator { get; set; }

        [Parameter("address", "token0", 3)]
        public string Token0 { get; set; }

        [Parameter("address", "token1", 4)]
        public string Token1 { get; set; }

        [Parameter("uint24", "fee", 5)]
        public BigInteger Fee { get; set; }

        [Parameter("int24", "tickLower", 6)]
        public int TickLower { get; set; }

        [Parameter("int24", "tickUpper", 7)]
        public int TickUpper { get; set; }

        [Parameter("uint128", "liquidity", 8)]
        public BigInteger Liquidity { get; set; }

        [Parameter("uint256", "feeGrowthInside0LastX128", 9)]
        public BigInteger FeeGrowthInside0LastX128 { get; set; }

        [Parameter("uint256", "feeGrowthInside1LastX128", 10)]
        public BigInteger FeeGrowthInside1LastX128 { get; set; }

        [Parameter("uint128", "tokensOwed0", 11)]
        public BigInteger TokensOwed0 { get; set; }

        [Parameter("uint128", "tokensOwed1", 12)]
        public BigInteger TokensOwed1 { get; set; }
    }
}