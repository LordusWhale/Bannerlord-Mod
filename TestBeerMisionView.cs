using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.Missions;
using TaleWorlds.ObjectSystem;

namespace TestMod
{
    public class TestBeerMisionView : MissionView
    {
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);

            if (Input.IsKeyPressed(TaleWorlds.InputSystem.InputKey.Q))
            {
                DrinkBeer();
            }
        }
        private void DrinkBeer()
        {
            if (!(Mission.Mode is MissionMode.Battle or MissionMode.Stealth)) return;
            var itemRoster = MobileParty.MainParty.ItemRoster;
            var testBeerObject = MBObjectManager.Instance.GetObject<ItemObject>("jims_beer");
            if (itemRoster.GetItemNumber(testBeerObject) <= 0) return;
            if (Mission.MainAgent.Health >= Mission.MainAgent.HealthLimit) return;
            itemRoster.AddToCounts(testBeerObject, -1);
            var PreviousHealth = Mission.MainAgent.Health;
            Mission.MainAgent.Health += 20;
            InformationManager.DisplayMessage(new InformationMessage(string.Format("Healed {0}", Mission.MainAgent.Health-PreviousHealth)));
        }
    }

}