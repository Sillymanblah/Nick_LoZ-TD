using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TheNicksin.Inputsystem
{
    public class InputManager : MonoBehaviour
    {
        #region Keybindings

        public KeyCode forward;
        public KeyCode backward;
        public KeyCode left;
        public KeyCode right;
        public KeyCode jump;
        public KeyCode run;
        public KeyCode crouch;
        public KeyCode mouse0;
        public KeyCode mouse1;
        public KeyCode interact;
        public KeyCode inventory;
        public KeyCode shift;
        public KeyCode focus;

        // bools
        // floats

        #endregion

        public float HorizontalMovement;
        public float VerticalMovement;

        private void Awake()
        {
            InitializeKeys();
        }
        // Start is called before the first frame update
        void Start()
        {
            
        }
        private void Update()
        {
            HorizontalMovement = AxisMovementValue(forward, backward);
            VerticalMovement = AxisMovementValue(right, left);
        }

        void InitializeKeys()
        {
            forward = KeyCode.W;
            backward = KeyCode.S;
            left = KeyCode.A;
            right = KeyCode.D;
            jump = KeyCode.Space;
            run = KeyCode.LeftShift;
            crouch = KeyCode.C;
            mouse0 = KeyCode.Mouse0;
            mouse1 = KeyCode.Mouse1;
            interact = KeyCode.E;
            inventory = KeyCode.Tab;
            shift = KeyCode.LeftShift;
            focus = KeyCode.F;
        }
        public bool StateFloatToBool(float state)
        {
            if (state >= 1) 
                return true;
            else return false;
        }

        public float KeyToFloat(KeyCode key)
        {
            if (Input.GetKey(key))
            {
                return 1f;
            } else return 0f;
        }
        public float KeyDownToFloat(KeyCode key)
        {
            if (Input.GetKeyDown(key))
            {
                return 1f;
            } else return 0f;
        }
        public float AxisMovementValue(KeyCode positive, KeyCode negative)
        {
            if (Input.GetKey(positive))
            {
                return 1f;
            }
            else if (Input.GetKey(negative))
            {
                return -1f;
            }
            else return 0f;
        }
    }
}
