using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Biostart.CameraEffects
{
    public class RotateToMouse : MonoBehaviour
    {
        public Camera cam; // Reference to the camera
        public float maximumLength = 100f; // Maximum ray length
        public Transform characterBody; // Reference to the character's body
        public Transform weaponHand; // Reference to the weapon hand

        private Ray rayMouse;
        private Vector3 direction;
        private Quaternion bodyRotation;
        private Quaternion weaponRotation;

        void Update()
        {
            if (cam != null)
            {
                RaycastHit hit;
                var mousePos = Input.mousePosition;
                rayMouse = cam.ScreenPointToRay(mousePos);

                // Check for collision with objects
                if (Physics.Raycast(rayMouse.origin, rayMouse.direction, out hit, maximumLength))
                {
                    RotateToMouseDirection(hit.point);
                }
                else
                {
                    var pos = rayMouse.GetPoint(maximumLength);
                    RotateToMouseDirection(pos);
                }
            }
            else
            {
                Debug.Log("No Camera");
            }
        }

        void RotateToMouseDirection(Vector3 destination)
        {
            // Rotate the character's body (around the Y-axis)
            Vector3 bodyDirection = new Vector3(destination.x, characterBody.position.y, destination.z) - characterBody.position;
            bodyRotation = Quaternion.LookRotation(bodyDirection); // Calculate rotation for the character's body

            // Rotate the weapon (around the Y and X axes)
            direction = destination - weaponHand.position; // Direction from the weapon hand to the target
            weaponRotation = Quaternion.LookRotation(direction); // Calculate rotation for the weapon

            // Smoothly rotate the character's body
            characterBody.rotation = Quaternion.Lerp(characterBody.rotation, bodyRotation, Time.deltaTime * 10f);

            // Smoothly rotate the weapon
            if (weaponHand != null)
            {
                weaponHand.rotation = Quaternion.Lerp(weaponHand.rotation, weaponRotation, Time.deltaTime * 10f);
            }
        }

        public Quaternion GetRotation()
        {
            return weaponRotation; // Return the weapon's rotation
        }
    }
}
