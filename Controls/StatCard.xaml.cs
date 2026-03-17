using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace WorkClosure.Controls;

public sealed partial class StatCard : UserControl
{
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title),
        typeof(string),
        typeof(StatCard),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(string),
        typeof(StatCard),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty SubtitleProperty = DependencyProperty.Register(
        nameof(Subtitle),
        typeof(string),
        typeof(StatCard),
        new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty IconSymbolProperty = DependencyProperty.Register(
        nameof(IconSymbol),
        typeof(Symbol),
        typeof(StatCard),
        new PropertyMetadata(Symbol.Page));

    public static readonly DependencyProperty CardBackgroundProperty = DependencyProperty.Register(
        nameof(CardBackground),
        typeof(Brush),
        typeof(StatCard),
        new PropertyMetadata(null));

    public static readonly DependencyProperty CardBorderBrushProperty = DependencyProperty.Register(
        nameof(CardBorderBrush),
        typeof(Brush),
        typeof(StatCard),
        new PropertyMetadata(null));

    public static readonly DependencyProperty AccentBrushProperty = DependencyProperty.Register(
        nameof(AccentBrush),
        typeof(Brush),
        typeof(StatCard),
        new PropertyMetadata(null));

    public static readonly DependencyProperty IconBackgroundProperty = DependencyProperty.Register(
        nameof(IconBackground),
        typeof(Brush),
        typeof(StatCard),
        new PropertyMetadata(null));

    public static readonly DependencyProperty IconForegroundProperty = DependencyProperty.Register(
        nameof(IconForeground),
        typeof(Brush),
        typeof(StatCard),
        new PropertyMetadata(null));

    public static readonly DependencyProperty ValueForegroundProperty = DependencyProperty.Register(
        nameof(ValueForeground),
        typeof(Brush),
        typeof(StatCard),
        new PropertyMetadata(null));

    public StatCard()
    {
        InitializeComponent();
        ApplyDefaultBrushes();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Value
    {
        get => (string)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public Symbol IconSymbol
    {
        get => (Symbol)GetValue(IconSymbolProperty);
        set => SetValue(IconSymbolProperty, value);
    }

    public Brush CardBackground
    {
        get => (Brush)GetValue(CardBackgroundProperty);
        set => SetValue(CardBackgroundProperty, value);
    }

    public Brush CardBorderBrush
    {
        get => (Brush)GetValue(CardBorderBrushProperty);
        set => SetValue(CardBorderBrushProperty, value);
    }

    public Brush AccentBrush
    {
        get => (Brush)GetValue(AccentBrushProperty);
        set => SetValue(AccentBrushProperty, value);
    }

    public Brush IconBackground
    {
        get => (Brush)GetValue(IconBackgroundProperty);
        set => SetValue(IconBackgroundProperty, value);
    }

    public Brush IconForeground
    {
        get => (Brush)GetValue(IconForegroundProperty);
        set => SetValue(IconForegroundProperty, value);
    }

    public Brush ValueForeground
    {
        get => (Brush)GetValue(ValueForegroundProperty);
        set => SetValue(ValueForegroundProperty, value);
    }

    private void ApplyDefaultBrushes()
    {
        ApplyDefaultBrush(CardBackgroundProperty, "AppSurfaceBrush");
        ApplyDefaultBrush(CardBorderBrushProperty, "AppBorderBrush");
        ApplyDefaultBrush(AccentBrushProperty, "AppAccentBrush");
        ApplyDefaultBrush(IconBackgroundProperty, "AppAccentSoftBrush");
        ApplyDefaultBrush(IconForegroundProperty, "AppAccentStrongBrush");
        ApplyDefaultBrush(ValueForegroundProperty, "AppTitleBrush");
    }

    private void ApplyDefaultBrush(DependencyProperty property, string resourceKey)
    {
        if (GetValue(property) is Brush)
        {
            return;
        }

        if (Application.Current.Resources.TryGetValue(resourceKey, out var resource) && resource is Brush brush)
        {
            SetValue(property, brush);
        }
    }
}
