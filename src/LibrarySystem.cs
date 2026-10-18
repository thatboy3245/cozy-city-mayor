using System;
using System.Collections.Generic;
using System.Drawing;

namespace CozyCityMayor
{
    public class LibrarySystem
    {
        public List<BookData> Books { get; private set; }
        public int SelectedBookIndex { get; set; }
        public int CurrentPageIndex { get; set; }

        public LibrarySystem()
        {
            Books = new List<BookData>();
            SelectedBookIndex = 0;
            CurrentPageIndex = 0;
            InitializeBooks();
        }

        private void InitializeBooks()
        {
            Books.Add(new BookData
            {
                Title = "1. The Cozy Mayor's Handbook: Taxes & Tea",
                Author = "Mayor Emeritus Hazelwood",
                CoverColor = Color.FromArgb(180, 75, 55),
                Pages = new List<string>
                {
                    "CHAPTER I: WELCOME TO CITY HALL\n\n" +
                    "Congratulations on your unanimous election victory! As Mayor of Maplewood Valley, your desk computer inside City Hall is the beating heart of municipal governance.\n\n" +
                    "Every morning, log into CozyOS on your desk computer to inspect employee morale, review Police Incidents, and print Property Tax Bills.",

                    "CHAPTER II: DELIVERING PROPERTY TAX BILLS\n\n" +
                    "Unlike cold, distant bureaucracies, Maplewood Valley mayors deliver Property Tax Bills in person!\n\n" +
                    "Walk up to citizens or houses marked with a golden Tax Scroll icon and press [E] / (A Button). You can collect the full amount, offer a Cozy Payment Plan (+Happiness), or grant a Community Discount!",

                    "CHAPTER III: DECLARING DAY TYPES\n\n" +
                    "As Mayor, you decide what kind of day the entire town celebrates:\n" +
                    "• TAX DAY: +25% bonus on all property tax bills collected!\n" +
                    "• COZY FESTIVAL DAY: Boosts citizen joy and hangs colorful lanterns.\n" +
                    "• CIVIC CONSTRUCTION DAY: Reduces city building costs by 20%.\n" +
                    "• COMMUNITY SAFETY DAY: Empowers Police Chief Barnaby & Officer Maple.\n" +
                    "• LIBRARY LITERACY DAY: Grants bonus research funding when reading books!"
                }
            });

            Books.Add(new BookData
            {
                Title = "2. Secrets of the Atmospheric Weather Tower",
                Author = "Dr. Nimbus Cloudwright",
                CoverColor = Color.FromArgb(55, 115, 165),
                Pages = new List<string>
                {
                    "CONTROLLING THE SKIES\n\n" +
                    "Built in 1924 atop City Hall, the Cozy Atmospheric Harmonizer lets the Mayor set the town's weather at the push of a button (via your Computer, your on-screen MayorPhone, or pressing [F]).",

                    "WEATHER EFFECTS ON MAPLEWOOD VALLEY\n\n" +
                    "• SUNNY & WARM: Golden sunbeams dance across town. Citizens stroll happily.\n" +
                    "• COZY RAIN: Gentle silver raindrops nourish community gardens and make the Library extra inviting.\n" +
                    "• AUTUMN BREEZE: Swirling amber maple leaves blanket the cobblestones.\n" +
                    "• GENTLE SNOW: Soft snowflakes turn Maplewood into a winter postcard.\n" +
                    "• STARLIGHT GLOW: Dreamy twilight skies filled with warm glowing fireflies."
                }
            });

            Books.Add(new BookData
            {
                Title = "3. Modern Cozy Policing: De-escalation & Donuts",
                Author = "Police Chief Barnaby",
                CoverColor = Color.FromArgb(45, 75, 130),
                Pages = new List<string>
                {
                    "THE PRECINCT PHILOSOPHY\n\n" +
                    "Here at the Maplewood Police Precinct, our motto is 'Protect the Peace, Rescue the Kittens, Share the Pastries.'\n\n" +
                    "When an incident appears on the Mayor's Computer, you decide how our officers respond!",

                    "CHOOSING YOUR RESPONSE\n\n" +
                    "• COZY MEDIATION: Resolves neighborly disputes over accordion music or runaway cider barrels with warm tea and mutual agreement (+Happiness, +Safety).\n\n" +
                    "• FULL INVESTIGATION: Sends Officer Maple with magnifying glass and notebook to solve bakery mysteries (+High Safety).\n\n" +
                    "• STRICT ORDINANCE FINE: Collects municipal fine revenue into the City Treasury, though citizens may grumble slightly!"
                }
            });

            Books.Add(new BookData
            {
                Title = "4. Architecture of a Thriving Cozy City",
                Author = "Master Builder Hugo",
                CoverColor = Color.FromArgb(145, 105, 50),
                Pages = new List<string>
                {
                    "BUILDING YOUR DREAM TOWN\n\n" +
                    "Across Maplewood Valley are 6 prime Buildable Civic Lots. Walk up to any signpost in town (or open the City Builder tab on your Computer / press [B]) to construct new buildings!",

                    "CATALOG OF CIVIC STRUCTURES\n\n" +
                    "• COZY BAKERY: Fills the air with cinnamon aroma (+Daily Tax, +Cozy).\n" +
                    "• MAPLE COFFEE SHOP: Boosts employee efficiency and morning cheer.\n" +
                    "• COMMUNITY PARK: Lush fountain & benches (+Happiness, +Cozy).\n" +
                    "• SOLAR GARDEN: Clean green energy for City Hall (+Treasury).\n" +
                    "• TOWN CLINIC: Keeps every citizen healthy (+Safety, +Happiness).\n" +
                    "• RETRO ARCADE & BOOKSTORE: Cultural landmarks that can be upgraded!"
                }
            });

            Books.Add(new BookData
            {
                Title = "5. Who Is Cornelius Von Monocle?",
                Author = "Clara the Librarian",
                CoverColor = Color.FromArgb(110, 60, 125),
                Pages = new List<string>
                {
                    "THE RIVAL CANDIDATE\n\n" +
                    "Cornelius Von Monocle ran against you in the Mayoral Election on a platform of 'Mandatory Concrete Lawns, Banning Birdsong After 6 AM, and 400% Tax on Muffins.'\n\n" +
                    "Unsurprisingly, he received 0 votes—even his own campaign manager Fitz voted for you!",

                    "REDEEMING CORNELIUS\n\n" +
                    "Despite his bluster, Cornelius still wanders Town Square adjusting his monocle. If you talk to him on Cozy Festival Days or build a nice park near his mansion, even his grumpy heart begins to warm up."
                }
            });

            Books.Add(new BookData
            {
                Title = "6. History of Maplewood Valley (1888 - Present)",
                Author = "Elder Oak",
                CoverColor = Color.FromArgb(65, 125, 75),
                Pages = new List<string>
                {
                    "THE FOUNDING ACORN TREATY\n\n" +
                    "Founded beside the Whispering River in 1888, Maplewood Valley was built as a sanctuary where neighbors greet each other by name and city government actually listens to the people.",

                    "THE LEGEND OF THE GOLDEN GAVEL\n\n" +
                    "Tradition holds that when a Mayor achieves 95% Happiness, builds out the town's empty lots, and reads the wisdom of the Library, Maplewood enters a Golden Era of Cozy Prosperity!"
                }
            });

            Books.Add(new BookData
            {
                Title = "7. Gamepad & Input Engineering Manual",
                Author = "Civic Tech Guild",
                CoverColor = Color.FromArgb(70, 90, 105),
                Pages = new List<string>
                {
                    "DUAL HARDWARE CONTROLLER ARCHITECTURE\n\n" +
                    "Cozy City Mayor natively supports both Microsoft XInput (xinput1_4.dll / Xbox controllers with dual-motor haptic rumble) and DirectInput8 (dinput8.dll / WinMM HID joysticks & DualShock/Nintendo controllers).",

                    "QUICK CONTROLS REFERENCE\n\n" +
                    "• Move: WASD / Arrow Keys / Left Thumbstick / D-Pad\n" +
                    "• Interact / Talk / Deliver Tax / Read: [E] / [Enter] / Gamepad (A)\n" +
                    "• Open On-Screen MayorPhone: [P] / [Tab] / Gamepad (Y)\n" +
                    "• Open City Build Menu: [B] / Gamepad (X)\n" +
                    "• Quick Weather / Day Cycle: [F] & [G] / Bumpers\n" +
                    "• Pause / Settings / Save: [Esc] / Gamepad (Start)"
                }
            });

            Books.Add(new BookData
            {
                Title = "8. [AI Powered] The Live Chronicle of Your Term",
                Author = "CozyAI Archivist v4.2",
                CoverColor = Color.FromArgb(175, 130, 40),
                Pages = new List<string>
                {
                    "Generating live AI historical chronicle..."
                }
            });
        }

