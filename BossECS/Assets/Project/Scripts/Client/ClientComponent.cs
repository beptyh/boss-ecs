using Unity.Entities;
using Common;

namespace Assets.Project.Scripts
{
    public struct ClientTeamRequest: IComponentData
    {
        public TeamType Value;        
    }

    public struct SceneLoadedTag : IComponentData { }
}
