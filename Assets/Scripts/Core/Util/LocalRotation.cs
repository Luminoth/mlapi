using UnityEngine;

namespace pdxpartyparrot.Core.Util
{
    public class LocalRotation : MonoBehaviour
    {
        [SerializeField]
        private Vector3 _rotation;

        [SerializeField]
        private float _speed = 100.0f;

        private Transform _transform;

        #region Unity Lifecycle

        private void Awake()
        {
            _transform = transform;
        }

        private void Update()
        {
            float dt = UnityEngine.Time.deltaTime;

            Vector3 rotation = _rotation * _speed * dt;
            _transform.Rotate(rotation, Space.Self);
        }

        #endregion
    }
}
