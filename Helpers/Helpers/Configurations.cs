using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Helpers
{
    public class Configurations
    {
        internal Settings Parameters;

        public Configurations()
        {
            Parameters = Load();
        }

        internal class Settings
        {
            public Event Keyboard { get; set; }
            public Event Mouse { get; set; }
            public Place Display { get; set; }
        }

        internal class Event
        {
            public Activity Action { get; set; }
            public Activity Sleep { get; set; }
        }

        internal class Place
        {
            public Dimension Min { get; set; }
            public Dimension Max { get; set; }
        }

        internal class Activity
        {
            public int Min { get; set; }
            public int Max { get; set; }
        }

        internal class Dimension
        {
            public int X { get; set; }
            public int Y { get; set; }
        }

        private Settings SetDefaults()
        {
            return new Settings()
            {
                Keyboard = new Event()
                {
                    Action = new Activity()
                    {
                        Min = 30,
                        Max = 100
                    },
                    Sleep = new Activity()
                    {
                        Min = 100,
                        Max = 150
                    }
                },
                Mouse = new Event()
                {
                    Action = new Activity()
                    {
                        Min = 5,
                        Max = 15
                    },
                    Sleep = new Activity()
                    {
                        Min = 100,
                        Max = 150
                    }
                },
                Display = new Place()
                {
                    Min = new Dimension()
                    {
                        X = 150,
                        Y = 100
                    },
                    Max = new Dimension()
                    {
                        X = 100,
                        Y = 150
                    }
                }
            };
        }

        private void Save(Settings settings)
        {
            string json = JsonConvert.SerializeObject(settings);

            File.WriteAllText("settings.json", json);
        }

        private Settings Load()
        {
            bool fileExists = File.Exists("settings.json");

            if (!fileExists)
                Save(SetDefaults());

            string json = File.ReadAllText("settings.json");

            Settings result = JsonConvert.DeserializeObject<Settings>(json);

            return result;
        }
    }

}
