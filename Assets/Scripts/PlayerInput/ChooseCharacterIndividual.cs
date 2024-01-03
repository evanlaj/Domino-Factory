using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.U2D.Animation;

public class ChooseCharacterIndividual : MonoBehaviour
{
    [SerializeField] Transform rightArrow;
    [SerializeField] Transform leftArrow;

    [SerializeField] List<SpriteLibraryAsset> skins;
    int selectedSkinIndex = 0;
    SpriteLibrary selfSpriteLib;

    [SerializeField] float delayBetweenInput = 0.15f;
    float timeSinceLastInput = 0;

    public PlayerJoinedManager manager;

    Animator animatorRightArrow;
    Animator animatorLeftArrow;

    bool selectionValidated;

    bool objectCompletlyCreated;

    Vector2 movement = Vector2.zero;



    // Start is called before the first frame update
    void Start()
    {
        animatorRightArrow = transform.Find("ArrowRParrent").GetComponentInChildren<Animator>();
        animatorLeftArrow = transform.Find("ArrowLParrent").GetComponentInChildren<Animator>();
        objectCompletlyCreated = true;

        selfSpriteLib = GetComponent<SpriteLibrary>();
        selfSpriteLib.spriteLibraryAsset = skins[selectedSkinIndex];
    }

    public void SelectCharacter(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            movement = ctx.ReadValue<Vector2>().normalized;
        }
        else if (ctx.canceled)
        {
            movement = Vector2.zero;
            timeSinceLastInput = delayBetweenInput;
        }
    }

    public void ValidateSelection(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && objectCompletlyCreated)
        {
            GetComponent<Animator>().SetBool("isMovingDown", true);
            transform.Find("ArrowLParrent").GetComponentInChildren<SpriteRenderer>().color = Color.clear;
            transform.Find("ArrowRParrent").GetComponentInChildren<SpriteRenderer>().color = Color.clear;
            selectionValidated = true;

            manager.PlayerValidate(GetComponent<PlayerInput>().playerIndex, skins[selectedSkinIndex]);
        }
    }

    void Update()
    {
        timeSinceLastInput += Time.deltaTime;

        if (movement != Vector2.zero && timeSinceLastInput >= delayBetweenInput && !selectionValidated && objectCompletlyCreated)
        {
            timeSinceLastInput = 0;

            if (movement.x < -0.5)
            {
                animatorLeftArrow.SetTrigger("Select");
                selectedSkinIndex--;
                if (selectedSkinIndex < 0)
                {
                    selectedSkinIndex = skins.Count - 1;
                }
            }
            else if (movement.x > 0.5)
            {
                animatorRightArrow.SetTrigger("Select");
                selectedSkinIndex++;
                if (selectedSkinIndex >= skins.Count)
                {
                    selectedSkinIndex = 0;
                }
            }

            selfSpriteLib.spriteLibraryAsset = skins[selectedSkinIndex];
        }
    }


}