# **Script Descriptions**
Scripts from the game I worked on, Digital Janitors: https://store.steampowered.com/app/1389850/Digital_Janitors/


## **Manager Scripts**
- [Save System](https://github.com/ShaneMakesGames/Code-Samples/blob/main/Digital-Janitors/SaveSystem.cs) : Handles saving & loading to disk as well as Steam Cloud saving
- [Data Manager](https://github.com/ShaneMakesGames/Code-Samples/blob/main/Digital-Janitors/DataManager.cs) : A singleton that holds all relevant data about player progress
- [Player Data](https://github.com/ShaneMakesGames/Code-Samples/blob/main/Digital-Janitors/PlayerData.cs) : A class containing all relevant data to be saved & loaded
- [Achievement Manager](https://github.com/ShaneMakesGames/Code-Samples/blob/main/Digital-Janitors/AchievementManager.cs) : Holds all achievement data and unlocks Steam achievements when requirements are met 

## **Folder Scripts**
- [Folder Class](https://github.com/ShaneMakesGames/Code-Samples/blob/main/Digital-Janitors/FolderClass.cs) : The Class for all Folder's default behaviors and variables
- [Moving Folder](https://github.com/ShaneMakesGames/Code-Samples/blob/main/Digital-Janitors/MovingFolder.cs) : Moves in a random direction after a set amount of time
- [Runaway Folder](https://github.com/ShaneMakesGames/Code-Samples/blob/main/Digital-Janitors/RunawayFolder.cs) : Moves in a random direction but when hovered over, chooses a new random direction and moves faster for a short period of time
- [Encrypted Folder](https://github.com/ShaneMakesGames/Code-Samples/blob/main/Digital-Janitors/EncryptedFolder.cs) : Must be unlocked through an interface before being sorted
- [Spam Folder](https://github.com/ShaneMakesGames/Code-Samples/blob/main/Digital-Janitors/SpamFolder.cs) : Spawns several Pop-Ups when clicked on for the first time

## **Other Systems**
- [Spawn Folders](https://github.com/ShaneMakesGames/Code-Samples/blob/main/Digital-Janitors/SpawnFolders.cs) : Handles the wave system for spawning Folders and Pop-Ups
- [Coin Combo](https://github.com/ShaneMakesGames/Code-Samples/blob/main/Digital-Janitors/CoinCombo.cs) : System for earning currency by sorting consecutive folders correctly
