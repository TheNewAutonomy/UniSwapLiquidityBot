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
using Google.Protobuf.WellKnownTypes;
using Org.BouncyCastle.Cms;
using System.Collections.Generic;
using Nethereum.ABI.Model;
using Nethereum.Util;

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

    public async Task RemoveLiquidityAsync(string privateKey, Account account, ulong positionId, CancellationToken cancellationToken)
    {
        if (account == null) throw new ArgumentNullException(nameof(account));

        // Get position information
        var position = await GetPosition(positionId);

        // Prepare multicall data
        var multicallData = new List<byte[]>();

        // 1. Decrease Liquidity to 0
        multicallData.Add(await PrepareDecreaseLiquidityCall(positionId, position));

        // 2. Collect all tokens
        multicallData.Add(await PrepareCollectTokensCall(positionId, account.Address));

        // 3. Burn the NFT position
        multicallData.Add(await PrepareBurnPositionCall(positionId));

        // Execute multicall
        await ExecuteMulticallAsync(account, multicallData);
    }

    private async Task<byte[]> PrepareDecreaseLiquidityCall(ulong positionId, Position position)
    {
        var functionCallEncoder = new FunctionCallEncoder();
        var sha3Keccack = new Sha3Keccack();

        // Calculate the function selector for "decreaseLiquidity((uint256,uint128,uint256,uint256,uint256))"
        string functionSignature = "decreaseLiquidity((uint256,uint128,uint256,uint256,uint256))";
        string functionSelector = sha3Keccack.CalculateHash(functionSignature).Substring(0, 8);

        var latestBlock = await _web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(BlockParameter.CreateLatest());
        var deadline = latestBlock.Timestamp.Value + 600; // 10 minutes from now

        // Define the parameters for the decreaseLiquidity function
        var parameters = new[]
        {
        new Parameter("uint256", 1),    // tokenId
        new Parameter("uint128", 2),    // liquidity
        new Parameter("uint256", 3),    // amount0Min
        new Parameter("uint256", 4),    // amount1Min
        new Parameter("uint256", 5)     // deadline
    };

        // Prepare the parameter values
        var parameterValues = new object[]
        {
        (BigInteger)positionId,         // tokenId
        position.Liquidity,             // liquidity
        BigInteger.Zero,                // amount0Min
        BigInteger.Zero,                // amount1Min
        (BigInteger)deadline        // deadline
        };

        // Encode the parameters
        var encodedParameters = functionCallEncoder.EncodeParameters(parameters, parameterValues);

        // Combine the function selector and encoded parameters
        var encodedFunctionCall = "0x" + functionSelector + encodedParameters.ToHex();
        return encodedFunctionCall.HexToByteArray();
    }

    private async Task<byte[]> PrepareCollectTokensCall(ulong positionId, string recipient)
    {
        var functionCallEncoder = new FunctionCallEncoder();
        var sha3Keccack = new Sha3Keccack();

        // Calculate the function selector for "collect((uint256,address,uint128,uint128))"
        string functionSignature = "collect((uint256,address,uint128,uint128))";
        string functionSelector = sha3Keccack.CalculateHash(functionSignature).Substring(0, 8);

        // Define the parameters for the collect function
        var parameters = new[]
        {
        new Parameter("uint256", 1),    // tokenId
        new Parameter("address", 2),    // recipient
        new Parameter("uint128", 3),    // amount0Max
        new Parameter("uint128", 4)     // amount1Max
    };

        // Prepare the parameter values
        var parameterValues = new object[]
        {
        (BigInteger)positionId,             // tokenId
        recipient,                          // recipient
        (BigInteger)ulong.MaxValue,         // amount0Max (maximum possible value)
        (BigInteger)ulong.MaxValue          // amount1Max (maximum possible value)
        };

        // Encode the parameters
        var encodedParameters = functionCallEncoder.EncodeParameters(parameters, parameterValues);

        // Combine the function selector and encoded parameters
        var encodedFunctionCall = "0x" + functionSelector + encodedParameters.ToHex();
        return encodedFunctionCall.HexToByteArray();
    }


    private async Task<byte[]> PrepareBurnPositionCall(ulong positionId)
    {
        var functionCallEncoder = new FunctionCallEncoder();
        var sha3Keccack = new Sha3Keccack();

        // Calculate the function selector for "burn(uint256)"
        string functionSignature = "burn(uint256)";
        string functionSelector = sha3Keccack.CalculateHash(functionSignature).Substring(0, 8);

        // Define the parameters for the burn function
        var parameters = new[]
        {
        new Parameter("uint256", 1)    // tokenId
    };

        // Prepare the parameter values
        var parameterValues = new object[]
        {
        (BigInteger)positionId        // tokenId
        };

        // Encode the parameters
        var encodedParameters = functionCallEncoder.EncodeParameters(parameters, parameterValues);

        // Combine the function selector and encoded parameters
        var encodedFunctionCall = "0x" + functionSelector + encodedParameters.ToHex();
        return encodedFunctionCall.HexToByteArray();
    }

    private async Task ExecuteMulticallAsync(Account account, List<byte[]> callsData)
    {
        var contract = _web3.Eth.GetContract(NONFUNGIBLE_POSITION_MANAGER_ABI, UniswapV3NFTPositionManagerAddress);
        var multicallFunction = contract.GetFunction("multicall");

        var transactionReceipt = await multicallFunction.SendTransactionAndWaitForReceiptAsync(
            from: account.Address,
            gas: new HexBigInteger(2000000),
            value: null,
            functionInput: new object[] { callsData }
        );

        _logger.LogInformation($"Multicall Transaction hash: {transactionReceipt.TransactionHash}");
    }

    public async Task<Position> GetPosition(ulong tokenId)
    {
        var contract = _web3.Eth.GetContract(NONFUNGIBLE_POSITION_MANAGER_ABI, UniswapV3NFTPositionManagerAddress);
        var positionsFunction = contract.GetFunction("positions");

        var result = await positionsFunction.CallDeserializingToObjectAsync<Position>(tokenId);
        return result;
    }

    [FunctionOutput]
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

    [Function("decreaseLiquidity", "void")]
    public class DecreaseLiquidityParams
    {
        [Parameter("uint256", "tokenId", 1)]
        public ulong TokenId { get; set; }

        [Parameter("uint128", "liquidity", 2)]
        public BigInteger Liquidity { get; set; }

        [Parameter("uint256", "amount0Min", 3)]
        public HexBigInteger Amount0Min { get; set; }

        [Parameter("uint256", "amount1Min", 4)]
        public HexBigInteger Amount1Min { get; set; }

        [Parameter("uint256", "deadline", 5)]
        public HexBigInteger Deadline { get; set; }
    }

    [Function("collect", "void")]
    public class CollectParams
    {
        [Parameter("uint256", "tokenId", 1)]
        public ulong TokenId { get; set; }

        [Parameter("address", "recipient", 2)]
        public string Recipient { get; set; }

        [Parameter("uint128", "amount0Max", 3)]
        public HexBigInteger Amount0Max { get; set; }

        [Parameter("uint128", "amount1Max", 4)]
        public HexBigInteger Amount1Max { get; set; }
    }
}