using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SelectMenuController : MonoBehaviour
{

    List<Transform> buttons;
    int index = 0;

    Color selectedColor = Color.cyan;
    Color defaultColor = Color.white;

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


    public void ChangeSelectMenu(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
        {
            Vector2 movement = ctx.ReadValue<Vector2>();

            if (movement.x > 0.5)
            {
                Previous();
            }
            else if (movement.x < -0.5)
            {
                Next();
            }
        }
    }
}
