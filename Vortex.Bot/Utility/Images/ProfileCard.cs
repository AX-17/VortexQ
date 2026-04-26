using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Vortex.Bot.Extension;

namespace Vortex.Bot.Utility.Images;

public class ProfileItem(string label, string value)
{
    public string Label { get; set; } = label;
    public string Value { get; set; } = value;
    public Color LabelColor { get; set; } = Color.DarkSlateGray;
    public Color ValueColor { get; set; } = Color.Black;
    public Color ValueBackgroundColor { get; set; } = Color.White;
    public bool UseEllipseBackground { get; set; } = true;
}

public class ProfileItemBuilder
{
    public List<ProfileItem> Items { get; } = [];
    internal ProfileCard Generator { get; } = new();

    public static ProfileItemBuilder Create() => new();

    public ProfileItemBuilder AddItem(string label, string value)
    {
        Items.Add(new ProfileItem(label, value));
        return this;
    }

    public ProfileItemBuilder AddItem(string label, string value, Color labelColor, Color valueColor, Color valueBackgroundColor)
    {
        ProfileItem item = new(label, value)
        {
            LabelColor = labelColor,
            ValueColor = valueColor,
            ValueBackgroundColor = valueBackgroundColor
        };
        Items.Add(item);
        return this;
    }

    public ProfileItemBuilder AddSpecialItem(string label, string value, bool useEllipseBackground)
    {
        ProfileItem item = new(label, value)
        {
            UseEllipseBackground = useEllipseBackground
        };
        Items.Add(item);
        return this;
    }

    public ProfileItemBuilder SetMemberUin(uint memberUin)
    {
        Generator.Config.MemberUin = memberUin;
        return this;
    }

    public ProfileItemBuilder SetCardOpacity(byte cardOpacity)
    {
        Generator.CardOpacity = cardOpacity;
        return this;
    }

    public ProfileItemBuilder SetCardWidth(int cardWidth)
    {
        Generator.CardWidth = cardWidth;
        return this;
    }

    public ProfileItemBuilder SetCardCornerRadius(float cardCornerRadius)
    {
        Generator.Config.CardCornerRadius = cardCornerRadius;
        return this;
    }

    public ProfileItemBuilder SetCardTopMargin(int cardTopMargin)
    {
        Generator.Config.CardTopMargin = cardTopMargin;
        return this;
    }

    public ProfileItemBuilder SetCardBottomMargin(int cardBottomMargin)
    {
        Generator.Config.CardBottomMargin = cardBottomMargin;
        return this;
    }

    public ProfileItemBuilder SetContentTopMargin(int contentTopMargin)
    {
        Generator.Config.ContentTopMargin = contentTopMargin;
        return this;
    }

    public ProfileItemBuilder SetContentBottomMargin(int contentBottomMargin)
    {
        Generator.Config.ContentBottomMargin = contentBottomMargin;
        return this;
    }

    public ProfileItemBuilder SetRowSpacing(int rowSpacing)
    {
        Generator.RowSpacing = rowSpacing;
        return this;
    }

    public ProfileItemBuilder SetTitle(string title)
    {
        Generator.Config.Title = title;
        return this;
    }

    public ProfileItemBuilder SetSignature(string signature)
    {
        Generator.Config.Signature = signature;
        return this;
    }

    public ProfileItemBuilder SetTitleColor(Color titleColor)
    {
        Generator.Config.TitleColor = titleColor;
        return this;
    }

    public ProfileItemBuilder SetSignatureColor(Color signatureColor)
    {
        Generator.Config.SignatureColor = signatureColor;
        return this;
    }

    public ProfileItemBuilder SetDefaultLabelColor(Color defaultLabelColor)
    {
        Generator.DefaultLabelColor = defaultLabelColor;
        return this;
    }

    public ProfileItemBuilder SetDefaultValueColor(Color defaultValueColor)
    {
        Generator.DefaultValueColor = defaultValueColor;
        return this;
    }

    public ProfileItemBuilder SetDefaultValueBackgroundColor(Color defaultValueBackgroundColor)
    {
        Generator.DefaultValueBackgroundColor = defaultValueBackgroundColor;
        return this;
    }

    public ProfileItemBuilder SetTitleFontSize(float titleFontSize)
    {
        Generator.Config.TitleFontSize = titleFontSize;
        return this;
    }

    public ProfileItemBuilder SetNormalFontSize(float normalFontSize)
    {
        Generator.NormalFontSize = normalFontSize;
        return this;
    }

    public ProfileItemBuilder SetSmallFontSize(float smallFontSize)
    {
        Generator.Config.SignatureFontSize = smallFontSize;
        return this;
    }

    public ProfileItemBuilder SetAvatarSize(int avatarSize)
    {
        Generator.Config.AvatarSize = avatarSize;
        return this;
    }

    public ProfileItemBuilder SetAvatarBorderSize(int avatarBorderSize)
    {
        Generator.AvatarBorderSize = avatarBorderSize;
        return this;
    }

    public byte[] Build()
    {
        return Generator.Generate(this);
    }
}

