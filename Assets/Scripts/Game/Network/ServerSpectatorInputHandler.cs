using pdxpartyparrot.Core.DebugMenu;
using pdxpartyparrot.Core.Input;
using pdxpartyparrot.Core.Util;

using UnityEngine;
using UnityEngine.InputSystem;

namespace pdxpartyparrot.Game.Network
{
    [RequireComponent(typeof(PlayerInput))]
    public class ServerSpectatorInputHandler : MonoBehaviour
    {
        private PlayerInput _input;

        // TODO: this isn't used anywhere
        [SerializeField]
        private float _mouseSensitivity = 0.5f;

        public float MouseSensitivity => _mouseSensitivity;

        [SerializeField]
        [ReadOnly]
        private Vector3 _lastMove;

        public Vector3 LastMove => _lastMove;

        [SerializeField]
        [ReadOnly]
        private Vector3 _lastLook;

        public Vector3 LastLook => _lastLook;

        [SerializeField]
        [ReadOnly]
        private bool _pollMove;

        [SerializeField]
        [ReadOnly]
        private bool _pollLook;

        private InputAction _moveAction;

        private InputAction _lookAction;

        private DebugMenuNode _debugMenuNode;

        #region Unity Lifecycle

        private void Awake()
        {
            _input = GetComponent<PlayerInput>();
            _input.SwitchCurrentControlScheme(InputManager.Instance.InputData.KeyboardAndMouseScheme, Keyboard.current, Mouse.current);

            _moveAction = _input.actions.FindAction(InputManager.Instance.InputData.MoveActionName);
            if(null == _moveAction) {
                Debug.LogWarning("Missing move action");
            }

            _lookAction = _input.actions.FindAction(InputManager.Instance.InputData.LookActionName);
            if(null == _lookAction) {
                Debug.LogWarning("Missing look action");
            }

            InitDebugMenu();
        }

        private void OnDestroy()
        {
            DestroyDebugMenu();
        }

        private void Update()
        {
            if(_pollMove) {
                DoPollMove();
            }

            if(_pollLook) {
                DoPollLook();
            }
        }

        #endregion

        // TODO: fix the naming conflict between these
        private bool InputAllowed => Application.isFocused;

        private bool IsInputAllowed(InputAction.CallbackContext ctx)
        {
            if(!InputAllowed) {
                return false;
            }

            // ignore keyboard/mouse while the debug menu is open
            if(DebugMenuManager.Instance.Enabled && (ctx.control.device == Keyboard.current || ctx.control.device == Mouse.current)) {
                return false;
            }

            return true;
        }

        private void DoMove(InputAction action)
        {
            Vector2 axes = action.ReadValue<Vector2>();

            // translate movement from x / y to x / z
            _lastMove = new Vector3(axes.x, 0.0f, axes.y);
        }

        private void DoPollMove()
        {
            if(!InputAllowed || null == _moveAction) {
                return;
            }

            DoMove(_moveAction);
        }

        private void DoLook(InputAction action)
        {
            Vector2 axes = action.ReadValue<Vector2>();
            _lastLook = new Vector3(axes.x, axes.y, 0.0f);
        }

        private void DoPollLook()
        {
            if(!InputAllowed || null == _lookAction) {
                return;
            }

            DoLook(_lookAction);
        }

        #region Common Actions

        public void OnMoveAction(InputAction.CallbackContext context)
        {
            if(!IsInputAllowed(context)) {
                return;
            }

            /*if(Core.Input.InputManager.Instance.EnableDebug) {
                Debug.Log($"Move: {context.action.phase}");
            }*/

            if(context.performed) {
                _pollMove = true;
                DoPollMove();
            } else if(context.canceled) {
                _pollMove = false;
                _lastMove = Vector3.zero;
            }
        }

        public void OnLookAction(InputAction.CallbackContext context)
        {
            if(!IsInputAllowed(context)) {
                return;
            }

            /*if(Core.Input.InputManager.Instance.EnableDebug) {
                Debug.Log($"Look: {context.action.phase}");
            }*/

            if(context.performed) {
                _pollLook = true;
                DoPollLook();
            } else if(context.canceled) {
                _pollLook = false;
                _lastLook = Vector3.zero;
            }
        }

        #endregion

        #region Debug Menu

        private void InitDebugMenu()
        {
            _debugMenuNode = DebugMenuManager.Instance.AddNode(() => $"Server Spectator Input");
            _debugMenuNode.RenderContentsAction = () => {
                /*GUILayout.BeginHorizontal();
                    GUILayout.Label("Mouse Sensitivity:");
                    _mouseSensitivity = GUIUtils.FloatField(_mouseSensitivity);
                GUILayout.EndHorizontal();*/

                GUILayout.Label($"Last Move: {_lastMove}");
                GUILayout.Label($"Last Look: {_lastLook}");
            };
        }

        private void DestroyDebugMenu()
        {
            if(DebugMenuManager.HasInstance) {
                DebugMenuManager.Instance.RemoveNode(_debugMenuNode);
            }
            _debugMenuNode = null;
        }

        #endregion
    }
}
