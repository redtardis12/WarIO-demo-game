using System;
using System.Collections;
using UnityEngine;

namespace TopDownShooter
{
    public class MovementCharacterController : MonoBehaviour
    {
        [Header("Player Controller Settings")] [Tooltip("Speed for the player.")]
        public float RunningSpeed = 5f;

        [Header("Speed when player is shooting")] [Range(0.2f, 1)] [Tooltip("This is the % from player normal speed.")]
        public float RunningShootSpeed;

        [Tooltip("Slope angle limit to slide.")]
        public float SlopeLimit = 45;

        [Tooltip("Slide friction.")] [Range(0.1f, 0.9f)]
        public float SlideFriction = 0.3f;

        [Tooltip("Gravity force.")] [Range(0, -100)]
        public float Gravity = -30f;

        [Tooltip("Maxima speed for the player when fall.")] [Range(0, 100)]
        public float MaxDownYVelocity = 15;

        [Tooltip("Can the user control the player?")]
        public bool CanControl = true;

        [Header("Jump Settings")] [Tooltip("This allow the character to jump.")]
        public bool CanJump = true;

        [Tooltip("Jump maxima elevation for the character.")]
        public float JumpHeight = 2f;

        [Tooltip("This allow the character to jump in air after another jump.")]
        public bool CanDobleJump = true;

        [Header("Dash Settings")] [Tooltip("The player have dash?.")]
        public bool CanDash = true;

        [Tooltip("Cooldown for the dash.")] public float DashColdown = 3;

        [Tooltip("Force for the dash, a greater value more distance for the dash.")]
        public float DashForce = 5f;

        [Tooltip("This is the drag force for the character, a standard value are (8, 0, 8). ")]
        public Vector3 DragForce;

        [Header("Enemy Detection Settings")]
        
        [Tooltip("This is the layer for the enemies.")]
        public LayerMask enemyLayer;
        
        [Tooltip("This is the radius to detect enemies.")]
        public float detectionRadius = 10f;
        
        [Tooltip("This is the speed at which the player rotates towards the closest enemy.")]
        public float rotationSpeed = 5f;

        [Tooltip("This is the animator for you character.")]
        public Animator PlayerAnimator;

        [Header("Effects")] [Tooltip("This position is in the character feet and is use to instantiate effects.")]
        public Transform LowZonePosition;

        public GameObject JumpEffect;
        public GameObject DashEffect;

        public PlayerController PlayerController;

        //Input.
        public float Horizontal;
        public float Vertical;
        public float Horizontal2;
        public float Vertical2;

        

        private Collider[] hitColliders; // Reusable array for Physics.OverlapSphereNonAlloc
        private const int maxColliders = 10; // Maximum number of colliders to detect

        //private vars
        private CharacterController _controller;
        private Vector3 _velocity;


        private bool _jump;
        private bool _dash;
        private bool _shooting;

        //get direction for the camera
        private Transform _cameraTransform;
        private Vector3 _forward;
        private Vector3 _right;

        //temporal vars
        private float _originalRunningSpeed;
        private float _dashColdown;
        private float _gravity;
        private bool _doubleJump;
        private bool _invertedControl;
        private bool _isCorrectGrounded;
        private Vector3 _hitNormal;
        private Vector3 _move;
        private Vector3 _direction;
        private bool _activeFall;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _originalRunningSpeed = RunningSpeed;
        }

        private void Start()
        {
            if (Camera.main != null) _cameraTransform = Camera.main.transform;
            _dashColdown = DashColdown;
            _gravity = Gravity;

            hitColliders = new Collider[maxColliders];
        }

        private void Update()
        {
            //capture input from direct input
            //this is for normal movement
            Horizontal = PlayerController.GetHorizontalValue();
            Vertical = PlayerController.GetVerticalValue();

            //this is for aim and shoot
            Horizontal2 = PlayerController.GetHorizontal2Value();
            Vertical2 = PlayerController.GetVertical2Value();

            //other buttons skills
            _jump = PlayerController.GetJumpValue();
            _dash = PlayerController.GetDashValue();

            //this invert controls 
            if (_invertedControl)
            {
                Horizontal *= -1;
                Vertical *= -1;
                Horizontal2 *= -1;
                Vertical2 *= -1;
                _jump = PlayerController.GetDashValue();
                _dash = PlayerController.GetJumpValue();
            }

            //if player can control the character
            if (CanControl)
            {
                //jump
                if (_jump)
                {
                    Jump(JumpHeight);
                }

                //dash
                if (_dash)
                {
                    Dash();
                }
                
            }
            else
            {
                Horizontal = 0;
                Vertical = 0;
                Vertical2 = 0;
                Horizontal2 = 0;
            }

            //dash colDown
            if (DashColdown > 0)
            {
                DashColdown -= Time.fixedDeltaTime;
            }
            else
            {
                DashColdown = 0;
            }

            //set running animation
            SetRunningAnimation((Math.Abs(Horizontal) > 0 || Math.Abs(Vertical) > 0));

            SetGorundedState();
        }

