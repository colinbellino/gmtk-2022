using System;

namespace UnityEngine.Localization.Settings
{
	/// <summary>
	/// Gets the user's selected language by using the system language (string comparison instead of code).
	/// </summary>
	[Serializable]
	public class SystemStringLanguageSelector : IStartupLocaleSelector, IInitialize
	{
		public void PostInitialization(LocalizationSettings settings) { }

		public Locale GetStartupLocale(ILocalesProvider availableLocales)
		{
			var systemLanguage = Application.systemLanguage;
			foreach (var locale in availableLocales.Locales)
			{
				if (locale.LocaleName == systemLanguage.ToString())
					return locale;
			}

			return null;
		}
	}
}
