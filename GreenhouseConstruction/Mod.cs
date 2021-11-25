using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;

using GreenhouseConstruction.Custom_Buildings.Greenhouse;
using GreenhouseConstruction.Config;

using StardewModdingAPI;
using StardewModdingAPI.Events;

using SpaceShared;
using SpaceShared.APIs;

using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Tools;

namespace GreenhouseConstruction
{
    internal class Mod : StardewModdingAPI.Mod, IAssetEditor, IAssetLoader
    {

        private ModConfig Config;

        public static StardewModdingAPI.Mod Instance;
        private Texture2D GreenhouseExterior;

        public override void Entry(IModHelper helper)
        {
            Mod.Instance = this;

            this.Config = this.Helper.ReadConfig<ModConfig>();

            this.GreenhouseExterior = this.Helper.Content.Load<Texture2D>("assets/GreenhouseConstruction_Greenhouse.png");

            helper.Events.GameLoop.GameLaunched += this.onGameLaunched;
            helper.Events.Display.MenuChanged += this.OnMenuChanged;
            helper.Events.Player.Warped += this.onWarped;
            helper.Events.GameLoop.SaveLoaded += this.FixWarps;

        }

        private void onGameLaunched(object sender, GameLaunchedEventArgs e)
        {

            var sc = this.Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
            sc.RegisterSerializerType(typeof(CustomGreenhouseBuilding));
            sc.RegisterSerializerType(typeof(CustomGreenhouseLocation));

            var configMenu = this.Helper.ModRegistry.GetApi<GreenhouseConstruction.Config.IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

            if (configMenu is null) {

                return;
            
            }

            configMenu.Register(
                
                mod: this.ModManifest,
                reset: () => this.Config = new ModConfig() { requiredMats = this.Config.requiredMats },
                save: () => this.Helper.WriteConfig(this.Config),
                titleScreenOnly: false
                
            );

            configMenu.AddTextOption(

                mod: this.ModManifest,
                name: () => "Gold Cost of Buildings",
                getValue: () => $"{this.Config.goldCost}",
                setValue: value => this.Config.goldCost= Int32.Parse(value),
                fieldId: $"{nameof(this.Config.goldCost)}"

            );

            configMenu.AddTextOption(
                
                mod: this.ModManifest,
                name: () => "Days to Build Buildings",
                getValue: () => $"{this.Config.daysToBuild}",
                setValue: value => this.Config.daysToBuild = Int32.Parse(value),
                fieldId: $"{nameof(this.Config.daysToBuild)}"
                
            );

            configMenu.AddTextOption(

                mod: this.ModManifest,
                name: () => "Locations can Build\n(Separate each entry with space)",
                getValue: () => this.Config.buildLocations,
                setValue: value => this.Config.buildLocations = value,
                fieldId: $"{nameof(this.Config.buildLocations)}"

            );

            configMenu.OnFieldChanged(

                mod: this.ModManifest,
                onChange: UpdateConfigs

            ) ;

        }

        private void UpdateConfigs(string valueName, object newValue)
        {

            this.Monitor.Log("Invalidating Cache prior to reassignment...", LogLevel.Debug);
            this.Monitor.Log($"Current values:\n{nameof(this.Config.goldCost)}: {this.Config.goldCost}\n{nameof(this.Config.daysToBuild)}: {this.Config.daysToBuild}\n{nameof(this.Config.buildLocations)}: {this.Config.buildLocations}", LogLevel.Debug);
            this.Monitor.Log($"Updating key: {valueName} with value: {newValue}", LogLevel.Debug);

            this.Helper.Content.InvalidateCache("Data\\Buildings");

            if (valueName == nameof(this.Config.goldCost))
            {

                this.Monitor.Log($"{valueName} registered {newValue}", LogLevel.Debug);
                this.Config.goldCost = Int32.Parse((string)newValue);

            }

            else if (valueName == nameof(this.Config.daysToBuild))
            {

                this.Monitor.Log($"{valueName} registered {newValue}", LogLevel.Debug);
                this.Config.daysToBuild = Int32.Parse((string)newValue);

            }

            else if (valueName == nameof(this.Config.buildLocations))
            {

                this.Monitor.Log($"{valueName} registered {newValue}", LogLevel.Debug);
                this.Config.buildLocations = (string)newValue;

            }

            else {

                this.Monitor.Log($"{valueName} did not register for {newValue}", LogLevel.Debug);
            
            }

            this.Monitor.Log("Invalidating Cache after reassignment...", LogLevel.Debug);
            this.Monitor.Log($"Current values:\n{nameof(this.Config.goldCost)}: {this.Config.goldCost}\n{nameof(this.Config.daysToBuild)}: {this.Config.daysToBuild}\n{nameof(this.Config.buildLocations)}: {this.Config.buildLocations}", LogLevel.Debug);


            this.Helper.Content.InvalidateCache("Data\\Buildings");

        }

