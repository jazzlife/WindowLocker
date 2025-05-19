using System;
using System.Windows;
using System.Globalization;
using System.Threading;
using System.Linq;

namespace WindowLocker.Managers
{
    public class LanguageManager
    {
        public static void SwitchLanguage(string cultureName)
        {
            // Change current thread culture
            Thread.CurrentThread.CurrentCulture = new CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(cultureName);

            // Load the resource dictionary
            ResourceDictionary dict = new ResourceDictionary();
            dict.Source = new Uri($"/WindowLocker;component/Resources/Strings.{cultureName}.xaml", UriKind.Relative);

            // Remove old resource dictionary and add new one
            var oldDict = Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source != null && d.Source.OriginalString.Contains("Strings."));

            if (oldDict != null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(oldDict);
            }

            Application.Current.Resources.MergedDictionaries.Add(dict);

            // Save the language setting
            Properties.Settings.Default.Language = cultureName;
            Properties.Settings.Default.Save();
        }

        public static void InitializeLanguage()
        {
            // Load saved language setting
            string savedLanguage = Properties.Settings.Default.Language;
            SwitchLanguage(savedLanguage);
        }
    }
}