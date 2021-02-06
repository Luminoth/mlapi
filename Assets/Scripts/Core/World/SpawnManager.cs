using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using pdxpartyparrot.Core.Collections;
using pdxpartyparrot.Core.Data;
using pdxpartyparrot.Core.Util;

using UnityEngine;
using UnityEngine.Assertions;

namespace pdxpartyparrot.Core.World
{
    public class SpawnManager : SingletonBehavior<SpawnManager>
    {
        private sealed class SpawnPointSet
        {
            public List<SpawnPoint> SpawnPoints { get; } = new List<SpawnPoint>();

            private int _nextRoundRobinIndex;

            public void SeedRoundRobin()
            {
                _nextRoundRobinIndex = PartyParrotManager.Instance.Random.Next(SpawnPoints.Count);
            }

            [CanBeNull]
            public SpawnPoint GetSpawnPoint(SpawnData.SpawnMethod spawnMethod)
            {
                if(SpawnPoints.Count < 1) {
                    return null;
                }

                // just in case
                if(_nextRoundRobinIndex >= SpawnPoints.Count) {
                    AdvanceRoundRobin();
                }

                switch(spawnMethod) {
                case SpawnData.SpawnMethod.RoundRobin:
                    SpawnPoint spawnPoint = SpawnPoints[_nextRoundRobinIndex];
                    AdvanceRoundRobin();
                    return spawnPoint;
                case SpawnData.SpawnMethod.Random:
                    return PartyParrotManager.Instance.Random.GetRandomEntry(SpawnPoints);
                default:
                    Debug.LogWarning($"Unsupported spawn method {spawnMethod}, using Random");
                    return PartyParrotManager.Instance.Random.GetRandomEntry(SpawnPoints);
                }
            }

            [CanBeNull]
            public SpawnPoint GetNearestSpawnPoint(Vector3 position)
            {
                if(SpawnPoints.Count < 1) {
                    return null;
                }
                return SpawnPoints.NearestManhattan(position, out _);
            }

            private void AdvanceRoundRobin()
            {
                _nextRoundRobinIndex = (_nextRoundRobinIndex + 1) % SpawnPoints.Count;
            }
        }

        [SerializeField]
        private SpawnData _spawnData;

        private readonly Dictionary<string, SpawnData.SpawnPointType> _spawnTypes = new Dictionary<string, SpawnData.SpawnPointType>();

        private readonly Dictionary<string, SpawnPointSet> _spawnPoints = new Dictionary<string, SpawnPointSet>();

        #region Unity Lifecycle

        private void Awake()
        {
            Assert.IsTrue(_spawnData.PlayerSpawnPointTags.Count > 0);

            foreach(var spawnPointType in _spawnData.Types) {
                if(_spawnTypes.ContainsKey(spawnPointType.Tag)) {
                    Debug.LogError($"Duplicate spawn point tag '{spawnPointType.Tag}', ignoring");
                    continue;
                }
                _spawnTypes.Add(spawnPointType.Tag, spawnPointType);
            }
        }

        #endregion

        #region Registration

        public virtual void RegisterSpawnPoint(SpawnPoint spawnPoint)
        {
            //Debug.Log($"Registering spawnpoint {spawnPoint.name} of type '{spawnPoint.Tags}'");

            foreach(string tag in spawnPoint.Tags) {
                _spawnPoints.GetOrAdd(tag).SpawnPoints.Add(spawnPoint);
            }
        }

        public virtual void UnregisterSpawnPoint(SpawnPoint spawnPoint)
        {
            //Debug.Log($"Unregistering spawnpoint '{spawnPoint.name}'");

            foreach(string tag in spawnPoint.Tags) {
                if(_spawnPoints.TryGetValue(tag, out var spawnPoints)) {
                    spawnPoints.SpawnPoints.Remove(spawnPoint);
                }
            }
        }

        #endregion

