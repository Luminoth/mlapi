using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using pdxpartyparrot.Core.Effects;
using pdxpartyparrot.Core.Util;

using UnityEngine;

namespace pdxpartyparrot.Game.Data.NPCs
{
    [Serializable]
    public abstract class SpawnWaveData : ScriptableObject
    {
        [SerializeField]
        private SpawnGroupData[] _spawnGroups;

        public IReadOnlyCollection<SpawnGroupData> SpawnGroups => _spawnGroups;

        #region Effects

        [SerializeField]
        [CanBeNull]
        private EffectTrigger _waveStartEffectTriggerPrefab;

        [CanBeNull]
        public EffectTrigger WaveStartEffectTriggerPrefab => _waveStartEffectTriggerPrefab;

        [SerializeField]
        [CanBeNull]
        private EffectTrigger _waveEndEffectTriggerPrefab;

        [CanBeNull]
        public EffectTrigger WaveEndEffectTriggerPrefab => _waveEndEffectTriggerPrefab;

        #endregion
    }
}
