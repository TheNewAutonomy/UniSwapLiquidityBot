# UniSwapLiquidityBot

A team of researchers at Harvard University looking into LP strategies for Uniswap v3 set out results that show how to obtain large improvements in LP earnings over existing allocation
strategy baselines. The researchers propose a new strategy called Strategic Liquidity Provision (SLP) that is designed to maximize LP earnings by dynamically adjusting the LP allocation over time. The SLP strategy is based on a novel model of the Uniswap v3 market that captures the key features of the market, including the time-varying nature of the LP earnings and the impact of LP actions on the market price. The researchers show that the SLP strategy can achieve large improvements in LP earnings over existing allocation strategy baselines, and that it can outperform the best existing strategies by a significant margin. The researchers also show that the SLP strategy is robust to changes in market conditions, and that it can achieve high earnings even in challenging market environments. The researchers conclude that the SLP strategy is a promising approach to maximizing LP earnings in Uniswap v3, and that it can be used to achieve significant improvements in LP earnings over existing allocation strategy baselines.

The original paper can be found [here](https://medium.com/gamma-strategies/expected-price-range-strategies-in-uniswap-v3-833dff253f84)

This bot is an implementation of these optimised strategies, running as a Durable Azure Function which ensures maximum up-time and response time.

A separate UI will be added to the bot to allow users to interact with the bot and monitor the performance of the bot.

Based on the performance of this bot, an AI model will be trained to predict the optimal strategy for a given market condition, to further optimise the results obtained.