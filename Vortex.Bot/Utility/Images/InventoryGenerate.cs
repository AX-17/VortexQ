using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Vortex.Bot.Extension;
using Vortex.Protocol.Models;

namespace Vortex.Bot.Utility.Images;

public class ItemSlot
{
    public int NetId { get; set; }
    public int Prefix { get; set; }
    public int Stack { get; set; }
    public string Name { get; set; } = string.Empty;

    public ItemSlot() { }

    public ItemSlot(Item item)
    {
        NetId = item.NetId;
        Prefix = item.Prefix;
        Stack = item.Stack;
        Name = item.NetId.ToString();
    }

    public bool IsEmpty => NetId == 0;
}

public class InventoryBuilder
{
    public List<ItemSlot> Inventory { get; set; } = [];
    public List<ItemSlot> Piggy { get; set; } = [];
    public List<ItemSlot> Safe { get; set; } = [];
    public List<ItemSlot> VoidVault { get; set; } = [];
    public List<ItemSlot> Forge { get; set; } = [];
    public List<ItemSlot> MiscEquip { get; set; } = [];
    public List<ItemSlot> MiscDye { get; set; } = [];
    public List<Suits> Loadouts { get; set; } = [];
    public ItemSlot? TrashItem { get; set; }
    public string PlayerName { get; set; } = "Player";
    public string ServerName { get; set; } = "Server";

    public static InventoryBuilder Create() => new();

    public InventoryBuilder SetPlayerName(string name)
    {
        PlayerName = name;
        return this;
    }

    public InventoryBuilder SetServerName(string name)
    {
        ServerName = name;
        return this;
    }

    public byte[] Build()
    {
        return new InventoryGenerate().Generate(this);
    }
}

public class InventoryGenerate
{
    public int SlotSize { get; set; } = 48;
    public int SlotSpacing { get; set; } = 4;
    public int CardPadding { get; set; } = 10;
    public int CardCornerRadius { get; set; } = 8;
    public int RegionGapX { get; set; } = 40;
    public int RegionGapY { get; set; } = 70;

    public Color BackgroundColor { get; set; } = Color.FromRgb(45, 55, 72);
    public Color CardColor { get; set; } = Color.FromRgb(65, 75, 95);
    public Color CardBorderColor { get; set; } = Color.FromRgb(90, 100, 125);
    public Color SlotEmptyColor { get; set; } = Color.FromRgb(55, 65, 85);
    public Color SlotBorderColor { get; set; } = Color.FromRgb(80, 90, 115);
    public Color TitleColor { get; set; } = Color.FromRgb(100, 200, 255);
    public Color RegionTitleColor { get; set; } = Color.FromRgb(200, 210, 230);
    public Color ItemCountColor { get; set; } = Color.FromRgb(255, 220, 100);

    public float TitleFontSize { get; set; } = 28;
    public float RegionTitleFontSize { get; set; } = 14;
    public float ItemCountFontSize { get; set; } = 10;

    private const int MarginX = 50;
    private const int MarginY = 80;
    private const int BottomMargin = 30;
    private const int TitleHeight = 60;

    private int _canvasHeight;

    public byte[] Generate(InventoryBuilder builder)
    {
        var (canvasWidth, canvasHeight) = CalculateCanvasSize(builder);
        _canvasHeight = canvasHeight;

        using var image = new Image<Rgba32>(canvasWidth, canvasHeight);
        DrawBackground(image, canvasWidth, canvasHeight);
        image.Mutate(ctx => DrawContent(ctx, builder, canvasWidth));
        return image.ToBytesAsync().Result;
    }

    private (int Width, int Height) CalculateCanvasSize(InventoryBuilder builder)
    {
        int mainWidth = 10 * SlotSize + 9 * SlotSpacing;
        int mainHeight = 5 * SlotSize + 4 * SlotSpacing;

        int coinWidth = 2 * SlotSize + 1 * SlotSpacing; 
        int coinHeight = 4 * SlotSize + 3 * SlotSpacing;

        int storageWidth = 10 * SlotSize + 9 * SlotSpacing;
        int storageHeight = 4 * SlotSize + 3 * SlotSpacing;

        int row1Height = Math.Max(mainHeight, coinHeight);
        int row2Height = storageHeight;
        int row3Height = storageHeight;

        int loadoutHeight = 10 * SlotSize + 9 * SlotSpacing;
        int totalHeight = MarginY + TitleHeight + row1Height + RegionGapY + row2Height + RegionGapY + row3Height + RegionGapY + loadoutHeight + BottomMargin;
        totalHeight += 4 * 25;
        int row1Width = mainWidth + RegionGapX + coinWidth;
        int row2Width = storageWidth + RegionGapX + storageWidth;
        int row3Width = storageWidth + RegionGapX + storageWidth;
        int row4Width = 3 * (3 * SlotSize + 2 * SlotSpacing) + 2 * 220;
        int maxWidth = Math.Max(Math.Max(row1Width, row2Width), Math.Max(row3Width, row4Width));
        int totalWidth = maxWidth + MarginX * 2;

        return (totalWidth, totalHeight);
    }

