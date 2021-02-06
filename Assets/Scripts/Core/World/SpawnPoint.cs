using System;

using JetBrains.Annotations;

using pdxpartyparrot.Core.Actors;
using pdxpartyparrot.Core.Data.Actors.Components;
using pdxpartyparrot.Core.Util;

using UnityEngine;

namespace pdxpartyparrot.Core.World
{
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField]
        private string[] _tags;

        public string[] Tags => _tags;

        [SerializeField]
        private FloatRangeConfig _spawnRange;

        [SerializeField]
        [ReadOnly]
        private Actor _owner;

        private Action _onRelease;

        #region Unity Lifecycle

        protected virtual void OnEnable()
        {
            Register();
        }

        protected virtual void OnDisable()
        {
            Release();
            Unregister();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, _spawnRange.Max <= 0.0f ? 1.0f : _spawnRange.Max);
        }

        #endregion

        private void Register()
        {
            if(enabled && SpawnManager.HasInstance) {
                SpawnManager.Instance.RegisterSpawnPoint(this);
            }
        }

        private void Unregister()
        {
            if(enabled && SpawnManager.HasInstance) {
                SpawnManager.Instance.UnregisterSpawnPoint(this);
            }
        }

        protected virtual void InitActor(Actor actor)
        {
            Transform actorTransform = actor.transform;
            Transform thisTransform = transform;

            Vector3 offset = new Vector3(_spawnRange.GetRandomValue() * PartyParrotManager.Instance.Random.NextSign(),
                                         0.0f,
                                         _spawnRange.GetRandomValue() * PartyParrotManager.Instance.Random.NextSign());

            actorTransform.position = thisTransform.position + offset;
            actorTransform.rotation = thisTransform.rotation;

            actor.gameObject.SetActive(true);
        }

        private void InitActor(Actor actor, Guid id, ActorBehaviorComponentData behaviorData)
        {
            actor.Initialize(id);

            InitActor(actor);

            if(null != actor.Behavior && null != behaviorData) {
                actor.Behavior.Initialize(behaviorData);
            }
        }

        [CanBeNull]
        public Actor SpawnFromPrefab(Actor prefab, ActorBehaviorComponentData behaviorData, Transform parent = null, bool activate = true)
        {
            return SpawnFromPrefab(prefab, Guid.NewGuid(), behaviorData, parent, activate);
        }

        [CanBeNull]
        public Actor SpawnFromPrefab(Actor prefab, Guid id, ActorBehaviorComponentData behaviorData, Transform parent = null, bool activate = true)
        {
#if USE_NETWORKING
            Debug.LogWarning("You probably meant to use NetworkManager.SpawnNetworkPrefab");
#endif

            Actor actor = Instantiate(prefab, parent);
            actor.gameObject.SetActive(activate);

            if(!Spawn(actor, id, behaviorData)) {
                Destroy(actor);
                return null;
            }
            return actor;
        }

        [CanBeNull]
        public Actor SpawnNPCPrefab(Actor prefab, ActorBehaviorComponentData behaviorData, Transform parent = null, bool active = true)
        {
            return SpawnFromPrefab(prefab, behaviorData, parent, active);
        }

        public bool Spawn(Actor actor, Guid id, ActorBehaviorComponentData behaviorData)
        {
            InitActor(actor, id, behaviorData);

            return actor.OnSpawn(this);
        }

        public bool SpawnPlayer(Actor actor)
        {
            InitActor(actor);

            // players spawn and then deactivate
            // so that the level can respawn them as it needs to

            bool ret = actor.OnSpawn(this);
            actor.gameObject.SetActive(false);
            return ret;
        }

        public bool ReSpawn(Actor actor)
        {
            InitActor(actor);

            return actor.OnReSpawn(this);
        }

        public bool Acquire(Actor owner, Action onRelease = null, bool force = false)
        {
            if(!force && null != _owner) {
                return false;
            }

            Release();

            _owner = owner;
            _onRelease = onRelease;

            Unregister();

            return true;
        }

        public void Release()
        {
            if(null == _owner) {
                return;
            }

            _onRelease?.Invoke();
            _owner = null;

            Register();
        }
    }
}
