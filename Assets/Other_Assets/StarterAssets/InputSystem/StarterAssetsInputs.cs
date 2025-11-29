using UnityEngine;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    public class StarterAssetsInputs : MonoBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

        private PlayerController playerController;

        // track if *this* component locked the cursor so we don't override other systems unintentionally
        private bool lockedByThis = false;

        private void Awake()
        {
            playerController = gameObject.GetComponent<PlayerController>();
        }

#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
        {
            if (cursorInputForLook)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        public void OnJump(InputValue value)
        {
            if (playerController.isBlocking || playerController.isKicking || playerController.isAttacking)
                return;
            JumpInput(value.isPressed);
        }

        public void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }
#endif

        public void MoveInput(Vector2 newMoveDirection) { move = newMoveDirection; }
        public void LookInput(Vector2 newLookDirection) { look = newLookDirection; }
        public void JumpInput(bool newJumpState) { jump = newJumpState; }
        public void SprintInput(bool newSprintState) { sprint = newSprintState; }

        private void Start()
        {
            // ensure initial cursor state matches the inspector flag
            SetCursorState(cursorLocked);
        }

        private void Update()
        {
            // If EventSystem exists and pointer is over UI, make cursor visible & unlocked so buttons can be clicked.
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                // keep cursor shown while over UI
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                lockedByThis = false;
                return; // skip further input handling this frame
            }

            // allow ESC to unlock and show cursor (useful to open UI/menus)
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                UnlockAndShowCursor();
            }

            // Optional: if you want first click to lock (common in FP templates)
            if (cursorLocked && !lockedByThis && Input.GetMouseButtonDown(0))
            {
                LockAndHideCursor();
            }
        }


        private void OnApplicationFocus(bool hasFocus)
        {
            // restore cursor only when application gains focus
            if (hasFocus)
            {
                SetCursorState(cursorLocked);
            }
        }

        private void SetCursorState(bool newState)
        {
            if (newState)
            {
                // lock and hide
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                lockedByThis = true;
            }
            else
            {
                // unlock and show
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                lockedByThis = false;
            }
        }

        private void LockAndHideCursor()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            lockedByThis = true;
        }

        private void UnlockAndShowCursor()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            lockedByThis = false;
        }

        // public helper so UI / other scripts can explicitly show cursor
        public void ForceShowCursor()
        {
            UnlockAndShowCursor();
        }
    }
}
