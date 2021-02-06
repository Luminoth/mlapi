using System;

using JetBrains.Annotations;

using pdxpartyparrot.Core.Util;
using pdxpartyparrot.Core.World;

using UnityEngine;
using UnityEngine.Assertions;

namespace pdxpartyparrot.Core.Actors.Components
{
    // TODO: if subclasses could register for specific action types (and we keep a dictionary ActionType => Listener)
    // then that would work out a lot faster and cleaner than how this is currently done
    public abstract class ActorComponent : MonoBehaviour
    {
        [SerializeField]
        [ReadOnly]
        [CanBeNull]
        private Actor _owner;

        [CanBeNull]
        public Actor Owner
        {
            get => _owner;
            protected set => _owner = value;
        }

        public virtual bool CanMove => true;

        #region Unity Lifecycle

        protected virtual void Awake()
        {
        }

        protected virtual void OnDestroy()
        {
        }

        #endregion

        public virtual void Initialize(Actor owner)
        {
            Assert.IsNull(Owner);

            Owner = owner;
        }

        #region Events

        public virtual bool OnThink(float dt)
        {
            return false;
        }

        public virtual bool OnSpawn(SpawnPoint spawnpoint)
        {
            return false;
        }

        public virtual bool OnReSpawn(SpawnPoint spawnpoint)
        {
            return false;
        }

        public virtual bool OnDeSpawn()
        {
            return false;
        }

        public virtual bool CollisionEnter(GameObject collideObject)
        {
            return false;
        }

        public virtual bool CollisionStay(GameObject collideObject)
        {
            return false;
        }

        public virtual bool CollisionExit(GameObject collideObject)
        {
            return false;
        }

        public virtual bool TriggerEnter(GameObject triggerObject)
        {
            return false;
        }

        public virtual bool TriggerStay(GameObject triggerObject)
        {
            return false;
        }

        public virtual bool TriggerExit(GameObject triggerObject)
        {
            return false;
        }

        public virtual bool OnSetFacing(Vector3 direction)
        {
            return false;
        }

        public virtual bool OnMoveStateChanged()
        {
            return false;
        }

        #endregion
    }
}
