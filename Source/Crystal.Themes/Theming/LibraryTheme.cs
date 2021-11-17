using Crystal.Themes.Internal;

#nullable enable

namespace Crystal.Themes.Theming
{
  /// <summary>
  /// Represents a theme.
  /// </summary>
  public class LibraryTheme
    {
        /// <summary>
        /// Gets the key for the library theme instance.
        /// </summary>
        public const string LibraryThemeInstanceKey = "Theme.LibraryThemeInstance";

        /// <summary>
        /// Gets the key for the theme color scheme.
        /// </summary>
        public const string LibraryThemeAlternativeColorSchemeKey = "Theme.AlternativeColorScheme";

        /// <summary>
        /// Gets the key for the color values being used to generate a runtime theme.
        /// </summary>
        public const string RuntimeThemeColorValuesKey = "Theme.RuntimeThemeColorValues";

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="resourceAddress">The URI of the theme ResourceDictionary.</param>
        /// <param name="libraryThemeProvider">The <see cref="Crystal.Themes.Theming.LibraryThemeProvider"/> which created this instance.</param>
        public LibraryTheme(Uri resourceAddress, LibraryThemeProvider? libraryThemeProvider)
            : this(CreateResourceDictionary(resourceAddress), libraryThemeProvider)
        {
        }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="resourceDictionary">The ResourceDictionary of the theme.</param>
        /// <param name="libraryThemeProvider">The <see cref="Crystal.Themes.Theming.LibraryThemeProvider"/> which created this instance.</param>
        public LibraryTheme(ResourceDictionary resourceDictionary, LibraryThemeProvider? libraryThemeProvider)
        {
            if (resourceDictionary is null)
            {
                throw new ArgumentNullException(nameof(resourceDictionary));
            }

            LibraryThemeProvider = libraryThemeProvider;

            IsRuntimeGenerated = (bool)(resourceDictionary[Theme.ThemeIsRuntimeGeneratedKey] ?? false);
            IsHighContrast = (bool)(resourceDictionary[Theme.ThemeIsHighContrastKey] ?? false);

            Name = (string)resourceDictionary[Theme.ThemeNameKey];
            Origin = (string)resourceDictionary[Theme.ThemeOriginKey];
            DisplayName = (string)resourceDictionary[Theme.ThemeDisplayNameKey];
            BaseColorScheme = (string)resourceDictionary[Theme.ThemeBaseColorSchemeKey];
            ColorScheme = (string)resourceDictionary[Theme.ThemeColorSchemeKey];
            AlternativeColorScheme = (string)resourceDictionary[LibraryTheme.LibraryThemeAlternativeColorSchemeKey];
            PrimaryAccentColor = resourceDictionary[Theme.ThemePrimaryAccentColorKey] as Color? ?? throw new ArgumentException($"Resource key \"{Theme.ThemePrimaryAccentColorKey}\" is missing, is null or is not a color.");
            ShowcaseBrush = (Brush)resourceDictionary[Theme.ThemeShowcaseBrushKey] ?? new SolidColorBrush(PrimaryAccentColor);

            AddResource(resourceDictionary);

            Resources[LibraryThemeInstanceKey] = this;
        }

        /// <inheritdoc cref="Theme.IsRuntimeGenerated"/>
        public bool IsRuntimeGenerated { get; }

        /// <inheritdoc cref="Theme.IsHighContrast"/>
        public bool IsHighContrast { get; }

        /// <inheritdoc cref="Theme.Name"/>
        public string Name { get; }

        /// <summary>
        /// Get the origin of the theme.
        /// </summary>
        public string? Origin { get; }

        /// <inheritdoc cref="Theme.DisplayName"/>
        public string DisplayName { get; }

        /// <inheritdoc cref="Theme.BaseColorScheme"/>
        public string BaseColorScheme { get; }

        /// <inheritdoc cref="Theme.ColorScheme"/>
        public string ColorScheme { get; }

        /// <inheritdoc cref="Theme.PrimaryAccentColor"/>
        public Color PrimaryAccentColor { get; set; }

        /// <inheritdoc cref="Theme.ShowcaseBrush"/>
        public Brush ShowcaseBrush { get; }

        /// <summary>
        /// The root <see cref="System.Windows.ResourceDictionary"/> containing all resource dictionaries belonging to this instance as <see cref="System.Windows.ResourceDictionary.MergedDictionaries"/>
        /// </summary>
        public ResourceDictionary Resources { get; } = new ResourceDictionary();

        /// <summary>
        /// Gets the alternative color scheme for this theme.
        /// </summary>
        public string AlternativeColorScheme { get; set; }

        public Theme? ParentTheme { get; internal set; }

        public LibraryThemeProvider? LibraryThemeProvider { get; }

        public virtual bool Matches(LibraryTheme libraryTheme)
        {
            return BaseColorScheme == libraryTheme.BaseColorScheme
                   && ColorScheme == libraryTheme.ColorScheme
                   && IsHighContrast == libraryTheme.IsHighContrast;
        }

        public virtual bool MatchesSecondTry(LibraryTheme libraryTheme)
        {
            return BaseColorScheme == libraryTheme.BaseColorScheme
                   && AlternativeColorScheme == libraryTheme.ColorScheme
                   && IsHighContrast == libraryTheme.IsHighContrast;
        }

        public virtual bool MatchesThirdTry(LibraryTheme libraryTheme)
        {
            return BaseColorScheme == libraryTheme.BaseColorScheme
                   && ShowcaseBrush.ToString() == libraryTheme.ShowcaseBrush.ToString()
                   && IsHighContrast == libraryTheme.IsHighContrast;
        }

        public LibraryTheme AddResource(ResourceDictionary resourceDictionary)
        {
            if (resourceDictionary is null)
            {
                throw new ArgumentNullException(nameof(resourceDictionary));
            }

            Resources.MergedDictionaries.Add(resourceDictionary);

            return this;
        }

        public override string ToString()
        {
            return $"DisplayName={DisplayName}, Name={Name}, Origin={Origin}, IsHighContrast={IsHighContrast}";
        }

        public static string? GetThemeName(ResourceDictionary resourceDictionary)
        {
            return Theme.GetThemeName(resourceDictionary);
        }

        public static bool IsThemeDictionary(ResourceDictionary resourceDictionary)
        {
            return Theme.IsThemeDictionary(resourceDictionary)
                || ResourceDictionaryHelper.ContainsKey(resourceDictionary, LibraryThemeInstanceKey);
        }

        public static bool IsRuntimeGeneratedThemeDictionary(ResourceDictionary resourceDictionary)
        {
            return Theme.IsRuntimeGeneratedThemeDictionary(resourceDictionary)
                || (ResourceDictionaryHelper.ContainsKey(resourceDictionary, LibraryThemeInstanceKey) && ((LibraryTheme)resourceDictionary[LibraryThemeInstanceKey]).IsRuntimeGenerated);
        }

        private static ResourceDictionary CreateResourceDictionary(Uri resourceAddress)
        {
            if (resourceAddress is null)
            {
                throw new ArgumentNullException(nameof(resourceAddress));
            }

            return new ResourceDictionary { Source = resourceAddress };
        }
    }
}