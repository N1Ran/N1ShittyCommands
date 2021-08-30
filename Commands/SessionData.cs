using Sandbox.Game.SessionComponents;
using Sandbox.Game.World;
using Torch.Commands;
using VRage.Game.ObjectBuilders.Components;

namespace N1ShittyCommands.Commands
{
    [Category("session")]
    public class SessionData:CommandModule
    {
        [Command("reset playercontainerdata", "fixes player no longer seeing unknown signals")]
        public void ResetContainer()
        {
            if (!(MySession.Static.GetComponent<MySessionComponentContainerDropSystem>().GetObjectBuilder() is MyObjectBuilder_SessionComponentContainerDropSystem containerDropSystem))
            {
                Context.Respond("DropSys is null");
                return;
            }

            var result = 0;
            foreach (var data in containerDropSystem.PlayerData)
            {
                if (data.Active) continue;
                data.Active = true;
                result++;
            }
            Context.Respond($"reset {result} player container data");
        }

    }
}