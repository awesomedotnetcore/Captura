﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace Captura
{
    public class LanguageManager : LanguageFields
    {
        readonly JObject _defaultLanguage;
        JObject _currentLanguage;
        readonly string _langDir;

        readonly Settings _settings;

        public static LanguageManager Instance { get; } = new LanguageManager();

        public LanguageManager()
        {
            _langDir = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Languages");

            _defaultLanguage = LoadLang("en");

            var cultures = new List<CultureInfo>();

            var files  = Directory.EnumerateFiles(_langDir, "*.json");

            _settings = ServiceProvider.Get<Settings>();

            foreach (var file in files)
            {
                var cultureName = Path.GetFileNameWithoutExtension(file);

                try
                {
                    if (cultureName == null)
                        continue;

                    var culture = CultureInfo.GetCultureInfo(cultureName);

                    cultures.Add(culture);

                    if (cultureName == _settings.UI.Language)
                    {
                        CurrentCulture = culture;
                    }
                }
                catch
                {
                    // Ignore
                }
            }

            if (_currentCulture == null)
                CurrentCulture = new CultureInfo("en");

            cultures.Sort((X, Y) => string.Compare(X.DisplayName, Y.DisplayName, StringComparison.Ordinal));

            AvailableCultures = cultures;
        }

        public IReadOnlyList<CultureInfo> AvailableCultures { get; }

        CultureInfo _currentCulture;

        public CultureInfo CurrentCulture
        {
            get => _currentCulture;
            set
            {
                _currentCulture = value;

                Thread.CurrentThread.CurrentUICulture = value;

                _settings.UI.Language = value.Name;

                _currentLanguage = LoadLang(value.Name);

                LanguageChanged?.Invoke(value);

                RaiseAllChanged();
            }
        }

        public event Action<CultureInfo> LanguageChanged;

        JObject LoadLang(string LanguageId)
        {
            var filePath = Path.Combine(_langDir, $"{LanguageId}.json");

            try
            {
                return JObject.Parse(File.ReadAllText(filePath));
            }
            catch
            {
                return new JObject();
            }
        }

        public string this[string Key]
        {
            get
            {
                if (Key == null)
                    return "";

                if (_currentLanguage != null
                    && _currentLanguage.TryGetValue(Key, out var value)
                    && value.ToString() is string s
                    && !string.IsNullOrWhiteSpace(s))
                    return s;

                if (_defaultLanguage.TryGetValue(Key, out value)
                    && value.ToString() is string t
                    && !string.IsNullOrWhiteSpace(t))
                    return t;

                return Key;
            }
        }

        protected override string GetValue(string PropertyName) => this[PropertyName];
    }
}
