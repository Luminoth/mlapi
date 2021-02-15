﻿using Cinemachine;

using pdxpartyparrot.Core.Actors;
using pdxpartyparrot.Core.Camera;
using pdxpartyparrot.Game.Data;

using UnityEngine;
using UnityEngine.Assertions;

namespace pdxpartyparrot.Game.Camera
{
    //[RequireComponent(typeof(CinemachineFramingTransposer))]
    //[RequireComponent(typeof(CinemachineConfiner))]
    public class SideScrollerViewer : CinemachineViewer, IPlayerViewer
    {
        [Space(10)]

        [Header("Target")]

        [SerializeField]
        private CinemachineTargetGroup _targetGroup;

        public Viewer Viewer => this;

        private CinemachineFramingTransposer _transposer;

        private CinemachineConfiner _confiner;

        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();

            _transposer = GetCinemachineComponent<CinemachineFramingTransposer>();
            Assert.IsNotNull(_transposer, "Set Body to Framing Transposer");

            _confiner = GetComponent<CinemachineConfiner>();
            Assert.IsNotNull(_confiner, "Add a Confiner Extension");
        }

        #endregion

        public virtual void Initialize(GameData gameData)
        {
            Set2D(gameData.ViewportSize);
            _confiner.m_ConfineScreenEdges = true;

            _transposer.m_GroupFramingMode = CinemachineFramingTransposer.FramingMode.HorizontalAndVertical;
            _transposer.m_MinimumOrthoSize = gameData.ViewportSize;
            _transposer.m_MaximumOrthoSize = gameData.ViewportSize * 2.0f;
        }

        public void SetBounds(Collider2D bounds)
        {
            Debug.Log("Setting viewer bounds");

            _confiner.m_BoundingShape2D = bounds;
        }

        public void AddTarget(Actor actor, float weight = 1.0f)
        {
            Debug.Log($"Adding viewer target {actor.Id}");

            _targetGroup.AddMember(actor.transform, weight, actor.Radius);
        }

        public void RemoveTarget(Actor actor)
        {
            Debug.Log($"Removing viewer target {actor.Id}");

            _targetGroup.RemoveMember(actor.transform);
        }
    }
}