    private void DrawBackground(Image<Rgba32> image, int width, int height)
    {
        image.Mutate(ctx =>
        {
            ctx.Fill(BackgroundColor);
            var gradient = new RadialGradientBrush(
                new PointF(width / 2, height / 2),
                Math.Max(width, height) / 2f,
                GradientRepetitionMode.None,
                new ColorStop(0, Color.FromRgb(55, 68, 90)),
                new ColorStop(1, BackgroundColor));
            ctx.Fill(gradient);
        });
    }

    private void DrawTitleCard(IImageProcessingContext ctx, InventoryBuilder builder, int canvasWidth, Font titleFont)
    {
        string serverLabel = "服务器:";
        string playerLabel = "玩家名:";
        string serverValue = builder.ServerName;
        string playerValue = builder.PlayerName;

        var serverLabelSize = TextMeasurer.MeasureSize(serverLabel, new TextOptions(titleFont));
        var playerLabelSize = TextMeasurer.MeasureSize(playerLabel, new TextOptions(titleFont));
        var serverValueSize = TextMeasurer.MeasureSize(serverValue, new TextOptions(titleFont));
        var playerValueSize = TextMeasurer.MeasureSize(playerValue, new TextOptions(titleFont));

        int maxLabelWidth = (int)Math.Max(serverLabelSize.Width, playerLabelSize.Width);
        int maxValueWidth = (int)Math.Max(serverValueSize.Width, playerValueSize.Width);
        int cardWidth = maxLabelWidth + maxValueWidth + 50;
        int cardHeight = 70;
        int cardX = (canvasWidth - cardWidth) / 2;
        int cardY = MarginY - 20;

        ctx.DrawRoundedRectangle(cardX + 3, cardY + 3, cardWidth, cardHeight, CardCornerRadius, Color.FromRgba(0, 0, 0, 60));
        ctx.DrawRoundedRectangle(cardX, cardY, cardWidth, cardHeight, CardCornerRadius, CardColor);
        ctx.DrawRoundedRectanglePath(cardX, cardY, cardWidth, cardHeight, CardCornerRadius, 1, CardBorderColor);

        int labelX = cardX + 20;
        int serverValueX = cardX + cardWidth - 20 - (int)serverValueSize.Width;
        int playerValueX = cardX + cardWidth - 20 - (int)playerValueSize.Width;

        ctx.DrawText(serverLabel, titleFont, Color.FromRgb(0, 0, 0), new PointF(labelX + 2, cardY + 12));
        ctx.DrawText(serverLabel, titleFont, TitleColor, new PointF(labelX, cardY + 10));
        ctx.DrawText(serverValue, titleFont, Color.FromRgb(0, 0, 0), new PointF(serverValueX + 2, cardY + 12));
        ctx.DrawText(serverValue, titleFont, RegionTitleColor, new PointF(serverValueX, cardY + 10));

        ctx.DrawText(playerLabel, titleFont, Color.FromRgb(0, 0, 0), new PointF(labelX + 2, cardY + 42));
        ctx.DrawText(playerLabel, titleFont, TitleColor, new PointF(labelX, cardY + 40));
        ctx.DrawText(playerValue, titleFont, Color.FromRgb(0, 0, 0), new PointF(playerValueX + 2, cardY + 42));
        ctx.DrawText(playerValue, titleFont, RegionTitleColor, new PointF(playerValueX, cardY + 40));
    }

