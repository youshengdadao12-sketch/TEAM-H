using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ShopSceneBuilder
{
    private const string ScenePath = "Assets/Scenes/Shop.unity";

    private static readonly Color Background = Hex("071017");
    private static readonly Color Panel = Hex("0D1A20", 0.96f);
    private static readonly Color Gold = Hex("E5B64E");
    private static readonly Color Green = Hex("77D9B1");
    private static readonly Color TextMain = Hex("F5F6EF");
    private static readonly Color TextMuted = Hex("8FA3A8");
    private static readonly Color ButtonDark = Hex("182C34");
    private static readonly Color SellRed = Hex("D86C5B");

    private sealed class ShopItemTemplate
    {
        public string Id;
        public string Name;
        public string Slot;
        public string Description;
        public int Price;
        public bool Owned;
    }

    private sealed class CategoryBuildResult
    {
        public string DisplayName;
        public Button CategoryButton;
        public GameObject ItemPanel;
        public List<ItemBuildResult> Items = new();
    }

    private sealed class ItemBuildResult
    {
        public ShopItemTemplate Data;
        public Button Button;
        public GameObject CardRoot;
        public Text PriceLabel;
        public Text OwnedLabel;
    }

    [MenuItem("TEAM-H/Build Shop Screen")]
    public static void Build()
    {
        if (!File.Exists(ScenePath))
        {
            throw new FileNotFoundException("Shop scene was not found.", ScenePath);
        }

        Scene scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            Object.DestroyImmediate(root);
        }

        CreateCamera();
        CreateEventSystem();

        Canvas canvas = CreateCanvas();
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        ShopUIController controller = canvas.gameObject.AddComponent<ShopUIController>();

        CreateImage("Background", canvasRect, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, Background);
        CreateImage("TopShade", canvasRect, new Vector2(0f, 0.72f), Vector2.one, Vector2.zero, Vector2.zero, Hex("173039", 0.58f));
        CreateImage("BottomShade", canvasRect, Vector2.zero, new Vector2(1f, 0.18f), Vector2.zero, Vector2.zero, Hex("03070A", 0.82f));

        CreateHeader(canvasRect);
        CreateFooter(canvasRect);

        RectTransform sidebar = CreatePanel("CategoryPanel", canvasRect, new Vector2(0.04f, 0.15f), new Vector2(0.22f, 0.84f));
        RectTransform itemArea = CreatePanel("ItemArea", canvasRect, new Vector2(0.25f, 0.15f), new Vector2(0.63f, 0.84f));
        RectTransform detail = CreatePanel("DetailPanel", canvasRect, new Vector2(0.66f, 0.15f), new Vector2(0.96f, 0.84f));

        CreateText("CategoryHeading", sidebar, "PARTS", 23, TextAnchor.MiddleLeft, Gold, FontStyle.Bold, new Vector2(0.11f, 0.86f), new Vector2(0.89f, 0.96f), Vector2.zero, Vector2.zero);
        CreateText("CategoryGuide", sidebar, "Weapons / Arms / Legs", 16, TextAnchor.UpperLeft, TextMuted, FontStyle.Normal, new Vector2(0.11f, 0.76f), new Vector2(0.89f, 0.86f), Vector2.zero, Vector2.zero);

        CreateText("InventoryHeading", itemArea, "ITEM LIST", 24, TextAnchor.MiddleLeft, Gold, FontStyle.Bold, new Vector2(0.06f, 0.88f), new Vector2(0.94f, 0.97f), Vector2.zero, Vector2.zero);
        CreateText("InventorySub", itemArea, "Choose from four parts in each category", 16, TextAnchor.MiddleRight, TextMuted, FontStyle.Normal, new Vector2(0.42f, 0.88f), new Vector2(0.94f, 0.97f), Vector2.zero, Vector2.zero);

        CategoryBuildResult[] categories =
        {
            CreateCategory(sidebar, itemArea, 0, "Weapons", "WEAPON", CreateWeaponItems()),
            CreateCategory(sidebar, itemArea, 1, "Arms", "ARMS", CreateArmItems()),
            CreateCategory(sidebar, itemArea, 2, "Legs", "LEGS", CreateLegItems())
        };

        categories[1].ItemPanel.SetActive(false);
        categories[2].ItemPanel.SetActive(false);

        Text detailCategory = CreateText("DetailCategory", detail, "Weapons", 20, TextAnchor.MiddleLeft, Gold, FontStyle.Bold, new Vector2(0.08f, 0.88f), new Vector2(0.92f, 0.96f), Vector2.zero, Vector2.zero);
        Text detailName = CreateText("DetailName", detail, "Blade", 38, TextAnchor.MiddleLeft, TextMain, FontStyle.Bold, new Vector2(0.08f, 0.72f), new Vector2(0.92f, 0.88f), Vector2.zero, Vector2.zero);
        Text detailSlot = CreateText("DetailSlot", detail, "WEAPON", 17, TextAnchor.MiddleLeft, Green, FontStyle.Bold, new Vector2(0.08f, 0.66f), new Vector2(0.92f, 0.72f), Vector2.zero, Vector2.zero);
        Text detailDescription = CreateText("DetailDescription", detail, "A reliable close-range starter weapon.", 20, TextAnchor.UpperLeft, TextMuted, FontStyle.Normal, new Vector2(0.08f, 0.42f), new Vector2(0.92f, 0.64f), Vector2.zero, Vector2.zero);

        RectTransform priceBox = CreateImage("PriceBox", detail, new Vector2(0.08f, 0.27f), new Vector2(0.92f, 0.39f), Vector2.zero, Vector2.zero, Hex("071017", 0.82f));
        Text priceText = CreateText("Price", priceBox, "Buy price 1,200 C", 27, TextAnchor.MiddleLeft, Gold, FontStyle.Bold, new Vector2(0.06f, 0f), new Vector2(0.66f, 1f), Vector2.zero, Vector2.zero);
        Text ownershipText = CreateText("Ownership", priceBox, "Not owned", 17, TextAnchor.MiddleRight, TextMuted, FontStyle.Bold, new Vector2(0.48f, 0f), new Vector2(0.94f, 1f), Vector2.zero, Vector2.zero);

        Text statusText = CreateText("Status", detail, "Buying this part adds it to your owned equipment.", 16, TextAnchor.UpperLeft, TextMuted, FontStyle.Normal, new Vector2(0.08f, 0.19f), new Vector2(0.92f, 0.25f), Vector2.zero, Vector2.zero);
        Button buyButton = CreateActionButton(detail, "BuyButton", "Buy", new Vector2(0.08f, 0.07f), new Vector2(0.39f, 0.16f), Gold, Background);
        Button sellButton = CreateActionButton(detail, "SellButton", "Sell", new Vector2(0.43f, 0.07f), new Vector2(0.68f, 0.16f), SellRed, TextMain);
        Button backButton = CreateActionButton(detail, "BackButton", "Back", new Vector2(0.72f, 0.07f), new Vector2(0.92f, 0.16f), ButtonDark, TextMain);

        AssignController(controller, categories, detailCategory, detailName, detailSlot, detailDescription, priceText, ownershipText, statusText, buyButton, sellButton, backButton);
        AddShopSceneToBuildSettings();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject = canvas.gameObject;
        Debug.Log("TEAM-H shop screen built successfully.");
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Background;
        camera.orthographic = true;
        cameraObject.AddComponent<AudioListener>();
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
    }

    private static void CreateEventSystem()
    {
        GameObject eventSystemObject = new("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }

    private static Canvas CreateCanvas()
    {
        GameObject canvasObject = new("ShopCanvas", typeof(RectTransform));
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.localScale = Vector3.one;

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    private static void CreateHeader(RectTransform parent)
    {
        RectTransform header = CreateImage("Header", parent, new Vector2(0f, 0.87f), Vector2.one, Vector2.zero, Vector2.zero, Hex("050A0D", 0.96f));
        CreateText("ShopTitle", header, "SHOP", 52, TextAnchor.MiddleLeft, TextMain, FontStyle.Bold, new Vector2(0.04f, 0f), new Vector2(0.40f, 1f), Vector2.zero, Vector2.zero);
        CreateText("ShopSubtitle", header, "PARTS BUY / SELL TERMINAL", 18, TextAnchor.MiddleLeft, Gold, FontStyle.Bold, new Vector2(0.19f, 0f), new Vector2(0.56f, 1f), Vector2.zero, Vector2.zero);
        CreateText("Credits", header, "CREDITS  8,400 C", 21, TextAnchor.MiddleRight, Green, FontStyle.Bold, new Vector2(0.55f, 0f), new Vector2(0.96f, 1f), Vector2.zero, Vector2.zero);
    }

    private static void CreateFooter(RectTransform parent)
    {
        RectTransform footer = CreateImage("Footer", parent, Vector2.zero, new Vector2(1f, 0.075f), Vector2.zero, Vector2.zero, Hex("050A0D", 0.96f));
        CreateText("FooterText", footer, "SELECT PARTS  /  BUY EQUIPMENT  /  SELL OWNED PARTS", 17, TextAnchor.MiddleLeft, TextMuted, FontStyle.Bold, new Vector2(0.04f, 0f), new Vector2(0.70f, 1f), Vector2.zero, Vector2.zero);
        CreateText("BuildLabel", footer, "SHOP UI PROTOTYPE", 15, TextAnchor.MiddleRight, Gold, FontStyle.Bold, new Vector2(0.55f, 0f), new Vector2(0.96f, 1f), Vector2.zero, Vector2.zero);
    }

    private static CategoryBuildResult CreateCategory(RectTransform sidebar, RectTransform itemArea, int index, string displayName, string englishName, ShopItemTemplate[] items)
    {
        float top = 0.66f - index * 0.17f;
        Button categoryButton = CreateCategoryButton(sidebar, displayName, englishName, new Vector2(0.11f, top), new Vector2(0.89f, top + 0.12f));

        GameObject panelObject = CreateUIObject($"{englishName}_Items", itemArea);
        RectTransform panel = panelObject.GetComponent<RectTransform>();
        SetRect(panel, new Vector2(0.06f, 0.08f), new Vector2(0.94f, 0.84f), Vector2.zero, Vector2.zero);

        CategoryBuildResult result = new()
        {
            DisplayName = displayName,
            CategoryButton = categoryButton,
            ItemPanel = panelObject
        };

        for (int itemIndex = 0; itemIndex < items.Length; itemIndex++)
        {
            result.Items.Add(CreateItemCard(panel, itemIndex, items[itemIndex]));
        }

        return result;
    }

    private static Button CreateCategoryButton(RectTransform parent, string primary, string secondary, Vector2 anchorMin, Vector2 anchorMax)
    {
        Button button = CreateBaseButton($"Category_{secondary}", parent, anchorMin, anchorMax, ButtonDark);
        RectTransform rect = button.GetComponent<RectTransform>();
        CreateText("Primary", rect, primary, 26, TextAnchor.MiddleLeft, TextMain, FontStyle.Bold, new Vector2(0.10f, 0.36f), new Vector2(0.90f, 0.96f), Vector2.zero, Vector2.zero);
        CreateText("Secondary", rect, secondary, 14, TextAnchor.MiddleLeft, TextMuted, FontStyle.Bold, new Vector2(0.10f, 0.08f), new Vector2(0.90f, 0.42f), Vector2.zero, Vector2.zero);
        return button;
    }

    private static ItemBuildResult CreateItemCard(RectTransform parent, int index, ShopItemTemplate item)
    {
        int column = index % 2;
        int row = index / 2;
        float xMin = column == 0 ? 0f : 0.52f;
        float xMax = column == 0 ? 0.48f : 1f;
        float yMax = row == 0 ? 1f : 0.47f;
        float yMin = row == 0 ? 0.53f : 0f;

        Button button = CreateBaseButton($"Item_{item.Id}", parent, new Vector2(xMin, yMin), new Vector2(xMax, yMax), Hex("102027"));
        RectTransform rect = button.GetComponent<RectTransform>();

        CreateText("Slot", rect, item.Slot, 14, TextAnchor.MiddleLeft, Green, FontStyle.Bold, new Vector2(0.08f, 0.76f), new Vector2(0.92f, 0.92f), Vector2.zero, Vector2.zero);
        CreateText("Name", rect, item.Name, 25, TextAnchor.MiddleLeft, TextMain, FontStyle.Bold, new Vector2(0.08f, 0.52f), new Vector2(0.92f, 0.76f), Vector2.zero, Vector2.zero);
        CreateText("Description", rect, item.Description, 15, TextAnchor.UpperLeft, TextMuted, FontStyle.Normal, new Vector2(0.08f, 0.24f), new Vector2(0.92f, 0.50f), Vector2.zero, Vector2.zero);
        Text price = CreateText("Price", rect, item.Owned ? $"SELL {Mathf.RoundToInt(item.Price * 0.6f):N0} C" : $"{item.Price:N0} C", 18, TextAnchor.LowerLeft, Gold, FontStyle.Bold, new Vector2(0.08f, 0.07f), new Vector2(0.60f, 0.22f), Vector2.zero, Vector2.zero);
        Text owned = CreateText("Owned", rect, "Owned", 14, TextAnchor.LowerRight, Green, FontStyle.Bold, new Vector2(0.52f, 0.07f), new Vector2(0.92f, 0.22f), Vector2.zero, Vector2.zero);
        owned.gameObject.SetActive(item.Owned);

        return new ItemBuildResult
        {
            Data = item,
            Button = button,
            CardRoot = button.gameObject,
            PriceLabel = price,
            OwnedLabel = owned
        };
    }

    private static Button CreateActionButton(RectTransform parent, string name, string label, Vector2 anchorMin, Vector2 anchorMax, Color background, Color foreground)
    {
        Button button = CreateBaseButton(name, parent, anchorMin, anchorMax, background);
        RectTransform rect = button.GetComponent<RectTransform>();
        CreateText("Label", rect, label, 24, TextAnchor.MiddleCenter, foreground, FontStyle.Bold, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        return button;
    }

    private static Button CreateBaseButton(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
    {
        GameObject buttonObject = CreateUIObject(name, parent);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        SetRect(rect, anchorMin, anchorMax, Vector2.zero, Vector2.zero);

        Image image = buttonObject.AddComponent<Image>();
        image.color = color;

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = image;

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Hex("FFF1C3");
        colors.pressedColor = Hex("C99635");
        colors.selectedColor = Hex("FFF1C3");
        colors.disabledColor = Hex("4A5254", 0.45f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        Outline outline = buttonObject.AddComponent<Outline>();
        outline.effectColor = Hex("E5B64E", 0.34f);
        outline.effectDistance = new Vector2(2f, -2f);
        return button;
    }

    private static RectTransform CreatePanel(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        RectTransform panel = CreateImage(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero, Panel);
        Outline outline = panel.gameObject.AddComponent<Outline>();
        outline.effectColor = Hex("395B63", 0.72f);
        outline.effectDistance = new Vector2(2f, -2f);
        return panel;
    }

    private static GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject gameObject = new(name, typeof(RectTransform));
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }

    private static RectTransform CreateImage(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, Color color)
    {
        GameObject gameObject = CreateUIObject(name, parent);
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        SetRect(rect, anchorMin, anchorMax, offsetMin, offsetMax);
        Image image = gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return rect;
    }

    private static Text CreateText(string name, RectTransform parent, string value, int size, TextAnchor alignment, Color color, FontStyle style, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        GameObject gameObject = CreateUIObject(name, parent);
        RectTransform rect = gameObject.GetComponent<RectTransform>();
        SetRect(rect, anchorMin, anchorMax, offsetMin, offsetMax);

        Text text = gameObject.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
        rect.localScale = Vector3.one;
    }

    private static void AssignController(
        ShopUIController controller,
        CategoryBuildResult[] categories,
        Text detailCategory,
        Text detailName,
        Text detailSlot,
        Text detailDescription,
        Text priceText,
        Text ownershipText,
        Text statusText,
        Button buyButton,
        Button sellButton,
        Button backButton)
    {
        SerializedObject data = new(controller);
        SerializedProperty categoryArray = data.FindProperty("categories");
        categoryArray.arraySize = categories.Length;

        for (int categoryIndex = 0; categoryIndex < categories.Length; categoryIndex++)
        {
            CategoryBuildResult category = categories[categoryIndex];
            SerializedProperty categoryProperty = categoryArray.GetArrayElementAtIndex(categoryIndex);
            categoryProperty.FindPropertyRelative("displayName").stringValue = category.DisplayName;
            categoryProperty.FindPropertyRelative("categoryButton").objectReferenceValue = category.CategoryButton;
            categoryProperty.FindPropertyRelative("itemPanel").objectReferenceValue = category.ItemPanel;

            SerializedProperty itemArray = categoryProperty.FindPropertyRelative("items");
            itemArray.arraySize = category.Items.Count;
            for (int itemIndex = 0; itemIndex < category.Items.Count; itemIndex++)
            {
                ItemBuildResult item = category.Items[itemIndex];
                SerializedProperty itemProperty = itemArray.GetArrayElementAtIndex(itemIndex);
                itemProperty.FindPropertyRelative("id").stringValue = item.Data.Id;
                itemProperty.FindPropertyRelative("displayName").stringValue = item.Data.Name;
                itemProperty.FindPropertyRelative("slotType").stringValue = item.Data.Slot;
                itemProperty.FindPropertyRelative("description").stringValue = item.Data.Description;
                itemProperty.FindPropertyRelative("price").intValue = item.Data.Price;
                itemProperty.FindPropertyRelative("owned").boolValue = item.Data.Owned;
                itemProperty.FindPropertyRelative("button").objectReferenceValue = item.Button;
                itemProperty.FindPropertyRelative("cardRoot").objectReferenceValue = item.CardRoot;
                itemProperty.FindPropertyRelative("priceLabel").objectReferenceValue = item.PriceLabel;
                itemProperty.FindPropertyRelative("ownedLabel").objectReferenceValue = item.OwnedLabel;
            }
        }

        data.FindProperty("categoryTitleText").objectReferenceValue = detailCategory;
        data.FindProperty("itemNameText").objectReferenceValue = detailName;
        data.FindProperty("slotTypeText").objectReferenceValue = detailSlot;
        data.FindProperty("descriptionText").objectReferenceValue = detailDescription;
        data.FindProperty("priceText").objectReferenceValue = priceText;
        data.FindProperty("ownershipText").objectReferenceValue = ownershipText;
        data.FindProperty("statusText").objectReferenceValue = statusText;
        data.FindProperty("buyButton").objectReferenceValue = buyButton;
        data.FindProperty("sellButton").objectReferenceValue = sellButton;
        data.FindProperty("backButton").objectReferenceValue = backButton;
        data.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AddShopSceneToBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes
            .Where(scene => scene.path != ScenePath)
            .ToList();
        scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static ShopItemTemplate[] CreateWeaponItems()
    {
        return new[]
        {
            new ShopItemTemplate { Id = "weapon_blade", Name = "Blade", Slot = "WEAPON", Description = "A reliable close-range starter weapon.", Price = 1200, Owned = true },
            new ShopItemTemplate { Id = "weapon_rifle", Name = "Rifle", Slot = "WEAPON", Description = "A mid-range weapon with steady firepower.", Price = 1800 },
            new ShopItemTemplate { Id = "weapon_shotgun", Name = "Shotgun", Slot = "WEAPON", Description = "Deals wide damage at close range.", Price = 2400 },
            new ShopItemTemplate { Id = "weapon_railgun", Name = "Railgun", Slot = "WEAPON", Description = "An expensive weapon with strong piercing power.", Price = 3600 }
        };
    }

    private static ShopItemTemplate[] CreateArmItems()
    {
        return new[]
        {
            new ShopItemTemplate { Id = "arms_power", Name = "Power Arm", Slot = "ARMS", Description = "Improves close-range attacks and recoil control.", Price = 1300, Owned = true },
            new ShopItemTemplate { Id = "arms_shield", Name = "Shield Arm", Slot = "ARMS", Description = "Raises defense when taking hits.", Price = 2200 },
            new ShopItemTemplate { Id = "arms_magnet", Name = "Magnet Arm", Slot = "ARMS", Description = "Makes distant items easier to collect.", Price = 2600 },
            new ShopItemTemplate { Id = "arms_repair", Name = "Repair Arm", Slot = "ARMS", Description = "Improves recovery efficiency during battle.", Price = 3200 }
        };
    }

    private static ShopItemTemplate[] CreateLegItems()
    {
        return new[]
        {
            new ShopItemTemplate { Id = "legs_light", Name = "Light Legs", Slot = "LEGS", Description = "A lightweight part that improves movement speed.", Price = 1000, Owned = true },
            new ShopItemTemplate { Id = "legs_hover", Name = "Hover Legs", Slot = "LEGS", Description = "Stabilizes control while airborne.", Price = 2100 },
            new ShopItemTemplate { Id = "legs_sprint", Name = "Sprint Legs", Slot = "LEGS", Description = "Boosts dash acceleration on the ground.", Price = 2500 },
            new ShopItemTemplate { Id = "legs_jump", Name = "Jump Legs", Slot = "LEGS", Description = "Improves jump height and landing recovery.", Price = 3000 }
        };
    }

    private static Color Hex(string value, float alpha = 1f)
    {
        ColorUtility.TryParseHtmlString($"#{value}", out Color color);
        color.a = alpha;
        return color;
    }
}
