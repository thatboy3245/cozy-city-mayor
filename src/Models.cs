using System;
using System.Collections.Generic;
using System.Drawing;

namespace CozyCityMayor
{
    public enum GameScene
    {
        TitleScreen,
        ElectionCutscene,
        TownExterior,
        CityHallInterior,
        PoliceStationInterior,
        LibraryInterior
    }

    public enum OverlayMode
    {
        None,
        MayorComputer,
        LibraryReader,
        BuildMenu,
        PauseSettings,
        HelpControls
    }

    public enum WeatherType
    {
        SunnyWarm,
        CozyRain,
        AutumnBreeze,
        GentleSnow,
        StarlightGlow
    }

    public enum DayType
    {
        TaxDay,
        CozyFestivalDay,
        CivicConstructionDay,
        CommunitySafetyDay,
        LibraryLiteracyDay
    }

    public enum BuildingType
    {
        EmptyLot,
        CozyBakery,
        MapleCoffeeShop,
        CommunityPark,
        SolarGarden,
        TownClinic,
        ArtisanBookstore,
        CozyApartments,
        RetroArcade
    }

    public class ClickRegion
    {
        public Rectangle Rect { get; set; }
        public Action OnClick { get; set; }

        public ClickRegion(Rectangle rect, Action onClick)
        {
            Rect = rect;
            OnClick = onClick;
        }
    }

    public class SynthNote
    {
        public double Freq;
        public double DurationSec;
        public double Volume;

        public SynthNote(double freq, double durationSec, double volume)
        {
            Freq = freq;
            DurationSec = durationSec;
            Volume = volume;
        }
    }

    public class Employee
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Department { get; set; }
        public int Salary { get; set; }
        public int Morale { get; set; }
        public int Efficiency { get; set; }
        public string CurrentAssignment { get; set; }
        public string Bio { get; set; }
        public string AIInsight { get; set; }
        public Color ShirtColor { get; set; }
    }

    public class PoliceIncident
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public string Severity { get; set; }
        public bool Resolved { get; set; }
        public string ResolutionSummary { get; set; }
        public string AssignedOfficer { get; set; }
        public int RewardBudget { get; set; }
        public int SafetyImpact { get; set; }
        public int HappinessImpact { get; set; }
    }

    public class PropertyTaxBill
    {
        public string Id { get; set; }
        public string PropertyName { get; set; }
        public string OwnerName { get; set; }
        public int AmountDue { get; set; }
        public bool Delivered { get; set; }
        public bool Paid { get; set; }
        public string Notes { get; set; }
        public int HouseIndex { get; set; }
    }

    public class CityBuildingPlot
    {
        public int PlotIndex { get; set; }
        public string PlotName { get; set; }
        public Rectangle WorldRect { get; set; }
        public BuildingType CurrentBuilding { get; set; }
        public int Level { get; set; }
        public int DailyTaxRevenue { get; set; }
        public int CozyBonus { get; set; }
    }

    public class CitizenNPC
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public GameScene Scene { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float HomeX { get; set; }
        public float HomeY { get; set; }
        public int Facing { get; set; }
        public Color PrimaryColor { get; set; }
        public Color HairColor { get; set; }
        public bool HasHat { get; set; }
        public bool HasTaxBillPending { get; set; }
        public string TaxBillId { get; set; }
        public float WanderTimer { get; set; }
        public float MoveDX { get; set; }
        public float MoveDY { get; set; }
    }

    public class ScheduleItem
    {
        public string TimeSlot { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool Completed { get; set; }
        public int RewardGold { get; set; }
    }

    public class BookData
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public Color CoverColor { get; set; }
        public List<string> Pages { get; set; }
        public bool HasBeenRead { get; set; }

        public BookData()
        {
            Pages = new List<string>();
        }
    }

    public class DialogueChoice
    {
        public string Text { get; set; }
        public Action OnSelect { get; set; }
    }

    public class DialogueMessage
    {
        public string Speaker { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public Color PortraitColor { get; set; }
        public List<DialogueChoice> Choices { get; set; }
        public Action OnFinish { get; set; }

        public DialogueMessage()
        {
            Choices = new List<DialogueChoice>();
        }
    }

    public class WeatherParticle
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float VX { get; set; }
        public float VY { get; set; }
        public float Size { get; set; }
        public float Alpha { get; set; }
        public Color Tint { get; set; }
        public int Variant { get; set; }
    }

    public class FloatingText
    {
        public string Text { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Timer { get; set; }
        public Color Color { get; set; }
    }
}