    private void DrawContent(IImageProcessingContext ctx, InventoryBuilder builder, int canvasWidth)
    {
        FontFamily fontFamily = CardRenderer.GetFontFamily();
        Font titleFont = fontFamily.CreateFont(TitleFontSize, FontStyle.Bold);
        Font regionTitleFont = fontFamily.CreateFont(RegionTitleFontSize, FontStyle.Bold);
        Font countFont = fontFamily.CreateFont(ItemCountFontSize);

        DrawTitleCard(ctx, builder, canvasWidth, titleFont);

        int currentY = MarginY + 90;

        currentY = DrawRow1(ctx, builder, regionTitleFont, countFont, canvasWidth, currentY);

        currentY += RegionGapY;
        currentY = DrawRow2(ctx, builder, regionTitleFont, countFont, canvasWidth, currentY);

        currentY += RegionGapY;
        currentY = DrawRow3(ctx, builder, regionTitleFont, countFont, canvasWidth, currentY);

        currentY += RegionGapY;
        DrawRow4(ctx, builder, regionTitleFont, countFont, canvasWidth, currentY);

        // 绘制签名
        DrawSignature(ctx, canvasWidth);
    }

    private void DrawSignature(IImageProcessingContext ctx, int canvasWidth)
    {
        string signature = "Generated by Vortex.Bot";
        FontFamily fontFamily = CardRenderer.GetFontFamily();
        Font signatureFont = fontFamily.CreateFont(12);
        
        var signatureSize = TextMeasurer.MeasureSize(signature, new TextOptions(signatureFont));
        float signatureX = (canvasWidth - signatureSize.Width) / 2;
        float signatureY = _canvasHeight - 20;
        
        ctx.DrawText(signature, signatureFont, Color.FromRgb(148, 163, 184), new PointF(signatureX, signatureY));
    }

    private int DrawRow1(IImageProcessingContext ctx, InventoryBuilder builder, Font titleFont, Font countFont, int canvasWidth, int y)
    {
        int mainWidth = 10 * SlotSize + 9 * SlotSpacing;
        int mainHeight = 5 * SlotSize + 4 * SlotSpacing;

        int coinWidth = 2 * SlotSize + 1 * SlotSpacing;
        int coinHeight = 4 * SlotSize + 3 * SlotSpacing;

        int rowHeight = Math.Max(mainHeight, coinHeight);

        int totalWidth = mainWidth + RegionGapX + coinWidth;
        int startX = (canvasWidth - totalWidth) / 2;

        DrawInventoryCard(ctx, startX, y, mainWidth, mainHeight, "主背包", titleFont);
        for (int i = 0; i < 50 && i < builder.Inventory.Count; i++)
        {
            int row = i / 10, col = i % 10;
            DrawSlot(ctx, builder.Inventory[i], startX + col * (SlotSize + SlotSpacing), y + row * (SlotSize + SlotSpacing), countFont);
        }

        int coinX = startX + mainWidth + RegionGapX;
        int coinAmmoHeight = coinHeight;

        if (builder.TrashItem != null && !builder.TrashItem.IsEmpty)
        {
            coinAmmoHeight = coinHeight + SlotSize + SlotSpacing + 30;
        }

        DrawInventoryCard(ctx, coinX, y, coinWidth, coinAmmoHeight, "钱币 / 弹药", titleFont);

        for (int i = 0; i < 8 && i + 50 < builder.Inventory.Count; i++)
        {
            int row = i / 2, col = i % 2;
            DrawSlot(ctx, builder.Inventory[i + 50], coinX + col * (SlotSize + SlotSpacing), y + row * (SlotSize + SlotSpacing), countFont);
        }


        if (builder.TrashItem != null && !builder.TrashItem.IsEmpty)
        {
            int trashY = y + coinHeight + 15;
            var trashTitleSize = TextMeasurer.MeasureSize("垃圾桶", new TextOptions(titleFont));
            float trashTitleX = coinX + (coinWidth - trashTitleSize.Width) / 2;
            ctx.DrawText("垃圾桶", titleFont, RegionTitleColor, new PointF(trashTitleX, trashY - 18));
            DrawSlot(ctx, builder.TrashItem, coinX + (coinWidth - SlotSize) / 2, trashY, countFont);
        }

        for (int i = 0; i < 50 && i < builder.Inventory.Count; i++)
        {
            int row = i / 10, col = i % 10;
            DrawSlotOverlay(ctx, builder.Inventory[i], startX + col * (SlotSize + SlotSpacing), y + row * (SlotSize + SlotSpacing), countFont);
        }
        for (int i = 0; i < 8 && i + 50 < builder.Inventory.Count; i++)
        {
            int row = i / 2, col = i % 2;
            DrawSlotOverlay(ctx, builder.Inventory[i + 50], coinX + col * (SlotSize + SlotSpacing), y + row * (SlotSize + SlotSpacing), countFont);
        }
        if (builder.TrashItem != null && !builder.TrashItem.IsEmpty)
        {
            int trashY = y + coinHeight + 15;
            DrawSlotOverlay(ctx, builder.TrashItem, coinX + (coinWidth - SlotSize) / 2, trashY, countFont);
        }

        return y + rowHeight;
    }

