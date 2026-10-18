using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace CozyCityMayor
{
    public class GameStateEngine
    {
        public InputSystem Input { get; private set; }
        public AudioSystem Audio { get; private set; }
        public CozyAISystem AI { get; private set; }
        public LibrarySystem Library { get; private set; }
        public ElectionScene Election { get; private set; }

        public GameScene CurrentScene { get; set; }
        public OverlayMode ActiveOverlay { get; set; }
        public int TitleMenuIndex { get; set; }

        public string MayorName { get; set; }
        public float PlayerX { get; set; }
        public float PlayerY { get; set; }
        public int PlayerFacing { get; set; }
        public float WalkAnimTime { get; set; }

        public int CurrentDay { get; set; }
        public float DayClockMinutes { get; set; }
        public int CityTreasury { get; set; }
        public int Happiness { get; set; }
        public int SafetyRating { get; set; }
        public int CozyScore { get; set; }
        public int PropertyTaxRate { get; set; }
        public int BooksReadCount { get; set; }
        public int TaxBillsDeliveredTotal { get; set; }
        public int IncidentsResolvedTotal { get; set; }

        public WeatherType CurrentWeather { get; set; }
        public DayType CurrentDayType { get; set; }
        public List<WeatherParticle> WeatherParticles { get; private set; }

        public int ComputerTabIndex { get; set; }
        public int ComputerSelectedRow { get; set; }

        public bool PhoneExpanded { get; set; }
        public int PhoneAppIndex { get; set; }
        public int PhoneSelectedRow { get; set; }
        public string PhoneNotification { get; set; }

        public int SelectedPlotIndex { get; set; }
        public int SelectedBuildOption { get; set; }

        public List<Employee> Employees { get; private set; }
        public List<PoliceIncident> Incidents { get; private set; }
        public List<PropertyTaxBill> TaxBills { get; private set; }
        public List<CityBuildingPlot> BuildingPlots { get; private set; }
        public List<CitizenNPC> Citizens { get; private set; }
        public List<ScheduleItem> DailySchedule { get; private set; }
        public List<FloatingText> FloatingTexts { get; private set; }

        public Queue<DialogueMessage> DialogueQueue { get; private set; }
        public DialogueMessage ActiveDialogue { get; set; }
        public int SelectedChoiceIndex { get; set; }
        public float DialogueCharIndex { get; set; }

        private readonly Random _rng = new Random();
        private readonly string _saveFilePath;

        public GameStateEngine()
        {
            Input = new InputSystem();
            Audio = new AudioSystem();
            AI = new CozyAISystem();
            Library = new LibrarySystem();
            Election = new ElectionScene();

            CurrentScene = GameScene.TitleScreen;
            ActiveOverlay = OverlayMode.None;
            MayorName = "Mayor Maple";
            PlayerX = 320f;
            PlayerY = 210f;
            CurrentDay = 1;
            DayClockMinutes = 8 * 60f;
            CityTreasury = 1450;
            Happiness = 86;
            SafetyRating = 90;
            CozyScore = 135;
            PropertyTaxRate = 8;

            CurrentWeather = WeatherType.SunnyWarm;
            CurrentDayType = DayType.TaxDay;
            WeatherParticles = new List<WeatherParticle>();
            PhoneExpanded = true;
            PhoneNotification = "Welcome to MayorPhone OS!";

            Employees = new List<Employee>();
            Incidents = new List<PoliceIncident>();
            TaxBills = new List<PropertyTaxBill>();
            BuildingPlots = new List<CityBuildingPlot>();
            Citizens = new List<CitizenNPC>();
            DailySchedule = new List<ScheduleItem>();
            FloatingTexts = new List<FloatingText>();
            DialogueQueue = new Queue<DialogueMessage>();

            string appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "CozyCityMayor");
            try { Directory.CreateDirectory(appData); } catch { }
            _saveFilePath = Path.Combine(appData, "savegame.cfg");

            InitializeGameData();
            InitWeatherParticles();
        }

        public void InitializeGameData()
        {
            Employees.Clear();
            Employees.Add(new Employee
            {
                Id = "emp_barnaby",
                Name = "Chief Barnaby",
                Role = "Chief of Police",
                Department = "Police Precinct",
                Salary = 120,
                Morale = 92,
                Efficiency = 94,
                CurrentAssignment = "Precinct Command & Pastry Patrol",
                Bio = "20-year veteran who believes most municipal crime can be solved with warm tea and calm listening.",
                AIInsight = "Peak morale. Excels at Cozy Mediation on Community Safety Days.",
                ShirtColor = Color.FromArgb(45, 80, 140)
            });
            Employees.Add(new Employee
            {
                Id = "emp_maple",
                Name = "Officer Maple",
                Role = "Field Constable",
                Department = "Police Precinct",
                Salary = 95,
                Morale = 88,
                Efficiency = 90,
                CurrentAssignment = "Town Square & Lost Corgi Unit",
                Bio = "Fastest bicycle responder in Maplewood Valley; carries emergency dog treats at all times.",
                AIInsight = "High field responsiveness. Pair with Chief Barnaby for rapid incident resolution.",
                ShirtColor = Color.FromArgb(60, 105, 170)
            });
            Employees.Add(new Employee
            {
                Id = "emp_penny",
                Name = "Penny Ledger",
                Role = "Chief Tax Assessor",
                Department = "City Hall Treasury",
                Salary = 110,
                Morale = 86,
                Efficiency = 96,
                CurrentAssignment = "Property Tax Assessment & Budget",
                Bio = "Keeps the City Hall computer humming and prepares golden-embossed Property Tax scrolls.",
                AIInsight = "Recommends declaring Tax Day before delivering property tax bills for a +25% bonus.",
                ShirtColor = Color.FromArgb(165, 70, 90)
            });
            Employees.Add(new Employee
            {
                Id = "emp_clara",
                Name = "Clara Page",
                Role = "Head Librarian",
                Department = "Public Library",
                Salary = 100,
                Morale = 95,
                Efficiency = 92,
                CurrentAssignment = "Rare Archives & AI Chronicle",
                Bio = "Curator of the Maplewood Public Library and caretaker of the Town History Archives.",
                AIInsight = "Thrives on Library Literacy Day and Cozy Rain weather.",
                ShirtColor = Color.FromArgb(120, 75, 145)
            });
            Employees.Add(new Employee
            {
                Id = "emp_hugo",
                Name = "Master Builder Hugo",
                Role = "City Architect",
                Department = "Public Works",
                Salary = 115,
                Morale = 84,
                Efficiency = 89,
                CurrentAssignment = "Civic Lot Construction & Zoning",
                Bio = "Crafts timber-frame bakeries, solar gardens, and parks with zero noise pollution.",
                AIInsight = "Cuts construction costs by 20% whenever Civic Construction Day is declared.",
                ShirtColor = Color.FromArgb(190, 125, 45)
            });
            Employees.Add(new Employee
            {
                Id = "emp_gus",
                Name = "Groundskeeper Gus",
                Role = "Weather & Parks Keeper",
                Department = "Atmospheric & Parks",
                Salary = 90,
                Morale = 91,
                Efficiency = 87,
                CurrentAssignment = "Weather Tower Calibration",
                Bio = "Tends the flowerbeds and polishes the brass gears of the City Hall Weather Harmonizer.",
                AIInsight = "Happiest during Sunny Warmth and Autumn Breeze.",
                ShirtColor = Color.FromArgb(65, 140, 75)
            });

            BuildingPlots.Clear();
            BuildingPlots.Add(new CityBuildingPlot { PlotIndex = 0, PlotName = "Lot #1: West Maple Corner", WorldRect = new Rectangle(45, 75, 76, 56), CurrentBuilding = BuildingType.CozyBakery, Level = 1, DailyTaxRevenue = 120, CozyBonus = 15 });
            BuildingPlots.Add(new CityBuildingPlot { PlotIndex = 1, PlotName = "Lot #2: North Fountain Plaza", WorldRect = new Rectangle(150, 75, 76, 56), CurrentBuilding = BuildingType.EmptyLot, Level = 0, DailyTaxRevenue = 0, CozyBonus = 0 });
            BuildingPlots.Add(new CityBuildingPlot { PlotIndex = 2, PlotName = "Lot #3: East Blossom Lane", WorldRect = new Rectangle(420, 75, 76, 56), CurrentBuilding = BuildingType.EmptyLot, Level = 0, DailyTaxRevenue = 0, CozyBonus = 0 });
            BuildingPlots.Add(new CityBuildingPlot { PlotIndex = 3, PlotName = "Lot #4: Riverfront Terrace", WorldRect = new Rectangle(520, 75, 76, 56), CurrentBuilding = BuildingType.EmptyLot, Level = 0, DailyTaxRevenue = 0, CozyBonus = 0 });
            BuildingPlots.Add(new CityBuildingPlot { PlotIndex = 4, PlotName = "Lot #5: South Garden Walk", WorldRect = new Rectangle(45, 255, 76, 56), CurrentBuilding = BuildingType.CommunityPark, Level = 1, DailyTaxRevenue = 60, CozyBonus = 25 });
            BuildingPlots.Add(new CityBuildingPlot { PlotIndex = 5, PlotName = "Lot #6: Lantern Promenade", WorldRect = new Rectangle(520, 255, 76, 56), CurrentBuilding = BuildingType.EmptyLot, Level = 0, DailyTaxRevenue = 0, CozyBonus = 0 });

            GenerateDailyTaxBills();
            InitializePoliceIncidents();
            InitializeCitizens();
            InitializeSchedule();
            Library.RefreshAIChronicle(MayorName, CurrentDay, CityTreasury, Happiness, SafetyRating, CozyScore, CurrentWeather, CurrentDayType, TaxBillsDeliveredTotal, IncidentsResolvedTotal, BuildingPlots);
        }

        public void GenerateDailyTaxBills()
        {
            TaxBills.Clear();
            int baseAmt = Math.Max(40, PropertyTaxRate * 15);
            TaxBills.Add(new PropertyTaxBill { Id = "tax_1", PropertyName = "101 Maple Bakery Cottage", OwnerName = "Baker Pippin", AmountDue = baseAmt + 20, Delivered = false, Paid = false, Notes = "Commercial Bakery Assessment", HouseIndex = 0 });
            TaxBills.Add(new PropertyTaxBill { Id = "tax_2", PropertyName = "104 Roast & Bean Roastery", OwnerName = "Barista Hazel", AmountDue = baseAmt + 15, Delivered = false, Paid = false, Notes = "Cafe Patio Assessment", HouseIndex = 1 });
            TaxBills.Add(new PropertyTaxBill { Id = "tax_3", PropertyName = "202 Petal & Stem Greenhouse", OwnerName = "Florist Daisy", AmountDue = baseAmt, Delivered = false, Paid = false, Notes = "Botanical Conservatory", HouseIndex = 2 });
            TaxBills.Add(new PropertyTaxBill { Id = "tax_4", PropertyName = "305 Whispering Oak Manor", OwnerName = "Elder Oak", AmountDue = baseAmt - 10, Delivered = false, Paid = false, Notes = "Historic Homestead", HouseIndex = 3 });
            TaxBills.Add(new PropertyTaxBill { Id = "tax_5", PropertyName = "400 Monocle Estate", OwnerName = "Cornelius Von Monocle", AmountDue = baseAmt + 65, Delivered = false, Paid = false, Notes = "Luxury Mansion & Top-Hat Vault", HouseIndex = 4 });

            foreach (var c in Citizens)
            {
                c.HasTaxBillPending = false;
                foreach (var b in TaxBills)
                {
                    if (b.OwnerName == c.Name && !b.Delivered)
                    {
                        c.HasTaxBillPending = true;
                        c.TaxBillId = b.Id;
                    }
                }
            }
        }

        private void InitializePoliceIncidents()
        {
            Incidents.Clear();
            Incidents.Add(new PoliceIncident
            {
                Id = "inc_1",
                Title = "The Great Croissant Caper",
                Location = "Pippin's Bakery Window",
                Description = "Three warm butter croissants vanished from the cooling rack! A plump squirrel wearing a miniature red scarf is the prime suspect.",
                Severity = "Low (Cozy)",
                Resolved = false,
                AssignedOfficer = "Officer Maple",
                RewardBudget = 95,
                SafetyImpact = 4,
                HappinessImpact = 5
            });
            Incidents.Add(new PoliceIncident
            {
                Id = "inc_2",
                Title = "Midnight Accordion Serenade",
                Location = "Maple Residential Lane",
                Description = "Musician Lyric was playing cheerful folk tunes after 9 PM. Half the neighbors brought cocoa to listen; Cornelius Von Monocle demanded arrest.",
                Severity = "Medium",
                Resolved = false,
                AssignedOfficer = "Chief Barnaby",
                RewardBudget = 110,
                SafetyImpact = 5,
                HappinessImpact = 6
            });
            Incidents.Add(new PoliceIncident
            {
                Id = "inc_3",
                Title = "Duckling Crosswalk Jam",
                Location = "Town Square Fountain",
                Description = "Mama Mallard and eight fuzzy ducklings are refusing to leave the warm cobblestones in front of City Hall.",
                Severity = "Low (Cozy)",
                Resolved = false,
                AssignedOfficer = "Officer Maple",
                RewardBudget = 85,
                SafetyImpact = 4,
                HappinessImpact = 6
            });
        }

        private void InitializeCitizens()
        {
            Citizens.Clear();
            Citizens.Add(new CitizenNPC { Id = "npc_pippin", Name = "Baker Pippin", Title = "Master Baker", Scene = GameScene.TownExterior, X = 115, Y = 155, HomeX = 115, HomeY = 155, PrimaryColor = Color.FromArgb(235, 145, 70), HairColor = Color.FromArgb(110, 65, 35), HasHat = true, HasTaxBillPending = true, TaxBillId = "tax_1" });
            Citizens.Add(new CitizenNPC { Id = "npc_hazel", Name = "Barista Hazel", Title = "Coffee Roaster", Scene = GameScene.TownExterior, X = 235, Y = 175, HomeX = 235, HomeY = 175, PrimaryColor = Color.FromArgb(145, 85, 55), HairColor = Color.FromArgb(65, 35, 25), HasHat = false, HasTaxBillPending = true, TaxBillId = "tax_2" });
            Citizens.Add(new CitizenNPC { Id = "npc_daisy", Name = "Florist Daisy", Title = "Town Botanist", Scene = GameScene.TownExterior, X = 415, Y = 220, HomeX = 415, HomeY = 220, PrimaryColor = Color.FromArgb(235, 110, 150), HairColor = Color.FromArgb(230, 190, 70), HasHat = false, HasTaxBillPending = true, TaxBillId = "tax_3" });
            Citizens.Add(new CitizenNPC { Id = "npc_oak", Name = "Elder Oak", Title = "Town Historian", Scene = GameScene.TownExterior, X = 190, Y = 280, HomeX = 190, HomeY = 280, PrimaryColor = Color.FromArgb(75, 135, 85), HairColor = Color.FromArgb(215, 215, 220), HasHat = true, HasTaxBillPending = true, TaxBillId = "tax_4" });
            Citizens.Add(new CitizenNPC { Id = "npc_cornelius", Name = "Cornelius Von Monocle", Title = "Defeated Rival (0 Votes)", Scene = GameScene.TownExterior, X = 485, Y = 175, HomeX = 485, HomeY = 175, PrimaryColor = Color.FromArgb(95, 55, 125), HairColor = Color.FromArgb(60, 60, 70), HasHat = true, HasTaxBillPending = true, TaxBillId = "tax_5" });
            Citizens.Add(new CitizenNPC { Id = "npc_hugo", Name = "Master Builder Hugo", Title = "City Architect", Scene = GameScene.TownExterior, X = 355, Y = 145, HomeX = 355, HomeY = 145, PrimaryColor = Color.FromArgb(200, 130, 50), HairColor = Color.FromArgb(95, 60, 30), HasHat = true, HasTaxBillPending = false });

            Citizens.Add(new CitizenNPC { Id = "npc_penny", Name = "Penny Ledger", Title = "Chief Tax Assessor", Scene = GameScene.CityHallInterior, X = 175, Y = 145, HomeX = 175, HomeY = 145, PrimaryColor = Color.FromArgb(180, 75, 95), HairColor = Color.FromArgb(140, 65, 35), HasHat = false });
            Citizens.Add(new CitizenNPC { Id = "npc_gus", Name = "Groundskeeper Gus", Title = "Weather Engineer", Scene = GameScene.CityHallInterior, X = 465, Y = 155, HomeX = 465, HomeY = 155, PrimaryColor = Color.FromArgb(70, 145, 80), HairColor = Color.FromArgb(120, 90, 60), HasHat = true });

            Citizens.Add(new CitizenNPC { Id = "npc_barnaby", Name = "Chief Barnaby", Title = "Police Chief", Scene = GameScene.PoliceStationInterior, X = 260, Y = 140, HomeX = 260, HomeY = 140, PrimaryColor = Color.FromArgb(45, 80, 145), HairColor = Color.FromArgb(180, 185, 195), HasHat = true });
            Citizens.Add(new CitizenNPC { Id = "npc_maple", Name = "Officer Maple", Title = "Field Constable", Scene = GameScene.PoliceStationInterior, X = 395, Y = 185, HomeX = 395, HomeY = 185, PrimaryColor = Color.FromArgb(65, 110, 180), HairColor = Color.FromArgb(150, 75, 40), HasHat = true });

            Citizens.Add(new CitizenNPC { Id = "npc_clara", Name = "Clara Page", Title = "Head Librarian", Scene = GameScene.LibraryInterior, X = 320, Y = 145, HomeX = 320, HomeY = 145, PrimaryColor = Color.FromArgb(130, 80, 155), HairColor = Color.FromArgb(75, 45, 35), HasHat = false });
        }

        public void InitializeSchedule()
        {
            DailySchedule.Clear();
            DailySchedule.Add(new ScheduleItem { TimeSlot = "08:00 AM", Title = "Log into Mayor's Desk Computer", Description = "Inspect Employees, Police & Taxes at your City Hall desk", Completed = false, RewardGold = 50 });
            DailySchedule.Add(new ScheduleItem { TimeSlot = "09:30 AM", Title = "Declare Day Type & Weather", Description = "Choose what kind of day it is & set town weather", Completed = false, RewardGold = 50 });
            DailySchedule.Add(new ScheduleItem { TimeSlot = "11:00 AM", Title = "Deliver Property Tax Bills", Description = "Walk around town and hand property tax bills to citizens", Completed = false, RewardGold = 100 });
            DailySchedule.Add(new ScheduleItem { TimeSlot = "01:30 PM", Title = "Resolve a Police Incident", Description = "Handle an incident via Computer or at the Police Precinct", Completed = false, RewardGold = 80 });
            DailySchedule.Add(new ScheduleItem { TimeSlot = "03:30 PM", Title = "Read a Book at the Library", Description = "Visit the Public Library and read any volume", Completed = false, RewardGold = 75 });
            DailySchedule.Add(new ScheduleItem { TimeSlot = "05:00 PM", Title = "Build or Upgrade a City Plot", Description = "Construct a cozy building on any town lot", Completed = false, RewardGold = 120 });
        }

        public void CompleteScheduleTask(int index)
        {
            if (index < 0 || index >= DailySchedule.Count) return;
            var item = DailySchedule[index];
            if (!item.Completed)
            {
                item.Completed = true;
                CityTreasury += item.RewardGold;
                CozyScore += 10;
                PhoneNotification = "Schedule Done: " + item.Title + " (+$" + item.RewardGold + ")";
                AddFloatingText("+$" + item.RewardGold + " Schedule Goal!", PlayerX, PlayerY - 18, Color.FromArgb(255, 225, 80));
                Audio.Play("coin");
            }
        }

        public void StartNewGameWithElection()
        {
            CurrentScene = GameScene.ElectionCutscene;
            ActiveOverlay = OverlayMode.None;
            Election.Reset();
            DialogueQueue.Clear();
            ActiveDialogue = null;

            EnqueueDialogue(new DialogueMessage
            {
                Speaker = "Election Commissioner Penny",
                Title = "Maplewood Valley Election Day",
                Text = "Welcome citizens! Today we elect the Mayor of Maplewood Valley! Let's hear final remarks from rival candidate Cornelius Von Monocle and our cozy favorite, Candidate Maple!",
                PortraitColor = Color.FromArgb(180, 75, 95)
            });

            EnqueueDialogue(new DialogueMessage
            {
                Speaker = "Cornelius Von Monocle",
                Title = "Pompous Rival Candidate",
                Text = "Hmph! Adjusts monocle. If elected Mayor, I shall pave over all flowerbeds, outlaw afternoon naps, and impose a 500% tax on warm pastries!",
                PortraitColor = Color.FromArgb(95, 55, 125)
            });

            var debateMsg = new DialogueMessage
            {
                Speaker = MayorName,
                Title = "Your Campaign Promise",
                Text = "What cozy vision will you promise the citizens of Maplewood Valley before the ballots are cast?",
                PortraitColor = Color.FromArgb(65, 155, 95)
            };
            debateMsg.Choices.Add(new DialogueChoice
            {
                Text = "1. 'Fair property taxes, lush parks, & warm pastries for all!' (+Happiness)",
                OnSelect = () =>
                {
                    Happiness = Math.Min(100, Happiness + 6);
                    Election.ChosenPlatform = "Fair Taxes & Lush Cozy Parks";
                    Election.StartBallotCounting();
                }
            });
            debateMsg.Choices.Add(new DialogueChoice
            {
                Text = "2. 'Friendly community policing, great libraries, & sunny skies!' (+Safety & Cozy)",
                OnSelect = () =>
                {
                    SafetyRating = Math.Min(100, SafetyRating + 6);
                    CozyScore += 20;
                    Election.ChosenPlatform = "Friendly Policing & Cozy Libraries";
                    Election.StartBallotCounting();
                }
            });
            debateMsg.Choices.Add(new DialogueChoice
            {
                Text = "3. 'Build a thriving city where every neighbor's voice matters!' (+Treasury Grant)",
                OnSelect = () =>
                {
                    CityTreasury += 250;
                    Election.ChosenPlatform = "Thriving Cozy Civic Growth";
                    Election.StartBallotCounting();
                }
            });

            EnqueueDialogue(debateMsg);
        }

        public void TransitionFromElectionToCityHallDesk()
        {
            CurrentScene = GameScene.CityHallInterior;
            PlayerX = 320f;
            PlayerY = 132f;
            PlayerFacing = 1;
            ActiveOverlay = OverlayMode.MayorComputer;
            ComputerTabIndex = 0;
            CompleteScheduleTask(0);
            AI.GenerateMayorBriefing(CurrentDay, CityTreasury, Happiness, SafetyRating, CozyScore, CurrentWeather, CurrentDayType, GetPendingTaxBillCount(), GetOpenIncidentCount(), GetBuiltPlotCount());
            PhoneNotification = "Elected 12-0! Seated at Mayor's Computer in City Hall.";
            Audio.Play("build");
        }

        public void EnqueueDialogue(DialogueMessage msg)
        {
            DialogueQueue.Enqueue(msg);
            if (ActiveDialogue == null)
            {
                AdvanceDialogue();
            }
        }

        public void AdvanceDialogue()
        {
            if (ActiveDialogue != null && ActiveDialogue.OnFinish != null)
            {
                var cb = ActiveDialogue.OnFinish;
                ActiveDialogue.OnFinish = null;
                cb.Invoke();
            }

            if (DialogueQueue.Count > 0)
            {
                ActiveDialogue = DialogueQueue.Dequeue();
                SelectedChoiceIndex = 0;
                DialogueCharIndex = 0f;
                Audio.Play("blip");
            }
            else
            {
                ActiveDialogue = null;
            }
        }

        public void SetWeather(WeatherType weather)
        {
            CurrentWeather = weather;
            InitWeatherParticles();
            CompleteScheduleTask(1);
            Audio.Play("weather");
            PhoneNotification = "Weather set to " + weather + "!";
            AddFloatingText("Weather: " + weather, PlayerX, PlayerY - 20, Color.FromArgb(120, 220, 255));
            Library.RefreshAIChronicle(MayorName, CurrentDay, CityTreasury, Happiness, SafetyRating, CozyScore, CurrentWeather, CurrentDayType, TaxBillsDeliveredTotal, IncidentsResolvedTotal, BuildingPlots);
        }

        public void SetDayType(DayType dayType)
        {
            CurrentDayType = dayType;
            CompleteScheduleTask(1);
            Audio.Play("weather");
            PhoneNotification = "Proclaimed " + dayType + "!";
            AddFloatingText("Day Type: " + dayType + "!", PlayerX, PlayerY - 26, Color.FromArgb(255, 215, 80));
            Library.RefreshAIChronicle(MayorName, CurrentDay, CityTreasury, Happiness, SafetyRating, CozyScore, CurrentWeather, CurrentDayType, TaxBillsDeliveredTotal, IncidentsResolvedTotal, BuildingPlots);
        }

        public void DeliverTaxBillWithOption(PropertyTaxBill bill, CitizenNPC npc, int optionMode)
        {
            if (bill == null || bill.Delivered) return;
            bill.Delivered = true;
            bill.Paid = true;
            if (npc != null) npc.HasTaxBillPending = false;

            float dayMultiplier = CurrentDayType == DayType.TaxDay ? 1.25f : 1.0f;
            int collected = (int)(bill.AmountDue * dayMultiplier);

            if (optionMode == 0)
            {
                CityTreasury += collected;
                AddFloatingText("+$" + collected + " Property Tax!", PlayerX, PlayerY - 20, Color.FromArgb(110, 245, 130));
            }
            else if (optionMode == 1)
            {
                collected = (int)(collected * 0.85f);
                CityTreasury += collected;
                Happiness = Math.Min(100, Happiness + 4);
                CozyScore += 8;
                AddFloatingText("+$" + collected + " & +4% Happy!", PlayerX, PlayerY - 20, Color.FromArgb(255, 210, 90));
            }
            else
            {
                collected = (int)(collected * 0.65f);
                CityTreasury += collected;
                Happiness = Math.Min(100, Happiness + 7);
                CozyScore += 15;
                AddFloatingText("+$" + collected + " & +15 Cozy!", PlayerX, PlayerY - 20, Color.FromArgb(255, 165, 220));
            }

            TaxBillsDeliveredTotal++;
            CompleteScheduleTask(2);
            Audio.Play("coin");
            Input.TriggerRumble(18000, 26000, 0.15f);
            PhoneNotification = "Tax collected from " + bill.OwnerName + " ($" + collected + ")";
            Library.RefreshAIChronicle(MayorName, CurrentDay, CityTreasury, Happiness, SafetyRating, CozyScore, CurrentWeather, CurrentDayType, TaxBillsDeliveredTotal, IncidentsResolvedTotal, BuildingPlots);
        }

        public void ResolvePoliceIncident(PoliceIncident inc, int strategy)
        {
            if (inc == null || inc.Resolved) return;
            inc.Resolved = true;
            IncidentsResolvedTotal++;

            int safetyBonus = CurrentDayType == DayType.CommunitySafetyDay ? 4 : 0;

            if (strategy == 0)
            {
                inc.ResolutionSummary = "Resolved via Cozy Mediation & warm pastries.";
                CityTreasury += inc.RewardBudget;
                SafetyRating = Math.Min(100, SafetyRating + inc.SafetyImpact + safetyBonus);
                Happiness = Math.Min(100, Happiness + inc.HappinessImpact);
                CozyScore += 12;
            }
            else if (strategy == 1)
            {
                inc.ResolutionSummary = "Solved by Precinct Detective Unit.";
                CityTreasury += inc.RewardBudget + 25;
                SafetyRating = Math.Min(100, SafetyRating + inc.SafetyImpact + 3 + safetyBonus);
            }
            else
            {
                inc.ResolutionSummary = "Issued Strict Municipal Fine.";
                CityTreasury += inc.RewardBudget + 75;
                SafetyRating = Math.Min(100, SafetyRating + inc.SafetyImpact + safetyBonus);
                Happiness = Math.Max(40, Happiness - 2);
            }

            CompleteScheduleTask(3);
            Audio.Play("police");
            Input.TriggerRumble(22000, 14000, 0.14f);
            PhoneNotification = "Incident Resolved: " + inc.Title;
            AddFloatingText("Police Case Closed! +$" + inc.RewardBudget, PlayerX, PlayerY - 20, Color.FromArgb(100, 200, 255));
            Library.RefreshAIChronicle(MayorName, CurrentDay, CityTreasury, Happiness, SafetyRating, CozyScore, CurrentWeather, CurrentDayType, TaxBillsDeliveredTotal, IncidentsResolvedTotal, BuildingPlots);
        }

        public void ConstructOrUpgradePlot(CityBuildingPlot plot, BuildingType newType)
        {
            if (plot == null) return;
            int baseCost = GetBuildingBaseCost(newType);
            if (CurrentDayType == DayType.CivicConstructionDay)
                baseCost = (int)(baseCost * 0.8f);

            if (CityTreasury < baseCost)
            {
                PhoneNotification = "Need $" + baseCost + " in Treasury to build " + newType + "!";
                Audio.Play("blip");
                return;
            }

            CityTreasury -= baseCost;
            if (plot.CurrentBuilding == newType)
            {
                plot.Level++;
            }
            else
            {
                plot.CurrentBuilding = newType;
                plot.Level = 1;
            }

            plot.DailyTaxRevenue = (45 + ((int)newType * 15)) * plot.Level;
            plot.CozyBonus = (12 + ((int)newType * 4)) * plot.Level;
            CozyScore += 20;
            Happiness = Math.Min(100, Happiness + 4);

            CompleteScheduleTask(5);
            Audio.Play("build");
            Input.TriggerRumble(30000, 30000, 0.22f);
            PhoneNotification = "Built " + newType + " (Lv." + plot.Level + ") on " + plot.PlotName + "!";
            AddFloatingText("Built " + newType + "!", PlayerX, PlayerY - 20, Color.FromArgb(255, 220, 90));
            Library.RefreshAIChronicle(MayorName, CurrentDay, CityTreasury, Happiness, SafetyRating, CozyScore, CurrentWeather, CurrentDayType, TaxBillsDeliveredTotal, IncidentsResolvedTotal, BuildingPlots);
        }

        public int GetBuildingBaseCost(BuildingType type)
        {
            switch (type)
            {
                case BuildingType.CozyBakery: return 220;
                case BuildingType.MapleCoffeeShop: return 240;
                case BuildingType.CommunityPark: return 180;
                case BuildingType.SolarGarden: return 280;
                case BuildingType.TownClinic: return 300;
                case BuildingType.ArtisanBookstore: return 210;
                case BuildingType.CozyApartments: return 320;
                case BuildingType.RetroArcade: return 260;
                default: return 200;
            }
        }

        public void AdvanceToNextDay()
        {
            CurrentDay++;
            DayClockMinutes = 8 * 60f;

            int passiveIncome = 0;
            foreach (var p in BuildingPlots)
            {
                if (p.CurrentBuilding != BuildingType.EmptyLot)
                    passiveIncome += p.DailyTaxRevenue;
            }

            int salaries = 0;
            foreach (var e in Employees) salaries += e.Salary / 3;

            int net = Math.Max(80, passiveIncome - salaries + 120);
            CityTreasury += net;

            GenerateDailyTaxBills();
            InitializeSchedule();

            string rawInc = AI.GenerateDynamicIncident(CurrentDay, CurrentWeather);
            string[] parts = rawInc.Split('|');
            if (parts.Length >= 4)
            {
                Incidents.Insert(0, new PoliceIncident
                {
                    Id = "inc_day_" + CurrentDay + "_" + _rng.Next(100, 999),
                    Title = parts[0],
                    Location = parts[1],
                    Description = parts[2],
                    Severity = parts[3],
                    Resolved = false,
                    AssignedOfficer = _rng.Next(2) == 0 ? "Chief Barnaby" : "Officer Maple",
                    RewardBudget = 105,
                    SafetyImpact = 5,
                    HappinessImpact = 5
                });
            }

            AI.GenerateMayorBriefing(CurrentDay, CityTreasury, Happiness, SafetyRating, CozyScore, CurrentWeather, CurrentDayType, GetPendingTaxBillCount(), GetOpenIncidentCount(), GetBuiltPlotCount());
            Audio.Play("fanfare");
            PhoneNotification = "Day " + CurrentDay + " Begun! Net Civic Revenue: +$" + net;
            AddFloatingText("Welcome to Day " + CurrentDay + "! (+$" + net + ")", PlayerX, PlayerY - 24, Color.FromArgb(255, 230, 100));
        }

        public int GetPendingTaxBillCount()
        {
            int c = 0;
            foreach (var b in TaxBills) if (!b.Delivered) c++;
            return c;
        }

        public int GetOpenIncidentCount()
        {
            int c = 0;
            foreach (var inc in Incidents) if (!inc.Resolved) c++;
            return c;
        }

        public int GetBuiltPlotCount()
        {
            int c = 0;
            foreach (var p in BuildingPlots) if (p.CurrentBuilding != BuildingType.EmptyLot) c++;
            return c;
        }

        public void AddFloatingText(string text, float x, float y, Color color)
        {
            float staggerY = y - (FloatingTexts.Count * 14f);
            FloatingTexts.Add(new FloatingText { Text = text, X = x, Y = staggerY, Timer = 1.6f, Color = color });
        }

        public void InitWeatherParticles()
        {
            WeatherParticles.Clear();
            int count = CurrentWeather == WeatherType.CozyRain ? 65 : 40;
            for (int i = 0; i < count; i++)
            {
                WeatherParticles.Add(CreateWeatherParticle(true));
            }
        }

        private WeatherParticle CreateWeatherParticle(bool randomY)
        {
            var p = new WeatherParticle
            {
                X = _rng.Next(0, 640),
                Y = randomY ? _rng.Next(0, 360) : -10,
                Variant = _rng.Next(0, 3)
            };

            switch (CurrentWeather)
            {
                case WeatherType.SunnyWarm:
                    p.VX = (float)(_rng.NextDouble() * 16 - 8);
                    p.VY = (float)(_rng.NextDouble() * 12 - 6);
                    p.Size = _rng.Next(2, 4);
                    p.Tint = Color.FromArgb(255, 245, 160);
                    break;
                case WeatherType.CozyRain:
                    p.VX = -22f;
                    p.VY = _rng.Next(150, 210);
                    p.Size = _rng.Next(4, 7);
                    p.Tint = Color.FromArgb(150, 205, 255);
                    break;
                case WeatherType.AutumnBreeze:
                    p.VX = _rng.Next(25, 55);
                    p.VY = _rng.Next(25, 50);
                    p.Size = _rng.Next(3, 6);
                    p.Tint = p.Variant == 0 ? Color.FromArgb(235, 110, 45) : Color.FromArgb(240, 175, 55);
                    break;
                case WeatherType.GentleSnow:
                    p.VX = (float)(_rng.NextDouble() * 24 - 12);
                    p.VY = _rng.Next(25, 48);
                    p.Size = _rng.Next(2, 5);
                    p.Tint = Color.FromArgb(240, 248, 255);
                    break;
                case WeatherType.StarlightGlow:
                    p.VX = (float)(_rng.NextDouble() * 14 - 7);
                    p.VY = (float)(_rng.NextDouble() * -15 - 4);
                    p.Size = _rng.Next(2, 4);
                    p.Tint = Color.FromArgb(190, 255, 160);
                    break;
            }
            return p;
        }

        public void UpdateWeatherParticles(float dt)
        {
            for (int i = 0; i < WeatherParticles.Count; i++)
            {
                var p = WeatherParticles[i];
                p.X += p.VX * dt;
                p.Y += p.VY * dt;
                if (p.Y > 365 || p.Y < -20 || p.X > 650 || p.X < -20)
                {
                    WeatherParticles[i] = CreateWeatherParticle(false);
                    if (CurrentWeather == WeatherType.StarlightGlow)
                        WeatherParticles[i].Y = 355;
                }
            }
        }

        public void SaveGame()
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("MayorName=" + MayorName);
                sb.AppendLine("CurrentDay=" + CurrentDay);
                sb.AppendLine("CityTreasury=" + CityTreasury);
                sb.AppendLine("Happiness=" + Happiness);
                sb.AppendLine("SafetyRating=" + SafetyRating);
                sb.AppendLine("CozyScore=" + CozyScore);
                sb.AppendLine("PropertyTaxRate=" + PropertyTaxRate);
                sb.AppendLine("BooksReadCount=" + BooksReadCount);
                sb.AppendLine("TaxBillsDeliveredTotal=" + TaxBillsDeliveredTotal);
                sb.AppendLine("IncidentsResolvedTotal=" + IncidentsResolvedTotal);
                sb.AppendLine("CurrentWeather=" + (int)CurrentWeather);
                sb.AppendLine("CurrentDayType=" + (int)CurrentDayType);
                for (int i = 0; i < BuildingPlots.Count; i++)
                {
                    sb.AppendLine("Plot_" + i + "=" + (int)BuildingPlots[i].CurrentBuilding + "," + BuildingPlots[i].Level);
                }
                File.WriteAllText(_saveFilePath, sb.ToString());
                PhoneNotification = "Game Saved Successfully!";
                AddFloatingText("Game Saved!", PlayerX, PlayerY - 22, Color.FromArgb(120, 255, 160));
                Audio.Play("coin");
            }
            catch (Exception ex)
            {
                PhoneNotification = "Save Notice: " + ex.Message;
            }
        }

        public bool LoadGame()
        {
            try
            {
                if (!File.Exists(_saveFilePath)) return false;
                string[] lines = File.ReadAllLines(_saveFilePath);
                int parsedInt;
                foreach (string line in lines)
                {
                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;
                    string k = line.Substring(0, eq).Trim();
                    string v = line.Substring(eq + 1).Trim();
                    switch (k)
                    {
                        case "MayorName": MayorName = v; break;
                        case "CurrentDay": if (int.TryParse(v, out parsedInt)) CurrentDay = Math.Max(1, parsedInt); break;
                        case "CityTreasury": if (int.TryParse(v, out parsedInt)) CityTreasury = parsedInt; break;
                        case "Happiness": if (int.TryParse(v, out parsedInt)) Happiness = parsedInt; break;
                        case "SafetyRating": if (int.TryParse(v, out parsedInt)) SafetyRating = parsedInt; break;
                        case "CozyScore": if (int.TryParse(v, out parsedInt)) CozyScore = parsedInt; break;
                        case "PropertyTaxRate": if (int.TryParse(v, out parsedInt)) PropertyTaxRate = parsedInt; break;
                        case "BooksReadCount": if (int.TryParse(v, out parsedInt)) BooksReadCount = parsedInt; break;
                        case "TaxBillsDeliveredTotal": if (int.TryParse(v, out parsedInt)) TaxBillsDeliveredTotal = parsedInt; break;
                        case "IncidentsResolvedTotal": if (int.TryParse(v, out parsedInt)) IncidentsResolvedTotal = parsedInt; break;
                        case "CurrentWeather":
                            if (int.TryParse(v, out parsedInt)) CurrentWeather = (WeatherType)(parsedInt % 5);
                            break;
                        case "CurrentDayType":
                            if (int.TryParse(v, out parsedInt)) CurrentDayType = (DayType)(parsedInt % 5);
                            break;
                        default:
                            if (k.StartsWith("Plot_"))
                            {
                                int pIdx;
                                if (int.TryParse(k.Substring(5), out pIdx) && pIdx >= 0 && pIdx < BuildingPlots.Count)
                                {
                                    string[] pts = v.Split(',');
                                    int bt, lv;
                                    if (pts.Length == 2 && int.TryParse(pts[0], out bt) && int.TryParse(pts[1], out lv))
                                    {
                                        BuildingPlots[pIdx].CurrentBuilding = (BuildingType)bt;
                                        BuildingPlots[pIdx].Level = lv;
                                        BuildingPlots[pIdx].DailyTaxRevenue = bt == 0 ? 0 : (45 + bt * 15) * lv;
                                        BuildingPlots[pIdx].CozyBonus = bt == 0 ? 0 : (12 + bt * 4) * lv;
                                    }
                                }
                            }
                            break;
                    }
                }
                InitWeatherParticles();
                Library.RefreshAIChronicle(MayorName, CurrentDay, CityTreasury, Happiness, SafetyRating, CozyScore, CurrentWeather, CurrentDayType, TaxBillsDeliveredTotal, IncidentsResolvedTotal, BuildingPlots);
                PhoneNotification = "Loaded Mayor " + MayorName + " (Day " + CurrentDay + ")!";
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool HasSaveFile() { return File.Exists(_saveFilePath); }
    }
}
