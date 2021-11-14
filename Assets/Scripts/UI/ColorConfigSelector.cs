using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Interfaces;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ColorConfigSelector : Initable
    {
        private List<string> _configs;
        public string Value
        {
            get
            {
                return _configs[Dropdown.value];
            }
            set
            {
                if (_configs == null)
                {
                    EnumerateColorConfigs();
                }
                var index = _configs.FindIndex(c => c.Equals(value, StringComparison.OrdinalIgnoreCase));
                if (index < 0) return;
                Dropdown.value = index;
            }
        }

        public Dropdown Dropdown;

        void Awake()
        {
            if (Dropdown == null)
            {
                Init();
            }
            EnumerateColorConfigs();
        }

        private void EnumerateColorConfigs()
        {
            Dropdown.ClearOptions();
            var options = Dropdown.options;
            var dir = FileSystemHelpers.ColorConfigs;
            var configs = dir.GetFiles("*.conf");
            if (!configs.Any())
            {
                NoteHelper.SerializeColorConfigs();
                configs = dir.GetFiles("*.conf");
            }
            _configs = configs.Select(c => c.Name).ToList();
            foreach (var file in configs)
            {
                var name = Path.GetFileNameWithoutExtension(file.Name);
                options.Add(new Dropdown.OptionData(name));
            }
            if (!options.Any())
            {
                options.Add(new Dropdown.OptionData("Configs not found"));
            }
            Dropdown.RefreshShownValue();
        }

        // ReSharper disable once InconsistentNaming
        public bool interactable
        {
            get { return Dropdown.interactable; }
            set { Dropdown.interactable = value; }
        }

        public override void Init()
        {
            Dropdown = transform.Find("Dropdown").GetComponent<Dropdown>();
        }
    }
}