    private int DrawRow2(IImageProcessingContext ctx, InventoryBuilder builder, Font titleFont, Font countFont, int canvasWidth, int y)
    {
        int storageWidth = 10 * SlotSize + 9 * SlotSpacing;
        int storageHeight = 4 * SlotSize + 3 * SlotSpacing;

        int totalWidth = storageWidth + RegionGapX + storageWidth;
        int startX = (canvasWidth - totalWidth) / 2;

        if (builder.Piggy.Count > 0)
        {
            DrawInventoryCard(ctx, startX, y, storageWidth, storageHeight, "猪猪储钱罐", titleFont);
            for (int i = 0; i < builder.Piggy.Count && i < 40; i++)
            {
                int row = i / 10, col = i % 10;
                DrawSlot(ctx, builder.Piggy[i], startX + col * (SlotSize + SlotSpacing), y + row * (SlotSize + SlotSpacing), countFont);
            }
        }

        int safeX = startX + storageWidth + RegionGapX;
        if (builder.Safe.Count > 0)
        {
            DrawInventoryCard(ctx, safeX, y, storageWidth, storageHeight, "保险箱", titleFont);
            for (int i = 0; i < builder.Safe.Count && i < 40; i++)
            {
                int row = i / 10, col = i % 10;
                DrawSlot(ctx, builder.Safe[i], safeX + col * (SlotSize + SlotSpacing), y + row * (SlotSize + SlotSpacing), countFont);
            }
        }

        if (builder.Piggy.Count > 0)
        {
            for (int i = 0; i < builder.Piggy.Count && i < 40; i++)
            {
                int row = i / 10, col = i % 10;
                DrawSlotOverlay(ctx, builder.Piggy[i], startX + col * (SlotSize + SlotSpacing), y + row * (SlotSize + SlotSpacing), countFont);
            }
        }
        if (builder.Safe.Count > 0)
        {
            for (int i = 0; i < builder.Safe.Count && i < 40; i++)
            {
                int row = i / 10, col = i % 10;
                DrawSlotOverlay(ctx, builder.Safe[i], safeX + col * (SlotSize + SlotSpacing), y + row * (SlotSize + SlotSpacing), countFont);
            }
        }

        return y + storageHeight;
    }

    private int DrawRow3(IImageProcessingContext ctx, InventoryBuilder builder, Font titleFont, Font countFont, int canvasWidth, int y)
    {
        int storageWidth = 10 * SlotSize + 9 * SlotSpacing;
        int storageHeight = 4 * SlotSize + 3 * SlotSpacing;

        int totalWidth = storageWidth + RegionGapX + storageWidth;
        int startX = (canvasWidth - totalWidth) / 2;

        if (builder.VoidVault.Count > 0)
        {
            DrawInventoryCard(ctx, startX, y, storageWidth, storageHeight, "虚空宝库", titleFont);
            for (int i = 0; i < builder.VoidVault.Count && i < 40; i++)
            {
                int row = i / 10, col = i % 10;
                DrawSlot(ctx, builder.VoidVault[i], startX + col * (SlotSize + SlotSpacing), y + row * (SlotSize + SlotSpacing), countFont);
            }
        }

        int forgeX = startX + storageWidth + RegionGapX;
        if (builder.Forge.Count > 0)
        {
            DrawInventoryCard(ctx, forgeX, y, storageWidth, storageHeight, "护卫熔炉", titleFont);
            for (int i = 0; i < builder.Forge.Count && i < 40; i++)
            {
                int row = i / 10, col = i % 10;
                DrawSlot(ctx, builder.Forge[i], forgeX + col * (SlotSize + SlotSpacing), y + row * (SlotSize + SlotSpacing), countFont);
            }
        }

        if (builder.VoidVault.Count > 0)
        {
            for (int i = 0; i < builder.VoidVault.Count && i < 40; i++)
            {
                int row = i / 10, col = i % 10;
                DrawSlotOverlay(ctx, builder.VoidVault[i], startX + col * (SlotSize + SlotSpacing), y + row * (SlotSize + SlotSpacing), countFont);
            }
        }
        if (builder.Forge.Count > 0)
        {
            for (int i = 0; i < builder.Forge.Count && i < 40; i++)
            {
                int row = i / 10, col = i % 10;
                DrawSlotOverlay(ctx, builder.Forge[i], forgeX + col * (SlotSize + SlotSpacing), y + row * (SlotSize + SlotSpacing), countFont);
            }
        }

        return y + storageHeight;
    }