        private void FixWarps(object sender, EventArgs e)
        {

            foreach (var loc in Game1.locations) {

                if (loc is BuildableGameLocation buildable) {

                    foreach (var building in buildable.buildings) {

                        if (building.indoors.Value == null) {

                            continue;

                        }

                        building.indoors.Value.updateWarps();
                        building.updateInteriorWarps();
                    
                    }
                
                }
            
            }

        }

        private void onWarped(object sender, WarpedEventArgs e)
        {
            if (!e.IsLocalPlayer) {

                return;
            
            }

            BuildableGameLocation farm = e.NewLocation as BuildableGameLocation ?? e.OldLocation as BuildableGameLocation;
            if (farm != null) {

                for (int i = 0; i < farm.buildings.Count; ++i) {

                    var b = farm.buildings[i];

                    if (b.buildingType.Value == "GreenhouseConstruction_SpecialGreenhouse" && !(b is CustomGreenhouseBuilding)) {

                        farm.buildings[i] = new CustomGreenhouseBuilding();
                        farm.buildings[i].buildingType.Value = b.buildingType.Value;
                        farm.buildings[i].daysOfConstructionLeft.Value = b.daysOfConstructionLeft.Value;
                        farm.buildings[i].indoors.Value = b.indoors.Value;
                        farm.buildings[i].tileX.Value = b.tileX.Value;
                        farm.buildings[i].tileY.Value = b.tileY.Value;
                        farm.buildings[i].tilesWide.Value = b.tilesWide.Value;
                        farm.buildings[i].tilesHigh.Value = b.tilesHigh.Value;
                        farm.buildings[i].load();
                    
                    }
                
                }
            
            }

        }

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {

            if (e.NewMenu is CarpenterMenu carp) {

                var blueprints = this.Helper.Reflection.GetField<List<BluePrint>>(carp, "blueprints").GetValue();

                if (Game1.MasterPlayer.mailReceived.Contains("ccPantry")){

                    blueprints.Add(new BluePrint("GreenhouseConstruction_SpecialGreenhouse"));
                
                }
            
            }

        }

        public bool CanEdit<T>(IAssetInfo asset)
        {
            return asset.AssetNameEquals("Data\\Blueprints");
        }

        public string BuildBlueprintData() {

            Dictionary<int, int> requiredMaterials = this.Config.requiredMats;
            string materials = "";
            foreach (KeyValuePair<int, int> entry in requiredMaterials) {

                materials += $"{entry.Key} " + $"{entry.Value} ";
            
            }
            materials = materials.Remove(materials.Length - 1, 1);

            int tilesWide = 7;
            int tilesTall = 3;
            int humanDoorX = 3;
            int humanDoorY = 2;
            int animalDoorX = -1;
            int animalDoorY = -1;
            string mapWarpTo = "GreenhouseConstruction_SpecialGreenhouse";
            string displayName = "Special Greenhouse";
            string description = "A place to grow crops year-round.";
            string bluePrintType = "Buildings";
            string buildingToUpgrade = "none";
            int rectangleX = 110;
            int rectangleY = 159;
            int maxOccupants = 20;
            string actionBehavior = "null";
            
            string namesBuildingLocations = this.Config.buildLocations;

            int goldRequired = this.Config.goldCost;
            bool isMagic = false;
            int daysToBuild = this.Config.daysToBuild;

            string[] outputText = { materials, tilesWide.ToString(), tilesTall.ToString(), humanDoorX.ToString(), humanDoorY.ToString(), animalDoorX.ToString(), animalDoorY.ToString(), mapWarpTo, displayName, description, bluePrintType, buildingToUpgrade, rectangleX.ToString(), rectangleY.ToString(), maxOccupants.ToString(), actionBehavior, namesBuildingLocations, goldRequired.ToString(), isMagic.ToString(), daysToBuild.ToString() };

            string outputString = string.Join("/", outputText);

            return outputString;

        }

        public void Edit<T>(IAssetData asset)
        {
            asset.AsDictionary<string, string>().Data.Add("GreenhouseConstruction_SpecialGreenhouse", BuildBlueprintData());
        }
        
        public bool CanLoad<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Buildings\\GreenhouseConstruction_SpecialGreenhouse") || asset.AssetNameEquals("Maps\\GreenhouseConstruction_SpecialGreenhouse")) {

                return true;
            
            }

            return false;

        }

        public T Load<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Buildings\\GreenhouseConstruction_SpecialGreenhouse"))
            {

                return (T)(object)this.GreenhouseExterior;

            }

            else if (asset.AssetNameEquals("Maps\\GreenhouseConstruction_SpecialGreenhouse"))
            {

                return (T)(object)Game1.content.Load<xTile.Map>("Maps\\Greenhouse");

            }

            return (T)(object)null;

        }

    }
}