        public void RefreshAIChronicle(
            string mayorName,
            int day,
            int treasury,
            int happiness,
            int safety,
            int cozyScore,
            WeatherType weather,
            DayType dayType,
            int billsDelivered,
            int incidentsResolved,
            List<CityBuildingPlot> plots)
        {
            var aiBook = Books[Books.Count - 1];
            aiBook.Pages.Clear();

            int builtPlots = 0;
            List<string> builtNames = new List<string>();
            foreach (var p in plots)
            {
                if (p.CurrentBuilding != BuildingType.EmptyLot)
                {
                    builtPlots++;
                    builtNames.Add(p.CurrentBuilding + " (Lv." + p.Level + ")");
                }
            }

            string builtList = builtNames.Count > 0 ? string.Join(", ", builtNames.ToArray()) : "awaiting first groundbreaking ceremony";

            aiBook.Pages.Add(
                "THE ANNALS OF " + mayorName.ToUpperInvariant() + " — DAY " + day + "\n" +
                "(Synthesized Live by CozyAI Archivist)\n\n" +
                "On Election Day, " + mayorName + " swept the polls in a historic 12-to-0 landslide over Cornelius Von Monocle. " +
                "Since taking office at City Hall, the Mayor has proclaimed today '" + dayType + "' beneath '" + weather + "' skies.\n\n" +
                "Current Treasury stands at $" + treasury + ", with Citizen Happiness at " + happiness + "% and Public Safety at " + safety + "%.");

            aiBook.Pages.Add(
                "FISCAL & CIVIC RECORD\n\n" +
                "• Personal Property Tax Bills Delivered: " + billsDelivered + "\n" +
                "• Police Incidents Resolved with Precinct: " + incidentsResolved + "\n" +
                "• Town Cozy Index Rating: " + cozyScore + " pts\n" +
                "• Developed City Plots (" + builtPlots + "/" + plots.Count + "): " + builtList + "\n\n" +
                "AI Historical Verdict: " + (happiness >= 85 ? "Maplewood Valley is flourishing in a golden age of warmth and civic trust!" : "A promising term underway—more parks and cozy mediation will lift spirits even higher!"));
        }
    }
}
