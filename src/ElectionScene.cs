using System;
using System.Collections.Generic;
using System.Drawing;

namespace CozyCityMayor
{
    public class ElectionScene
    {
        public int Phase { get; set; }
        public int PlayerVotes { get; set; }
        public int RivalVotes { get; set; }
        public int TotalVoters { get; private set; }
        public float VoteTimer { get; set; }
        public string LastVoterAnnouncement { get; set; }
        public string ChosenPlatform { get; set; }

        private readonly string[] _voterNames = new[]
        {
            "Baker Pippin ('For warm croissants & fair taxes!')",
            "Librarian Clara ('For the Public Library & literacy!')",
            "Police Chief Barnaby ('For cozy community safety!')",
            "Officer Maple ('Reporting for a cozy town!')",
            "Tax Assessor Penny ('City Hall is ready for you!')",
            "Barista Hazel ('Free espresso for our new Mayor!')",
            "Master Builder Hugo ('Let's build a dream city!')",
            "Florist Daisy ('More gardens and sunny weather!')",
            "Elder Oak ('Finest candidate since 1888!')",
            "Musician Lyric ('Festivals and music for all!')",
            "Young Toby ('Yay! No puppy tax!')",
            "Campaign Manager Fitz ('Sorry Cornelius... I'm voting for Mayor Maple too!')"
        };

        public List<WeatherParticle> Confetti { get; private set; }
        private readonly Random _rng = new Random();

        public ElectionScene()
        {
            TotalVoters = 12;
            Confetti = new List<WeatherParticle>();
            LastVoterAnnouncement = "The polls are opening in Maplewood Town Square!";
            ChosenPlatform = "Cozy Parks, Fair Taxes & Warm Community";
        }

        public void Reset()
        {
            Phase = 0;
            PlayerVotes = 0;
            RivalVotes = 0;
            VoteTimer = 0f;
            LastVoterAnnouncement = "All 12 citizens of Maplewood Valley have gathered at the Ballot Box!";
            Confetti.Clear();
        }

        public void StartBallotCounting()
        {
            Phase = 1;
            PlayerVotes = 0;
            RivalVotes = 0;
            VoteTimer = 0.35f;
            SpawnConfettiBurst(35);
        }

        public void Update(float dt, AudioSystem audio, InputSystem input, Action onVictoryReady)
        {
            for (int i = Confetti.Count - 1; i >= 0; i--)
            {
                var p = Confetti[i];
                p.X += p.VX * dt;
                p.Y += p.VY * dt;
                p.VY += 45f * dt;
                if (p.Y > 380)
                {
                    if (Phase == 2)
                    {
                        p.Y = -10;
                        p.X = _rng.Next(20, 620);
                        p.VY = _rng.Next(30, 85);
                    }
                    else
                    {
                        Confetti.RemoveAt(i);
                    }
                }
            }

            if (Phase == 1)
            {
                VoteTimer -= dt;
                if (VoteTimer <= 0f && PlayerVotes < TotalVoters)
                {
                    LastVoterAnnouncement = "Ballot #" + (PlayerVotes + 1) + ": " + _voterNames[PlayerVotes];
                    PlayerVotes++;
                    audio.Play("coin");
                    input.TriggerRumble(12000, 18000, 0.08f);
                    SpawnConfettiBurst(12);
                    VoteTimer = 0.42f;

                    if (PlayerVotes >= TotalVoters)
                    {
                        Phase = 2;
                        audio.Play("fanfare");
                        input.TriggerRumble(38000, 48000, 0.45f);
                        SpawnConfettiBurst(120);
                        LastVoterAnnouncement = "UNANIMOUS LANDSLIDE! 12 Votes to 0! Everybody voted for YOU!";
                        if (onVictoryReady != null) onVictoryReady();
                    }
                }
            }
        }

        public void SpawnConfettiBurst(int count)
        {
            Color[] colors = new[]
            {
                Color.FromArgb(255, 215, 60),
                Color.FromArgb(255, 95, 110),
                Color.FromArgb(80, 210, 255),
                Color.FromArgb(110, 235, 130),
                Color.FromArgb(230, 140, 255)
            };

            for (int i = 0; i < count; i++)
            {
                Confetti.Add(new WeatherParticle
                {
                    X = _rng.Next(40, 600),
                    Y = _rng.Next(10, 160),
                    VX = (float)(_rng.NextDouble() * 120.0 - 60.0),
                    VY = (float)(_rng.NextDouble() * -90.0 + 20.0),
                    Size = _rng.Next(3, 6),
                    Tint = colors[_rng.Next(colors.Length)]
                });
            }
        }
    }
}
