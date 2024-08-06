using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Contracts;
using System.Numerics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Nethereum.ABI.FunctionEncoding.Attributes;
using System;

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
        private readonly Contract _contract;

        public NonfungiblePositionManagerService(Web3 web3, string contractAddress)
        {
            _web3 = web3;
            _contract = _web3.Eth.GetContract(abi, contractAddress);
        }

        private static readonly string abi = @"[
            // Insert the ABI of the Nonfungible Position Manager contract here
        ]";

        public async Task<BigInteger> MintPositionAsync(MintPositionParams mintParams)
        {
            var mintFunction = _contract.GetFunction("mint");

            var gas = await mintFunction.EstimateGasAsync(
                mintParams.Token0,
                mintParams.Token1,
                mintParams.Fee,
                mintParams.TickLower,
                mintParams.TickUpper,
                mintParams.Amount0Desired,
                mintParams.Amount1Desired,
                mintParams.Amount0Min,
                mintParams.Amount1Min,
                mintParams.Recipient,
                mintParams.Deadline
            );

            var transactionReceipt = await mintFunction.SendTransactionAndWaitForReceiptAsync(_web3.TransactionManager.Account.Address, new Nethereum.Hex.HexTypes.HexBigInteger(gas), null, null,
                mintParams.Token0,
                mintParams.Token1,
                mintParams.Fee,
                mintParams.TickLower,
                mintParams.TickUpper,
                mintParams.Amount0Desired,
                mintParams.Amount1Desired,
                mintParams.Amount0Min,
                mintParams.Amount1Min,
                mintParams.Recipient,
                mintParams.Deadline
            );

            var logs = transactionReceipt.DecodeAllEvents<PositionMintedEventDTO>();
            if (logs.Count > 0)
            {
                return logs[0].Event.TokenId;
            }
            else
            {
                throw new Exception("Minting position failed.");
            }
        }

        public class PositionMintedEventDTO : IEventDTO
        {
            [Parameter("uint256", "tokenId", 1, true)]
            public BigInteger TokenId { get; set; }

            [Parameter("address", "owner", 2, true)]
            public string Owner { get; set; }

            [Parameter("uint128", "liquidity", 3, false)]
            public BigInteger Liquidity { get; set; }

            [Parameter("uint256", "amount0", 4, false)]
            public BigInteger Amount0 { get; set; }

            [Parameter("uint256", "amount1", 5, false)]
            public BigInteger Amount1 { get; set; }
        }
    }
}
