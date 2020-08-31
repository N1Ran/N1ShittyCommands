using Torch.Commands;

namespace N1ShittyCommands.Commands
{
    [Category("reputation")]
    public class Reputation : CommandModule
    {
        [Command("reset", "resets all reputations pairs to default")]
        public void ResetRep()
        {
            Context.Respond("I don't do shit yet.");
        }
    }
}