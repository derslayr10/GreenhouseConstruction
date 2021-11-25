using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenhouseConstruction.Config
{
    class ModConfig
    {

        public Dictionary<int, int> requiredMats { get; set; }

        public int goldCost { get; set; }

        public int daysToBuild { get; set; }

        public string buildLocations { get; set; }

        public ModConfig() {

            this.requiredMats = new Dictionary<int, int>() { { 390, 300 }, { 709, 100 }, { 335, 50 } };

            this.goldCost = 100000;

            this.daysToBuild = 2;

            this.buildLocations = "Farm";
        
        }

    }
}
