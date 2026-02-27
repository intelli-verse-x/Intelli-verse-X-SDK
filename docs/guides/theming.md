# Theming Guide

Customize the SDK's visual appearance.

## Theme Configuration

### Colors

```csharp
IVXTheme.PrimaryColor = new Color(0.4f, 0.2f, 0.8f);
IVXTheme.SecondaryColor = new Color(0.2f, 0.6f, 0.9f);
```

### Fonts

Configure TMPro font assets in the theme settings.

### UI Sprites

Replace default sprites in the theme's sprite atlas.

## Dark Mode

Toggle between light and dark themes:

```csharp
IVXTheme.SetDarkMode(true);
```

## See Also

- [UI Module](../modules/ui.md)
