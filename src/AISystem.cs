using System;
using System.Collections.Generic;

namespace CozyCityMayor
{
    public class CozyAISystem
    {
        private readonly Random _rng = new Random();
        public List<string> ChatHistory { get; private set; }
        public string LatestCitizenPetition { get; private set; }
        public string LatestPolicyBriefing { get; private set; }

        public CozyAISystem()
        {
            ChatHistory = new List<string>();
            ChatHistory.Add("CozyAI v4.2: Welcome, Mayor! I am your AI Chief of Staff.");
            ChatHistory.Add("CozyAI: Ask me for 'Policy Advice', 'Police Analysis', 'Tax Forecast', or 'Citizen Mood'!");
            LatestCitizenPetition = "AI Citizen Pulse: Baker Pippin asks if you can declare Cozy Festival Day soon or build a Maple Coffee Shop near Lot #2!";
            LatestPolicyBriefing = "AI Morning Brief: Unanimous 12-0 election victory recorded! Property tax bills are ready at your City Hall computer.";
        }

        public string GenerateMayorBriefing(
            int day,
            int treasury,
            int happiness,
            int safety,
            int cozyScore,
            WeatherType weather,
            DayType dayType,
            int pendingTaxBills,
            int activeIncidents,
            int builtCount)
        {
            string weatherNote = GetWeatherCommentary(weather);
            string dayNote = GetDayTypeCommentary(dayType);

            string urgentItem;
            if (pendingTaxBills > 0)
                urgentItem = pendingTaxBills + " property tax bill(s) awaiting personal delivery in town.";
            else if (activeIncidents > 0)
                urgentItem = activeIncidents + " open police incident(s) awaiting dispatch on your Computer.";
            else
                urgentItem = "All tax bills delivered and police blotter clear! Great time to build or read at the Library.";

            LatestPolicyBriefing =
                "[CozyAI Day " + day + " Executive Synthesis]\n" +
                "• Atmospheric State: " + weatherNote + "\n" +
                "• Civic Proclamation: " + dayNote + "\n" +
                "• Fiscal & Social Index: Treasury $" + treasury + " | Happiness " + happiness + "% | Safety " + safety + "% | Cozy Rating " + cozyScore + "\n" +
                "• Priority Action: " + urgentItem + "\n" +
                "• Urban Growth: " + builtCount + "/6 buildable city plots developed.";

            return LatestPolicyBriefing;
        }

        public string AskAdvisor(
            string topic,
            int day,
            int treasury,
            int happiness,
            int safety,
            int taxRate,
            WeatherType weather,
            DayType dayType,
            List<Employee> employees,
            List<PoliceIncident> incidents,
            List<PropertyTaxBill> bills)
        {
            ChatHistory.Add("You: " + topic);

            string reply;
            string lower = topic.ToLowerInvariant();

            if (lower.Contains("tax") || lower.Contains("budget") || lower.Contains("money"))
            {
                int unpaid = 0;
                int unpaidSum = 0;
                foreach (var b in bills)
                {
                    if (!b.Paid) { unpaid++; unpaidSum += b.AmountDue; }
                }
                string rateAdvice = taxRate > 12
                    ? "Your property tax rate is quite high; consider Cozy Payment Plans when delivering bills to preserve citizen happiness."
                    : "Your property tax rate is very citizen-friendly! Setting the day to 'Tax Day' boosts collection bonuses by +25%.";
                reply = "CozyAI Fiscal Model: " + unpaid + " bills remain uncollected ($" + unpaidSum + " projected). " + rateAdvice;
            }
            else if (lower.Contains("police") || lower.Contains("crime") || lower.Contains("safety"))
            {
                int openCount = 0;
                string sampleIncident = "none";
                foreach (var inc in incidents)
                {
                    if (!inc.Resolved)
                    {
                        openCount++;
                        sampleIncident = inc.Title;
                    }
                }
                reply = openCount > 0
                    ? ("CozyAI Tactical Scan: " + openCount + " active incident(s), notably '" + sampleIncident + "'. Dispatch Chief Barnaby or Officer Maple with Cozy Mediation for +Safety AND +Happiness!")
                    : ("CozyAI Tactical Scan: Safety is at " + safety + "%. Zero unresolved incidents! Declaring 'Community Safety Day' will further inspire the precinct.");
            }
            else if (lower.Contains("employee") || lower.Contains("staff") || lower.Contains("morale"))
            {
                Employee lowest = null;
                foreach (var e in employees)
                {
                    if (lowest == null || e.Morale < lowest.Morale) lowest = e;
                }
                reply = lowest != null
                    ? ("CozyAI HR Analytics: " + lowest.Name + " (" + lowest.Role + ") currently has the lowest morale (" + lowest.Morale + "%). Consider awarding a Cozy Bonus ($75) on the Employees tab!")
                    : "CozyAI HR Analytics: All municipal employees are operating at peak cozy efficiency.";
            }
            else if (lower.Contains("weather") || lower.Contains("day") || lower.Contains("schedule"))
            {
                reply = "CozyAI Enviro-Forecast: Current weather is " + weather + " on " + dayType + ". Tip: Citizens love Sunny Warmth on Festival Days, and Cozy Rain while reading inside the Public Library!";
            }
            else
            {
                string[] insights = new[]
                {
                    "CozyAI Synthesis: With $" + treasury + " in treasury and " + happiness + "% happiness, building a Maple Coffee Shop or Community Park will boost daily passive revenue!",
                    "CozyAI Citizen Sentiment: Even Cornelius Von Monocle was spotted secretly enjoying your town's " + weather + " weather today!",
                    "CozyAI Literary Note: Visit Librarian Clara in the Library to read 'The AI Chronicle', which dynamically records your mayoral legacy!"
                };
                reply = insights[_rng.Next(insights.Length)];
            }

            ChatHistory.Add(reply);
            while (ChatHistory.Count > 14)
            {
                ChatHistory.RemoveAt(0);
            }
            return reply;
        }

