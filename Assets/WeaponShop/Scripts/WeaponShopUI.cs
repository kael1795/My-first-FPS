using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Blacksite.WeaponShop
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Blacksite/Weapon Shop UI")]
    public sealed class WeaponShopUI : MonoBehaviour
    {
        private static readonly Color Background = new Color32(16, 18, 15, 255);
        private static readonly Color Surface = new Color32(24, 27, 22, 255);
        private static readonly Color Surface2 = new Color32(32, 36, 29, 255);
        private static readonly Color Surface3 = new Color32(41, 46, 37, 255);
        private static readonly Color Line = new Color32(59, 65, 53, 255);
        private static readonly Color LineSoft = new Color32(44, 49, 41, 255);
        private static readonly Color Ink = new Color32(241, 242, 233, 255);
        private static readonly Color Muted = new Color32(158, 166, 149, 255);
        private static readonly Color Dim = new Color32(105, 113, 99, 255);
        private static readonly Color Acid = new Color32(201, 244, 74, 255);
        private static readonly Color Amber = new Color32(255, 184, 77, 255);
        private static readonly Color Danger = new Color32(255, 107, 87, 255);
        private static readonly Color Cyan = new Color32(98, 215, 218, 255);

        [Header("Economy")]
        [SerializeField] private int startingCredits = 42680;
        [SerializeField] private int startingTokens = 18;

        [Header("Catalog")]
        [SerializeField] private List<ShopItemData> catalog = new List<ShopItemData>();

        private readonly Dictionary<string, int> cart = new Dictionary<string, int>();
        private readonly HashSet<string> ownedWeapons = new HashSet<string>();
        private readonly Dictionary<string, Button> filterButtons = new Dictionary<string, Button>();
        private readonly Dictionary<string, Text> filterButtonLabels = new Dictionary<string, Text>();
        private readonly Dictionary<string, Sprite> roundedSprites = new Dictionary<string, Sprite>();

        private Canvas canvas;
        private Font font;
        private RectTransform appRoot;
        private RectTransform catalogPanel;
        private RectTransform cartPanel;
        private RectTransform productContent;
        private RectTransform cartContent;
        private RectTransform toastRoot;
        private GridLayoutGroup productGrid;
        private InputField searchInput;
        private Text resultText;
        private Text creditsText;
        private Text cartCountText;
        private Text weaponSubtotalText;
        private Text ammoSubtotalText;
        private Text orderTotalText;
        private Button checkoutButton;
        private Image checkoutButtonImage;
        private string activeFilter = "all";
        private string searchTerm = "";
        private int lastColumnCount = -1;
        private float lastResponsiveWidth = -1f;
        private bool showingNoResults;

        private void Awake()
        {
            if (catalog == null || catalog.Count == 0)
            {
                catalog = DefaultWeaponShopCatalog.Create();
            }

            font = ResolveFont();
            EnsureEventSystem();
            BuildInterface();
            ApplyResponsiveLayout(true);
            RefreshCatalog();
            RefreshCart();
        }

        private void Update()
        {
            if (appRoot.rect.width > 0f && Mathf.Abs(appRoot.rect.width - lastResponsiveWidth) > 1f)
            {
                ApplyResponsiveLayout(false);
            }
        }

        private void OnDestroy()
        {
            foreach (Sprite sprite in roundedSprites.Values)
            {
                if (sprite != null)
                {
                    Destroy(sprite.texture);
                    Destroy(sprite);
                }
            }
        }

        private void BuildInterface()
        {
            GameObject canvasObject = new GameObject(
                "WeaponShopCanvas",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );
            canvasObject.transform.SetParent(transform, false);

            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform background = CreateRect("Background", canvas.transform);
            Stretch(background);
            Image backgroundImage = background.gameObject.AddComponent<Image>();
            backgroundImage.color = Background;

            appRoot = CreateRect("App", canvas.transform);
            Stretch(appRoot, 16f, 16f, 16f, 16f);

            BuildHeader(appRoot);
            BuildCommandStrip(appRoot);
            BuildMainLayout(appRoot);

            toastRoot = CreateRect("ToastStack", canvas.transform);
            toastRoot.anchorMin = new Vector2(1f, 0f);
            toastRoot.anchorMax = new Vector2(1f, 0f);
            toastRoot.pivot = new Vector2(1f, 0f);
            toastRoot.anchoredPosition = new Vector2(-20f, 20f);
            toastRoot.sizeDelta = new Vector2(350f, 300f);
            VerticalLayoutGroup toastLayout = toastRoot.gameObject.AddComponent<VerticalLayoutGroup>();
            toastLayout.childAlignment = TextAnchor.LowerRight;
            toastLayout.spacing = 8f;
            toastLayout.childControlWidth = true;
            toastLayout.childControlHeight = true;
            toastLayout.childForceExpandWidth = false;
            toastLayout.childForceExpandHeight = false;
        }

        private void BuildHeader(RectTransform parent)
        {
            RectTransform header = CreatePanel("Header", parent, Surface, 7);
            TopStretch(header, 64f, 0f);
            AddOutline(header.gameObject, Line, 1f);

            HorizontalLayoutGroup layout = header.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(14, 12, 9, 9);
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            RectTransform brand = CreateRect("Brand", header);
            SetLayoutSize(brand, 250f, 44f, 0f);
            HorizontalLayoutGroup brandLayout = brand.gameObject.AddComponent<HorizontalLayoutGroup>();
            brandLayout.spacing = 10f;
            brandLayout.childAlignment = TextAnchor.MiddleLeft;
            brandLayout.childControlWidth = true;
            brandLayout.childControlHeight = true;
            brandLayout.childForceExpandWidth = false;
            brandLayout.childForceExpandHeight = false;

            RectTransform mark = CreatePanel("Mark", brand, new Color32(17, 20, 15, 255), 6);
            SetLayoutSize(mark, 40f, 40f, 0f);
            AddOutline(mark.gameObject, new Color32(87, 97, 75, 255), 1f);
            Image markIcon = CreateImage("Icon", mark, Color.white);
            Stretch(markIcon.rectTransform, 7f, 7f, 7f, 7f);
            markIcon.sprite = WeaponIconFactory.Get("rifle", Acid);
            markIcon.preserveAspect = true;

            RectTransform brandCopy = CreateRect("Copy", brand);
            SetLayoutSize(brandCopy, 196f, 40f, 1f);
            VerticalLayoutGroup brandCopyLayout = brandCopy.gameObject.AddComponent<VerticalLayoutGroup>();
            brandCopyLayout.childAlignment = TextAnchor.MiddleLeft;
            brandCopyLayout.spacing = 2f;
            brandCopyLayout.childControlWidth = true;
            brandCopyLayout.childControlHeight = true;
            brandCopyLayout.childForceExpandWidth = true;
            brandCopyLayout.childForceExpandHeight = false;

            Text title = CreateText("Title", brandCopy, "黑站军需终端", 15, Ink, FontStyle.Bold);
            title.horizontalOverflow = HorizontalWrapMode.Overflow;
            SetLayoutSize(title.rectTransform, 0f, 21f, 1f);

            Text subtitle = CreateText("Subtitle", brandCopy, "BLACKSITE ARMORY // 07", 10, Dim);
            subtitle.horizontalOverflow = HorizontalWrapMode.Overflow;
            SetLayoutSize(subtitle.rectTransform, 0f, 15f, 1f);

            RectTransform spacer = CreateRect("Spacer", header);
            SetLayoutSize(spacer, 0f, 1f, 1f);

            RectTransform status = CreateRect("SystemStatus", header);
            SetLayoutSize(status, 205f, 32f, 0f);
            HorizontalLayoutGroup statusLayout = status.gameObject.AddComponent<HorizontalLayoutGroup>();
            statusLayout.spacing = 8f;
            statusLayout.childAlignment = TextAnchor.MiddleLeft;
            statusLayout.childControlWidth = true;
            statusLayout.childControlHeight = true;
            statusLayout.childForceExpandWidth = false;
            statusLayout.childForceExpandHeight = false;

            Image dot = CreateImage("StatusDot", status, Acid);
            dot.sprite = GetRoundedSprite(8);
            dot.type = Image.Type.Sliced;
            SetLayoutSize(dot.rectTransform, 8f, 8f, 0f);

            Text statusText = CreateText("StatusText", status, "补给链路在线 · 折扣同步 100%", 11, Muted);
            statusText.horizontalOverflow = HorizontalWrapMode.Overflow;
            SetLayoutSize(statusText.rectTransform, 188f, 28f, 0f);

            RectTransform currencyGroup = CreateRect("CurrencyGroup", header);
            SetLayoutSize(currencyGroup, 300f, 46f, 0f);
            HorizontalLayoutGroup currencyLayout = currencyGroup.gameObject.AddComponent<HorizontalLayoutGroup>();
            currencyLayout.spacing = 8f;
            currencyLayout.childAlignment = TextAnchor.MiddleLeft;
            currencyLayout.childControlWidth = true;
            currencyLayout.childControlHeight = true;
            currencyLayout.childForceExpandWidth = true;
            currencyLayout.childForceExpandHeight = false;

            CreateCurrencyPill(currencyGroup, "Credits", "信用点", startingCredits.ToString("N0"), Amber, out creditsText);
            CreateCurrencyPill(currencyGroup, "Tokens", "战备代币", startingTokens.ToString("N0"), Cyan, out _);

            RectTransform commander = CreateRect("Commander", header);
            SetLayoutSize(commander, 126f, 44f, 0f);
            HorizontalLayoutGroup commanderLayout = commander.gameObject.AddComponent<HorizontalLayoutGroup>();
            commanderLayout.spacing = 8f;
            commanderLayout.childAlignment = TextAnchor.MiddleLeft;
            commanderLayout.childControlWidth = true;
            commanderLayout.childControlHeight = true;
            commanderLayout.childForceExpandWidth = false;
            commanderLayout.childForceExpandHeight = false;

            RectTransform avatar = CreatePanel("Avatar", commander, new Color32(36, 41, 31, 255), 6);
            SetLayoutSize(avatar, 38f, 38f, 0f);
            AddOutline(avatar.gameObject, new Color32(89, 96, 72, 255), 1f);
            Text avatarText = CreateText("Text", avatar, "GH", 13, Ink, FontStyle.Bold);
            Stretch(avatarText.rectTransform, 2f, 2f, 2f, 2f);
            avatarText.alignment = TextAnchor.MiddleCenter;

            RectTransform commanderCopy = CreateRect("Copy", commander);
            SetLayoutSize(commanderCopy, 78f, 38f, 0f);
            VerticalLayoutGroup commanderCopyLayout = commanderCopy.gameObject.AddComponent<VerticalLayoutGroup>();
            commanderCopyLayout.childAlignment = TextAnchor.MiddleLeft;
            commanderCopyLayout.childControlWidth = true;
            commanderCopyLayout.childControlHeight = true;
            commanderCopyLayout.childForceExpandWidth = true;
            commanderCopyLayout.childForceExpandHeight = false;

            Text commanderName = CreateText("Name", commanderCopy, "GHOST-07", 11, Ink, FontStyle.Bold);
            commanderName.horizontalOverflow = HorizontalWrapMode.Overflow;
            SetLayoutSize(commanderName.rectTransform, 0f, 18f, 1f);
            Text commanderRank = CreateText("Rank", commanderCopy, "权限等级 12", 9, Dim);
            commanderRank.horizontalOverflow = HorizontalWrapMode.Overflow;
            SetLayoutSize(commanderRank.rectTransform, 0f, 15f, 1f);
        }

        private void CreateCurrencyPill(
            RectTransform parent,
            string name,
            string label,
            string value,
            Color accent,
            out Text valueText
        )
        {
            RectTransform pill = CreatePanel(name, parent, new Color32(17, 20, 15, 255), 6);
            SetLayoutSize(pill, 142f, 46f, 1f);
            AddOutline(pill.gameObject, Line, 1f);

            Image icon = CreateImage("Icon", pill, accent);
            icon.sprite = GetRoundedSprite(8);
            icon.type = Image.Type.Sliced;
            SetRect(icon.rectTransform, 0f, 0f, 17f, 17f, 11f, 14f, false);

            Text labelText = CreateText("Label", pill, label, 9, Dim);
            SetRect(labelText.rectTransform, 36f, 0f, 96f, 13f, 6f, 25f, false);

            valueText = CreateText("Value", pill, value, 15, Ink, FontStyle.Bold);
            valueText.font = font;
            SetRect(valueText.rectTransform, 36f, 0f, 96f, 20f, 19f, 5f, false);
        }

        private void BuildCommandStrip(RectTransform parent)
        {
            RectTransform strip = CreateRect("CommandStrip", parent);
            TopStretch(strip, 60f, 74f);
            HorizontalLayoutGroup layout = strip.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            RectTransform lead = CreatePanel("Cycle", strip, new Color32(24, 27, 22, 245), 6);
            SetLayoutSize(lead, 460f, 60f, 1f);
            AddOutline(lead.gameObject, LineSoft, 1f);
            Image accent = CreateImage("Accent", lead, Amber);
            accent.sprite = GetRoundedSprite(1);
            SetRect(accent.rectTransform, 0f, 10f, 3f, 40f, 0f, 0f, false);
            AddCommandLabel(lead, "当前补给周期", "灰湾防线 · 第 4 阶段", 16f, 31f);

            RectTransform timer = CreatePanel("Timer", lead, new Color32(45, 37, 25, 255), 5);
            timer.anchorMin = new Vector2(1f, 0.5f);
            timer.anchorMax = new Vector2(1f, 0.5f);
            timer.pivot = new Vector2(1f, 0.5f);
            timer.anchoredPosition = new Vector2(-13f, 0f);
            timer.sizeDelta = new Vector2(112f, 28f);
            AddOutline(timer.gameObject, new Color32(115, 82, 39, 255), 1f);
            Text timerText = CreateText("Text", timer, "剩余 01:42:18", 10, Amber);
            Stretch(timerText.rectTransform, 5f, 5f, 3f, 3f);
            timerText.alignment = TextAnchor.MiddleCenter;

            CreateCommandCell(strip, "本周成交", "12,480 CR");
            CreateCommandCell(strip, "军需等级", "资深 · 12");
            CreateCommandCell(strip, "下次空投", "11 分钟后");
        }

        private void AddCommandLabel(RectTransform parent, string label, string value, float top, float valueHeight)
        {
            Text labelText = CreateText("Label", parent, label, 9, Dim);
            SetRect(labelText.rectTransform, 14f, 146f, 0f, 14f, top, 0f, true);
            Text valueText = CreateText("Value", parent, value, 14, Ink, FontStyle.Bold);
            valueText.horizontalOverflow = HorizontalWrapMode.Overflow;
            SetRect(valueText.rectTransform, 14f, 146f, 0f, valueHeight, top + 14f, 0f, true);
        }

        private void CreateCommandCell(RectTransform parent, string label, string value)
        {
            RectTransform cell = CreatePanel(label, parent, new Color32(24, 27, 22, 180), 6);
            SetLayoutSize(cell, 160f, 60f, 0f);
            AddOutline(cell.gameObject, LineSoft, 1f);
            Text labelText = CreateText("Label", cell, label, 9, Dim);
            SetRect(labelText.rectTransform, 12f, 12f, 0f, 14f, 11f, 0f, true);
            Text valueText = CreateText("Value", cell, value, 14, Ink, FontStyle.Bold);
            SetRect(valueText.rectTransform, 12f, 12f, 0f, 20f, 27f, 0f, true);
        }

        private void BuildMainLayout(RectTransform parent)
        {
            RectTransform main = CreateRect("Main", parent);
            main.anchorMin = new Vector2(0f, 0f);
            main.anchorMax = new Vector2(1f, 1f);
            main.offsetMin = Vector2.zero;
            main.offsetMax = new Vector2(0f, -144f);

            catalogPanel = CreateRect("Catalog", main);
            catalogPanel.anchorMin = Vector2.zero;
            catalogPanel.anchorMax = Vector2.one;
            catalogPanel.offsetMin = Vector2.zero;
            catalogPanel.offsetMax = new Vector2(-372f, 0f);

            cartPanel = CreatePanel("Cart", main, Surface, 7);
            cartPanel.anchorMin = new Vector2(1f, 0f);
            cartPanel.anchorMax = new Vector2(1f, 1f);
            cartPanel.pivot = new Vector2(1f, 0.5f);
            cartPanel.offsetMin = new Vector2(-356f, 0f);
            cartPanel.offsetMax = Vector2.zero;
            AddOutline(cartPanel.gameObject, Line, 1f);

            BuildCatalog(catalogPanel);
            BuildCart(cartPanel);
        }

        private void BuildCatalog(RectTransform parent)
        {
            RectTransform toolbar = CreatePanel("Toolbar", parent, Surface, 7);
            TopStretch(toolbar, 58f, 0f);
            AddOutline(toolbar.gameObject, Line, 1f);
            HorizontalLayoutGroup toolbarLayout = toolbar.gameObject.AddComponent<HorizontalLayoutGroup>();
            toolbarLayout.padding = new RectOffset(9, 9, 9, 9);
            toolbarLayout.spacing = 9f;
            toolbarLayout.childAlignment = TextAnchor.MiddleLeft;
            toolbarLayout.childControlWidth = true;
            toolbarLayout.childControlHeight = true;
            toolbarLayout.childForceExpandWidth = false;
            toolbarLayout.childForceExpandHeight = false;

            searchInput = CreateSearchField(toolbar);
            SetLayoutSize(searchInput.GetComponent<RectTransform>(), 310f, 38f, 1f);

            RectTransform filters = CreateRect("Filters", toolbar);
            SetLayoutSize(filters, 570f, 38f, 0f);
            HorizontalLayoutGroup filterLayout = filters.gameObject.AddComponent<HorizontalLayoutGroup>();
            filterLayout.spacing = 4f;
            filterLayout.childAlignment = TextAnchor.MiddleLeft;
            filterLayout.childControlWidth = true;
            filterLayout.childControlHeight = true;
            filterLayout.childForceExpandWidth = false;
            filterLayout.childForceExpandHeight = false;

            CreateFilterButton(filters, "all", "全部", 58f);
            CreateFilterButton(filters, "rifle", "突击步枪", 82f);
            CreateFilterButton(filters, "smg", "冲锋枪", 70f);
            CreateFilterButton(filters, "sniper", "狙击枪", 64f);
            CreateFilterButton(filters, "shotgun", "霰弹枪", 64f);
            CreateFilterButton(filters, "pistol", "手枪", 52f);
            CreateFilterButton(filters, "ammo", "弹药", 52f);

            RectTransform meta = CreateRect("CatalogMeta", parent);
            TopStretch(meta, 32f, 68f);
            Text heading = CreateText("Heading", meta, "可用军需", 15, Ink, FontStyle.Bold);
            SetRect(heading.rectTransform, 2f, 0f, 360f, 26f, 3f, 0f, true);
            resultText = CreateText("ResultCount", meta, "", 10, Dim);
            resultText.alignment = TextAnchor.MiddleRight;
            SetRect(resultText.rectTransform, 0f, 2f, 0f, 26f, 3f, 0f, true);

            RectTransform scrollRoot = CreateRect("ProductScroll", parent);
            Stretch(scrollRoot, 0f, 13f, 107f, 0f);
            ScrollRect scrollRect = scrollRoot.gameObject.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 36f;

            RectTransform viewport = CreateRect("Viewport", scrollRoot);
            Stretch(viewport);
            viewport.gameObject.AddComponent<RectMask2D>();
            Image viewportRaycast = viewport.gameObject.AddComponent<Image>();
            viewportRaycast.color = Color.clear;

            productContent = CreateRect("Content", viewport);
            productContent.anchorMin = new Vector2(0f, 1f);
            productContent.anchorMax = new Vector2(1f, 1f);
            productContent.pivot = new Vector2(0.5f, 1f);
            productContent.anchoredPosition = Vector2.zero;
            productContent.sizeDelta = Vector2.zero;

            productGrid = productContent.gameObject.AddComponent<GridLayoutGroup>();
            productGrid.cellSize = new Vector2(248f, 350f);
            productGrid.spacing = new Vector2(10f, 10f);
            productGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            productGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
            productGrid.childAlignment = TextAnchor.UpperLeft;
            productGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            productGrid.constraintCount = 3;

            ContentSizeFitter contentFitter = productContent.gameObject.AddComponent<ContentSizeFitter>();
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            RectTransform scrollbarRect = CreatePanel("Scrollbar", scrollRoot, new Color32(33, 37, 30, 255), 3);
            scrollbarRect.anchorMin = new Vector2(1f, 0f);
            scrollbarRect.anchorMax = new Vector2(1f, 1f);
            scrollbarRect.pivot = new Vector2(1f, 0.5f);
            scrollbarRect.offsetMin = new Vector2(-6f, 2f);
            scrollbarRect.offsetMax = new Vector2(0f, -2f);
            Scrollbar scrollbar = scrollbarRect.gameObject.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;

            RectTransform handle = CreatePanel("Handle", scrollbarRect, new Color32(92, 101, 81, 255), 3);
            Stretch(handle);
            Image handleImage = handle.GetComponent<Image>();
            scrollbar.handleRect = handle;
            scrollbar.targetGraphic = handleImage;
            scrollbar.size = 0.16f;

            scrollRect.viewport = viewport;
            scrollRect.content = productContent;
            scrollRect.verticalScrollbar = scrollbar;
            scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHide;
        }

        private InputField CreateSearchField(RectTransform parent)
        {
            RectTransform root = CreatePanel("Search", parent, new Color32(17, 20, 15, 255), 5);
            AddOutline(root.gameObject, Line, 1f);
            InputField input = root.gameObject.AddComponent<InputField>();
            input.lineType = InputField.LineType.SingleLine;
            input.characterLimit = 32;

            Text valueText = CreateText("Text", root, "", 11, Ink);
            valueText.supportRichText = false;
            Stretch(valueText.rectTransform, 12f, 10f, 4f, 4f);
            valueText.alignment = TextAnchor.MiddleLeft;
            valueText.horizontalOverflow = HorizontalWrapMode.Overflow;

            Text placeholder = CreateText("Placeholder", root, "搜索武器、弹药或适配型号", 11, new Color32(105, 113, 99, 210));
            placeholder.supportRichText = false;
            Stretch(placeholder.rectTransform, 12f, 10f, 4f, 4f);
            placeholder.alignment = TextAnchor.MiddleLeft;
            placeholder.horizontalOverflow = HorizontalWrapMode.Overflow;

            input.textComponent = valueText;
            input.placeholder = placeholder;
            input.targetGraphic = root.GetComponent<Image>();
            input.onValueChanged.AddListener(value =>
            {
                searchTerm = string.IsNullOrWhiteSpace(value) ? "" : value.Trim().ToLowerInvariant();
                RefreshCatalog();
            });
            return input;
        }

        private void CreateFilterButton(RectTransform parent, string filterId, string label, float width)
        {
            RectTransform root = CreatePanel($"Filter-{filterId}", parent, Color.clear, 5);
            SetLayoutSize(root, width, 38f, 0f);
            Button button = root.gameObject.AddComponent<Button>();
            Image image = root.GetComponent<Image>();
            button.targetGraphic = image;
            SetButtonColors(button, Color.clear, Surface2, Surface3);

            Text text = CreateText("Label", root, label, 11, Muted);
            Stretch(text.rectTransform, 4f, 4f, 4f, 4f);
            text.alignment = TextAnchor.MiddleCenter;

            button.onClick.AddListener(() =>
            {
                activeFilter = filterId;
                UpdateFilterButtonStyles();
                RefreshCatalog();
            });

            filterButtons[filterId] = button;
            filterButtonLabels[filterId] = text;
        }

        private void BuildCart(RectTransform parent)
        {
            RectTransform header = CreateRect("CartHeader", parent);
            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.offsetMin = new Vector2(0f, -58f);
            header.offsetMax = Vector2.zero;
            Image headerBackground = header.gameObject.AddComponent<Image>();
            headerBackground.color = new Color32(27, 31, 25, 255);

            Text title = CreateText("Title", header, "采购清单", 13, Ink, FontStyle.Bold);
            SetRect(title.rectTransform, 14f, 0f, 230f, 28f, 15f, 0f, true);
            title.alignment = TextAnchor.MiddleLeft;

            RectTransform countChip = CreatePanel("CountChip", header, Acid, 12);
            countChip.anchorMin = new Vector2(1f, 1f);
            countChip.anchorMax = new Vector2(1f, 1f);
            countChip.pivot = new Vector2(1f, 1f);
            countChip.anchoredPosition = new Vector2(-104f, -15f);
            countChip.sizeDelta = new Vector2(28f, 26f);
            cartCountText = CreateText("Count", countChip, "0", 10, new Color32(22, 26, 18, 255), FontStyle.Bold);
            Stretch(cartCountText.rectTransform, 2f, 2f, 2f, 2f);
            cartCountText.alignment = TextAnchor.MiddleCenter;

            RectTransform clear = CreatePanel("Clear", header, Color.clear, 5);
            clear.anchorMin = new Vector2(1f, 1f);
            clear.anchorMax = new Vector2(1f, 1f);
            clear.pivot = new Vector2(1f, 1f);
            clear.anchoredPosition = new Vector2(-12f, -13f);
            clear.sizeDelta = new Vector2(80f, 32f);
            AddOutline(clear.gameObject, Line, 1f);
            Button clearButton = clear.gameObject.AddComponent<Button>();
            clearButton.targetGraphic = clear.GetComponent<Image>();
            SetButtonColors(clearButton, Color.clear, Surface2, Surface3);
            Text clearText = CreateText("Label", clear, "清空", 10, Muted);
            Stretch(clearText.rectTransform, 2f, 2f, 2f, 2f);
            clearText.alignment = TextAnchor.MiddleCenter;
            clearButton.onClick.AddListener(ClearCart);

            RectTransform body = CreateRect("CartBody", parent);
            body.anchorMin = Vector2.zero;
            body.anchorMax = Vector2.one;
            body.offsetMin = new Vector2(0f, 156f);
            body.offsetMax = new Vector2(0f, -58f);

            ScrollRect cartScroll = body.gameObject.AddComponent<ScrollRect>();
            cartScroll.horizontal = false;
            cartScroll.vertical = true;
            cartScroll.movementType = ScrollRect.MovementType.Clamped;
            cartScroll.scrollSensitivity = 28f;

            RectTransform cartViewport = CreateRect("Viewport", body);
            Stretch(cartViewport);
            cartViewport.gameObject.AddComponent<RectMask2D>();
            Image cartViewportRaycast = cartViewport.gameObject.AddComponent<Image>();
            cartViewportRaycast.color = Color.clear;

            cartContent = CreateRect("Content", cartViewport);
            cartContent.anchorMin = new Vector2(0f, 1f);
            cartContent.anchorMax = new Vector2(1f, 1f);
            cartContent.pivot = new Vector2(0.5f, 1f);
            cartContent.anchoredPosition = Vector2.zero;
            cartContent.sizeDelta = Vector2.zero;
            VerticalLayoutGroup cartLayout = cartContent.gameObject.AddComponent<VerticalLayoutGroup>();
            cartLayout.childControlWidth = true;
            cartLayout.childControlHeight = true;
            cartLayout.childForceExpandWidth = true;
            cartLayout.childForceExpandHeight = false;
            cartLayout.spacing = 0f;
            ContentSizeFitter cartFitter = cartContent.gameObject.AddComponent<ContentSizeFitter>();
            cartFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            cartFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            cartScroll.viewport = cartViewport;
            cartScroll.content = cartContent;

            RectTransform checkout = CreateRect("Checkout", parent);
            checkout.anchorMin = new Vector2(0f, 0f);
            checkout.anchorMax = new Vector2(1f, 0f);
            checkout.pivot = new Vector2(0.5f, 0f);
            checkout.offsetMin = Vector2.zero;
            checkout.offsetMax = new Vector2(0f, 156f);
            Image checkoutBackground = checkout.gameObject.AddComponent<Image>();
            checkoutBackground.color = new Color32(27, 31, 25, 255);

            CreateSummaryRow(checkout, "装备小计", 10f, out weaponSubtotalText);
            CreateSummaryRow(checkout, "弹药小计", 35f, out ammoSubtotalText);
            CreateSummaryRow(checkout, "合计", 67f, out orderTotalText, true);

            RectTransform checkoutButtonRect = CreatePanel("CheckoutButton", checkout, new Color32(39, 44, 35, 255), 5);
            checkoutButtonRect.anchorMin = new Vector2(0f, 0f);
            checkoutButtonRect.anchorMax = new Vector2(1f, 0f);
            checkoutButtonRect.pivot = new Vector2(0.5f, 0f);
            checkoutButtonRect.offsetMin = new Vector2(13f, 13f);
            checkoutButtonRect.offsetMax = new Vector2(-13f, 55f);
            checkoutButtonImage = checkoutButtonRect.GetComponent<Image>();
            checkoutButton = checkoutButtonRect.gameObject.AddComponent<Button>();
            checkoutButton.targetGraphic = checkoutButtonImage;
            SetButtonColors(checkoutButton, new Color32(39, 44, 35, 255), new Color32(53, 60, 47, 255), Surface3);
            Text checkoutText = CreateText(
                "Label",
                checkoutButtonRect,
                "确认补给申请",
                12,
                new Color32(102, 110, 96, 255),
                FontStyle.Bold
            );
            Stretch(checkoutText.rectTransform, 4f, 4f, 4f, 4f);
            checkoutText.alignment = TextAnchor.MiddleCenter;
            checkoutButton.onClick.AddListener(Checkout);
            SetCheckoutEnabled(false);
        }

        private void CreateSummaryRow(
            RectTransform parent,
            string label,
            float top,
            out Text valueText,
            bool total = false
        )
        {
            Text labelText = CreateText(label, parent, label, total ? 11 : 9, total ? Ink : Muted);
            SetRect(labelText.rectTransform, 13f, 0f, 200f, total ? 24f : 18f, top, 0f, true);

            valueText = CreateText(label, parent, "0 CR", total ? 15 : 10, total ? Amber : Muted, total ? FontStyle.Bold : FontStyle.Normal);
            valueText.alignment = TextAnchor.MiddleRight;
            SetRect(valueText.rectTransform, 150f, 13f, 0f, total ? 24f : 18f, top, 0f, true);
        }

        private void RefreshCatalog()
        {
            if (productContent == null)
            {
                return;
            }

            ClearChildren(productContent);
            int visibleCount = 0;
            showingNoResults = false;

            foreach (ShopItemData item in catalog)
            {
                if (!ItemMatches(item))
                {
                    continue;
                }

                CreateProductCard(item);
                visibleCount++;
            }

            if (visibleCount == 0)
            {
                showingNoResults = true;
                CreateNoResults();
            }

            resultText.text = $"{visibleCount} 项可用 · 库存随补给周期刷新";
            UpdateFilterButtonStyles();
            UpdateGridColumns(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(productContent);
        }

        private bool ItemMatches(ShopItemData item)
        {
            bool filterMatches = activeFilter == "all"
                || item.categoryId == activeFilter
                || (activeFilter == "ammo" && item.type == ShopItemType.Ammo);

            if (!filterMatches)
            {
                return false;
            }

            if (string.IsNullOrEmpty(searchTerm))
            {
                return true;
            }

            string haystack = string.Join(
                " ",
                item.displayName,
                item.categoryLabel,
                item.rarityLabel,
                item.description,
                item.compatibleWeapons
            ).ToLowerInvariant();
            return haystack.Contains(searchTerm);
        }

        private void CreateProductCard(ShopItemData item)
        {
            RectTransform card = CreatePanel($"Item-{item.id}", productContent, Surface, 7);
            AddOutline(card.gameObject, LineSoft, 1f);

            Text type = CreateText("Type", card, $"{item.categoryLabel} · {item.rarityLabel}", 9, item.rarityColor);
            SetRect(type.rectTransform, 12f, 78f, 0f, 15f, 10f, 0f, true);
            type.horizontalOverflow = HorizontalWrapMode.Overflow;

            Text name = CreateText("Name", card, item.displayName, 14, Ink, FontStyle.Bold);
            name.horizontalOverflow = HorizontalWrapMode.Overflow;
            SetRect(name.rectTransform, 12f, 64f, 0f, 23f, 25f, 0f, true);

            Text stock = CreateText(
                "Stock",
                card,
                item.type == ShopItemType.Ammo ? (item.stock >= 999 ? "库存充足" : $"库存 {item.stock}") : $"库存 {item.stock}",
                9,
                item.stock <= 5 ? Amber : Muted,
                item.stock <= 5 ? FontStyle.Bold : FontStyle.Normal
            );
            stock.alignment = TextAnchor.MiddleRight;
            SetRect(stock.rectTransform, 180f, 12f, 0f, 20f, 13f, 0f, true);

            RectTransform visual = CreatePanel("Visual", card, new Color32(18, 21, 16, 255), 6);
            visual.anchorMin = new Vector2(0f, 1f);
            visual.anchorMax = new Vector2(1f, 1f);
            visual.pivot = new Vector2(0.5f, 1f);
            visual.offsetMin = new Vector2(12f, -166f);
            visual.offsetMax = new Vector2(-12f, -54f);
            AddOutline(visual.gameObject, new Color32(46, 52, 42, 255), 1f);

            Image icon = CreateImage("Icon", visual, Color.white);
            Stretch(icon.rectTransform, 10f, 10f, 10f, 10f);
            icon.sprite = WeaponIconFactory.Get(item.iconKey, item.rarityColor);
            icon.preserveAspect = true;

            Text description = CreateText("Description", card, item.description, 10, Muted);
            description.horizontalOverflow = HorizontalWrapMode.Wrap;
            description.verticalOverflow = VerticalWrapMode.Truncate;
            SetRect(description.rectTransform, 12f, 12f, 0f, 39f, 174f, 0f, true);

            if (item.type == ShopItemType.Ammo)
            {
                CreateAmmoSpec(card, "每包", $"{item.packSize} 发", 12f);
                CreateAmmoSpec(card, "适配", item.compatibleWeapons, 126f);
            }
            else
            {
                CreateStatRow(card, "伤害", item.damage, 214f);
                CreateStatRow(card, "射速", item.fireRate, 231f);
                CreateStatRow(card, "稳定", item.stability, 248f);
                CreateStatRow(card, "机动", item.mobility, 265f);
            }

            Text price = CreateText(
                "Price",
                card,
                item.type == ShopItemType.Ammo
                    ? $"每包 {item.packSize} 发\n{AmberHex(item.price)} CR"
                    : $"单件采购价\n{AmberHex(item.price)} CR",
                12,
                Amber,
                FontStyle.Bold
            );
            SetRect(price.rectTransform, 12f, 12f, 112f, 42f, 298f, 0f, true);
            price.lineSpacing = 1f;

            bool owned = item.type == ShopItemType.Weapon && ownedWeapons.Contains(item.id);
            RectTransform add = CreatePanel("Add", card, owned ? Surface3 : Acid, 5);
            add.anchorMin = new Vector2(1f, 0f);
            add.anchorMax = new Vector2(1f, 0f);
            add.pivot = new Vector2(1f, 0f);
            add.anchoredPosition = new Vector2(-12f, 12f);
            add.sizeDelta = new Vector2(98f, 38f);

            Button addButton = add.gameObject.AddComponent<Button>();
            addButton.targetGraphic = add.GetComponent<Image>();
            SetButtonColors(
                addButton,
                owned ? Surface3 : Acid,
                owned ? Surface3 : new Color32(214, 255, 95, 255),
                Surface3
            );
            addButton.interactable = !owned;
            Text addText = CreateText(
                "Label",
                add,
                owned ? "已拥有" : "加入采购",
                10,
                owned ? Dim : new Color32(17, 20, 15, 255),
                FontStyle.Bold
            );
            Stretch(addText.rectTransform, 3f, 3f, 3f, 3f);
            addText.alignment = TextAnchor.MiddleCenter;
            addButton.onClick.AddListener(() => AddToCart(item.id));
        }

        private void CreateAmmoSpec(RectTransform parent, string label, string value, float left)
        {
            RectTransform spec = CreatePanel($"Spec-{label}", parent, new Color32(20, 23, 18, 255), 4);
            spec.anchorMin = new Vector2(0f, 1f);
            spec.anchorMax = new Vector2(0f, 1f);
            spec.pivot = new Vector2(0f, 1f);
            spec.anchoredPosition = new Vector2(left, -214f);
            spec.sizeDelta = new Vector2(108f, 48f);
            AddOutline(spec.gameObject, LineSoft, 1f);

            Text labelText = CreateText("Label", spec, label, 8, Dim);
            SetRect(labelText.rectTransform, 7f, 5f, 0f, 14f, 4f, 0f, true);
            Text valueText = CreateText("Value", spec, value, 10, Ink, FontStyle.Bold);
            valueText.horizontalOverflow = HorizontalWrapMode.Overflow;
            SetRect(valueText.rectTransform, 7f, 5f, 0f, 20f, 20f, 0f, true);
        }

        private void CreateStatRow(RectTransform parent, string label, int value, float top)
        {
            Text labelText = CreateText(label, parent, label, 9, Dim);
            SetRect(labelText.rectTransform, 12f, 0f, 39f, 13f, top, 0f, true);

            RectTransform track = CreatePanel("Track", parent, new Color32(48, 54, 43, 255), 2);
            track.anchorMin = new Vector2(0f, 1f);
            track.anchorMax = new Vector2(1f, 1f);
            track.pivot = new Vector2(0f, 1f);
            track.offsetMin = new Vector2(54f, -top - 9f);
            track.offsetMax = new Vector2(-39f, -top - 4f);
            RectTransform fill = CreatePanel("Fill", track, Acid, 2);
            fill.anchorMin = Vector2.zero;
            fill.anchorMax = new Vector2(Mathf.Clamp01(value / 100f), 1f);
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;

            Text valueText = CreateText("Value", parent, value.ToString(), 9, Muted);
            valueText.alignment = TextAnchor.MiddleRight;
            SetRect(valueText.rectTransform, 215f, 12f, 0f, 13f, top, 0f, true);
        }

        private void CreateNoResults()
        {
            RectTransform panel = CreatePanel("NoResults", productContent, new Color32(24, 27, 22, 132), 7);
            panel.sizeDelta = new Vector2(0f, 300f);
            LayoutElement layout = panel.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 300f;
            AddOutline(panel.gameObject, Line, 1f);
            Text text = CreateText("Text", panel, "未找到匹配军需\n尝试缩短关键词或切换分类", 13, Dim);
            Stretch(text.rectTransform, 20f, 20f, 20f, 20f);
            text.alignment = TextAnchor.MiddleCenter;
        }

        private void RefreshCart()
        {
            ClearChildren(cartContent);
            int count = GetCartCount();
            CartTotals totals = GetCartTotals();

            cartCountText.text = count.ToString();
            creditsText.text = startingCredits.ToString("N0");
            weaponSubtotalText.text = $"{totals.Weapons:N0} CR";
            ammoSubtotalText.text = $"{totals.Ammo:N0} CR";
            orderTotalText.text = $"{totals.Total:N0} CR";
            SetCheckoutEnabled(count > 0);

            if (count == 0)
            {
                RectTransform empty = CreateRect("Empty", cartContent);
                LayoutElement layout = empty.gameObject.AddComponent<LayoutElement>();
                layout.preferredHeight = 230f;
                Text text = CreateText("Text", empty, "采购清单为空\n从左侧选择武器或弹药", 11, Dim);
                Stretch(text.rectTransform, 20f, 20f, 20f, 20f);
                text.alignment = TextAnchor.MiddleCenter;
                return;
            }

            foreach (KeyValuePair<string, int> pair in cart)
            {
                ShopItemData item = FindItem(pair.Key);
                if (item != null)
                {
                    CreateCartItem(item, pair.Value);
                }
            }
        }

        private void CreateCartItem(ShopItemData item, int quantity)
        {
            RectTransform row = CreatePanel($"Cart-{item.id}", cartContent, new Color32(24, 27, 22, 255), 0);
            LayoutElement layout = row.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 84f;
            layout.minHeight = 84f;
            Image divider = CreateImage("Divider", row, LineSoft);
            divider.sprite = GetRoundedSprite(0);
            divider.rectTransform.anchorMin = new Vector2(0f, 0f);
            divider.rectTransform.anchorMax = new Vector2(1f, 0f);
            divider.rectTransform.pivot = new Vector2(0.5f, 0f);
            divider.rectTransform.offsetMin = Vector2.zero;
            divider.rectTransform.offsetMax = new Vector2(0f, 1f);

            RectTransform thumb = CreatePanel("Thumb", row, new Color32(18, 21, 16, 255), 5);
            thumb.anchorMin = new Vector2(0f, 1f);
            thumb.anchorMax = new Vector2(0f, 1f);
            thumb.pivot = new Vector2(0f, 1f);
            thumb.anchoredPosition = new Vector2(12f, -11f);
            thumb.sizeDelta = new Vector2(50f, 50f);
            AddOutline(thumb.gameObject, Line, 1f);
            Image thumbIcon = CreateImage("Icon", thumb, Color.white);
            Stretch(thumbIcon.rectTransform, 6f, 6f, 8f, 8f);
            thumbIcon.sprite = WeaponIconFactory.Get(item.iconKey, item.rarityColor);
            thumbIcon.preserveAspect = true;

            Text name = CreateText("Name", row, item.displayName, 11, Ink, FontStyle.Bold);
            name.horizontalOverflow = HorizontalWrapMode.Overflow;
            SetRect(name.rectTransform, 72f, 116f, 0f, 19f, 10f, 0f, true);

            Text type = CreateText(
                "Type",
                row,
                item.type == ShopItemType.Ammo
                    ? $"{item.packSize} 发/包 · {item.price:N0} CR"
                    : $"{item.categoryLabel} · {item.price:N0} CR",
                9,
                Dim
            );
            type.horizontalOverflow = HorizontalWrapMode.Overflow;
            SetRect(type.rectTransform, 72f, 116f, 0f, 16f, 29f, 0f, true);

            RectTransform quantityControl = CreatePanel("Quantity", row, new Color32(17, 20, 15, 255), 4);
            quantityControl.anchorMin = new Vector2(0f, 0f);
            quantityControl.anchorMax = new Vector2(0f, 0f);
            quantityControl.pivot = new Vector2(0f, 0f);
            quantityControl.anchoredPosition = new Vector2(72f, 10f);
            quantityControl.sizeDelta = new Vector2(82f, 26f);
            AddOutline(quantityControl.gameObject, Line, 1f);

            Button minus = CreateSquareButton(quantityControl, "Minus", "−", quantity > 1, 26f);
            minus.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
            Text count = CreateText("Count", quantityControl, quantity.ToString(), 10, Ink, FontStyle.Bold);
            count.rectTransform.anchorMin = Vector2.zero;
            count.rectTransform.anchorMax = Vector2.zero;
            count.rectTransform.pivot = Vector2.zero;
            count.rectTransform.anchoredPosition = new Vector2(27f, 0f);
            count.rectTransform.sizeDelta = new Vector2(28f, 26f);
            count.alignment = TextAnchor.MiddleCenter;
            bool canIncrement = item.type == ShopItemType.Ammo && quantity < 10;
            Button plus = CreateSquareButton(quantityControl, "Plus", "+", canIncrement, 26f);
            RectTransform plusRect = plus.GetComponent<RectTransform>();
            plusRect.anchorMin = new Vector2(1f, 0f);
            plusRect.anchorMax = new Vector2(1f, 0f);
            plusRect.pivot = new Vector2(1f, 0f);
            plusRect.anchoredPosition = Vector2.zero;

            minus.onClick.AddListener(() => ChangeQuantity(item.id, -1));
            plus.onClick.AddListener(() => ChangeQuantity(item.id, 1));

            Text lineTotal = CreateText("Total", row, $"{item.price * quantity:N0} CR", 12, Amber, FontStyle.Bold);
            lineTotal.alignment = TextAnchor.UpperRight;
            SetRect(lineTotal.rectTransform, 230f, 12f, 0f, 22f, 12f, 0f, true);
        }

        private Button CreateSquareButton(RectTransform parent, string name, string label, bool interactable, float size)
        {
            RectTransform root = CreatePanel(name, parent, new Color32(17, 20, 15, 255), 0);
            root.anchorMin = new Vector2(0f, 0f);
            root.anchorMax = new Vector2(0f, 0f);
            root.pivot = new Vector2(0f, 0f);
            root.sizeDelta = new Vector2(size, 26f);
            Button button = root.gameObject.AddComponent<Button>();
            button.targetGraphic = root.GetComponent<Image>();
            button.interactable = interactable;
            SetButtonColors(button, new Color32(17, 20, 15, 255), Surface3, Surface3);
            Text text = CreateText("Label", root, label, 13, interactable ? Muted : new Color32(62, 68, 58, 255), FontStyle.Bold);
            Stretch(text.rectTransform);
            text.alignment = TextAnchor.MiddleCenter;
            return button;
        }

        private void AddToCart(string itemId)
        {
            ShopItemData item = FindItem(itemId);
            if (item == null || (item.type == ShopItemType.Weapon && ownedWeapons.Contains(item.id)))
            {
                return;
            }

            int current = cart.TryGetValue(itemId, out int quantity) ? quantity : 0;
            if (item.type == ShopItemType.Weapon && current >= 1)
            {
                ShowToast("已达到限购数量", "武器每次补给仅可采购一件。", true);
                return;
            }

            if (item.type == ShopItemType.Ammo && current >= 10)
            {
                ShowToast("达到单次采购上限", "同规格弹药单次最多采购 10 包。", true);
                return;
            }

            cart[itemId] = current + 1;
            RefreshCart();
            ShowToast("已加入采购清单", $"{item.displayName} × 1", false);
        }

        private void ChangeQuantity(string itemId, int delta)
        {
            ShopItemData item = FindItem(itemId);
            if (item == null || !cart.TryGetValue(itemId, out int current))
            {
                return;
            }

            int next = current + delta;
            if (next <= 0)
            {
                cart.Remove(itemId);
            }
            else if (item.type == ShopItemType.Weapon && next > 1)
            {
                return;
            }
            else if (item.type == ShopItemType.Ammo && next > 10)
            {
                ShowToast("达到单次采购上限", "同规格弹药单次最多采购 10 包。", true);
                return;
            }
            else
            {
                cart[itemId] = next;
            }

            RefreshCart();
        }

        private void ClearCart()
        {
            cart.Clear();
            RefreshCart();
            ShowToast("采购清单已清空", "未提交的军需已释放。", false);
        }

        private void Checkout()
        {
            CartTotals totals = GetCartTotals();
            if (totals.Total <= 0)
            {
                return;
            }

            if (totals.Total > startingCredits)
            {
                ShowToast("信用点不足", $"当前缺少 {totals.Total - startingCredits:N0} CR。", true);
                return;
            }

            List<string> purchasedWeapons = new List<string>();
            int ammoPacks = 0;
            foreach (KeyValuePair<string, int> pair in cart)
            {
                ShopItemData item = FindItem(pair.Key);
                if (item == null)
                {
                    continue;
                }

                if (item.type == ShopItemType.Ammo)
                {
                    ammoPacks += pair.Value;
                }
                else
                {
                    ownedWeapons.Add(item.id);
                    purchasedWeapons.Add(item.displayName);
                }
            }

            startingCredits -= totals.Total;
            cart.Clear();
            RefreshCatalog();
            RefreshCart();

            string detail;
            if (purchasedWeapons.Count > 0)
            {
                detail = string.Join("、", purchasedWeapons);
                if (ammoPacks > 0)
                {
                    detail += $"，另含弹药 {ammoPacks} 包";
                }
            }
            else
            {
                detail = $"弹药补给 {ammoPacks} 包";
            }

            ShowToast("补给申请已批准", detail, false);
        }

        private void UpdateFilterButtonStyles()
        {
            foreach (string key in filterButtons.Keys)
            {
                bool active = key == activeFilter;
                Image image = filterButtons[key].GetComponent<Image>();
                image.color = active ? new Color32(43, 49, 35, 255) : Color.clear;
                filterButtonLabels[key].color = active ? Acid : Muted;
            }
        }

        private void SetCheckoutEnabled(bool enabled)
        {
            checkoutButton.interactable = enabled;
            checkoutButtonImage.color = enabled ? Acid : new Color32(39, 44, 35, 255);
            Text label = checkoutButton.GetComponentInChildren<Text>();
            if (label != null)
            {
                label.color = enabled ? new Color32(17, 20, 15, 255) : new Color32(102, 110, 96, 255);
            }
        }

        private int GetCartCount()
        {
            int count = 0;
            foreach (int quantity in cart.Values)
            {
                count += quantity;
            }
            return count;
        }

        private CartTotals GetCartTotals()
        {
            CartTotals totals = new CartTotals();
            foreach (KeyValuePair<string, int> pair in cart)
            {
                ShopItemData item = FindItem(pair.Key);
                if (item == null)
                {
                    continue;
                }

                int lineTotal = item.price * pair.Value;
                if (item.type == ShopItemType.Ammo)
                {
                    totals.Ammo += lineTotal;
                }
                else
                {
                    totals.Weapons += lineTotal;
                }
            }
            return totals;
        }

        private ShopItemData FindItem(string itemId)
        {
            return catalog.Find(item => item.id == itemId);
        }

        private void ApplyResponsiveLayout(bool force)
        {
            float width = appRoot.rect.width;
            if (!force && Mathf.Abs(width - lastResponsiveWidth) <= 1f)
            {
                return;
            }

            lastResponsiveWidth = width;
            bool compact = width < 1280f;
            float cartWidth = compact ? 318f : 356f;
            float gap = compact ? 10f : 16f;
            cartPanel.offsetMin = new Vector2(-cartWidth, 0f);
            catalogPanel.offsetMax = new Vector2(-(cartWidth + gap), 0f);
            UpdateGridColumns(true);
        }

        private void UpdateGridColumns(bool force)
        {
            if (productGrid == null || catalogPanel == null || catalogPanel.rect.width <= 1f)
            {
                return;
            }

            float availableWidth = Mathf.Max(120f, catalogPanel.rect.width - 13f);
            const float spacing = 10f;
            int columns = showingNoResults
                ? 1
                : Mathf.Clamp(
                    Mathf.FloorToInt((availableWidth + spacing) / (248f + spacing)),
                    1,
                    4
                );

            if (!force && columns == lastColumnCount)
            {
                return;
            }

            lastColumnCount = columns;
            float cellWidth = Mathf.Floor((availableWidth - spacing * (columns - 1)) / columns);
            productGrid.constraintCount = columns;
            productGrid.cellSize = new Vector2(Mathf.Max(180f, cellWidth), showingNoResults ? 300f : 350f);
        }

        private void ShowToast(string title, string message, bool error)
        {
            StartCoroutine(ToastRoutine(title, message, error));
        }

        private IEnumerator ToastRoutine(string title, string message, bool error)
        {
            RectTransform toast = CreatePanel("Toast", toastRoot, new Color32(32, 36, 29, 255), 6);
            LayoutElement layout = toast.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = 340f;
            layout.preferredHeight = 58f;
            layout.minHeight = 58f;
            AddOutline(toast.gameObject, error ? new Color32(106, 61, 53, 255) : new Color32(89, 97, 73, 255), 1f);

            Image accent = CreateImage("Accent", toast, error ? Danger : Acid);
            accent.sprite = GetRoundedSprite(1);
            SetRect(accent.rectTransform, 0f, 0f, 3f, 58f, 0f, 0f, false);

            Text titleText = CreateText("Title", toast, title, 11, Ink, FontStyle.Bold);
            SetRect(titleText.rectTransform, 15f, 10f, 0f, 18f, 9f, 0f, true);
            Text messageText = CreateText("Message", toast, message, 9, error ? new Color32(214, 151, 139, 255) : Muted);
            messageText.horizontalOverflow = HorizontalWrapMode.Wrap;
            SetRect(messageText.rectTransform, 15f, 10f, 0f, 28f, 28f, 0f, true);

            CanvasGroup group = toast.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            toast.localScale = Vector3.one * 0.98f;
            float elapsed = 0f;
            while (elapsed < 0.16f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / 0.16f);
                group.alpha = t;
                toast.localScale = Vector3.Lerp(Vector3.one * 0.98f, Vector3.one, t);
                yield return null;
            }

            group.alpha = 1f;
            toast.localScale = Vector3.one;
            yield return new WaitForSecondsRealtime(2.6f);

            elapsed = 0f;
            while (elapsed < 0.18f)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = 1f - Mathf.Clamp01(elapsed / 0.18f);
                yield return null;
            }

            Destroy(toast.gameObject);
        }

        private Font ResolveFont()
        {
            try
            {
                Font dynamicFont = Font.CreateDynamicFontFromOSFont(
                    new[] { "Microsoft YaHei UI", "Microsoft YaHei", "Noto Sans CJK SC", "PingFang SC", "Arial Unicode MS", "Arial" },
                    18
                );
                if (dynamicFont != null)
                {
                    return dynamicFont;
                }
            }
            catch (Exception)
            {
                // Fall through to Unity's bundled font for platforms without the requested OS fonts.
            }

            try
            {
                return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            }
            catch (Exception)
            {
                return Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
        }

        private void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            Type inputSystemModule = Type.GetType(
                "UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem"
            );
            if (inputSystemModule != null)
            {
                eventSystemObject.AddComponent(inputSystemModule);
                return;
            }
#endif
            eventSystemObject.AddComponent<StandaloneInputModule>();
        }

        private RectTransform CreateRect(string name, Transform parent)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            RectTransform rect = gameObject.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.localScale = Vector3.one;
            return rect;
        }

        private RectTransform CreatePanel(string name, Transform parent, Color color, int radius)
        {
            RectTransform rect = CreateRect(name, parent);
            Image image = rect.gameObject.AddComponent<Image>();
            image.sprite = GetRoundedSprite(radius);
            image.type = Image.Type.Sliced;
            image.color = color;
            return rect;
        }

        private Image CreateImage(string name, Transform parent, Color color)
        {
            RectTransform rect = CreateRect(name, parent);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private Text CreateText(
            string name,
            Transform parent,
            string value,
            int size,
            Color color,
            FontStyle style = FontStyle.Normal
        )
        {
            RectTransform rect = CreateRect(name, parent);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.text = value;
            text.fontSize = size;
            text.fontStyle = style;
            text.color = color;
            text.alignment = TextAnchor.MiddleLeft;
            text.supportRichText = false;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private Sprite GetRoundedSprite(int radius)
        {
            int normalizedRadius = Mathf.Clamp(radius, 0, 16);
            string key = normalizedRadius.ToString();
            if (roundedSprites.TryGetValue(key, out Sprite sprite))
            {
                return sprite;
            }

            const int size = 32;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = $"Runtime-Rounded-{normalizedRadius}"
            };
            Color[] pixels = new Color[size * size];
            float radiusValue = normalizedRadius;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float alpha = 1f;
                    if (normalizedRadius > 0)
                    {
                        float nearestX = Mathf.Clamp(x + 0.5f, radiusValue, size - radiusValue);
                        float nearestY = Mathf.Clamp(y + 0.5f, radiusValue, size - radiusValue);
                        float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(nearestX, nearestY));
                        alpha = Mathf.Clamp01(radiusValue - distance + 0.5f);
                    }
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false, true);
            int border = normalizedRadius > 0 ? normalizedRadius + 1 : 0;
            sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(border, border, border, border)
            );
            sprite.name = $"Runtime-Rounded-{normalizedRadius}";
            roundedSprites[key] = sprite;
            return sprite;
        }

        private static void AddOutline(GameObject gameObject, Color color, float distance)
        {
            Outline outline = gameObject.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(distance, -distance);
            outline.useGraphicAlpha = true;
        }

        private static void SetButtonColors(Button button, Color normal, Color highlighted, Color pressed)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = normal;
            colors.highlightedColor = highlighted;
            colors.pressedColor = pressed;
            colors.selectedColor = highlighted;
            colors.disabledColor = normal;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;
        }

        private static void ClearChildren(RectTransform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                GameObject child = parent.GetChild(i).gameObject;
                child.SetActive(false);
                child.transform.SetParent(null, false);
                UnityEngine.Object.Destroy(child);
            }
        }

        private static void Stretch(RectTransform rect, float left = 0f, float right = 0f, float top = 0f, float bottom = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void TopStretch(RectTransform rect, float height, float top, float left = 0f, float right = 0f)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.offsetMin = new Vector2(left, -top - height);
            rect.offsetMax = new Vector2(-right, -top);
        }

        private static void SetRect(
            RectTransform rect,
            float left,
            float right,
            float width,
            float height,
            float top,
            float bottom,
            bool stretchWidth
        )
        {
            if (stretchWidth)
            {
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.offsetMin = new Vector2(left, -top - height);
                rect.offsetMax = new Vector2(-right, -top);
            }
            else
            {
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.anchoredPosition = new Vector2(left, -top);
                rect.sizeDelta = new Vector2(width, height);
            }
        }

        private static void SetLayoutSize(RectTransform rect, float preferredWidth, float preferredHeight, float flexibleWidth)
        {
            LayoutElement layout = rect.gameObject.GetComponent<LayoutElement>();
            if (layout == null)
            {
                layout = rect.gameObject.AddComponent<LayoutElement>();
            }

            if (preferredWidth > 0f)
            {
                layout.preferredWidth = preferredWidth;
                layout.minWidth = preferredWidth;
            }
            if (preferredHeight > 0f)
            {
                layout.preferredHeight = preferredHeight;
                layout.minHeight = preferredHeight;
            }
            layout.flexibleWidth = flexibleWidth;
        }

        private static string AmberHex(int value)
        {
            return value.ToString("N0");
        }

        private struct CartTotals
        {
            public int Weapons;
            public int Ammo;
            public int Total => Weapons + Ammo;
        }
    }
}
