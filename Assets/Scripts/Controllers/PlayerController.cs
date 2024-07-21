using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using KJ24.Managers;

namespace KJ24.Controllers
{
    /// <summary>
    /// Class <c>PlayerController</c> controls the player's movement.
    /// </summary>
    public class PlayerController : MonoBehaviour
    {
        /// <value>Property <c>_controls</c> represents the input controls.</value>
        private Controls _controls;

        /// <value>Property <c>_movement</c> represents the movement input action.</value>
        private InputAction _movement;
        
        /// <value>Property <c>_isMoving</c> represents the player's movement state.</value>
        private bool _isMoving;

        /// <value>Property <c>onGoal</c> represents the player's goal state.</value>
        public bool onGoal;
        
        /// <summary>
        /// Method <c>Awake</c> is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            _controls = new Controls();
        }

        /// <summary>
        /// Method <c>OnEnable</c> is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            _movement = _controls.Character.Move;
            _controls.Enable();
        }
        
        /// <summary>
        /// Method <c>OnDisable</c> is called when the behaviour becomes disabled.
        /// </summary>
        private void OnDisable()
        {
            _controls.Disable();
        }

        /// <summary>
        /// Method <c>Update</c> is called every frame.
        /// </summary>
        private void Update()
        {
            GetMovement();
        }
        
        /// <summary>
        /// Method <c>GetMovement</c> gets the player's movement.
        /// </summary>
        private void GetMovement()
        {
            // Check if the player is moving
            if (_isMoving || GameManager.Instance.isFlipping || GameManager.Instance.gameOver)
                return;
            // Get the input value
            var movement = _movement.ReadValue<Vector2>();
            // Calculate the target local position
            var targetLocalPosition = transform.localPosition;
            switch (movement)
            {
                case var _ when movement.x > 0:
                    targetLocalPosition += new Vector3(0.5f, 0.0f, 0.5f);
                    break;
                case var _ when movement.x < 0:
                    targetLocalPosition += new Vector3(-0.5f, 0.0f, -0.5f);
                    break;
                case var _ when movement.y > 0:
                    targetLocalPosition += new Vector3(-0.5f, 0.0f, 0.5f);
                    break;
                case var _ when movement.y < 0:
                    targetLocalPosition += new Vector3(0.5f, 0.0f, -0.5f);
                    break;
            }
            if (targetLocalPosition == transform.localPosition)
            {
                StartCoroutine(Wait(0.5f));
                return;
            }
            // Calculate the target world position
            var targetWorldPosition = transform.parent.TransformPoint(targetLocalPosition);
            // Check if there's another object in the target position
            var hit = Physics.Raycast(
                transform.position,
                targetWorldPosition - transform.position,
                out var hitInfo,
                1.0f);
            if (hit && hitInfo.collider.CompareTag("Obstacle"))
            {
                StartCoroutine(Wait(0.5f));
                return;
            }
            // Rotate towards the target position
            var targetRotation = Quaternion.LookRotation(targetLocalPosition - transform.localPosition);
            transform.localRotation = Quaternion.Euler(0.0f, targetRotation.eulerAngles.y, 0.0f);
            // Move
            StartCoroutine(Move(targetLocalPosition));
        }
        
        /// <summary>
        /// Coroutine <c>Move</c> moves the player to the target position.
        /// </summary>
        /// <param name="targetLocalPosition">The target local position.</param>
        /// <param name="duration">The duration of the movement.</param>
        private IEnumerator Move(Vector3 targetLocalPosition, float duration = 0.5f)
        {
            _isMoving = true;
            var speed = Vector3.Distance(transform.localPosition, targetLocalPosition) / duration;
            while (Vector3.Distance(transform.localPosition, targetLocalPosition) > 0.0f)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetLocalPosition, speed * Time.deltaTime);
                yield return null;
            }
            _isMoving = false;
        }
        
        /// <summary>
        /// Coroutine <c>Wait</c> waits for a duration.
        /// </summary>
        /// <param name="duration">The duration to wait.</param>
        private IEnumerator Wait(float duration)
        {
            yield return new WaitForSeconds(duration);
        }

        /// <summary>
        /// Method <c>OnTriggerEnter</c> is called when the Collider other enters the trigger.
        /// </summary>
        /// <param name="other">The other Collider.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Goal"))
                return;
            Debug.Log(transform.name + " is on the goal.");
            onGoal = true;
        }
        
        /// <summary>
        /// Method <c>OnTriggerExit</c> is called when the Collider other has stopped touching the trigger.
        /// </summary>
        /// <param name="other">The other Collider.</param>
        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Goal"))
                return;
            Debug.Log(transform.name + " left the goal.");
            onGoal = false;
        }
    }
}
