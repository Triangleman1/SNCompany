# SNCompany Changelog

## 1.2.0
- Grading algorithm improvements
  - Finds the number of fire exits, lowers dungeon size accordingly
  - Separated moon traversal time into its own equation
    - Average time between ship and entrances experimentally determined and applied for every vanilla moon
  - Accounts for scrap density
  - No longer affected by tile size variable (mansion would have significantly lower grades due to its unique value)
  - Automatically calculates final constant based off a certain threshold, allowing for config adjustments without issues
- Config
  - Enable/Disable each feature. Logo vandalism is off by default. 
  - Can alter certain subjective factors:
    - How much to prioritize scrap quantity versus scrap quality (value)
    - How much additional fire exits reduce dungeon size
    - The relationship between number of players and how much is expected of them (efficiency per player generally decreases in larger groups)


## 1.1.0
Revamped grading algorithm to better represent player effort and efficiency
- Takes into account:
  - Dungeon Size (approximates exponential and linear components of the increased time required to search larger dungeons)
  - Player Number (at start and end of each round)
  - Quantity of scrap recovered (weighted average with scrap value)
    - This lowers the effect of extremely high value scrap, like sellbodies corpses and the facility meltdown apparatus
- With more players at small dungeon sizes, it will not be possible to get an S. This is intentional. Four players 100% clearing adamance/offense is the S threshold I balanced for.

## 1.0.0

- Release
- Replaces main menu logo
- Removes fines
