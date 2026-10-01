using UnityEngine;

public class MenuInput : MonoBehaviour
{
    MenuManager _menu;

    private void Awake()
    {
        _menu = GetComponent<MenuManager>();
    }

    private void Update()
    {
        GetMobileInput();
    }

    private void GetMobileInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if ((_menu.GetCurrentMenu == MenuType.Main))
            {
                _menu.OpenMenu(MenuType.Exit);
            }
            else if (_menu.GetCurrentMenu == MenuType.Setting ||
                     _menu.GetCurrentMenu == MenuType.Credit ||
                     _menu.GetCurrentMenu == MenuType.Shop)
            {
                _menu.CloseMenu();
            }
        }
    }
}