    private void DrawRow4(IImageProcessingContext ctx, InventoryBuilder builder, Font titleFont, Font countFont, int canvasWidth, int y)
    {
        if (builder.Loadouts.Count == 0) return;

        int loadoutWidth = 3 * SlotSize + 2 * SlotSpacing;
        int loadoutHeight = 10 * SlotSize + 9 * SlotSpacing;

        int totalWidth = 3 * loadoutWidth + 2 * 220;
        int startX = (canvasWidth - totalWidth) / 2;

        for (int i = 0; i < builder.Loadouts.Count && i < 3; i++)
        {
            var loadout = builder.Loadouts[i];
            int loadoutX = startX + i * (loadoutWidth + 220);

            DrawInventoryCard(ctx, loadoutX, y, loadoutWidth, loadoutHeight, $"套装 {i + 1}", titleFont);

            var equipItems = builder.MiscEquip.Concat(builder.MiscDye).ToList();
            for (int j = 0; j < equipItems.Count && j < 10; j++)
            {
                DrawSlot(ctx, equipItems[j], loadoutX, y + j * (SlotSize + SlotSpacing), countFont);
            }

            List<ItemSlot> armorItems2 = [];
            if (loadout.Armor?.Length > 10)
            {
                armorItems2 = loadout.Armor.Skip(10).Take(10).Select(a => new ItemSlot(a)).ToList();
                for (int j = 0; j < armorItems2.Count && j < 10; j++)
                {
                    DrawSlot(ctx, armorItems2[j], loadoutX + SlotSize + SlotSpacing, y + j * (SlotSize + SlotSpacing), countFont);
                }
            }

            List<ItemSlot> armorItems1 = [];
            if (loadout.Armor?.Length > 0)
            {
                armorItems1 = loadout.Armor.Take(10).Select(a => new ItemSlot(a)).ToList();
                for (int j = 0; j < armorItems1.Count && j < 10; j++)
                {
                    DrawSlot(ctx, armorItems1[j], loadoutX + 2 * (SlotSize + SlotSpacing), y + j * (SlotSize + SlotSpacing), countFont);
                }
            }

            for (int j = 0; j < equipItems.Count && j < 10; j++)
            {
                DrawSlotOverlay(ctx, equipItems[j], loadoutX, y + j * (SlotSize + SlotSpacing), countFont);
            }
            for (int j = 0; j < armorItems2.Count && j < 10; j++)
            {
                DrawSlotOverlay(ctx, armorItems2[j], loadoutX + SlotSize + SlotSpacing, y + j * (SlotSize + SlotSpacing), countFont);
            }
            for (int j = 0; j < armorItems1.Count && j < 10; j++)
            {
                DrawSlotOverlay(ctx, armorItems1[j], loadoutX + 2 * (SlotSize + SlotSpacing), y + j * (SlotSize + SlotSpacing), countFont);
            }
        }
    }

    private void DrawInventoryCard(IImageProcessingContext ctx, int x, int y, int width, int height, string title, Font titleFont)
    {
        int cardWidth = width + CardPadding * 2;
        int cardHeight = height + CardPadding * 2 + 20;
        int cardX = x - CardPadding;
        int cardY = y - CardPadding - 20;

        ctx.DrawRoundedRectangle(cardX + 2, cardY + 2, cardWidth, cardHeight, CardCornerRadius, Color.FromRgba(0, 0, 0, 50));
        ctx.DrawRoundedRectangle(cardX, cardY, cardWidth, cardHeight, CardCornerRadius, CardColor);
        ctx.DrawRoundedRectanglePath(cardX, cardY, cardWidth, cardHeight, CardCornerRadius, 1, CardBorderColor);

        var titleSize = TextMeasurer.MeasureSize(title, new TextOptions(titleFont));
        float titleX = x + (width - titleSize.Width) / 2;
        ctx.DrawText(title, titleFont, RegionTitleColor, new PointF(titleX, y - 16));
    }