        private void FixedUpdate()
        {
            //get the input direction for the camera position.
            _forward = _cameraTransform.TransformDirection(Vector3.forward);
            _forward.y = 0f;
            _forward = _forward.normalized;
            _right = new Vector3(_forward.z, 0.0f, -_forward.x);

            _move = (Horizontal * _right + Vertical * _forward);

            //if no is correct grounded then slide.
            if (!_isCorrectGrounded && _controller.isGrounded)
            {
                _move.x += (1f - _hitNormal.y) * _hitNormal.x * (1f - SlideFriction);
                _move.z += (1f - _hitNormal.y) * _hitNormal.z * (1f - SlideFriction);
            }

            _move.Normalize();
            SetAimAnimation(_move);
            //move the player if no is active the slow fall(this avoid change the speed for the fall)
            if (!_activeFall && _controller.enabled)
            {
                if (_shooting)
                {
                    _controller.Move(Time.deltaTime * RunningSpeed * RunningShootSpeed * _move);
                }
                else
                {
                    _controller.Move(Time.deltaTime * RunningSpeed * _move);
                }
            }

            //Check if is correct grounded.
            _isCorrectGrounded = (Vector3.Angle(Vector3.up, _hitNormal) <= SlopeLimit);

            //set the forward direction if have some weapons loaded
            if (PlayerController.ShooterController.CurrentWeaponClass != Weapon.WeaponType.Hands)
            {
                if (PlayerController.MovCharController.TensionFoRightStickLowerThan(0.1f))
                {
                Transform closestEnemy = FindClosestEnemy();
                _shooting = true;

                // Rotate towards the closest enemy
                    if (closestEnemy != null)
                    {
                        RotateTowards(closestEnemy);
                    }
                }
                else
                {
                    if (_move != Vector3.zero && PlayerController.ShooterController.DelayToTurnOn <= 0)
                    {
                        _shooting = false;
                        transform.forward = Vector3.Lerp(transform.forward, _move, 0.5f);
                    }
                }
            }
            

            PlayerController.ShooterController.ManageShoot();

            //gravity force
            if (_velocity.y >= -MaxDownYVelocity)
            {
                _velocity.y += Gravity * Time.deltaTime;
            }

            _velocity.x /= 1 + DragForce.x * Time.deltaTime;
            _velocity.y /= 1 + DragForce.y * Time.deltaTime;
            _velocity.z /= 1 + DragForce.z * Time.deltaTime;
            if (_controller.enabled)
            {
                _controller.Move(_velocity * Time.deltaTime);
            }
        }

        Transform FindClosestEnemy()
        {
            // Detect all colliders within the detection radius on the enemy layer
            int numColliders = Physics.OverlapSphereNonAlloc(transform.position, detectionRadius, hitColliders, enemyLayer);
            Transform closestEnemy = null;
            float closestDistance = Mathf.Infinity;

            // Iterate through the detected colliders
            for (int i = 0; i < numColliders; i++)
            {
                // Calculate the distance to the enemy
                float distance = Vector3.Distance(transform.position, hitColliders[i].transform.position);
                // Update the closest enemy if this one is closer
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestEnemy = hitColliders[i].transform;
                }
            }