public class ProfileCard : ImageGeneratorBase, IImageGenerator<ProfileItemBuilder>
{
    public byte CardOpacity { get; set; } = 230;
    public int CardWidth { get; set; } = 450;
    public int RowSpacing { get; set; } = 50;
    public Color DefaultLabelColor { get; set; } = Color.DarkSlateGray;
    public Color DefaultValueColor { get; set; } = Color.Black;
    public Color DefaultValueBackgroundColor { get; set; } = Color.White;
    public float NormalFontSize { get; set; } = 18;
    public int AvatarBorderSize { get; set; } = 5;

    private ProfileItemBuilder? _currentBuilder;
    private int _cardHeight;
    private int _backgroundWidth;
    private int _backgroundHeight;
    private int _cardX;
    private int _cardY;

    public byte[] Generate(ProfileItemBuilder builder)
    {
        _currentBuilder = builder;
        try
        {
            return base.Generate();
        }
        finally
        {
            _currentBuilder = null;
        }
    }

    protected override (int Width, int Height) ComputeLayout()
    {
        if (_currentBuilder == null) throw new InvalidOperationException("Builder not set");

        Font titleFont = CreateFont(Config.TitleFontSize, FontStyle.Bold);
        Font normalFont = CreateFont(NormalFontSize);

        int titleHeight = !string.IsNullOrEmpty(Config.Title) ?
            (int)TextMeasurer.MeasureSize(Config.Title, new TextOptions(titleFont)).Height + 30 : 0;
        int avatarAreaHeight = Config.AvatarSize + 30;
        int itemAreaHeight = (_currentBuilder.Items.Count * RowSpacing) + 20;
        int signatureHeight = !string.IsNullOrEmpty(Config.Signature) ? 50 : 0;

        int contentHeight = Config.ContentTopMargin + titleHeight + avatarAreaHeight + itemAreaHeight + signatureHeight + Config.ContentBottomMargin;
        _cardHeight = contentHeight;

        _backgroundHeight = _cardHeight + Config.CardTopMargin + Config.CardBottomMargin;
        _backgroundWidth = CardWidth + 50;

        _cardX = (_backgroundWidth - CardWidth) / 2;
        _cardY = Config.CardTopMargin;

        return (_backgroundWidth, _backgroundHeight);
    }

    protected override void DrawContent(IImageProcessingContext ctx, int width, int height)
    {
        if (_currentBuilder == null) throw new InvalidOperationException("Builder not set");

        Font titleFont = CreateFont(Config.TitleFontSize, FontStyle.Bold);
        Font normalFont = CreateFont(NormalFontSize);
        Font smallFont = CreateFont(Config.SignatureFontSize);

        ctx.DrawRoundedRectangle(_cardX, _cardY, CardWidth, _cardHeight, Config.CardCornerRadius, new Rgba32(255, 255, 255, CardOpacity));

        int currentY = _cardY + Config.ContentTopMargin;

        if (!string.IsNullOrEmpty(Config.Title))
        {
            currentY = DrawTitleWithOffset(ctx, Config.Title, titleFont, currentY, width, 20);
        }

        currentY = DrawAvatarWithOffset(ctx, currentY, width);
        currentY = DrawProfileItems(ctx, normalFont, currentY);

        if (!string.IsNullOrEmpty(Config.Signature))
        {
            DrawSignatureCentered(ctx, smallFont, currentY + 10, width);
        }
    }

    private int DrawAvatarWithOffset(IImageProcessingContext ctx, int currentY, int canvasWidth)
    {
        int avatarX = (canvasWidth / 2) - (Config.AvatarSize / 2);
        DrawAvatar(ctx, avatarX, currentY, Config.AvatarSize);
        return currentY + Config.AvatarSize + 30;
    }

    private int DrawProfileItems(IImageProcessingContext ctx, Font normalFont, int currentY)
    {
        if (_currentBuilder == null) return currentY;

        int leftMargin = _cardX + 40;
        int rightMargin = _cardX + CardWidth - 40;

        foreach (ProfileItem item in _currentBuilder.Items)
        {
            RichTextOptions labelOptions = new RichTextOptions(normalFont)
            {
                Origin = new PointF(leftMargin, currentY),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ctx.DrawText(labelOptions, item.Label, item.LabelColor);

            FontRectangle valueTextSize = TextMeasurer.MeasureSize(item.Value, new TextOptions(normalFont));
            int paddingX = 15;
            int paddingY = 5;

            float backgroundWidth = valueTextSize.Width + (paddingX * 2);
            float backgroundHeight = valueTextSize.Height + (paddingY * 2);

            float valueTextX = rightMargin - backgroundWidth + paddingX;
            float valueTextY = currentY;

            if (item.UseEllipseBackground)
            {
                float ellipseCenterX = rightMargin - (backgroundWidth / 2);
                float ellipseCenterY = currentY + (valueTextSize.Height / 2);

                EllipsePolygon ellipse = new EllipsePolygon(ellipseCenterX, ellipseCenterY, backgroundWidth / 2, backgroundHeight / 2);
                ctx.Fill(item.ValueBackgroundColor, ellipse);
            }
            else
            {
                ctx.Fill(item.ValueBackgroundColor, new RectangleF(
                    rightMargin - backgroundWidth,
                    currentY,
                    backgroundWidth,
                    backgroundHeight
                ));
            }

            RichTextOptions valueTextOptions = new RichTextOptions(normalFont)
            {
                Origin = new PointF(valueTextX, valueTextY),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            ctx.DrawText(valueTextOptions, item.Value, item.ValueColor);

            currentY += RowSpacing;
        }

        return currentY;
    }
}
