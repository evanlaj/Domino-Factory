using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CharacterController : MonoBehaviour
{
    Rigidbody2D rigidbodyCharacter;
    Animator animator;
    SpriteRenderer spriteRenderer;

    AssemblerBehavior assembler;

    Transform objectCarried;
    Transform objectDetected;

    bool isStopped;
    bool isDashing = false;
    bool canDash = true;
    float dashTimer;

    HologramBehavior assemblerHologram;

    Transform interactTriggerZone;
    Vector2 movement = Vector2.zero;

    //Data from last frame for calculation and animation
    Vector2 lastDirection;

    [SerializeField] float objectCarriedDistanceFactor;
    [SerializeField] Vector3 offsetObjectCarriedDistance;

    [SerializeField] float speed;
    [SerializeField] float dashSpeed;

    [SerializeField] float dashDuration;
    [SerializeField] float dashCooldown;

    [SerializeField] float delayBetweenInput;
    float timeSinceLastInput;

    void Start()
    {
        rigidbodyCharacter = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        var assemblerGameObject = GameObject.FindGameObjectWithTag("Assembler");
        assembler = assemblerGameObject.GetComponent<AssemblerBehavior>();

        interactTriggerZone = transform.GetChild(0);
    }

    void FixedUpdate()
    {
        timeSinceLastInput += Time.deltaTime;

        if (IsInAssembler)
        {
            UpdateSelection();
        }
        else
        {
            UpdateMovements();
            UpdateOutlines();
        }
    }

    void UpdateSelection()
    {
        //Pour les déplacement dans l'assembleur, on ajoute un délai entre chaque input pour éviter le spamme du bouton
        if (movement != Vector2.zero && timeSinceLastInput >= delayBetweenInput)
        {
            timeSinceLastInput = 0;

            var domino = objectCarried.GetComponent<DominoBehavior>();

            if (movement.normalized.x > 0.8)
            {
                domino.MoveDominoRight();
            }
            else if (movement.normalized.x < -0.8)
            {
                domino.MoveDominoLeft();
            }
            else if (movement.normalized.y < 0)
            {
                domino.MoveDominoDown();
            }
            else if (movement.normalized.y > 0)
            {
                domino.MoveDominoUp();
            }

            assemblerManager.CreateSpriteForAddedDomino(domino.domino);
        }
    }

    void UpdateMovements()
    {
        var isMovingSide = false;
        var isMovingUp = false;
        var isMovingDown = false;

        if (!isStopped)
        {
            dashTimer += Time.fixedDeltaTime;

            if (dashTimer > dashCooldown)
            {
                canDash = true;
            }

            if (isDashing)
            {
                rigidbodyCharacter.MovePosition(rigidbodyCharacter.position + dashSpeed * Time.fixedDeltaTime * lastDirection);


                if (dashTimer > dashDuration)
                {
                    isDashing = false;
                }
            }
            else
            {
                if (movement != Vector2.zero && movement.magnitude > 0.8)
                {
                    lastDirection = movement;
                }

                if (movement.y < -0.5)
                {
                    isMovingDown = true;
                }
                else if (movement.y > 0.5)
                {
                    isMovingUp = true;
                }
                else if (movement.x != 0)
                {
                    isMovingSide = true;
                    spriteRenderer.flipX = movement.x < 0;
                }

                interactTriggerZone.rotation = Quaternion.Euler(0, 0, Vector2.SignedAngle(Vector2.down, lastDirection));
                rigidbodyCharacter.MovePosition(rigidbodyCharacter.position + speed * Time.fixedDeltaTime * movement);
            }

            animator.SetBool("isMovingSide", isMovingSide);
            animator.SetBool("isMovingUp", isMovingUp);
            animator.SetBool("isMovingDown", isMovingDown);

            if (objectCarried != null)
            {
                if (movement != Vector2.zero)
                {
                    objectCarried.position = transform.position + (Vector3)movement.normalized * objectCarriedDistanceFactor + offsetObjectCarriedDistance;
                    objectCarried.GetComponent<SpriteRenderer>().sortingOrder = movement.y < 0 && movement.x < 0.4 && movement.x > -0.4 ? 15 : 5;
                }
                else
                {
                    objectCarried.position = transform.position + (Vector3)(lastDirection * objectCarriedDistanceFactor) + offsetObjectCarriedDistance;
                    objectCarried.GetComponent<SpriteRenderer>().sortingOrder = lastDirection.y > 0.5 ? 5 : 15;
                }

            }
        }


        #region InputAction CallBack
        public void Move(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                movement = ctx.ReadValue<Vector2>();
            }
            //Dans le cas ou le joueur est dans l'assembleur, on l'autorise à spammer les boutons de directions pour aller plus vites plutot que maintenir le bouton
            else if (ctx.canceled)
            {
                movement = Vector2.zero;
                timeSinceLastInput = delayBetweenInput;
            }
        }

        public void Dash(InputAction.CallbackContext ctx)
        {
            if (ctx.performed && canDash)
            {
                CameraShake.Instance.LittleShake();
                canDash = false;
                isDashing = true;
                dashTimer = 0f;
            }
        }

        public void Interact(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;

            if (assemblerHologram != null)
            {
                QuitAssemblerAndInsertDomino();
            }
            else
            {
                var interactableObjectsNear = GetColliderAroundPlayerOrderByDistance(getTriggerCollider: true);
                var interactableObjectsInFront = GetColliderInFrontPlayerOrderByDistance();

                //Si le joueur à un objet il va le déposer
                if (objectCarried != null)
                {
                    if (interactibleObjectsNear.Any(o => o.transform.CompareTag("AssemblerButton")))
                        AddHologramToAssembler();
                    else
                        DropObject();
                }
                //Sinon il va tenter d'en attraper un
                else
                {
                    if ((interactibleObjectsNear.Any(o => o.transform.CompareTag("Assembler")) || interactibleObjectsInFront.Any(o => o.transform.CompareTag("Assembler"))) && !assembler.IsEmpty())
                    {
                        objectCarried = assembler.TakeDominoOut().transform;
                        objectCarried.GetComponent<Collider2D>().isTrigger = true;
                    }
                    else
                    {
                        GetObjectNear();
                    }
                }

            }
        }


        public void RotateClockwise(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            RotateDomino(true);
        }

        public void RotateCounterClockwise(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed) return;
            RotateDomino(false);
        }

        private void RotateDomino(bool clockwise)
        {
            if (objectCarried == null) return;

            var domino = objectCarried.GetComponent<DominoBehavior>();

            if (clockwise) domino.RotateDominoClockwise();
            else domino.RotateDominoCounterClockwise();

            if (assemblerHologram != null) assemblerHologram.SetDomino(domino.domino);
        }

        #endregion

        #region Interact with object utils functions
        void GetObjectNear()
        {
            IEnumerable<Collider2D> interactableObjects = GetColliderInFrontPlayerOrderByDistance();

            Collider2D block = GetNearestObjectInCollidersByTag(interactableObjects, "Blocks");

            //D'abord on vérifie si on peut prendre un bloc devant nous
            if (block != null)
            {
                block.isTrigger = true;
                objectCarried = block.transform;
                return;
            }

            //Sinon on essaie de prendre un bloc d'une table
            TableBehaviour tableBehaviour = GetFirstTableWithObject(interactableObjects);
            if (tableBehaviour != null)
            {
                objectCarried = tableBehaviour.GetObjectCarried();
                return;
            }

            Collider2D box = GetNearestObjectInCollidersByTag(interactableObjects, "Crate");
            if (box != null)
            {
                CrateBehaviour boxBehaviour = box.GetComponent<CrateBehaviour>();
                objectCarried = boxBehaviour.GetObject();
                return;
            }


            //Dans le dernier cas on vérifie si on ne peux pas récupérer un bloc proche autour du joueur
            Collider2D blockAroundPlayer = GetNearestObjectInCollidersByTag(interactableObjects, "Blocks");
            if (blockAroundPlayer != null)
            {
                blockAroundPlayer.isTrigger = true;
                objectCarried = blockAroundPlayer.transform;
            }

        }

        private void AddHologramToAssembler()
        {
            var domino = objectCarried.GetComponent<DominoBehavior>();
            assemblerHologram = assembler.AddHologram(domino.domino);
        }

        private void QuitAssemblerAndInsertDomino()
        {
            if (assembler.TryInsertHologram(assemblerHologram))
            {

                Destroy(objectCarried.gameObject);
                objectCarried = null;

                Destroy(assemblerHologram.gameObject);
                assemblerHologram = null;
            }
        }

        void DropObject()
        {
            objectCarried.GetComponent<SpriteRenderer>().sortingOrder = 5;

            IEnumerable<Collider2D> interactableObjects = GetColliderInFrontPlayerOrderByDistance();

            Collider2D deliveryPointTransform = GetNearestObjectInCollidersByTag(interactableObjects, "DeliveryPoint");

            //On vérifie si le point de livraison se trouve dans dans notre champs d'action 
            if (deliveryPointTransform != null)
            {
                var deliveryPoint = deliveryPointTransform.GetComponent<DeliveryPointBehaviour>();
                var domino = objectCarried.GetComponent<DominoBehavior>().domino;

                deliveryPoint.DeliveryDomino(domino);
                objectCarried.gameObject.SetActive(false);
                objectCarried = null;
                return;
            }

            //On vérifie si une table se trouve dans notre champs d'action
            if (interactableObjects.Any(o => o.transform.CompareTag("Table")))
            {
                var tables = interactableObjects.Where(o => o.transform.CompareTag("Table"));

                foreach (var table in tables)
                {
                    TableBehaviour tableBehaviour = table.GetComponent<TableBehaviour>();
                    if (tableBehaviour.CanAcceptObject())
                    {
                        tableBehaviour.SetObjectCarried(objectCarried);
                        objectCarried = null;
                        return;
                    }
                }
            }

            //On vérifie que l'objet ne tombe pas dans un mur, si c'est le cas on set la position à la position du joueur
            var objectCarriedCollider = objectCarried.GetComponent<Collider2D>();
            List<Collider2D> collidersWhenDropObjectCarried = new();
            Physics2D.OverlapCollider(objectCarriedCollider, new ContactFilter2D(), collidersWhenDropObjectCarried);

            //Ici on compare à la fois les mur et les tables, car si on arrive à ce point dans la fonction c'est que toutes les tables sont prises
            if (collidersWhenDropObjectCarried.Any(c => c.transform.CompareTag("Walls") || c.transform.CompareTag("Table")))
            {
                objectCarried.position = transform.position;
            }

            objectCarriedCollider.isTrigger = false;
            objectCarried = null;

        }

        //Récupére les Colliders que le joueur colle, dans sa zone Circle
        IEnumerable<Collider2D> GetColliderAroundPlayerOrderByDistance(bool getTriggerCollider = false)
        {
            var circleTriggerZone = interactTriggerZone.GetComponent<CircleCollider2D>();

            var colliders = new List<Collider2D>();
            var contactFilter = new ContactFilter2D
            {
                useTriggers = getTriggerCollider
            };
            Physics2D.OverlapCollider(circleTriggerZone, contactFilter, colliders);

            return colliders.OrderBy(c => Vector2.Distance(c.transform.position, transform.position)).ToList();
        }

        //Récupére les Colliders qui sont devant le joueur, dans sa zone Capsule
        IEnumerable<Collider2D> GetColliderInFrontPlayerOrderByDistance(bool getTriggerCollider = false)
        {
            var capsuleTriggerZone = interactTriggerZone.GetComponent<CapsuleCollider2D>();

            var colliders = new List<Collider2D>();
            var contactFilter = new ContactFilter2D
            {
                useTriggers = getTriggerCollider
            };
            Physics2D.OverlapCollider(capsuleTriggerZone, contactFilter, colliders);

            return colliders.OrderBy(c => Vector2.Distance(c.transform.position, transform.position)).ToList();
        }

        Collider2D GetNearestObjectInCollidersByTag(IEnumerable<Collider2D> colliders, string tagName)
        {
            //D'abord on vérifie si on peut prendre un bloc devant nous
            if (colliders.Any(o => o.transform.CompareTag(tagName)))
            {
                return colliders.First(o => o.transform.CompareTag(tagName));
            }

            return null;
        }

        TableBehaviour GetFirstTableWithObject(IEnumerable<Collider2D> colliders)
        {
            if (colliders.Any(o => o.transform.CompareTag("Table")))
            {
                var tables = colliders.Where(o => o.transform.CompareTag("Table"));

                foreach (var table in tables)
                {
                    TableBehaviour tableBehaviour = table.GetComponent<TableBehaviour>();
                    if (!tableBehaviour.CanAcceptObject())
                    {
                        return tableBehaviour;
                    }
                }
            }

            return null;
        }

        #endregion

        void UpdateOutlines()
        {
            IEnumerable<Collider2D> interactableObjects = GetColliderInFrontPlayerOrderByDistance();
            Collider2D block = GetNearestObjectInCollidersByTag(interactableObjects, "Blocks");
            TableBehaviour tableBehaviour = GetFirstTableWithObject(interactableObjects);

            if (block != null)
            {
                var newObjectOutlined = block.transform;

                if (newObjectOutlined != objectDetected && objectDetected != null)
                    objectDetected.GetComponent<SpriteRenderer>().material.SetInt("_IsDetected", 0);

                newObjectOutlined.GetComponent<SpriteRenderer>().material.SetInt("_IsDetected", 1);
                objectDetected = newObjectOutlined;

            }

            //Check si une table se trouve devant nous, dans ce cas on on ajoute une outline sur l'objet dessus
            else if (tableBehaviour != null)
            {
                var newObjectOutlined = tableBehaviour.GetReferenceObjectCarried().transform;

                if (newObjectOutlined != objectDetected && objectDetected != null)
                    objectDetected.GetComponent<SpriteRenderer>().material.SetInt("_IsDetected", 0);

                newObjectOutlined.GetComponent<SpriteRenderer>().material.SetInt("_IsDetected", 1);
                objectDetected = newObjectOutlined;
            }

            else if (objectDetected != null)
            {
                objectDetected.GetComponent<SpriteRenderer>().material.SetInt("_IsDetected", 0);
                objectDetected = null;
            }
        }

        //Todo : a passer dans une autre classe ? Oui 👍
        public void LoadChooseLevel(InputAction.CallbackContext ctx)
        {
            if (ctx.performed)
            {
                SceneManager.LoadScene("ChooseLvl");
            }
        }

    }
