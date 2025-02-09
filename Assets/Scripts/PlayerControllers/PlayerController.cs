﻿using UnityEngine;

/* Script to easy setup your own input configurations.
 * You can use the virtual joystick solution in this pack or use another solution.
 * Note: if you go to use joystick like a Xbox controller you need add this two
 * new axis to the input manager.
 * */

namespace TopDownShooter
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Scripts reference")] public MovementCharacterController MovCharController;
        public ShooterController ShooterController;

        [Header("Use mouse to shoot and rotate player")]
        public bool UseMouseToRotate = true;

        [Tooltip("This is the layer for the ground.")]
        public LayerMask GroundLayer;

        private bool _activeJetPack;
        private bool _activeSlowFall;

        public float GetHorizontalValue()
        {
            return Input.GetAxis("Horizontal");
        }

        public float GetVerticalValue()
        {
            return Input.GetAxis("Vertical");
        }

        public float GetHorizontal2Value()
        {
            if (UseMouseToRotate)
            {
                return GetMouseDirection().x;
            }

            return Input.GetAxis("Horizontal");
        }

        public float GetVertical2Value()
        {
            if (UseMouseToRotate)
            {
                return GetMouseDirection().z;
            }

            //if you go to use a joystick like a Xbox joystick replace "Input.GetAxis("Vertical")" put you new Vertical axis in this place and uncheck mouse and virtual joystick like this:
            //return Input.GetAxis("NewControlAxis");

            return Input.GetAxis("Vertical");
        }

        public bool GetJumpValue()
        {
            return Input.GetKeyDown(KeyCode.Space);
        }

        public bool GetDashValue()
        {
            return Input.GetKeyDown(KeyCode.LeftShift);
        }

        public bool GetJetPackValue()
        {
            return Input.GetKey(KeyCode.X);
        }

        public bool GetSlowFallValue()
        {
            return Input.GetKeyDown(KeyCode.V);
        }

        public bool GetDropWeaponValue()
        {
            return Input.GetKeyDown(KeyCode.G);
        }

        public bool GetReloadWeaponValue()
        {
            return Input.GetKeyDown(KeyCode.R);
        }

        public Vector3 GetMouseDirection()
        {
            if (Camera.main == null) return Vector3.zero;
            var newRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit groundHit;

            //check if the player press mouse button and the ray hit the ground
            if (Input.GetMouseButton(0) && Physics.Raycast(newRay, out groundHit, 1000, GroundLayer))
            {
                var playerToMouse = groundHit.point - transform.position;

                playerToMouse.y = 0f;

                return playerToMouse;
            }

            return Vector3.zero;
        }
    }
}