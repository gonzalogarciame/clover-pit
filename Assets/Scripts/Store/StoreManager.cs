using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class StoreManager : MonoBehaviour
{
    public static StoreManager Instance { get; private set; }

    [Header("Store Pool (todos los items posibles)")]
    [SerializeField] private List<Item> availableItems;

    [Header("UI")]
    [SerializeField] private Transform storeUIParent;
    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private GameObject storePanel;
    [SerializeField] private TMP_Text storeTitleText;

    [Header("Rotation Rules")]
    [SerializeField] private int maxItemsInStore = 4;
    [SerializeField] private int refreshEverySpins = 5;

    private readonly List<StoreItemButton> currentItemButtons = new();
    private List<Item> currentOffer = new();
    private List<Item> previousOffer = new();

    private int spinsSinceRefresh = 0;
    private HashSet<Item> purchasedItemsThisRotation = new();
    private bool subscribed = false;
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnSpinFinished += HandleSpinFinished;
    }

    void Update()
    {
        if (!subscribed)
            TrySubscribe();
    }

    void OnDisable()
    {
        if (subscribed && GameManager.Instance != null)
            GameManager.Instance.OnSpinFinished -= HandleSpinFinished;

        subscribed = false;
    }

    void Start()
    {
        TrySubscribe();
        RefreshStoreOffer(force: true);
        HideStore();
    }

    private void HandleSpinFinished()
    {
        spinsSinceRefresh++;

        if (spinsSinceRefresh >= refreshEverySpins)
        {
            RefreshStoreOffer(force: false);
        }

        if (storePanel != null && storePanel.activeSelf)
            UpdateStoreUI();
    }

    private void RefreshStoreOffer(bool force)
    {
        spinsSinceRefresh = 0;

        purchasedItemsThisRotation.Clear();

        previousOffer = currentOffer;
        currentOffer = PickNewOffer(maxItemsInStore, previousOffer);

        BuildStoreButtons(currentOffer);
        UpdateStoreUI();
    }


    private List<Item> PickNewOffer(int count, List<Item> avoid)
    {
        count = Mathf.Clamp(count, 0, availableItems.Count);

        var pool = availableItems.Where(i => i != null).ToList();
        var filtered = pool.Where(i => avoid == null || !avoid.Contains(i)).ToList();

        var source = filtered.Count >= count ? filtered : pool;

        Shuffle(source);
        return source.Take(count).ToList();
    }

    private void BuildStoreButtons(List<Item> offer)
    {
        foreach (Transform child in storeUIParent)
            Destroy(child.gameObject);

        currentItemButtons.Clear();

        foreach (Item item in offer)
        {
            if (itemButtonPrefab == null || storeUIParent == null) continue;

            GameObject go = Instantiate(itemButtonPrefab, storeUIParent);
            StoreItemButton btn = go.GetComponent<StoreItemButton>();

            if (btn != null)
            {
                btn.Initialize(item, OnItemPurchased);
                currentItemButtons.Add(btn);
            }
        }
    }

    private void OnItemPurchased(Item item)
    {
        if (purchasedItemsThisRotation.Contains(item))
        {
            UIManager.Instance?.ShowEffectMessage("Este objeto ya fue comprado en esta rotación");
            return;
        }

        if (GameManager.Instance.Money >= item.price)
        {
            purchasedItemsThisRotation.Add(item);

            GameManager.Instance.AddMoney(-item.price);

            ItemEffects.Instance.ApplyItemEffect(item);

            UIManager.Instance?.ShowEffectMessage($"Comprado: {item.itemName}");
        }
        else
        {
            UIManager.Instance?.ShowEffectMessage("¡Dinero insuficiente!");
        }

        UpdateStoreUI();
    }


    private void UpdateStoreUI()
    {
        foreach (StoreItemButton button in currentItemButtons)
        {
            bool alreadyBought = purchasedItemsThisRotation.Contains(button.Item);
            button.SetLocked(alreadyBought);
            button.UpdateInteractability(GameManager.Instance.Money);
        }
    }



    public void ShowStore()
    {
        if (storePanel != null)
        {
            storePanel.SetActive(true);
            UpdateStoreUI();
        }
    }

    public void HideStore()
    {
        if (storePanel != null)
            storePanel.SetActive(false);
    }

    public void ToggleStore()
    {
        if (storePanel == null) return;

        bool isActive = storePanel.activeSelf;
        storePanel.SetActive(!isActive);

        if (!isActive)
            UpdateStoreUI();
    }

    private static void Shuffle<T>(IList<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
    private void TrySubscribe()
    {
        if (subscribed) return;
        if (GameManager.Instance == null) return;

        GameManager.Instance.OnSpinFinished += HandleSpinFinished;
        subscribed = true;
    }

    

    
}
