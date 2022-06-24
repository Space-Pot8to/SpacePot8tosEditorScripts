using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.Library;

namespace SpacePot8tosEditorScripts
{
    public class MassVectorArgumentSetter : ScriptComponentBehavior
    {
        public bool setVec1X;
        public float vec1X;
        public bool setVec1Y;
        public float vec1Y;
        public bool setVec1Z;
        public float vec1Z;
        public bool setVec1W;
        public float vec1W;

        public bool setVec2X;
        public float vec2X;
        public bool setVec2Y;
        public float vec2Y;
        public bool setVec2Z;
        public float vec2Z;

        public string tag;

        public SimpleButton getTargetEntities;
        public SimpleButton clearTargetEntities;
        public bool showTargetEntities;
        public SimpleButton setVectorArguments;

        private List<GameEntity> _selectedEntities;

        protected override void OnEditorVariableChanged(string variableName)
        {
            base.OnEditorVariableChanged(variableName);

            if (variableName == "getTargetEntities")
            {
                SelectEntities();
            }

            if (variableName == "clearTargetEntities")
            {
                _selectedEntities?.Clear();
            }

            if (variableName == "showTargetEntities")
            {
                if(showTargetEntities)
                    _selectedEntities.ForEach(x => x.SelectEntityOnEditor());
                else
                    _selectedEntities.ForEach(x => x.DeselectEntityOnEditor());
            }

            if (variableName == "setVectorArguments")
            {
                SetVectorArguments();
            }
            
        }

        private void SelectEntities()
        {
            List<GameEntity> allEntities = new List<GameEntity>();
            base.Scene.GetEntities(ref allEntities);
            // entities with at least one mesh
            _selectedEntities = allEntities.Where(x => x.GetComponentCount(GameEntity.ComponentType.MetaMesh) > 0).ToList();
            if (tag != "" && tag != null)
            {
                _selectedEntities = _selectedEntities.Where(x => x.HasTag(tag)).ToList();
            }
        }

        private void SetVectorArguments()
        {
            foreach(GameEntity entity in _selectedEntities)
            {
                MetaMesh mesh = entity.GetMetaMesh(0);
                Vec3 vec1 = mesh.VectorUserData;
                Vec3 vec2 = mesh.GetVectorArgument2();

                Vec3 newVec1 = new Vec3(setVec1X ? vec1X : vec1.X,
                                        setVec1Y ? vec1Y : vec1.Y,
                                        setVec1Z ? vec1Z : vec1.Z,
                                        setVec1W ? vec1W : vec1.w);
                mesh.SetVectorArgument(newVec1.X, newVec1.Y, newVec1.Z, newVec1.w);
            }
        }

        protected override void OnInit()
        {
            base.OnInit();
            base.SetScriptComponentToTick(this.GetTickRequirement());
        }
        protected override void OnEditorInit()
        {
            Debug.Print("MassVectorArgumentSetter is available");
            base.OnEditorInit();
        }

        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            return ScriptComponentBehavior.TickRequirement.Tick;
        }
    }
}
