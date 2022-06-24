using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using Debug = TaleWorlds.Library.Debug;

namespace SpacePot8tosEditorScripts
{
    public class PathPlacer : ScriptComponentBehavior
    {
        public string pathName;
        public string prefabName;
        public float separation;
        public bool snapToGround;
        public bool alignToPath;
        public Vec3 rotation;
        public Vec3 scale = Vec3.One;
        public Vec3 offest;
        public string parentName;
        public SimpleButton placeEntities;
        public SimpleButton clearCurrentParent;

        private GameEntity _currentParent;

        protected override void OnEditorVariableChanged(string variableName)
        {
            base.OnEditorVariableChanged(variableName);

            if (variableName == "placeEntities")
            {
                if (this.IsValid())
                    this.OnPlaceEntities();
                return;
            }

            if (variableName == "clearCurrentParent")
            {
                this._currentParent = null;
                return;
            }

            if (CanLiveEdit())
            {
                base.Scene.RemoveEntity(_currentParent, 0);
                this.OnPlaceEntities();
                return;
            }
        }

        public bool CanLiveEdit()
        {
            bool canEdit = _currentParent != null;

            return canEdit;
        }

        public bool IsValid()
        {
            bool error = false;
            // separation needs to be greater than zero
            if (separation <= 0)
            {
                MBEditor.AddEntityWarning(base.GameEntity, "PathPlacer -- separation must be greater than zero");
                error = true;
            }

            if (base.Scene.GetPathWithName(pathName) == null)
            {
                MBEditor.AddEntityWarning(base.GameEntity, "PathPlacer -- path does not exist");
                error = true;
            }

            if (!GameEntity.PrefabExists(prefabName))
            {
                MBEditor.AddEntityWarning(base.GameEntity, "PathPlacer -- prefab does not exist");
                error = true;
            }
            return !error;
        }

        private void OnPlaceEntities()
        {
            Path path = base.Scene.GetPathWithName(pathName);
            PathTracker tracker = new PathTracker(path, Vec3.One);
            MatrixFrame currentFrame;
            Vec3 color;
            tracker.CurrentFrameAndColor(out currentFrame, out color);

            MatrixFrame[] pathPoints = new MatrixFrame[2];
            Vec3 angles = currentFrame.rotation.GetEulerAngles();
            float zRotBias = angles.Z;
            path.GetPoints(pathPoints);
            Vec3 forward = (pathPoints[1].origin - pathPoints[0].origin).NormalizedCopy();
            float angleOffset = forward.AsVec2.AngleBetween(Vec2.Forward);
            currentFrame.Rotate(-angleOffset, Vec3.Up);
            currentFrame.Rotate(-zRotBias, Vec3.Up);

            GameEntity parent = null;
            if (parentName != "" && parentName != null)
            {
                parent = GameEntity.Instantiate(base.Scene, "rock_004", currentFrame);
                parent.Name = parentName;
                parent.AddTag("path_placer_parent");
                parent.BreakPrefab();
                _currentParent = parent;
            }
            while (!tracker.HasReachedEnd)
            {
                currentFrame = tracker.CurrentFrame;
                currentFrame.Rotate(-zRotBias, Vec3.Up);
                //currentFrame.Rotate(MBMath.PI, Vec3.Up);
                if (alignToPath)
                {
                    //currentFrame.Rotate(-angleOffset, Vec3.Up);
                    float zAngle = Vec2.Side.AngleBetween(forward.AsVec2);
                    currentFrame.rotation.RotateAboutUp(zAngle);
                    float yAngle = -MathF.Sign(Vec3.DotProduct(forward, Vec3.Up)) * Vec3.AngleBetweenTwoVectors(currentFrame.rotation.s, forward);
                    currentFrame.rotation.RotateAboutForward(yAngle);
                    currentFrame.Rotate(rotation.X.ToRadians(), Vec3.Side);
                    currentFrame.Rotate(rotation.Z.ToRadians(), Vec3.Up);
                    currentFrame.Rotate(rotation.X.ToRadians(), Vec3.Side);
                    currentFrame.Rotate(rotation.Y.ToRadians(), Vec3.Forward);
                    currentFrame.Elevate(offest.Z);
                    currentFrame.Strafe(offest.x);
                    currentFrame.Advance(offest.y);
                }
                else
                {
                    currentFrame.Rotate(rotation.Z.ToRadians(), Vec3.Up);
                    currentFrame.Rotate(rotation.X.ToRadians(), Vec3.Side);
                    currentFrame.Rotate(rotation.Y.ToRadians(), Vec3.Forward);
                    currentFrame.Elevate(offest.Z);
                    currentFrame.Strafe(offest.x);
                    currentFrame.Advance(offest.y);
                }
                currentFrame.Scale(scale);

                if (snapToGround)
                {

                }

                GameEntity entity = GameEntity.Instantiate(base.Scene, prefabName, currentFrame);
                if (parent != null)
                {
                    parent.AddChild(entity, true);
                }
                tracker.Advance(separation);
            }
            MBEditor.UpdateSceneTree();
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
