using UnityEngine;
using UnityEngine.Assertions;

namespace pdxpartyparrot.Core.Effects.EffectTriggerComponents
{
    [RequireComponent(typeof(EffectTrigger))]
    public abstract class EffectTriggerComponent : MonoBehaviour
    {
        protected EffectTrigger Owner { get; private set; }

        public abstract bool WaitForComplete { get; }

        public virtual bool IsDone => true;

        public virtual void Initialize(EffectTrigger owner)
        {
            Assert.IsNull(Owner);

            Owner = owner;
        }

        public abstract void OnStart();

        public virtual void OnStop()
        {
        }

        public virtual void OnReset()
        {
        }

        public virtual void OnUpdate(float dt)
        {
        }
    }
}