        public string GenerateEmployeeEvaluation(Employee emp, DayType dayType, WeatherType weather)
        {
            string moodDesc = emp.Morale >= 85 ? "radiating enthusiasm" : (emp.Morale >= 65 ? "steady and dependable" : "in need of a coffee break and bonus");
            return "[AI HR Review] " + emp.Name + " (" + emp.Role + ") is " + moodDesc + " (" + emp.Morale + "% morale, " + emp.Efficiency + "% efficiency). During " + dayType + " & " + weather + ", recommended focus: '" + emp.CurrentAssignment + "'.";
        }

        public string GenerateDynamicIncident(int day, WeatherType weather)
        {
            string[][] pool = new[]
            {
                new[] { "Great Croissant Caper", "Pippin's Bakery", "Three golden butter croissants vanished from the cooling windowsill! Witnesses saw a very plump squirrel wearing a tiny bandana.", "Low (Cozy)" },
                new[] { "Unauthorized Duck Parade", "Main Street Crosswalk", "A mother mallard and nine ducklings are holding up bicycle traffic while admiring the town flowerbeds.", "Low (Cozy)" },
                new[] { "Midnight Accordion Serenade", "Maple Residential Lane", "Someone is playing cheerful polka music at 10 PM. Half the street is dancing; Cornelius Von Monocle filed 14 complaints.", "Medium" },
                new[] { "Runaway Cider Barrel", "Town Square Fountain", "A barrel of warm spiced apple cider rolled down from the market and is bobbing in the town fountain.", "Low (Cozy)" },
                new[] { "Missing Library Bookmark", "Public Library", "Librarian Clara reports the Golden Pressed-Leaf Bookmark was accidentally borrowed inside an oversized encyclopedia.", "Low (Cozy)" },
                new[] { "Monocle Parking Violation", "City Hall Steps", "Cornelius Von Monocle parked his vintage penny-farthing bicycle diagonally across three cozy bench spots.", "Medium" }
            };
            var pick = pool[_rng.Next(pool.Length)];
            return pick[0] + "|" + pick[1] + "|" + pick[2] + "|" + pick[3];
        }

        private string GetWeatherCommentary(WeatherType w)
        {
            switch (w)
            {
                case WeatherType.SunnyWarm: return "Sunny & Golden (+Happiness, ideal for walking & tax delivery)";
                case WeatherType.CozyRain: return "Cozy Raindrops (+Library comfort, crops & gardens thriving)";
                case WeatherType.AutumnBreeze: return "Crisp Autumn Breeze (+Cozy aesthetics, falling maple leaves)";
                case WeatherType.GentleSnow: return "Gentle Snowfall (+Winter wonderland charm, warm cocoa demand)";
                case WeatherType.StarlightGlow: return "Starlight Twilight (+Dreamy evening ambiance, fireflies active)";
                default: return "Balanced Skies";
            }
        }

        private string GetDayTypeCommentary(DayType d)
        {
            switch (d)
            {
                case DayType.TaxDay: return "Official Tax Day (+25% Property Tax collection bonus)";
                case DayType.CozyFestivalDay: return "Cozy Festival Day (+Happiness & festive bunting across town)";
                case DayType.CivicConstructionDay: return "Civic Construction Day (20% discount on all city building costs)";
                case DayType.CommunitySafetyDay: return "Community Safety Day (+Police resolution bonus & safety boost)";
                case DayType.LibraryLiteracyDay: return "Library & Literacy Day (+Bonus wisdom & grant when reading books)";
                default: return "Standard Civic Day";
            }
        }
    }
}
