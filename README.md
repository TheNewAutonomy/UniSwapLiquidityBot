# UniSwapLiquidityBot

A team of researchers at Harvard University looking into LP strategies for Uniswap v3 set out results that show how to obtain large improvements in LP earnings over existing allocation
strategy baselines. The researchers propose a new strategy called Strategic Liquidity Provision (SLP) that is designed to maximize LP earnings by dynamically adjusting the LP allocation over time. The SLP strategy is based on a novel model of the Uniswap v3 market that captures the key features of the market, including the time-varying nature of the LP earnings and the impact of LP actions on the market price. The researchers show that the SLP strategy can achieve large improvements in LP earnings over existing allocation strategy baselines, and that it can outperform the best existing strategies by a significant margin. The researchers also show that the SLP strategy is robust to changes in market conditions, and that it can achieve high earnings even in challenging market environments. The researchers conclude that the SLP strategy is a promising approach to maximizing LP earnings in Uniswap v3, and that it can be used to achieve significant improvements in LP earnings over existing allocation strategy baselines.

The original paper can be found [here](https://medium.com/gamma-strategies/expected-price-range-strategies-in-uniswap-v3-833dff253f84)

This bot is an implementation of these optimised strategies, running as a Durable Azure Function which ensures maximum up-time and response time.

A separate UI will be added to the bot to allow users to interact with the bot and monitor the performance of the bot.

Based on the performance of this bot, an AI model will be trained to predict the optimal strategy for a given market condition, to further optimise the results obtained.

## Overview
This version of the bot monitors a single narrow position against the limits defined by the user for the position.
Moving outside of the position limits results in the position being closed and a new optimised position created in its place.
This happens continuously to maintain a high performing narrow position in the market.

The new position is calculated as followws:

```
        Steps to Calculate the Amount of Tokens
        Get the Current Reserves: Get the current reserves of token0 and token1 in the pool.
        Calculate the Price Bounds: Determine the lower and upper price bounds for the new position.
        Calculate the Amounts of Token0 and Token1: Use the Uniswap V3 formulas to calculate the required amounts of each token to provide liquidity within the given price range.

        Uniswap V3 Liquidity Math
        The key formulas to use are:

        Liquidity (L):

        L = amount0 * √P current * √P upper
            --------------------------------
                 √P upper - √ P lower


        L =       amount1
             -------------------
             √P upper - √P lower

        Where:
        P current is the current price.
        P upper and P lower are the upper and lower price bounds.
        √P is the square root of the price
```

This version of the bot still requires the user to configure the position limits and so there is an element of fine tuning the bot based on risk/return appetite.

## Improvements
The next version of the bot will include an AI model to predict the optimal position limits based on the current market conditions. This will allow the bot to be fully automated and require no user input.

It will also support multiple positions and multiple pools to allow for a more diversified portfolio. This will also allow the bot to be more resilient to market changes and to optimise returns further by having multiple pots around a central position as set out in the Harvard paper.

## Installation
Installation requires an active Azure subscription and SQL Azure database for storing configuration data and positions.
