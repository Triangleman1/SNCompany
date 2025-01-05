# SNCompany Changelog

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