            return closestEnemy;
        }

        void RotateTowards(Transform target)
        {
            // Calculate the direction to the target
            Vector3 direction = (target.position - transform.position).normalized;
            direction.y = 0; // Ensure the player doesn't tilt up or down

            // Create a rotation towards the target
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Smoothly rotate towards the target
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }


        //This check how much the player are pushing the fire stick
        public bool TensionFoRightStickLowerThan(float value)
        {
            return (Mathf.Abs(PlayerController.MovCharController.Horizontal2) > value ||
                    Mathf.Abs(PlayerController.MovCharController.Vertical2) > value);
        }

        public void Jump(float jumpHeight)
        {
            if (!CanJump)
            {
                return;
            }

            _activeFall = false;

            //
            if (_controller.isGrounded)
            {
                _hitNormal = Vector3.zero;
                SetJumpAnimation();
                _doubleJump = true;
                _velocity.y = 0;
                _velocity.y += Mathf.Sqrt(jumpHeight * -2f * Gravity);

                //Instatiate jump effect
                if (JumpEffect)
                {
                    Instantiate(JumpEffect, LowZonePosition.position, LowZonePosition.rotation);
                }
            }
            else if (CanDobleJump && _doubleJump)
            {
                _doubleJump = false;
                _velocity.y = 0;
                _velocity.y += Mathf.Sqrt(jumpHeight * -2f * Gravity);

                //Instatiate jump effect
                if (JumpEffect)
                {
                    Instantiate(JumpEffect, LowZonePosition.position, LowZonePosition.rotation);
                }
            }
        }

        public void JumpWhitCurrentForce()
        {
            Jump(JumpHeight);
        }

        public void Dash()
        {
            if (!CanDash || DashColdown > 0)
            {
                return;
            }

            DashColdown = _dashColdown;

            if (DashEffect)
            {
                Instantiate(DashEffect, transform.position, transform.rotation);
            }

            SetDashAnimation();
            StartCoroutine(Dashing(0.1f));

            if (_direction != Vector3.zero && _move != Vector3.zero)
            {
                _velocity += Vector3.Scale(_move,
                    DashForce * new Vector3((Mathf.Log(1f / (Time.deltaTime * DragForce.x + 1)) / -Time.deltaTime),
                        0, (Mathf.Log(1f / (Time.deltaTime * DragForce.z + 1)) / -Time.deltaTime)));
            }
            else
            {
                _velocity += Vector3.Scale(transform.forward,
                    DashForce * new Vector3((Mathf.Log(1f / (Time.deltaTime * DragForce.x + 1)) / -Time.deltaTime),
                        0, (Mathf.Log(1f / (Time.deltaTime * DragForce.z + 1)) / -Time.deltaTime)));
            }
        }

 
        public void ResetOriginalSpeed()
        {
            RunningSpeed = _originalRunningSpeed;
        }

        //change the speed for the player
        public void ChangeSpeed(float speed)
        {
            RunningSpeed = speed;
        }

        //change the speed for the player for a time period
        public void ChangeSpeedInTime(float speedPlus, float time)
        {
            StartCoroutine(ModifySpeedByTime(speedPlus, time));
        }

        //invert player control(like a confuse skill)
        public void InvertPlayerControls(float invertTime)
        {
            //check if not are already inverted
            if (!_invertedControl)
            {
                StartCoroutine(InvertControls(invertTime));
            }
        }

        public void ActivateDeactivateJump(bool canJump)
        {
            CanJump = canJump;
        }

        public void ActivateDeactivateDoubleJump(bool canDoubleJump)
        {
            //if double jump is active activate normal jump
            if (canDoubleJump)
            {
                CanJump = true;
            }

            CanDobleJump = canDoubleJump;
        }

        public void ActivateDeactivateDash(bool canDash)
        {
            CanDash = canDash;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            _hitNormal = hit.normal;
        }
        //Animation

        #region Animator

        private void SetRunningAnimation(bool run)
        {
            PlayerAnimator.SetBool("Running", run);
        }

        private void SetAimAnimation(Vector3 movementDirection)
        {
            Vector3 aimDirection = transform.InverseTransformDirection(movementDirection);
            PlayerAnimator.SetFloat("Y", aimDirection.z, 0.1f, Time.deltaTime);
            PlayerAnimator.SetFloat("X", aimDirection.x, 0.1f, Time.deltaTime);
        }

        private void SetJumpAnimation()
        {
            PlayerAnimator.SetTrigger("Jump");
        }

        private void SetDashAnimation()
        {
            PlayerAnimator.SetTrigger("Dash");
        }

        private void SetGorundedState()
        {
            //avoid set the grounded var in animator multiple time
            if (PlayerAnimator.GetBool("Grounded") != _controller.isGrounded)
            {
                PlayerAnimator.SetBool("Grounded", _controller.isGrounded);
            }
        }

        #endregion

        #region Coroutine

        //Use this to deactivate te player control for a period of time.
        public IEnumerator DeactivatePlayerControlByTime(float time)
        {
            _controller.enabled = false;
            CanControl = false;
            yield return new WaitForSeconds(time);
            CanControl = true;
            _controller.enabled = true;
        }

        //dash coroutine.
        private IEnumerator Dashing(float time)
        {
            CanControl = false;
            if (!_controller.isGrounded)
            {
                Gravity = 0;
                _velocity.y = 0;
            }

            //animate hear to true
            yield return new WaitForSeconds(time);
            CanControl = true;
            //animate hear to false
            Gravity = _gravity;
        }

        //modify speed by time coroutine.
        private IEnumerator ModifySpeedByTime(float speedPlus, float time)
        {
            if (RunningSpeed + speedPlus > 0)
            {
                RunningSpeed += speedPlus;
            }
            else
            {
                RunningSpeed = 0;
            }

            yield return new WaitForSeconds(time);
            RunningSpeed = _originalRunningSpeed;
        }

        private IEnumerator InvertControls(float invertTime)
        {
            yield return new WaitForSeconds(0.1f);
            _invertedControl = true;
            yield return new WaitForSeconds(invertTime);
            _invertedControl = false;
        }

        #endregion
    }
}