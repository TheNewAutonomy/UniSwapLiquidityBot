using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Nethereum.ABI.FunctionEncoding.Attributes;
using System;
using NBitcoin.Secp256k1;
using Nethereum.Contracts.Standards.ERC20.TokenList;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Org.BouncyCastle.Cms;

namespace UniSwapTradingBot.ContractHelpers
{
    public class MintPositionParams
    {
        public string Token0 { get; set; }
        public string Token1 { get; set; }
        public int Fee { get; set; }
        public int TickLower { get; set; }
        public int TickUpper { get; set; }
        public BigInteger Amount0Desired { get; set; }
        public BigInteger Amount1Desired { get; set; }
        public BigInteger Amount0Min { get; set; }
        public BigInteger Amount1Min { get; set; }
        public string Recipient { get; set; }
        public ulong Deadline { get; set; }
    }

    public class NonfungiblePositionManagerService
    {
        private readonly Web3 _web3;
        private readonly string _routerAddress;

        public NonfungiblePositionManagerService(Web3 web3, string contractAddress)
        {
            _web3 = web3;
            _routerAddress = contractAddress;
        }

        private static readonly string abi = @"[
        {
            ""inputs"": [
              {
                ""internalType"": ""address"",
                ""name"": ""_factory"",
                ""type"": ""address""
              },
              {
                ""internalType"": ""address"",
                ""name"": ""_WETH9"",
                ""type"": ""address""
              },
              {
                ""internalType"": ""address"",
                ""name"": ""_tokenDescriptor_"",
                ""type"": ""address""
              }
            ],
            ""stateMutability"": ""nonpayable"",
            ""type"": ""constructor""
          },
          {
            ""anonymous"": false,
            ""inputs"": [
              {
                ""indexed"": true,
                ""internalType"": ""address"",
                ""name"": ""owner"",
                ""type"": ""address""
              },
              {
                ""indexed"": true,
                ""internalType"": ""address"",
                ""name"": ""approved"",
                ""type"": ""address""
              },
              {
                ""indexed"": true,
                ""internalType"": ""uint256"",
                ""name"": ""tokenId"",
                ""type"": ""uint256""
              }
            ],
            ""name"": ""Approval"",
            ""type"": ""event""
          },
          {
            ""anonymous"": false,
            ""inputs"": [
              {
                ""indexed"": true,
                ""internalType"": ""address"",
                ""name"": ""owner"",
                ""type"": ""address""
              },
              {
                ""indexed"": true,
                ""internalType"": ""address"",
                ""name"": ""operator"",
                ""type"": ""address""
              },
              {
                ""indexed"": false,
                ""internalType"": ""bool"",
                ""name"": ""approved"",
                ""type"": ""bool""
              }
            ],
            ""name"": ""ApprovalForAll"",
            ""type"": ""event""
          },
          {
            ""anonymous"": false,
            ""inputs"": [
              {
                ""indexed"": true,
                ""internalType"": ""uint256"",
                ""name"": ""tokenId"",
                ""type"": ""uint256""
              },
              {
                ""indexed"": false,
                ""internalType"": ""address"",
                ""name"": ""recipient"",
                ""type"": ""address""
              },
              {
                ""indexed"": false,
                ""internalType"": ""uint256"",
                ""name"": ""amount0"",
                ""type"": ""uint256""
              },
              {
                ""indexed"": false,
                ""internalType"": ""uint256"",
                ""name"": ""amount1"",
                ""type"": ""uint256""
              }
            ],
            ""name"": ""Collect"",
            ""type"": ""event""
          },
          {
            ""anonymous"": false,
            ""inputs"": [
              {
                ""indexed"": true,
                ""internalType"": ""uint256"",
                ""name"": ""tokenId"",
                ""type"": ""uint256""
              },
              {
                ""indexed"": false,
                ""internalType"": ""uint128"",
                ""name"": ""liquidity"",
                ""type"": ""uint128""
              },
              {
                ""indexed"": false,
                ""internalType"": ""uint256"",
                ""name"": ""amount0"",
                ""type"": ""uint256""
              },
              {
                ""indexed"": false,
                ""internalType"": ""uint256"",
                ""name"": ""amount1"",
                ""type"": ""uint256""
              }
            ],
            ""name"": ""DecreaseLiquidity"",
            ""type"": ""event""
          },
          {
            ""anonymous"": false,
            ""inputs"": [
              {
                ""indexed"": true,
                ""internalType"": ""uint256"",
                ""name"": ""tokenId"",
                ""type"": ""uint256""
              },
              {
                ""indexed"": false,
                ""internalType"": ""uint128"",
                ""name"": ""liquidity"",
                ""type"": ""uint128""
              },
              {
                ""indexed"": false,
                ""internalType"": ""uint256"",
                ""name"": ""amount0"",
                ""type"": ""uint256""
              },
              {
                ""indexed"": false,
                ""internalType"": ""uint256"",
                ""name"": ""amount1"",
                ""type"": ""uint256""
              }
            ],
            ""name"": ""IncreaseLiquidity"",
            ""type"": ""event""
          },
          {
            ""anonymous"": false,
            ""inputs"": [
              {
                ""indexed"": true,
                ""internalType"": ""address"",
                ""name"": ""from"",
                ""type"": ""address""
              },
              {
                ""indexed"": true,
                ""internalType"": ""address"",
                ""name"": ""to"",
                ""type"": ""address""
              },
              {
                ""indexed"": true,
                ""internalType"": ""uint256"",
                ""name"": ""tokenId"",
                ""type"": ""uint256""
              }
            ],
            ""name"": ""Transfer"",
            ""type"": ""event""
          },
          {
            ""inputs"": [],
            ""name"": ""DOMAIN_SEPARATOR"",
            ""outputs"": [
              {
                ""internalType"": ""bytes32"",
                ""name"": """",
                ""type"": ""bytes32""
              }
            ],
            ""stateMutability"": ""view"",
            ""type"": ""function""
          },
          {
            ""inputs"": [],
            ""name"": ""PERMIT_TYPEHASH"",
            ""outputs"": [
              {
                ""internalType"": ""bytes32"",
                ""name"": """",
                ""type"": ""bytes32""
              }
            ],
            ""stateMutability"": ""view"",
            ""type"": ""function""
          },
          {
            ""inputs"": [],
            ""name"": ""WETH9"",
            ""outputs"": [
              {
                ""internalType"": ""address"",
                ""name"": """",
                ""type"": ""address""
              }
            ],
            ""stateMutability"": ""view"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""address"",
                ""name"": ""to"",
                ""type"": ""address""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""tokenId"",
                ""type"": ""uint256""
              }
            ],
            ""name"": ""approve"",
            ""outputs"": [],
            ""stateMutability"": ""nonpayable"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""address"",
                ""name"": ""owner"",
                ""type"": ""address""
              }
            ],
            ""name"": ""balanceOf"",
            ""outputs"": [
              {
                ""internalType"": ""uint256"",
                ""name"": """",
                ""type"": ""uint256""
              }
            ],
            ""stateMutability"": ""view"",
            ""type"": ""function""
          },
          {
            ""inputs"": [],
            ""name"": ""baseURI"",
            ""outputs"": [
              {
                ""internalType"": ""string"",
                ""name"": """",
                ""type"": ""string""
              }
            ],
            ""stateMutability"": ""pure"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""uint256"",
                ""name"": ""tokenId"",
                ""type"": ""uint256""
              }
            ],
            ""name"": ""burn"",
            ""outputs"": [],
            ""stateMutability"": ""payable"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""components"": [
                  {
                    ""internalType"": ""uint256"",
                    ""name"": ""tokenId"",
                    ""type"": ""uint256""
                  },
                  {
                    ""internalType"": ""address"",
                    ""name"": ""recipient"",
                    ""type"": ""address""
                  },
                  {
                    ""internalType"": ""uint128"",
                    ""name"": ""amount0Max"",
                    ""type"": ""uint128""
                  },
                  {
                    ""internalType"": ""uint128"",
                    ""name"": ""amount1Max"",
                    ""type"": ""uint128""
                  }
                ],
                ""internalType"": ""struct INonfungiblePositionManager.CollectParams"",
                ""name"": ""params"",
                ""type"": ""tuple""
              }
            ],
            ""name"": ""collect"",
            ""outputs"": [
              {
                ""internalType"": ""uint256"",
                ""name"": ""amount0"",
                ""type"": ""uint256""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""amount1"",
                ""type"": ""uint256""
              }
            ],
            ""stateMutability"": ""payable"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""address"",
                ""name"": ""token0"",
                ""type"": ""address""
              },
              {
                ""internalType"": ""address"",
                ""name"": ""token1"",
                ""type"": ""address""
              },
              {
                ""internalType"": ""uint24"",
                ""name"": ""fee"",
                ""type"": ""uint24""
              },
              {
                ""internalType"": ""uint160"",
                ""name"": ""sqrtPriceX96"",
                ""type"": ""uint160""
              }
            ],
            ""name"": ""createAndInitializePoolIfNecessary"",
            ""outputs"": [
              {
                ""internalType"": ""address"",
                ""name"": ""pool"",
                ""type"": ""address""
              }
            ],
            ""stateMutability"": ""payable"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""components"": [
                  {
                    ""internalType"": ""uint256"",
                    ""name"": ""tokenId"",
                    ""type"": ""uint256""
                  },
                  {
                    ""internalType"": ""uint128"",
                    ""name"": ""liquidity"",
                    ""type"": ""uint128""
                  },
                  {
                    ""internalType"": ""uint256"",
                    ""name"": ""amount0Min"",
                    ""type"": ""uint256""
                  },
                  {
                    ""internalType"": ""uint256"",
                    ""name"": ""amount1Min"",
                    ""type"": ""uint256""
                  },
                  {
                    ""internalType"": ""uint256"",
                    ""name"": ""deadline"",
                    ""type"": ""uint256""
                  }
                ],
                ""internalType"": ""struct INonfungiblePositionManager.DecreaseLiquidityParams"",
                ""name"": ""params"",
                ""type"": ""tuple""
              }
            ],
            ""name"": ""decreaseLiquidity"",
            ""outputs"": [
              {
                ""internalType"": ""uint256"",
                ""name"": ""amount0"",
                ""type"": ""uint256""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""amount1"",
                ""type"": ""uint256""
              }
            ],
            ""stateMutability"": ""payable"",
            ""type"": ""function""
          },
          {
            ""inputs"": [],
            ""name"": ""factory"",
            ""outputs"": [
              {
                ""internalType"": ""address"",
                ""name"": """",
                ""type"": ""address""
              }
            ],
            ""stateMutability"": ""view"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""uint256"",
                ""name"": ""tokenId"",
                ""type"": ""uint256""
              }
            ],
            ""name"": ""getApproved"",
            ""outputs"": [
              {
                ""internalType"": ""address"",
                ""name"": """",
                ""type"": ""address""
              }
            ],
            ""stateMutability"": ""view"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""components"": [
                  {
                    ""internalType"": ""uint256"",
                    ""name"": ""tokenId"",
                    ""type"": ""uint256""
                  },
                  {
                    ""internalType"": ""uint256"",
                    ""name"": ""amount0Desired"",
                    ""type"": ""uint256""
                  },
                  {
                    ""internalType"": ""uint256"",
                    ""name"": ""amount1Desired"",
                    ""type"": ""uint256""
                  },
                  {
                    ""internalType"": ""uint256"",
                    ""name"": ""amount0Min"",
                    ""type"": ""uint256""
                  },
                  {
                    ""internalType"": ""uint256"",
                    ""name"": ""amount1Min"",
                    ""type"": ""uint256""
                  },
                  {
                    ""internalType"": ""uint256"",
                    ""name"": ""deadline"",
                    ""type"": ""uint256""
                  }
                ],
                ""internalType"": ""struct INonfungiblePositionManager.IncreaseLiquidityParams"",
                ""name"": ""params"",
                ""type"": ""tuple""
              }
            ],
            ""name"": ""increaseLiquidity"",
            ""outputs"": [
              {
                ""internalType"": ""uint128"",
                ""name"": ""liquidity"",
                ""type"": ""uint128""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""amount0"",
                ""type"": ""uint256""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""amount1"",
                ""type"": ""uint256""
              }
            ],
            ""stateMutability"": ""payable"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""address"",
                ""name"": ""owner"",
                ""type"": ""address""
              },
              {
                ""internalType"": ""address"",
                ""name"": ""operator"",
                ""type"": ""address""
              }
            ],
            ""name"": ""isApprovedForAll"",
            ""outputs"": [
              {
                ""internalType"": ""bool"",
                ""name"": """",
                ""type"": ""bool""
              }
            ],
            ""stateMutability"": ""view"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""components"": [
                  {
                    ""internalType"": ""address"",
                    ""name"": ""token0"",
                    ""type"": ""address""
                  },
                  {
                    ""internalType"": ""address"",
                    ""name"": ""token1"",
                    ""type"": ""address""
                  },
                  {
                    ""internalType"": ""uint24"",
                    ""name"": ""fee"",
                    ""type"": ""uint24""
                  },
                  {
                    ""internalType"": ""int24"",
                    ""name"": ""tickLower"",
                    ""type"": ""int24""
                  },
                  {
                    ""internalType"": ""int24"",
                    ""name"": ""tickUpper"",
                    ""type"": ""int24""
                  },
                  {
                    ""internalType"": ""uint256"",
                    ""name"": ""amount0Desired"",
                    ""type"": ""uint256""
                  },
                  {
                    ""internalType"": ""uint256"",
                    ""name"": ""amount1Desired"",
                    ""type"": ""uint256""
                  },
                  {
                    ""internalType"": ""uint256"",
                    ""name"": ""amount0Min"",
                    ""type"": ""uint256""
                  },
                  {
                    ""internalType"": ""uint256"",
                    ""name"": ""amount1Min"",
                    ""type"": ""uint256""
                  },
                  {
                    ""internalType"": ""address"",
                    ""name"": ""recipient"",
                    ""type"": ""address""
                  },
                  {
                    ""internalType"": ""uint256"",
                    ""name"": ""deadline"",
                    ""type"": ""uint256""
                  }
                ],
                ""internalType"": ""struct INonfungiblePositionManager.MintParams"",
                ""name"": ""params"",
                ""type"": ""tuple""
              }
            ],
            ""name"": ""mint"",
            ""outputs"": [
              {
                ""internalType"": ""uint256"",
                ""name"": ""tokenId"",
                ""type"": ""uint256""
              },
              {
                ""internalType"": ""uint128"",
                ""name"": ""liquidity"",
                ""type"": ""uint128""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""amount0"",
                ""type"": ""uint256""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""amount1"",
                ""type"": ""uint256""
              }
            ],
            ""stateMutability"": ""payable"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""bytes[]"",
                ""name"": ""data"",
                ""type"": ""bytes[]""
              }
            ],
            ""name"": ""multicall"",
            ""outputs"": [
              {
                ""internalType"": ""bytes[]"",
                ""name"": ""results"",
                ""type"": ""bytes[]""
              }
            ],
            ""stateMutability"": ""payable"",
            ""type"": ""function""
          },
          {
            ""inputs"": [],
            ""name"": ""name"",
            ""outputs"": [
              {
                ""internalType"": ""string"",
                ""name"": """",
                ""type"": ""string""
              }
            ],
            ""stateMutability"": ""view"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""uint256"",
                ""name"": ""tokenId"",
                ""type"": ""uint256""
              }
            ],
            ""name"": ""ownerOf"",
            ""outputs"": [
              {
                ""internalType"": ""address"",
                ""name"": """",
                ""type"": ""address""
              }
            ],
            ""stateMutability"": ""view"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""address"",
                ""name"": ""spender"",
                ""type"": ""address""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""tokenId"",
                ""type"": ""uint256""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""deadline"",
                ""type"": ""uint256""
              },
              {
                ""internalType"": ""uint8"",
                ""name"": ""v"",
                ""type"": ""uint8""
              },
              {
                ""internalType"": ""bytes32"",
                ""name"": ""r"",
                ""type"": ""bytes32""
              },
              {
                ""internalType"": ""bytes32"",
                ""name"": ""s"",
                ""type"": ""bytes32""
              }
            ],
            ""name"": ""permit"",
            ""outputs"": [],
            ""stateMutability"": ""payable"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""uint256"",
                ""name"": ""tokenId"",
                ""type"": ""uint256""
              }
            ],
            ""name"": ""positions"",
            ""outputs"": [
              {
                ""internalType"": ""uint96"",
                ""name"": ""nonce"",
                ""type"": ""uint96""
              },
              {
                ""internalType"": ""address"",
                ""name"": ""operator"",
                ""type"": ""address""
              },
              {
                ""internalType"": ""address"",
                ""name"": ""token0"",
                ""type"": ""address""
              },
              {
                ""internalType"": ""address"",
                ""name"": ""token1"",
                ""type"": ""address""
              },
              {
                ""internalType"": ""uint24"",
                ""name"": ""fee"",
                ""type"": ""uint24""
              },
              {
                ""internalType"": ""int24"",
                ""name"": ""tickLower"",
                ""type"": ""int24""
              },
              {
                ""internalType"": ""int24"",
                ""name"": ""tickUpper"",
                ""type"": ""int24""
              },
              {
                ""internalType"": ""uint128"",
                ""name"": ""liquidity"",
                ""type"": ""uint128""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""feeGrowthInside0LastX128"",
                ""type"": ""uint256""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""feeGrowthInside1LastX128"",
                ""type"": ""uint256""
              },
              {
                ""internalType"": ""uint128"",
                ""name"": ""tokensOwed0"",
                ""type"": ""uint128""
              },
              {
                ""internalType"": ""uint128"",
                ""name"": ""tokensOwed1"",
                ""type"": ""uint128""
              }
            ],
            ""stateMutability"": ""view"",
            ""type"": ""function""
          },
          {
            ""inputs"": [],
            ""name"": ""refundETH"",
            ""outputs"": [],
            ""stateMutability"": ""payable"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""address"",
                ""name"": ""from"",
                ""type"": ""address""
              },
              {
                ""internalType"": ""address"",
                ""name"": ""to"",
                ""type"": ""address""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""tokenId"",
                ""type"": ""uint256""
              }
            ],
            ""name"": ""safeTransferFrom"",
            ""outputs"": [],
            ""stateMutability"": ""nonpayable"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""address"",
                ""name"": ""from"",
                ""type"": ""address""
              },
              {
                ""internalType"": ""address"",
                ""name"": ""to"",
                ""type"": ""address""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""tokenId"",
                ""type"": ""uint256""
              },
              {
                ""internalType"": ""bytes"",
                ""name"": ""_data"",
                ""type"": ""bytes""
              }
            ],
            ""name"": ""safeTransferFrom"",
            ""outputs"": [],
            ""stateMutability"": ""nonpayable"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""address"",
                ""name"": ""token"",
                ""type"": ""address""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""value"",
                ""type"": ""uint256""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""deadline"",
                ""type"": ""uint256""
              },
              {
                ""internalType"": ""uint8"",
                ""name"": ""v"",
                ""type"": ""uint8""
              },
              {
                ""internalType"": ""bytes32"",
                ""name"": ""r"",
                ""type"": ""bytes32""
              },
              {
                ""internalType"": ""bytes32"",
                ""name"": ""s"",
                ""type"": ""bytes32""
              }
            ],
            ""name"": ""selfPermit"",
            ""outputs"": [],
            ""stateMutability"": ""payable"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""address"",
                ""name"": ""token"",
                ""type"": ""address""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""nonce"",
                ""type"": ""uint256""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""expiry"",
                ""type"": ""uint256""
              },
              {
                ""internalType"": ""uint8"",
                ""name"": ""v"",
                ""type"": ""uint8""
              },
              {
                ""internalType"": ""bytes32"",
                ""name"": ""r"",
                ""type"": ""bytes32""
              },
              {
                ""internalType"": ""bytes32"",
                ""name"": ""s"",
                ""type"": ""bytes32""
              }
            ],
            ""name"": ""selfPermitAllowed"",
            ""outputs"": [],
            ""stateMutability"": ""payable"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""address"",
                ""name"": ""token"",
                ""type"": ""address""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""nonce"",
                ""type"": ""uint256""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""expiry"",
                ""type"": ""uint256""
              },
              {
                ""internalType"": ""uint8"",
                ""name"": ""v"",
                ""type"": ""uint8""
              },
              {
                ""internalType"": ""bytes32"",
                ""name"": ""r"",
                ""type"": ""bytes32""
              },
              {
                ""internalType"": ""bytes32"",
                ""name"": ""s"",
                ""type"": ""bytes32""
              }
            ],
            ""name"": ""selfPermitAllowedIfNecessary"",
            ""outputs"": [],
            ""stateMutability"": ""payable"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""address"",
                ""name"": ""token"",
                ""type"": ""address""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""value"",
                ""type"": ""uint256""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""deadline"",
                ""type"": ""uint256""
              },
              {
                ""internalType"": ""uint8"",
                ""name"": ""v"",
                ""type"": ""uint8""
              },
              {
                ""internalType"": ""bytes32"",
                ""name"": ""r"",
                ""type"": ""bytes32""
              },
              {
                ""internalType"": ""bytes32"",
                ""name"": ""s"",
                ""type"": ""bytes32""
              }
            ],
            ""name"": ""selfPermitIfNecessary"",
            ""outputs"": [],
            ""stateMutability"": ""payable"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""address"",
                ""name"": ""operator"",
                ""type"": ""address""
              },
              {
                ""internalType"": ""bool"",
                ""name"": ""approved"",
                ""type"": ""bool""
              }
            ],
            ""name"": ""setApprovalForAll"",
            ""outputs"": [],
            ""stateMutability"": ""nonpayable"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""bytes4"",
                ""name"": ""interfaceId"",
                ""type"": ""bytes4""
              }
            ],
            ""name"": ""supportsInterface"",
            ""outputs"": [
              {
                ""internalType"": ""bool"",
                ""name"": """",
                ""type"": ""bool""
              }
            ],
            ""stateMutability"": ""view"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""address"",
                ""name"": ""token"",
                ""type"": ""address""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""amountMinimum"",
                ""type"": ""uint256""
              },
              {
                ""internalType"": ""address"",
                ""name"": ""recipient"",
                ""type"": ""address""
              }
            ],
            ""name"": ""sweepToken"",
            ""outputs"": [],
            ""stateMutability"": ""payable"",
            ""type"": ""function""
          },
          {
            ""inputs"": [],
            ""name"": ""symbol"",
            ""outputs"": [
              {
                ""internalType"": ""string"",
                ""name"": """",
                ""type"": ""string""
              }
            ],
            ""stateMutability"": ""view"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""uint256"",
                ""name"": ""index"",
                ""type"": ""uint256""
              }
            ],
            ""name"": ""tokenByIndex"",
            ""outputs"": [
              {
                ""internalType"": ""uint256"",
                ""name"": """",
                ""type"": ""uint256""
              }
            ],
            ""stateMutability"": ""view"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""address"",
                ""name"": ""owner"",
                ""type"": ""address""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""index"",
                ""type"": ""uint256""
              }
            ],
            ""name"": ""tokenOfOwnerByIndex"",
            ""outputs"": [
              {
                ""internalType"": ""uint256"",
                ""name"": """",
                ""type"": ""uint256""
              }
            ],
            ""stateMutability"": ""view"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""uint256"",
                ""name"": ""tokenId"",
                ""type"": ""uint256""
              }
            ],
            ""name"": ""tokenURI"",
            ""outputs"": [
              {
                ""internalType"": ""string"",
                ""name"": """",
                ""type"": ""string""
              }
            ],
            ""stateMutability"": ""view"",
            ""type"": ""function""
          },
          {
            ""inputs"": [],
            ""name"": ""totalSupply"",
            ""outputs"": [
              {
                ""internalType"": ""uint256"",
                ""name"": """",
                ""type"": ""uint256""
              }
            ],
            ""stateMutability"": ""view"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""address"",
                ""name"": ""from"",
                ""type"": ""address""
              },
              {
                ""internalType"": ""address"",
                ""name"": ""to"",
                ""type"": ""address""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""tokenId"",
                ""type"": ""uint256""
              }
            ],
            ""name"": ""transferFrom"",
            ""outputs"": [],
            ""stateMutability"": ""nonpayable"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""uint256"",
                ""name"": ""amount0Owed"",
                ""type"": ""uint256""
              },
              {
                ""internalType"": ""uint256"",
                ""name"": ""amount1Owed"",
                ""type"": ""uint256""
              },
              {
                ""internalType"": ""bytes"",
                ""name"": ""data"",
                ""type"": ""bytes""
              }
            ],
            ""name"": ""uniswapV3MintCallback"",
            ""outputs"": [],
            ""stateMutability"": ""nonpayable"",
            ""type"": ""function""
          },
          {
            ""inputs"": [
              {
                ""internalType"": ""uint256"",
                ""name"": ""amountMinimum"",
                ""type"": ""uint256""
              },
              {
                ""internalType"": ""address"",
                ""name"": ""recipient"",
                ""type"": ""address""
              }
            ],
            ""name"": ""unwrapWETH9"",
            ""outputs"": [],
            ""stateMutability"": ""payable"",
            ""type"": ""function""
          },
          {
            ""stateMutability"": ""payable"",
            ""type"": ""receive""
          }
        ]";

        public async Task<string> MintPositionAsync(MintPositionParams mintParams)
        {
            string transactionHash = string.Empty;

            try
            {
                // Load the contract using ABI and Router Address
                var contract = _web3.Eth.GetContract(abi, _routerAddress);
                var mintFunction = contract.GetFunction("mint");

                // Create input data for the exactInputSingle function
                var parameters = new MintPositionParams
                {
                    Token0 = mintParams.Token0,
                    Token1 = mintParams.Token1,
                    Fee = mintParams.Fee,
                    TickLower = mintParams.TickLower,
                    TickUpper = mintParams.TickUpper,
                    Amount0Desired = mintParams.Amount0Desired,
                    Amount1Desired = mintParams.Amount1Desired,
                    Amount0Min = mintParams.Amount0Min,
                    Amount1Min = mintParams.Amount1Min,
                    Recipient = mintParams.Recipient,
                    Deadline = mintParams.Deadline
                };

                // Encode the parameters into transaction input data
                var transactionInputData = mintFunction.GetData(parameters);

                // Get the transaction count (nonce)
                var nonce = await _web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(
                    _web3.TransactionManager.Account.Address, BlockParameter.CreatePending());

                // Fetch the current gas price dynamically
                var gasPrice = await _web3.Eth.GasPrice.SendRequestAsync();

                // Try estimating gas for the transaction
                HexBigInteger gasEstimate;
                try
                {
                    gasEstimate = await mintFunction.EstimateGasAsync(transactionInputData);
                }
                catch (Exception ex)
                {
                    // Fallback to a manual gas estimate if dynamic estimation fails
                    Console.WriteLine($"Gas estimation failed: {ex.Message}. Using fallback gas limit.");
                    gasEstimate = new HexBigInteger(500000);  // Set a fallback gas limit (adjust as needed)
                }

                // Create a transaction object
                var transaction = new TransactionInput
                {
                    From = _web3.TransactionManager.Account.Address,
                    To = _routerAddress,
                    Data = transactionInputData,
                    Gas = gasEstimate,
                    GasPrice = gasPrice,
                    Nonce = new HexBigInteger(nonce),
                    Value = new HexBigInteger(0)  // Adjust if ETH is involved
                };

                // Sign the transaction
                var signedTransaction = await _web3.TransactionManager.Account.TransactionManager.SignTransactionAsync(transaction);

                // Send the signed transaction
                transactionHash = await _web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedTransaction);

                Console.WriteLine($"Transaction sent successfully. Hash: {transactionHash}");
            }
            catch (Exception ex)
            {
                // Improved error logging
                Console.WriteLine($"Error during SwapExactInputSingleAsync: {ex.Message}");
            }

            return transactionHash;
        }

        public class PositionMintedEventDTO : IEventDTO
        {
            [Parameter("uint256", "tokenId", 1, true)]
            public BigInteger TokenId { get; set; }

            [Parameter("uint128", "liquidity", 2, false)]
            public BigInteger Liquidity { get; set; }

            [Parameter("uint256", "amount0", 3, false)]
            public BigInteger Amount0 { get; set; }

            [Parameter("uint256", "amount1", 4, false)]
            public BigInteger Amount1 { get; set; }
        }
    }
}
