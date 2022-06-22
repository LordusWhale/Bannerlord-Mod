using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.CampaignSystem.Settlements.Workshops;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace TestMod
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

        }
        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            base.OnMissionBehaviorInitialize(mission);
            mission.AddMissionBehavior(new TestBeerMisionView());
        }
        protected override void InitializeGameStarter(Game game, IGameStarter starterObject)
        {
            if (starterObject is CampaignGameStarter starter)
            {
                starter.AddBehavior(new TestBeerBehavior());
            }
        }

    }

    public class TestBeerBehavior : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnWorkshopChangedEvent.AddNonSerializedListener(this, OnWorkshopChangedEvent);
            CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, DailyTick);
            CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener(this, LocationCharactersAreReadyToSpawn);
            CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener(this, JimsCrossbowDealer);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        private void JimsCrossbowDealer(Dictionary<string, int> unusedUsablePointCount)
        {
            Location locationWithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("center");
            if (!(CampaignMission.Current.Location == locationWithId && CampaignTime.Now.IsDayTime)) return;


            Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
            foreach (Workshop workshop in settlement.Town.Workshops)
            {
                if (workshop.IsRunning && workshop.WorkshopType.StringId == "brewery")
                {
                    int num;
                    unusedUsablePointCount.TryGetValue(workshop.Tag, out num);
                    if (num > 0f)
                    {
                        CharacterObject caravanMaster = Settlement.CurrentSettlement.Culture.ArmedTrader;
                        LocationCharacter locationCharacter = new LocationCharacter(new AgentData
                            (new SimpleAgentOrigin(caravanMaster)).Monster(Campaign.Current.HumanMonsterSettlement),
                            new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors),
                            workshop.Tag, true, LocationCharacter.CharacterRelations.Neutral, null, true, false, null, false, false, true);
                        locationWithId.AddCharacter(locationCharacter);


                    }
                }
            }
        }

        ItemObject _testBeer;
        ItemObject _jimsCrossbow;
        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            _testBeer = MBObjectManager.Instance.GetObject<ItemObject>("jims_beer");
            _jimsCrossbow = MBObjectManager.Instance.GetObject<ItemObject>("jims_crossbow");
            AddDialogs(starter);
        }

        private void AddDialogs(CampaignGameStarter starter)
        {
            {
                starter.AddPlayerLine("tavernkeeper_talk_ask_test_beer", "tavernkeeper_talk", "tavernkeeper_test_beer", "Do you sell test beer?", null, null);
                starter.AddDialogLine("taverkeeper_talk_test_beer", "tavernkeeper_test_beer", "tavernkeeper_talk_no_beer", "Bah. Greedy bastard at the brewery doesn't want to sell his stuff to me. Something about getting better rates selling directly to the customer...", null, null);

            }
            {
                starter.AddDialogLine("test_brewer_talk", "start", "test_brewer", "Howdy. Would you like to purchase some Jims Beer?", () => 
                {
                    try
                    {
                        if (CharacterObject.OneToOneConversationCharacter == Settlement.CurrentSettlement.Culture.CaravanMaster)
                        {
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                    return false;
                }, null);
                starter.AddPlayerLine("test_brewer_buy", "test_brewer", "test_brewer_purchased", "Sure, I'll take one.", null, () => 
                {
                    Hero.MainHero.ChangeHeroGold(-200);
                    MobileParty.MainParty.ItemRoster.AddToCounts(_testBeer, 1);
                    InformationManager.DisplayMessage(new InformationMessage("Added 1 Jims Beer to inventory."));
                }, 100, (out TextObject explanation) =>
                {
                     if (Hero.MainHero.Gold < 200)  
                    {
                        explanation = new TextObject("Not enough money.");
                        return false;
                    }
                    else
                    {
                        explanation = TextObject.Empty;
                        return true;
                    }
                }) ;

                starter.AddDialogLine("test_beer_thanks", "test_brewer_purchased", "end", "Thank you come again!", null, null);

                starter.AddPlayerLine("test_brewer_buy_refuse", "test_brewer", "test_brewer_declined", "Nah, I'm good, thanks.", null, null);
                starter.AddDialogLine("test_brewer_your_loss", "test_brewer_declined", "end", "Your loss!", null, null);
            }
            {
                starter.AddDialogLine("crossbow_dealer_talk", "start", "crossbow_dealer", "Howdy. Would you like to purchase a crossbow for 10000g?", () =>
                {
                    try
                    {
                        if (CharacterObject.OneToOneConversationCharacter == Settlement.CurrentSettlement.Culture.ArmedTrader)
                        {
                            return true;
                        }
                    }
                    catch (Exception ex)
                    {
                        return false;
                    }
                    return false;
                }, null);
                starter.AddPlayerLine("crossbow_dealer_buy", "crossbow_dealer", "crossbow_dealer_purchased", "Sure, I'll take one.", null, () =>
                {
                    Hero.MainHero.ChangeHeroGold(-10000);
                    MobileParty.MainParty.ItemRoster.AddToCounts(_jimsCrossbow, 1);
                    InformationManager.DisplayMessage(new InformationMessage("Added 1 Jims Crossbow to inventory."));
                }, 100, (out TextObject explanation) =>
                {
                    if (Hero.MainHero.Gold < 10000)
                    {
                        explanation = new TextObject("Not enough money.");
                        return false;
                    }
                    else
                    {
                        explanation = TextObject.Empty;
                        return true;
                    }
                });

                starter.AddDialogLine("crossbow_dealer_thanks", "crossbow_dealer_purchased", "end", "Thank you come again!", null, null);

                starter.AddPlayerLine("crossbow_dealer_buy_refuse", "crossbow_dealer", "crossbow_dealer_declined", "Nah, I'm good, thanks.", null, null);
                starter.AddDialogLine("crossbow_dealer_your_loss", "crossbow_dealer_declined", "end", "Your loss!", null, null);
            }
        }
        private void LocationCharactersAreReadyToSpawn(Dictionary<string, int> unusedUsablePointCount)
        {
            Location locationWithId = Settlement.CurrentSettlement.LocationComplex.GetLocationWithId("center");
            if (!(CampaignMission.Current.Location == locationWithId && CampaignTime.Now.IsDayTime)) return;


            Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
            foreach (Workshop workshop in settlement.Town.Workshops)
            {
                if (workshop.IsRunning && workshop.WorkshopType.StringId == "brewery")
                {
                    int num;
                    unusedUsablePointCount.TryGetValue(workshop.Tag, out num);
                    if (num > 0f)
                    {
                        CharacterObject caravanMaster = Settlement.CurrentSettlement.Culture.CaravanMaster;
                        LocationCharacter locationCharacter = new LocationCharacter(new AgentData
                            (new SimpleAgentOrigin(caravanMaster)).Monster(Campaign.Current.HumanMonsterSettlement),
                            new LocationCharacter.AddBehaviorsDelegate(SandBoxManager.Instance.AgentBehaviorManager.AddWandererBehaviors),
                            workshop.Tag, true, LocationCharacter.CharacterRelations.Neutral, null, true, false, null, false, false, true);
                        locationWithId.AddCharacter(locationCharacter);


                    }
                }
            }

        }

        private void DailyTick(Town town)
        {
            var hero = Hero.MainHero;
            foreach (var workshop in town.Workshops)
            {
                //InformationManager.DisplayMessage(new InformationMessage(String.Format("{0} had a workshop {1}", town.Name, workshop.Name)));
                if (workshop.Owner == hero)
                {
                    workshop.ChangeGold(500000);
                }

            }
        }

        private void OnWorkshopChangedEvent(Workshop workshop, Hero oldOwningHero, WorkshopType type)
        {

        }

        public override void SyncData(IDataStore dataStore)
        {

        }
    }

}