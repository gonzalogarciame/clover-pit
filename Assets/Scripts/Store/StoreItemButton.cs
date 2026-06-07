using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StoreItemButton : MonoBehaviour
{
    public Item Item => currentItem;

    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text itemPriceText;
    [SerializeField] private TMP_Text itemDescriptionText;
    [SerializeField] private Button purchaseButton;

    private Item currentItem;
    private System.Action<Item> onPurchaseCallback;

    private bool isLocked = false;   

    public void Initialize(Item item, System.Action<Item> onPurchase)
    {
        currentItem = item;
        onPurchaseCallback = onPurchase;

        itemIcon.sprite = item.icon;
        itemNameText.text = item.itemName;
        itemPriceText.text = $"${item.price}";
        itemDescriptionText.text = item.description;

        purchaseButton.onClick.RemoveAllListeners(); 
        purchaseButton.onClick.AddListener(OnPurchaseClicked);
    }

    private void OnPurchaseClicked()
    {
        if (isLocked) return; // si está bloqueado, no compra
        onPurchaseCallback?.Invoke(currentItem);
    }

    public void SetLocked(bool locked) 
    {
        isLocked = locked;
        UpdateInteractability(GameManager.Instance ? GameManager.Instance.Money : 0);
    }

    public void UpdateInteractability(int playerMoney)
    {
        if (currentItem == null)
        {
            purchaseButton.interactable = false;
            return;
        }

        bool canAfford = playerMoney >= currentItem.price;

        // si está bloqueado, NO se puede comprar aunque tengas dinero
        purchaseButton.interactable = (!isLocked) && canAfford;

        itemPriceText.color = canAfford ? Color.white : Color.red;
    }
}
