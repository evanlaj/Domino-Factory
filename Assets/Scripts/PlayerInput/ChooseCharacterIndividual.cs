using UnityEngine;
using UnityEngine.InputSystem;

public class ChooseCharacterIndividual : MonoBehaviour
{
    [SerializeField] Transform rightArrow;
    [SerializeField] Transform leftArrow;

    public PlayerJoinedManager manager;

    SpriteRenderer characterSprite;
    SpriteRenderer checkSprite;

    Animator animatorRightArrow;
    Animator animatorLeftArrow;

    bool selectionValidated;

    bool objectCompletlyCreated;

    // Todo � remplacer par soit un changement de sprite / Soit un changement d'animator
    Color[] colors = new Color[] { Color.red, Color.blue, Color.green, Color.gray, Color.cyan, Color.magenta };

    int selectedColorIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        characterSprite = GetComponent<SpriteRenderer>();
        checkSprite = transform.Find("Check").GetComponentInChildren<SpriteRenderer>();

        animatorRightArrow = transform.Find("ArrowRParrent").GetComponentInChildren<Animator>();
        animatorLeftArrow = transform.Find("ArrowLParrent").GetComponentInChildren<Animator>();

        characterSprite.color = colors[selectedColorIndex];
        checkSprite.color = Color.clear;
        objectCompletlyCreated = true;
    }

    public void SelectCharacter(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && !selectionValidated && objectCompletlyCreated)
        {
            Vector2 movement = ctx.ReadValue<Vector2>();
            if (movement == Vector2.left)
            {
                animatorLeftArrow.SetTrigger("Select");
                selectedColorIndex--;
                if (selectedColorIndex < 0)
                {
                    selectedColorIndex = colors.Length - 1;
                }
                characterSprite.color = colors[selectedColorIndex];

            }
            else if (movement == Vector2.right)
            {
                animatorRightArrow.SetTrigger("Select");
                selectedColorIndex++;
                if (selectedColorIndex >= colors.Length)
                {
                    selectedColorIndex = 0;
                }
                characterSprite.color = colors[selectedColorIndex];
            }
        }
    }

    public void ValidateSelection(InputAction.CallbackContext ctx)
    {
        if (ctx.performed && objectCompletlyCreated)
        {
            GetComponent<Animator>().SetBool("isMovingDown", true);
            transform.Find("ArrowLParrent").GetComponentInChildren<SpriteRenderer>().color = Color.clear;
            transform.Find("ArrowRParrent").GetComponentInChildren<SpriteRenderer>().color = Color.clear;
            checkSprite.color = Color.white;
            selectionValidated = true;

            manager.PlayerValidate(GetComponent<PlayerInput>().playerIndex, colors[selectedColorIndex]);
        }
    }


}