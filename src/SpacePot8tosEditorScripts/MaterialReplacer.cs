using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace SpacePot8tosEditorScripts
{
    public class MaterialReplacer : ScriptComponentBehavior
    {
        public Material targetMaterial;
        public Material replacementMaterial;
        public SimpleButton switchMaterials;

        public string tag;
        public string entityName;
        public string scriptName;
        public bool searchChildren;

        public SimpleButton selectTargetEntities;
        public SimpleButton clearTargetEntities;
        public bool showTargetEntities;
        public SimpleButton replaceMaterial;

        private List<GameEntity> _selectedEntities;

        protected override void OnEditorVariableChanged(string variableName)
        {
            if (variableName == "selectTargetEntities")
            {
                SelectTargetEntities();
            }

            if (variableName == "clearTargetEntities")
            {
                _selectedEntities?.Clear();
            }

            if (variableName == "showTargetEntities")
            {
                if (showTargetEntities)
                    _selectedEntities.ForEach(x => x.SelectEntityOnEditor());
                else
                    _selectedEntities.ForEach(x => x.DeselectEntityOnEditor());
            }

            if (variableName == "switchMaterials")
            {
                this.SwitchMaterials();
            }

            if (variableName == "replaceMaterial")
            {
                if (_selectedEntities == null || _selectedEntities.Count < 1)
                {
                    MBEditor.AddEntityWarning(base.GameEntity, "MaterialReplacer -- no selected entities");
                    return;
                }
                ReplaceMaterial();
            }
            MBEditor.UpdateSceneTree();
        }

        private void SwitchMaterials()
        {
            if (this.targetMaterial != null && this.replacementMaterial != null)
            {
                Material temp = this.targetMaterial;
                this.targetMaterial = this.replacementMaterial;
                this.replacementMaterial = temp;
            }
        }

        private void SelectTargetEntities()
        {
            _selectedEntities = new List<GameEntity>();
            base.Scene.GetEntities(ref _selectedEntities);
            // entities with at least one mesh
            // _selectedEntities = allEntities.Where(x => x.GetComponentCount(GameEntity.ComponentType.MetaMesh) > 0).ToList();
            // filter by tag
            if (tag != "" && tag != null)
            {
                _selectedEntities = _selectedEntities.Where(x => x.HasTag(tag)).ToList();
            }
            // filter by entity name
            if (entityName != "" && entityName != null)
            {
                _selectedEntities = _selectedEntities.Where(x => x.Name == entityName).ToList();
            }
            // filter by script name
            if (scriptName != "" && scriptName != null)
            {
                _selectedEntities = _selectedEntities.Where(x => x.HasScriptComponent(scriptName)).ToList();
            }
            // search children of selected parents
            if (searchChildren)
            {
                List<GameEntity> toAdd = new List<GameEntity>();
                foreach(GameEntity entity in _selectedEntities)
                {
                    List<GameEntity> children = entity.GetChildren().ToList();
                    toAdd.AppendList(children);
                }
                _selectedEntities.AppendList(toAdd);
            }
            // make sure we only deal with entities that have meshes
            _selectedEntities = _selectedEntities.Where(x => x.GetComponentCount(GameEntity.ComponentType.MetaMesh) > 0).ToList();
        }

        private void ReplaceMaterial()
        {
            foreach(GameEntity entity in _selectedEntities)
            {
                int metaMeshes = entity.GetComponentCount(GameEntity.ComponentType.MetaMesh);
                for (int i = 0; i < metaMeshes; i++)
                {
                    MetaMesh metaMesh = entity.GetMetaMesh(0);
                    for (int j = 0; j < metaMesh.MeshCount; j++)
                    {
                        Mesh mesh = metaMesh.GetMeshAtIndex(j);
                        if (mesh.GetMaterial().Name.Contains(this.targetMaterial.Name))
                        {
                            Material mat = this.replacementMaterial.CreateCopy();
                            mesh.SetMaterial(mat);
                        }
                    }
                }
            }
        }
        protected override void OnInit()
        {
            base.OnInit();
            base.SetScriptComponentToTick(this.GetTickRequirement());
            this._selectedEntities = new List<GameEntity>();
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
