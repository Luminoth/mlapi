namespace pdxpartyparrot.Game.World
{
    public interface IWorldBoundaryCollisionListener
    {
        void OnWorldBoundaryCollisionEnter(WorldBoundary boundary);

        void OnWorldBoundaryCollisionExit(WorldBoundary boundary);
    }
}
