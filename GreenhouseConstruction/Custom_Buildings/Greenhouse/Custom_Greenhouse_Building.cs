using System.Collections.Generic;
using Microsoft.Xna.Framework;
using PyTK.CustomElementHandler;
using StardewValley;
using StardewValley.Buildings;

namespace GreenhouseConstruction.Custom_Buildings.Greenhouse
{
    public class Custom_Greenhouse_Building : Building, ISaveElement
    {

        private static readonly BluePrint BluePrint = new BluePrint("SpecialGreenhouse");

        public Custom_Greenhouse_Building() : base(Custom_Greenhouse_Building.BluePrint, Vector2.Zero) {
        

        
        }

        protected override GameLocation getIndoors(string nameOfIndoorsWithoutUnique)
        {
            return new Custom_Greenhouse_Location();
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            return new Dictionary<string, string>();
        }

        public object getReplacement()
        {

            Mill building = new Mill(new BluePrint("SpecialGreenhouse"), new Vector2(this.tileX.Value, this.tileY.Value));
            building.indoors.Value = this.indoors.Value;
            building.daysOfConstructionLeft.Value = this.daysOfConstructionLeft.Value;
            building.tileX.Value = this.tileX.Value;
            building.tileY.Value = this.tileY.Value;
            return building;

        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {

            Mill building = (Mill)replacement;

            this.indoors.Value = building.indoors.Value;
            this.daysOfConstructionLeft.Value = building.daysOfConstructionLeft.Value;
            this.tileX.Value = building.tileX.Value;
            this.tileY.Value = building.tileY.Value;

            this.indoors.Value.map = Game1.content.Load<xTile.Map>("Maps\\GreenhouseInterior");
            this.indoors.Value.updateWarps();
            this.updateInteriorWarps();

        }
    }
}
