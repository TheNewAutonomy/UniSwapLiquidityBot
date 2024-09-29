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

        private static readonly string abi = @"[{""inputs"":[],""stateMutability"":""nonpayable"",""type"":""constructor""},{""anonymous"":false,""inputs"":[{""indexed"":true,""internalType"":""address"",""name"":""owner"",""type"":""address""},{""indexed"":true,""internalType"":""int24"",""name"":""tickLower"",""type"":""int24""},{""indexed"":true,""internalType"":""int24"",""name"":""tickUpper"",""type"":""int24""},{""indexed"":false,""internalType"":""uint128"",""name"":""amount"",""type"":""uint128""},{""indexed"":false,""internalType"":""uint256"",""name"":""amount0"",""type"":""uint256""},{""indexed"":false,""internalType"":""uint256"",""name"":""amount1"",""type"":""uint256""}],""name"":""Burn"",""type"":""event""},{""anonymous"":false,""inputs"":[{""indexed"":true,""internalType"":""address"",""name"":""owner"",""type"":""address""},{""indexed"":false,""internalType"":""address"",""name"":""recipient"",""type"":""address""},{""indexed"":true,""internalType"":""int24"",""name"":""tickLower"",""type"":""int24""},{""indexed"":true,""internalType"":""int24"",""name"":""tickUpper"",""type"":""int24""},{""indexed"":false,""internalType"":""uint128"",""name"":""amount0"",""type"":""uint128""},{""indexed"":false,""internalType"":""uint128"",""name"":""amount1"",""type"":""uint128""}],""name"":""Collect"",""type"":""event""},{""anonymous"":false,""inputs"":[{""indexed"":true,""internalType"":""address"",""name"":""sender"",""type"":""address""},{""indexed"":true,""internalType"":""address"",""name"":""recipient"",""type"":""address""},{""indexed"":false,""internalType"":""uint128"",""name"":""amount0"",""type"":""uint128""},{""indexed"":false,""internalType"":""uint128"",""name"":""amount1"",""type"":""uint128""}],""name"":""CollectProtocol"",""type"":""event""},{""anonymous"":false,""inputs"":[{""indexed"":true,""internalType"":""address"",""name"":""sender"",""type"":""address""},{""indexed"":true,""internalType"":""address"",""name"":""recipient"",""type"":""address""},{""indexed"":false,""internalType"":""uint256"",""name"":""amount0"",""type"":""uint256""},{""indexed"":false,""internalType"":""uint256"",""name"":""amount1"",""type"":""uint256""},{""indexed"":false,""internalType"":""uint256"",""name"":""paid0"",""type"":""uint256""},{""indexed"":false,""internalType"":""uint256"",""name"":""paid1"",""type"":""uint256""}],""name"":""Flash"",""type"":""event""},{""anonymous"":false,""inputs"":[{""indexed"":false,""internalType"":""uint16"",""name"":""observationCardinalityNextOld"",""type"":""uint16""},{""indexed"":false,""internalType"":""uint16"",""name"":""observationCardinalityNextNew"",""type"":""uint16""}],""name"":""IncreaseObservationCardinalityNext"",""type"":""event""},{""anonymous"":false,""inputs"":[{""indexed"":false,""internalType"":""uint160"",""name"":""sqrtPriceX96"",""type"":""uint160""},{""indexed"":false,""internalType"":""int24"",""name"":""tick"",""type"":""int24""}],""name"":""Initialize"",""type"":""event""},{""anonymous"":false,""inputs"":[{""indexed"":false,""internalType"":""address"",""name"":""sender"",""type"":""address""},{""indexed"":true,""internalType"":""address"",""name"":""owner"",""type"":""address""},{""indexed"":true,""internalType"":""int24"",""name"":""tickLower"",""type"":""int24""},{""indexed"":true,""internalType"":""int24"",""name"":""tickUpper"",""type"":""int24""},{""indexed"":false,""internalType"":""uint128"",""name"":""amount"",""type"":""uint128""},{""indexed"":false,""internalType"":""uint256"",""name"":""amount0"",""type"":""uint256""},{""indexed"":false,""internalType"":""uint256"",""name"":""amount1"",""type"":""uint256""}],""name"":""Mint"",""type"":""event""},{""anonymous"":false,""inputs"":[{""indexed"":false,""internalType"":""uint8"",""name"":""feeProtocol0Old"",""type"":""uint8""},{""indexed"":false,""internalType"":""uint8"",""name"":""feeProtocol1Old"",""type"":""uint8""},{""indexed"":false,""internalType"":""uint8"",""name"":""feeProtocol0New"",""type"":""uint8""},{""indexed"":false,""internalType"":""uint8"",""name"":""feeProtocol1New"",""type"":""uint8""}],""name"":""SetFeeProtocol"",""type"":""event""},{""anonymous"":false,""inputs"":[{""indexed"":true,""internalType"":""address"",""name"":""sender"",""type"":""address""},{""indexed"":true,""internalType"":""address"",""name"":""recipient"",""type"":""address""},{""indexed"":false,""internalType"":""int256"",""name"":""amount0"",""type"":""int256""},{""indexed"":false,""internalType"":""int256"",""name"":""amount1"",""type"":""int256""},{""indexed"":false,""internalType"":""uint160"",""name"":""sqrtPriceX96"",""type"":""uint160""},{""indexed"":false,""internalType"":""uint128"",""name"":""liquidity"",""type"":""uint128""},{""indexed"":false,""internalType"":""int24"",""name"":""tick"",""type"":""int24""}],""name"":""Swap"",""type"":""event""},{""inputs"":[{""internalType"":""int24"",""name"":""tickLower"",""type"":""int24""},{""internalType"":""int24"",""name"":""tickUpper"",""type"":""int24""},{""internalType"":""uint128"",""name"":""amount"",""type"":""uint128""}],""name"":""burn"",""outputs"":[{""internalType"":""uint256"",""name"":""amount0"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""amount1"",""type"":""uint256""}],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[{""internalType"":""address"",""name"":""recipient"",""type"":""address""},{""internalType"":""int24"",""name"":""tickLower"",""type"":""int24""},{""internalType"":""int24"",""name"":""tickUpper"",""type"":""int24""},{""internalType"":""uint128"",""name"":""amount0Requested"",""type"":""uint128""},{""internalType"":""uint128"",""name"":""amount1Requested"",""type"":""uint128""}],""name"":""collect"",""outputs"":[{""internalType"":""uint128"",""name"":""amount0"",""type"":""uint128""},{""internalType"":""uint128"",""name"":""amount1"",""type"":""uint128""}],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[{""internalType"":""address"",""name"":""recipient"",""type"":""address""},{""internalType"":""uint128"",""name"":""amount0Requested"",""type"":""uint128""},{""internalType"":""uint128"",""name"":""amount1Requested"",""type"":""uint128""}],""name"":""collectProtocol"",""outputs"":[{""internalType"":""uint128"",""name"":""amount0"",""type"":""uint128""},{""internalType"":""uint128"",""name"":""amount1"",""type"":""uint128""}],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[],""name"":""factory"",""outputs"":[{""internalType"":""address"",""name"":"""",""type"":""address""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[],""name"":""fee"",""outputs"":[{""internalType"":""uint24"",""name"":"""",""type"":""uint24""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[],""name"":""feeGrowthGlobal0X128"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[],""name"":""feeGrowthGlobal1X128"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[{""internalType"":""address"",""name"":""recipient"",""type"":""address""},{""internalType"":""uint256"",""name"":""amount0"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""amount1"",""type"":""uint256""},{""internalType"":""bytes"",""name"":""data"",""type"":""bytes""}],""name"":""flash"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[{""internalType"":""uint16"",""name"":""observationCardinalityNext"",""type"":""uint16""}],""name"":""increaseObservationCardinalityNext"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[{""internalType"":""uint160"",""name"":""sqrtPriceX96"",""type"":""uint160""}],""name"":""initialize"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[],""name"":""liquidity"",""outputs"":[{""internalType"":""uint128"",""name"":"""",""type"":""uint128""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[],""name"":""maxLiquidityPerTick"",""outputs"":[{""internalType"":""uint128"",""name"":"""",""type"":""uint128""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[{""internalType"":""address"",""name"":""recipient"",""type"":""address""},{""internalType"":""int24"",""name"":""tickLower"",""type"":""int24""},{""internalType"":""int24"",""name"":""tickUpper"",""type"":""int24""},{""internalType"":""uint128"",""name"":""amount"",""type"":""uint128""},{""internalType"":""bytes"",""name"":""data"",""type"":""bytes""}],""name"":""mint"",""outputs"":[{""internalType"":""uint256"",""name"":""amount0"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""amount1"",""type"":""uint256""}],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""name"":""observations"",""outputs"":[{""internalType"":""uint32"",""name"":""blockTimestamp"",""type"":""uint32""},{""internalType"":""int56"",""name"":""tickCumulative"",""type"":""int56""},{""internalType"":""uint160"",""name"":""secondsPerLiquidityCumulativeX128"",""type"":""uint160""},{""internalType"":""bool"",""name"":""initialized"",""type"":""bool""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[{""internalType"":""uint32[]"",""name"":""secondsAgos"",""type"":""uint32[]""}],""name"":""observe"",""outputs"":[{""internalType"":""int56[]"",""name"":""tickCumulatives"",""type"":""int56[]""},{""internalType"":""uint160[]"",""name"":""secondsPerLiquidityCumulativeX128s"",""type"":""uint160[]""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[{""internalType"":""bytes32"",""name"":"""",""type"":""bytes32""}],""name"":""positions"",""outputs"":[{""internalType"":""uint128"",""name"":""liquidity"",""type"":""uint128""},{""internalType"":""uint256"",""name"":""feeGrowthInside0LastX128"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""feeGrowthInside1LastX128"",""type"":""uint256""},{""internalType"":""uint128"",""name"":""tokensOwed0"",""type"":""uint128""},{""internalType"":""uint128"",""name"":""tokensOwed1"",""type"":""uint128""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[],""name"":""protocolFees"",""outputs"":[{""internalType"":""uint128"",""name"":""token0"",""type"":""uint128""},{""internalType"":""uint128"",""name"":""token1"",""type"":""uint128""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[{""internalType"":""uint8"",""name"":""feeProtocol0"",""type"":""uint8""},{""internalType"":""uint8"",""name"":""feeProtocol1"",""type"":""uint8""}],""name"":""setFeeProtocol"",""outputs"":[],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[],""name"":""slot0"",""outputs"":[{""internalType"":""uint160"",""name"":""sqrtPriceX96"",""type"":""uint160""},{""internalType"":""int24"",""name"":""tick"",""type"":""int24""},{""internalType"":""uint16"",""name"":""observationIndex"",""type"":""uint16""},{""internalType"":""uint16"",""name"":""observationCardinality"",""type"":""uint16""},{""internalType"":""uint16"",""name"":""observationCardinalityNext"",""type"":""uint16""},{""internalType"":""uint8"",""name"":""feeProtocol"",""type"":""uint8""},{""internalType"":""bool"",""name"":""unlocked"",""type"":""bool""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[{""internalType"":""int24"",""name"":""tickLower"",""type"":""int24""},{""internalType"":""int24"",""name"":""tickUpper"",""type"":""int24""}],""name"":""snapshotCumulativesInside"",""outputs"":[{""internalType"":""int56"",""name"":""tickCumulativeInside"",""type"":""int56""},{""internalType"":""uint160"",""name"":""secondsPerLiquidityInsideX128"",""type"":""uint160""},{""internalType"":""uint32"",""name"":""secondsInside"",""type"":""uint32""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[{""internalType"":""address"",""name"":""recipient"",""type"":""address""},{""internalType"":""bool"",""name"":""zeroForOne"",""type"":""bool""},{""internalType"":""int256"",""name"":""amountSpecified"",""type"":""int256""},{""internalType"":""uint160"",""name"":""sqrtPriceLimitX96"",""type"":""uint160""},{""internalType"":""bytes"",""name"":""data"",""type"":""bytes""}],""name"":""swap"",""outputs"":[{""internalType"":""int256"",""name"":""amount0"",""type"":""int256""},{""internalType"":""int256"",""name"":""amount1"",""type"":""int256""}],""stateMutability"":""nonpayable"",""type"":""function""},{""inputs"":[{""internalType"":""int16"",""name"":"""",""type"":""int16""}],""name"":""tickBitmap"",""outputs"":[{""internalType"":""uint256"",""name"":"""",""type"":""uint256""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[],""name"":""tickSpacing"",""outputs"":[{""internalType"":""int24"",""name"":"""",""type"":""int24""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[{""internalType"":""int24"",""name"":"""",""type"":""int24""}],""name"":""ticks"",""outputs"":[{""internalType"":""uint128"",""name"":""liquidityGross"",""type"":""uint128""},{""internalType"":""int128"",""name"":""liquidityNet"",""type"":""int128""},{""internalType"":""uint256"",""name"":""feeGrowthOutside0X128"",""type"":""uint256""},{""internalType"":""uint256"",""name"":""feeGrowthOutside1X128"",""type"":""uint256""},{""internalType"":""int56"",""name"":""tickCumulativeOutside"",""type"":""int56""},{""internalType"":""uint160"",""name"":""secondsPerLiquidityOutsideX128"",""type"":""uint160""},{""internalType"":""uint32"",""name"":""secondsOutside"",""type"":""uint32""},{""internalType"":""bool"",""name"":""initialized"",""type"":""bool""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[],""name"":""token0"",""outputs"":[{""internalType"":""address"",""name"":"""",""type"":""address""}],""stateMutability"":""view"",""type"":""function""},{""inputs"":[],""name"":""token1"",""outputs"":[{""internalType"":""address"",""name"":"""",""type"":""address""}],""stateMutability"":""view"",""type"":""function""}]";

        public async Task<string> MintPositionAsync(MintPositionParams mintParams)
        {
            string transactionHash = string.Empty;

            try
            {
                // Load the contract using ABI and Router Address
                var contract = _web3.Eth.GetContract(abi, _routerAddress);
                var mintFunction = contract.GetFunction<MintParams>();

                // Create the structured MintParams object
                var mintParamsObj = new MintParams
                {
                    Token0 = mintParams.Token0,
                    Token1 = mintParams.Token1,
                    Fee = (uint)mintParams.Fee,
                    TickLower = mintParams.TickLower,
                    TickUpper = mintParams.TickUpper,
                    Amount0Desired = mintParams.Amount0Desired,
                    Amount1Desired = mintParams.Amount1Desired,
                    Amount0Min = mintParams.Amount0Min,
                    Amount1Min = mintParams.Amount1Min,
                    Recipient = mintParams.Recipient,
                    Deadline = new BigInteger(mintParams.Deadline)
                };

                // Encode the parameters into transaction input data
                var transactionInputData = mintFunction.GetData(mintParamsObj);

                // Rest of the code remains the same
                // Fetch the current gas price dynamically
                var gasPrice = await _web3.Eth.GasPrice.SendRequestAsync();

                // Check and approve Token0 if needed
                var allowanceIn = await TokenHelper.GetAllowance(_web3, mintParams.Token0, _web3.TransactionManager.Account.Address, _routerAddress);
                if (allowanceIn < mintParams.Amount0Desired)
                {
                    await TokenHelper.ApproveToken(_web3, mintParams.Token0, _routerAddress, mintParams.Amount0Desired);
                }

                // Check and approve Token1 if needed
                var allowanceOut = await TokenHelper.GetAllowance(_web3, mintParams.Token1, _web3.TransactionManager.Account.Address, _routerAddress);
                if (allowanceOut < mintParams.Amount1Desired)
                {
                    await TokenHelper.ApproveToken(_web3, mintParams.Token1, _routerAddress, mintParams.Amount1Desired);
                }

                // Estimate gas for the transaction
                HexBigInteger gasEstimate;
                try
                {
                    gasEstimate = await mintFunction.EstimateGasAsync(mintParamsObj);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Gas estimation failed: {ex.Message}. Using fallback gas limit.");
                    gasEstimate = new HexBigInteger(1200000);  // Fallback gas limit
                }

                // Get the transaction count (nonce)
                var nonce = await _web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(
                    _web3.TransactionManager.Account.Address, BlockParameter.CreatePending());

                // Create the transaction input
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
                Console.WriteLine($"Error during MintPositionAsync: {ex.Message}");
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

        [Function("mint", "uint256")]
        public class MintParams
        {
            [Parameter("address", "token0", 1)]
            public string Token0 { get; set; }

            [Parameter("address", "token1", 2)]
            public string Token1 { get; set; }

            [Parameter("uint24", "fee", 3)]
            public uint Fee { get; set; }

            [Parameter("int24", "tickLower", 4)]
            public int TickLower { get; set; }

            [Parameter("int24", "tickUpper", 5)]
            public int TickUpper { get; set; }

            [Parameter("uint256", "amount0Desired", 6)]
            public BigInteger Amount0Desired { get; set; }

            [Parameter("uint256", "amount1Desired", 7)]
            public BigInteger Amount1Desired { get; set; }

            [Parameter("uint256", "amount0Min", 8)]
            public BigInteger Amount0Min { get; set; }

            [Parameter("uint256", "amount1Min", 9)]
            public BigInteger Amount1Min { get; set; }

            [Parameter("address", "recipient", 10)]
            public string Recipient { get; set; }

            [Parameter("uint256", "deadline", 11)]
            public BigInteger Deadline { get; set; }
        }

    }
}
