using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using KJ24.Controllers;

namespace KJ24.Managers
{
    /// <summary>
    /// Class <c>GameManager</c> represents the game manager.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        /// <value>Property <c>Instance</c> represents the singleton instance of the class.</value>
        public static GameManager Instance;

        /// <value>Property <c>_controls</c> represents the input controls.</value>
        private Controls _controls;

        /// <value>Property <c>_flip</c> represents the flip input action.</value>
        private InputAction _flip;
        
        /// <value>Property <c>isFlipping</c> represents the flip state.</value>
        public bool isFlipping;

        /// <value>Property <c>gameOver</c> represents the game over state.</value>
        public bool gameOver;

        /// <summary>
        /// Method <c>Awake</c> is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            // Initialize input controls
            _controls = new Controls();
        }
        
        /// <summary>
        /// Method <c>OnEnable</c> is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            _flip = _controls.World.Flip;
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
            GetFlip();
            CheckGameOver();
        }
        
        /// <summary>
        /// Method <c>GetFlip</c> gets the flip input.
        /// </summary>
        private void GetFlip()
        {
            // Get the input value
            var flipValue = _flip.ReadValue<float>();
            // Check if flip input is received and there is no flip in progress
            if (flipValue == 0 || isFlipping || gameOver)
                return;
            StartCoroutine(Flip());
        }
        
        /// <summary>
        /// Method <c>Flip</c> flips all grid objects.
        /// </summary>
        private IEnumerator Flip()
        {
            isFlipping = true;
            var coroutines = new List<Coroutine>();
            // Get all grid objects
            var gridObjects = FindObjectsOfType<Grid>();
            // Flip all grid objects
            foreach (var gridObject in gridObjects)
            {
                var targetRotation = gridObject.transform.rotation.eulerAngles;
                targetRotation = (targetRotation.x == 0)
                    ? new Vector3(60, 0, 180)
                    : new Vector3(0, 0, 0);
                coroutines.Add(StartCoroutine(FlipObject(gridObject.gameObject, Quaternion.Euler(targetRotation), 0.5f)));
            }
            // Wait for all objects to flip
            foreach (var coroutine in coroutines)
            {
                yield return coroutine;
            }
            isFlipping = false;
        }
        
        /// <summary>
        /// Coroutine <c>FlipObject</c> flips an object.
        /// </summary>
        /// <param name="go">The object to flip.</param>
        /// <param name="targetRotation">The target rotation.</param>
        /// <param name="duration">The flip duration.</param>
        private IEnumerator FlipObject(GameObject go, Quaternion targetRotation, float duration)
        {
            var time = 0.0f;
            var startRotation = go.transform.rotation;
            while (time < duration)
            {
                go.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, time / duration);
                time += Time.deltaTime;
                yield return null;
            }
            go.transform.rotation = targetRotation;
        }
        
        /// <summary>
        /// Method <c>CheckGameOver</c> checks if the game is over.
        /// </summary>
        private void CheckGameOver()
        {
            // Check if game over state is set
            if (gameOver)
                return;
            // Get all players
            var players = FindObjectsOfType<PlayerController>();
            // Check if all players are on the goal
            if (players.Any(player => !player.onGoal))
                return;
            // Set game over state
            Debug.Log("Game Over");
            gameOver = true;
        }
    }
}
