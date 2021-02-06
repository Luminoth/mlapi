using pdxpartyparrot.Core.Camera;

namespace pdxpartyparrot.Game.Camera
{
    // TODO: why is this named *player* viewer?

    public interface IPlayerViewer
    {
        Viewer Viewer { get; }
    }
}
