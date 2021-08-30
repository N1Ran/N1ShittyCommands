using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Engine.Voxels;
using Sandbox.Game.Entities;
using Sandbox.Game.SessionComponents;
using Sandbox.Game.World;
using Torch.Commands;

namespace N1ShittyCommands.Commands
{
    #if DEBUG
    [Category("voxels")]
    public class Voxels : CommandModule
    {
        //ToDo Make this command run reversion method and refill all voxels not close to grids/players
        [Command("revert", "Reverts all voxel maps")]
        public void RevertAll()
        {
            var voxelMaps = MyEntities.GetEntities().OfType<MyVoxelBase>();
            foreach (var map in voxelMaps)
            {
                MyPlanet.RevertBoulderServer(map);
            }
        }
    }
#endif
}
