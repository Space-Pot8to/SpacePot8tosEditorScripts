using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SpacePot8tosEditorScripts
{
    public class SettlementComponentChecker : ScriptComponentBehavior
    {
        public SimpleButton checkTowns;
        public SimpleButton checkCastles;
        public SimpleButton checkVillages;
        public SimpleButton checkHideouts;

        protected override void OnEditorVariableChanged(string variableName)
        {
            if(variableName == "checkTowns")
            {
                CheckTowns();
            }
        }

        public void CheckTowns()
        {
            List<GameEntity> towns = new List<GameEntity>();
            base.Scene.GetEntities(ref towns);
            towns = towns.Where(x => x.Name.Contains("town") && x.Parent == null).ToList();
            Dictionary<GameEntity, List<string>> errors = new Dictionary<GameEntity, List<string>>();
            foreach (GameEntity strategicEntity in towns)
            {
                errors[strategicEntity] = new List<string>();
                List<GameEntity> children = new List<GameEntity>();
                strategicEntity.GetChildrenRecursive(ref children);
                if (children.Count == 0)
                {
                    errors[strategicEntity].Add("town " + strategicEntity.Name + " has no children!");
                    return;
                }

                // check attacker siege engines for each level
                List<GameEntity> attackerSiegeEngines = children
                                                        .Where(x => x
                                                            .Tags.Any(y => y
                                                                .Contains("map_siege_engine")))
                                                        .ToList();

                if(attackerSiegeEngines.Count != 4)
                    errors[strategicEntity].Add("town " + strategicEntity.Name + " doesn't have 4 attacker siege engine children");

                // check defender siege engines for each level
                List<GameEntity> defenderSiegeEngines = children.FindAll(x => x.Tags.Any(y => y.Contains("map_defensive_engine")));
                if (defenderSiegeEngines.Count != 12)
                    errors[strategicEntity].Add("town " + strategicEntity.Name + " doesn't have 12 children with the \"map_defensive_siege_engine\" tag");
                else
                {
                    
                    for (uint i = 1; i < 4; i++)
                    {
                        List<GameEntity> defEngines = defenderSiegeEngines.FindAll(x => x.GetUpgradeLevelOfEntity() == i);
                        if (defEngines.Count != 4)
                            errors[strategicEntity].Add("town " + strategicEntity.Name + " doesn't have 4 defender siege engines for level_" +i);
                    }
                }

                // check for battering ram
                List<GameEntity> batteringRam = children.Where(x => x.HasTag("map_siege_ram")).ToList();
                if (batteringRam.Count != 1)
                    errors[strategicEntity].Add("for town " + strategicEntity.Name + " count of children with the \"map_siege_ram\" tag is not 1");

                // check for siege towers
                List<GameEntity> siegeTowers = children.Where(x => x.HasTag("map_siege_tower")).ToList();
                if(siegeTowers.Count != 2)
                    errors[strategicEntity].Add("for town " + strategicEntity.Name + " count of children with the \"map_siege_tower\" tag is not 2");

                // check for breaches
                List<GameEntity> breaches = children.Where(x => x.HasTag("map_breachable_wall")).ToList();
                if (breaches.Count != 6)
                    errors[strategicEntity].Add("for town " + strategicEntity.Name + " count of children with the  \"map_breachable_wall\" tag is not 6");

                // bo_town
                List<GameEntity> townBos = children.FindAll(x => x.HasTag("bo_town"));
                if(townBos.Count < 1)
                    errors[strategicEntity].Add("town " + strategicEntity.Name + " doesn't have at least 1 child with the  \"bo_town\" tag");

                // main gate
                List<GameEntity> mainGate = children.FindAll(x => x.HasTag("main_map_city_gate"));
                if (mainGate.Count != 1)
                    errors[strategicEntity].Add("for town " + strategicEntity.Name + " count of children with the \"main_map_city_gate\" tag is not 1");
                else 
                {
                    Vec2 pos = mainGate[0].GlobalPosition.AsVec2;
                    PathFaceRecord pathFaceRecord = new PathFaceRecord(-1, -1, -1);
                    base.GameEntity.Scene.GetNavMeshFaceIndex(ref pathFaceRecord, pos, true, false);
                    int num = pathFaceRecord.FaceGroupIndex;
                    bool isFaceValid = pathFaceRecord.IsValid();
                    bool isTerrainValid = (num == 0 || num == 7 || num == 8 || num == 10 || num == 11 || num == 13 || num == 14);
                    if (!isFaceValid )//|| !isTerrainValid)
                        errors[strategicEntity].Add("main gate for town " + strategicEntity.Name + " is not on a valid navmesh face");
                }

                // map settlement circle
                List<GameEntity> settlementCircle = children.FindAll(x => x.HasTag("map_settlement_circle"));
                if (settlementCircle.Count != 1)
                    errors[strategicEntity].Add("town " + strategicEntity.Name + " more or less than one child with the \"map_settlement_circle\" tag");

                // banner holders
                List<GameEntity> bannerHolders = children.FindAll(x => x.HasTag("map_banner_placeholder"));
                foreach(GameEntity bannerHolder in bannerHolders)
                {
                    GameEntity child = bannerHolder.GetChildren().FirstOrDefault();
                    if (child == null) 
                    {
                        int level = bannerHolder.GetUpgradeLevelOfEntity();
                        errors[strategicEntity].Add("for town " + strategicEntity.Name + " banner_pos for level_" + level + " has no child");
                    }
                    else if (!child.HasScriptOfType<VolumeBox>())
                    {
                        int level = bannerHolder.GetUpgradeLevelOfEntity();
                        errors[strategicEntity].Add("for town " + strategicEntity.Name + " the child of banner_pos for level_" + level + " has no VolumeBox script");
                    }
                }
                List<GameEntity> generalHolders = bannerHolders.FindAll(x => x.Parent.GetUpgradeLevelOfEntity() == 0);
                List<GameEntity> l1Holders = bannerHolders.FindAll(x => x.Parent.GetUpgradeLevelOfEntity() == 1);
                List<GameEntity> l2Holders = bannerHolders.FindAll(x => x.Parent.GetUpgradeLevelOfEntity() == 2);
                List<GameEntity> l3Holders = bannerHolders.FindAll(x => x.Parent.GetUpgradeLevelOfEntity() == 3);
                if (l1Holders.Count == 0 && generalHolders.Count == 0)
                    errors[strategicEntity].Add("town " + strategicEntity.Name + " has no children with the tag \"map_banner_placeholder\" for level_1");
                if (l2Holders.Count == 0 && generalHolders.Count == 0)
                    errors[strategicEntity].Add("town " + strategicEntity.Name + " has no children with the tag \"map_banner_placeholder\" for level_2");
                if (l3Holders.Count == 0 && generalHolders.Count == 0)
                    errors[strategicEntity].Add("town " + strategicEntity.Name + " has no children with the tag \"map_banner_placeholder\" for level_3");

                // camp areas
                List<GameEntity> campArea1 = children.FindAll(x => x.HasTag("map_camp_area_1"));
                List<GameEntity> campArea2 = children.FindAll(x => x.HasTag("map_camp_area_2"));
                if(campArea1.Count < 1)
                    errors[strategicEntity].Add("town " + strategicEntity.Name + " has no children with the \"map_camp_area_1\" tag");
                if (campArea2.Count < 1)
                    errors[strategicEntity].Add("town " + strategicEntity.Name + " has no children with the \"map_camp_area_2\" tag");

                // remove entity from errors if no errors are recorded
                if (errors[strategicEntity].Count == 0)
                    errors.Remove(strategicEntity);
            }


            foreach (KeyValuePair<GameEntity, List<string>> e in errors)
            {
                GameEntity entity = e.Key;
                List<string> lst = e.Value;
                foreach(string s in lst)
                    MBEditor.AddEntityWarning(entity, s);
            }
            if (errors.Count == 0)
            {
                MBEditor.AddEditorWarning("All towns have necessary entities with proper tags");
            }
        }

        protected override void OnInit()
        {
            base.OnInit();
            base.SetScriptComponentToTick(this.GetTickRequirement());
        }
        protected override void OnEditorInit()
        {
            Debug.Print("PathPlacer is available");
            base.OnEditorInit();
        }

        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            return ScriptComponentBehavior.TickRequirement.Tick;
        }
    }
}