        public void Initialize()
        {
            Debug.Log("Seeding spawn points...");
            foreach(var kvp in _spawnPoints) {
                kvp.Value.SeedRoundRobin();
            }
        }

        public int SpawnPointCount(string tag)
        {
            if(!_spawnPoints.TryGetValue(tag, out var spawnPoints)) {
                Debug.LogWarning($"No spawn points with tag '{tag}' registered on spawn, are there any in the scene?");
                return 0;
            }

            return spawnPoints.SpawnPoints.Count;
        }

        [CanBeNull]
        public SpawnPoint GetSpawnPoint(string tag)
        {
            if(!_spawnPoints.TryGetValue(tag, out var spawnPoints)) {
                Debug.LogWarning($"No spawn points with tag '{tag}' registered on spawn, are there any in the scene?");
                return null;
            }

            var spawnPointType = _spawnTypes.GetOrDefault(tag);
            return spawnPoints.GetSpawnPoint(spawnPointType.SpawnMethod);
        }

        // gets a random spawnpoint regardless of how the spawnpoints are configured
        [CanBeNull]
        public SpawnPoint GetRandomSpawnPoint(string tag)
        {
            if(!_spawnPoints.TryGetValue(tag, out var spawnPoints)) {
                Debug.LogWarning($"No spawn points with tag '{tag}' registered on spawn, are there any in the scene?");
                return null;
            }
            return spawnPoints.GetSpawnPoint(SpawnData.SpawnMethod.Random);
        }

        // gets the spawnpoint nearest the given position
        [CanBeNull]
        public SpawnPoint GetNearestSpawnPoint(string tag, Vector3 position)
        {
            if(!_spawnPoints.TryGetValue(tag, out var spawnPoints)) {
                Debug.LogWarning($"No spawn points with tag '{tag}' registered on spawn, are there any in the scene?");
                return null;
            }
            return spawnPoints.GetNearestSpawnPoint(position);
        }

        #region Player Spawnpoints

        [CanBeNull]
        public SpawnPoint GetPlayerSpawnPoint(int controllerId)
        {
            int spawnPointIdx = Mathf.Clamp(controllerId, 0, _spawnData.PlayerSpawnPointTags.Count - 1);
            return GetSpawnPoint(_spawnData.PlayerSpawnPointTags.ElementAt(spawnPointIdx));
        }

        // gets a random player spawnpoint regardless of how the spawnpoints are configured
        [CanBeNull]
        public SpawnPoint GetRandomPlayerSpawnPoint(int controllerId)
        {
            int spawnPointIdx = Mathf.Clamp(controllerId, 0, _spawnData.PlayerSpawnPointTags.Count - 1);
            return GetRandomSpawnPoint(_spawnData.PlayerSpawnPointTags.ElementAt(spawnPointIdx));
        }

        // gets the player spawnpoint nearest the given position
        [CanBeNull]
        public SpawnPoint GetNearestPlayerSpawnPoint(int controllerId, Vector3 position)
        {
            int spawnPointIdx = Mathf.Clamp(controllerId, 0, _spawnData.PlayerSpawnPointTags.Count - 1);
            return GetNearestSpawnPoint(_spawnData.PlayerSpawnPointTags.ElementAt(spawnPointIdx), position);
        }

        [CanBeNull]
        public SpawnPoint GetPlayerSpawnPoint(string tag)
        {
            return GetSpawnPoint(tag);
        }

        // gets a random player spawnpoint regardless of how the spawnpoints are configured
        [CanBeNull]
        public SpawnPoint GetRandomPlayerSpawnPoint(string tag)
        {
            return GetRandomSpawnPoint(tag);
        }

        // gets the player spawnpoint nearest the given position
        [CanBeNull]
        public SpawnPoint GetNearestPlayerSpawnPoint(string tag, Vector3 position)
        {
            return GetNearestSpawnPoint(tag, position);
        }

        #endregion
    }
}
