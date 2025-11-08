using System;
using Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Interaction
{
    public class InteractionManager : Singleton<InteractionManager>, InputSystem_Actions.IPlayerActions
    {
        public override bool IsDontDestroyOnLoad => true;
        
        InputSystem_Actions inputActions;
        
        [SerializeField] float clickThreshold = 0.2f;
        private void Start()
        {
            inputActions = new InputSystem_Actions();
            inputActions.Player.SetCallbacks(this);
            inputActions.Player.Enable();
        }
        
        private void OnDestroy()
        {
            if (inputActions != null)
                inputActions.Player.Disable();
        }
        
        public event Action<InputAction.CallbackContext> PointerClickEvent;
        public event Action<InputAction.CallbackContext> PointerPressEvent;
        public event Action<InputAction.CallbackContext> PointerReleaseEvent;
        
        public event Action ConfirmEvent;
        public event Action CancelEvent;
        

        public void OnClick(InputAction.CallbackContext context)
        {
            if (context.started)
            {
                PointerPressEvent?.Invoke(context);
            }
            else if (context.canceled)
            {
                PointerReleaseEvent?.Invoke(context);
                if (context.duration < clickThreshold)
                {
                    PointerClickEvent?.Invoke(context);
                }
            }
        }
        public void OnExit(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                CancelEvent?.Invoke();
            }
        }
        public void OnConfirm(InputAction.CallbackContext context)
        {
            if  (context.performed) 
            {
                ConfirmEvent?.Invoke();
            }
        }

        #region 쓰레기통
        


        public void OnInteract(InputAction.CallbackContext context)
        {
            
        }

        public void OnCtrl(InputAction.CallbackContext context)
        {
           
        }

        public void OnSpace(InputAction.CallbackContext context)
        {
            
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
            
        }

        public void OnNext(InputAction.CallbackContext context)
        {
            
        }

        public void OnShift(InputAction.CallbackContext context)
        {
            
        }

        #endregion



    }
}