    private void DrawSlot(IImageProcessingContext ctx, ItemSlot item, int x, int y, Font countFont)
    {
        Color slotColor = item.IsEmpty ? SlotEmptyColor : Color.FromRgb(48, 48, 68);
        ctx.DrawRoundedRectangle(x, y, SlotSize, SlotSize, 4, slotColor);
        ctx.DrawRoundedRectanglePath(x, y, SlotSize, SlotSize, 4, 1, SlotBorderColor);

        if (!item.IsEmpty)
        {
            DrawItemIcon(ctx, item.NetId, x, y);
        }
    }

    private void DrawSlotOverlay(IImageProcessingContext ctx, ItemSlot item, int x, int y, Font countFont)
    {
        if (!item.IsEmpty && item.Stack >= 1)
        {
            string countText = item.Stack.ToString();
            var countSize = TextMeasurer.MeasureSize(countText, new TextOptions(countFont));
            float countX = x + SlotSize - countSize.Width - 3;
            float countY = y + SlotSize - countSize.Height - 2;
            ctx.Fill(Color.FromRgba(0, 0, 0, 140), new RectangleF(countX - 2, countY - 1, countSize.Width + 4, countSize.Height + 2));
            ctx.DrawText(countText, countFont, ItemCountColor, new PointF(countX, countY));
        }
    }

    private void DrawItemIcon(IImageProcessingContext ctx, int netId, int x, int y)
    {
        if (netId <= 0) return;

        string itemPath = Path.Combine(AppContext.BaseDirectory, "Resources", "Item", $"{netId}.png");
        if (!File.Exists(itemPath)) return;

        try
        {
            using Image<Rgba32> itemImage = Image.Load<Rgba32>(itemPath);
            int padding = 2;
            int maxIconSize = SlotSize - (padding * 2);

            float scale = Math.Min((float)maxIconSize / itemImage.Width, (float)maxIconSize / itemImage.Height);
            int drawWidth = (int)(itemImage.Width * scale);
            int drawHeight = (int)(itemImage.Height * scale);
            int drawX = x + (SlotSize - drawWidth) / 2;
            int drawY = y + (SlotSize - drawHeight) / 2;

            if (drawWidth != itemImage.Width || drawHeight != itemImage.Height)
                itemImage.Mutate(i => i.Resize(drawWidth, drawHeight));

            ctx.DrawImage(itemImage, new Point(drawX, drawY), 1f);
        }
        catch { }
    }
}

public static class InventoryGenerateExtensions
{
    public static InventoryBuilder FromPlayerData(PlayerData data, string serverName)
    {
        var builder = InventoryBuilder.Create()
            .SetPlayerName(data.Username)
            .SetServerName(serverName);

        if (data.Inventory?.Length > 0)
            builder.Inventory = [.. data.Inventory.Select(i => new ItemSlot(i))];

        if (data.TrashItem?.Length > 0 && data.TrashItem[0].NetId != 0)
            builder.TrashItem = new ItemSlot(data.TrashItem[0]);

        if (data.Piggy?.Length > 0)
            builder.Piggy = [.. data.Piggy.Select(i => new ItemSlot(i))];

        if (data.Safe?.Length > 0)
            builder.Safe = [.. data.Safe.Select(i => new ItemSlot(i))];

        if (data.VoidVault?.Length > 0)
            builder.VoidVault = [.. data.VoidVault.Select(i => new ItemSlot(i))];

        if (data.Forge?.Length > 0)
            builder.Forge = [.. data.Forge.Select(i => new ItemSlot(i))];

        if (data.MiscEquip?.Length > 0)
            builder.MiscEquip = [.. data.MiscEquip.Select(i => new ItemSlot(i))];

        if (data.MiscDye?.Length > 0)
            builder.MiscDye = [.. data.MiscDye.Select(i => new ItemSlot(i))];

        if (data.Loadout?.Length > 0)
            builder.Loadouts = [.. data.Loadout];

        return builder;
    }
}
