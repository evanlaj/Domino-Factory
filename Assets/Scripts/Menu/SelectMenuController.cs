using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SelectMenuController : MonoBehaviour
{

    List<Transform> buttons;
    int index = 0;

    [SerializeField] Color selectedColor = Color.cyan;
    Color defaultColor = Color.white;

    [SerializeField] float delayBetweenInputs = 0.2f;
    float timeSinceLastInput = 0;

    // Start is called before the first frame update
    void Start()
    {
        buttons = GetComponentsInChildren<Button>().Select(a => a.transform).ToList();
        buttons[index].GetComponent<Image>().color = selectedColor;
    }

    private void Next()
    {
        buttons[index].GetComponent<Image>().color = defaultColor;
        index--;
        if (index < 0)
        {
            index = buttons.Count - 1;
        }
        buttons[index].GetComponent<Image>().color = selectedColor;
    }

    private void Previous()
    {
        buttons[index].GetComponent<Image>().color = defaultColor;
        index++;
        if (index >= buttons.Count)
        {
            index = 0;
        }
        buttons[index].GetComponent<Image>().color = selectedColor;
    }

    public void Select(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            buttons[index].GetComponent<Button>().onClick.Invoke();
        }
    }

    Vector2 currentMovement = Vector2.zero;

    public void ChangeSelectMenu(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            currentMovement = ctx.ReadValue<Vector2>();
        }
        else if (ctx.canceled)
        {
            currentMovement = Vector2.zero;
            timeSinceLastInput = delayBetweenInputs;
        }
    }

    void Update()
    {
        timeSinceLastInput += Time.deltaTime;

        if (currentMovement != Vector2.zero && timeSinceLastInput >= delayBetweenInputs)
        {
            timeSinceLastInput = 0;

            if (currentMovement.x > 0.5)
            {
                Previous();
            }
            else if (currentMovement.x < -0.5)
            {
                Next();
            }
        }
    }
}
