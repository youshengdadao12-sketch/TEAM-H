using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class ShopUIController : MonoBehaviour
{
    [Serializable]
    private sealed class ShopCategorySection
    {
        [SerializeField] private string displayName;
        [SerializeField] private Button categoryButton;
        [SerializeField] private GameObject itemPanel;
        [SerializeField] private ShopItemDefinition[] items;

        public string DisplayName => displayName;
        public Button CategoryButton => categoryButton;
        public GameObject ItemPanel => itemPanel;
        public ShopItemDefinition[] Items => items;
    }

    [Serializable]
    private sealed class ShopItemDefinition
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private string slotType;
        [TextArea]
        [SerializeField] private string description;
        [SerializeField] private int price;
        [SerializeField] private bool owned;
        [SerializeField] private Button button;
        [SerializeField] private GameObject cardRoot;
        [SerializeField] private Text priceLabel;
        [SerializeField] private Text ownedLabel;

        public string Id => id;
        public string DisplayName => displayName;
        public string SlotType => slotType;
        public string Description => description;
        public int Price => price;
        public int SellPrice => Mathf.RoundToInt(price * 0.6f);
        public bool Owned
        {
            get => owned;
            set => owned = value;
        }

        public Button Button => button;
        public GameObject CardRoot => cardRoot;
        public Text PriceLabel => priceLabel;
        public Text OwnedLabel => ownedLabel;
    }

    [Header("Shop Data")]
    [SerializeField] private ShopCategorySection[] categories;

    [Header("Detail View")]
    [SerializeField] private Text categoryTitleText;
    [SerializeField] private Text itemNameText;
    [SerializeField] private Text slotTypeText;
    [SerializeField] private Text descriptionText;
    [SerializeField] private Text priceText;
    [SerializeField] private Text ownershipText;
    [SerializeField] private Text statusText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button sellButton;
    [SerializeField] private Button backButton;

    private static readonly Color CategoryNormal = Hex("182C34");
    private static readonly Color CategorySelected = Hex("E5B64E");
    private static readonly Color ItemNormal = Hex("102027");
    private static readonly Color ItemSelected = Hex("273A3F");
    private static readonly Color TextDark = Hex("071017");
    private static readonly Color TextLight = Hex("F5F6EF");

    private int selectedCategoryIndex;
    private int selectedItemIndex;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        ApplyJapaneseFont();
        BindButtons();
    }

    private void Start()
    {
        SelectCategory(0);
    }

    private void BindButtons()
    {
        if (categories != null)
        {
            for (int categoryIndex = 0; categoryIndex < categories.Length; categoryIndex++)
            {
                int capturedCategory = categoryIndex;
                Button categoryButton = categories[categoryIndex]?.CategoryButton;
                if (categoryButton != null)
                {
                    categoryButton.onClick.AddListener(() => SelectCategory(capturedCategory));
                }

                ShopItemDefinition[] items = categories[categoryIndex]?.Items;
                if (items == null)
                {
                    continue;
                }

                for (int itemIndex = 0; itemIndex < items.Length; itemIndex++)
                {
                    int capturedItem = itemIndex;
                    Button itemButton = items[itemIndex]?.Button;
                    if (itemButton != null)
                    {
                        itemButton.onClick.AddListener(() => SelectItem(capturedCategory, capturedItem));
                    }
                }
            }
        }

        if (buyButton != null)
        {
            buyButton.onClick.AddListener(BuySelectedItem);
        }

        if (sellButton != null)
        {
            sellButton.onClick.AddListener(SellSelectedItem);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(CloseShop);
        }
    }

    private void SelectCategory(int categoryIndex)
    {
        if (categories == null || categories.Length == 0)
        {
            return;
        }

        selectedCategoryIndex = Mathf.Clamp(categoryIndex, 0, categories.Length - 1);
        selectedItemIndex = 0;

        for (int index = 0; index < categories.Length; index++)
        {
            ShopCategorySection category = categories[index];
            if (category == null)
            {
                continue;
            }

            bool selected = index == selectedCategoryIndex;
            if (category.ItemPanel != null)
            {
                category.ItemPanel.SetActive(selected);
            }

            SetButtonImageColor(category.CategoryButton, selected ? CategorySelected : CategoryNormal);
            SetButtonTextColor(category.CategoryButton, selected ? TextDark : TextLight);
        }

        UpdateDetail();
    }

    private void SelectItem(int categoryIndex, int itemIndex)
    {
        if (categories == null || categories.Length == 0)
        {
            return;
        }

        selectedCategoryIndex = Mathf.Clamp(categoryIndex, 0, categories.Length - 1);
        ShopItemDefinition[] items = categories[selectedCategoryIndex].Items;
        selectedItemIndex = Mathf.Clamp(itemIndex, 0, items.Length - 1);
        UpdateDetail();
    }

    private void UpdateDetail()
    {
        ShopCategorySection category = GetSelectedCategory();
        ShopItemDefinition item = GetSelectedItem();
        if (category == null || item == null)
        {
            return;
        }

        categoryTitleText.text = category.DisplayName;
        itemNameText.text = item.DisplayName;
        slotTypeText.text = item.SlotType;
        descriptionText.text = item.Description;
        priceText.text = item.Owned ? $"Sell price {item.SellPrice:N0} C" : $"Buy price {item.Price:N0} C";
        ownershipText.text = item.Owned ? "Owned" : "Not owned";
        statusText.text = item.Owned
            ? "This part can be sold and returned to the item list later."
            : "Buying this part adds it to your owned equipment.";

        UpdateActionButton(buyButton, !item.Owned, item.Owned ? "Purchased" : "Buy");
        UpdateActionButton(sellButton, item.Owned, "Sell");
        UpdateItemCards();

        item.Button?.Select();
        EventSystem.current?.SetSelectedGameObject(item.Button != null ? item.Button.gameObject : null);
    }

    private void UpdateItemCards()
    {
        for (int categoryIndex = 0; categoryIndex < categories.Length; categoryIndex++)
        {
            ShopItemDefinition[] items = categories[categoryIndex].Items;
            if (items == null)
            {
                continue;
            }

            for (int itemIndex = 0; itemIndex < items.Length; itemIndex++)
            {
                ShopItemDefinition current = items[itemIndex];
                bool selected = categoryIndex == selectedCategoryIndex && itemIndex == selectedItemIndex;
                SetCardColor(current?.CardRoot, selected ? ItemSelected : ItemNormal);

                if (current?.PriceLabel != null)
                {
                    current.PriceLabel.text = current.Owned ? $"SELL {current.SellPrice:N0} C" : $"{current.Price:N0} C";
                }

                if (current?.OwnedLabel != null)
                {
                    current.OwnedLabel.gameObject.SetActive(current.Owned);
                }
            }
        }
    }

    private void BuySelectedItem()
    {
        ShopItemDefinition item = GetSelectedItem();
        if (item == null || item.Owned)
        {
            return;
        }

        item.Owned = true;
        Debug.Log($"Purchased shop item: {item.Id}");
        UpdateDetail();
    }

    private void SellSelectedItem()
    {
        ShopItemDefinition item = GetSelectedItem();
        if (item == null || !item.Owned)
        {
            return;
        }

        item.Owned = false;
        Debug.Log($"Sold shop item: {item.Id}");
        UpdateDetail();
    }

    private void CloseShop()
    {
        int activeIndex = SceneManager.GetActiveScene().buildIndex;
        if (activeIndex > 0)
        {
            SceneManager.LoadScene(activeIndex - 1);
            return;
        }

        Debug.Log("Back button pressed from Shop scene.");
    }

    private ShopCategorySection GetSelectedCategory()
    {
        if (categories == null || categories.Length == 0)
        {
            return null;
        }

        selectedCategoryIndex = Mathf.Clamp(selectedCategoryIndex, 0, categories.Length - 1);
        return categories[selectedCategoryIndex];
    }

    private ShopItemDefinition GetSelectedItem()
    {
        ShopCategorySection category = GetSelectedCategory();
        ShopItemDefinition[] items = category?.Items;
        if (items == null || items.Length == 0)
        {
            return null;
        }

        selectedItemIndex = Mathf.Clamp(selectedItemIndex, 0, items.Length - 1);
        return items[selectedItemIndex];
    }

    private static void UpdateActionButton(Button button, bool interactable, string label)
    {
        if (button == null)
        {
            return;
        }

        button.interactable = interactable;
        Text buttonText = button.GetComponentInChildren<Text>(true);
        if (buttonText != null)
        {
            buttonText.text = label;
        }
    }

    private static void SetButtonImageColor(Button button, Color color)
    {
        if (button != null && button.targetGraphic is Image image)
        {
            image.color = color;
        }
    }

    private static void SetButtonTextColor(Button button, Color color)
    {
        if (button == null)
        {
            return;
        }

        foreach (Text text in button.GetComponentsInChildren<Text>(true))
        {
            text.color = color;
        }
    }

    private static void SetCardColor(GameObject card, Color color)
    {
        if (card != null && card.TryGetComponent(out Image image))
        {
            image.color = color;
        }
    }

    private void ApplyJapaneseFont()
    {
        Font font = CreateJapaneseFont();
        if (font == null)
        {
            return;
        }

        foreach (Text label in GetComponentsInChildren<Text>(true))
        {
            label.font = font;
        }
    }

    private static Font CreateJapaneseFont()
    {
        string[] preferredFonts =
        {
            "Yu Gothic UI",
            "Yu Gothic",
            "Meiryo UI",
            "Meiryo",
            "Noto Sans CJK JP"
        };

        foreach (string fontName in preferredFonts)
        {
            Font font = Font.CreateDynamicFontFromOSFont(fontName, 32);
            if (font != null)
            {
                return font;
            }
        }

        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    private static Color Hex(string value)
    {
        ColorUtility.TryParseHtmlString($"#{value}", out Color color);
        return color;
    }
}
