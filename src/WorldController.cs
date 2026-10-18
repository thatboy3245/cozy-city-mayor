using System;
using System.Collections.Generic;
using System.Drawing;

namespace CozyCityMayor
{
    public class WorldController
    {
        private readonly GameStateEngine _game;

        public WorldController(GameStateEngine game)
        {
            _game = game;
        }

        public void Update(float dt)
        {
            _game.Input.Update(dt);

            if (_game.CurrentScene != GameScene.TitleScreen && _game.CurrentScene != GameScene.ElectionCutscene)
            {
                _game.DayClockMinutes += dt * 2.2f;
                if (_game.DayClockMinutes >= 22 * 60f)
                {
                    _game.DayClockMinutes = 8 * 60f;
                    _game.AdvanceToNextDay();
                }
            }

            for (int i = _game.FloatingTexts.Count - 1; i >= 0; i--)
            {
                var ft = _game.FloatingTexts[i];
                ft.Y -= 16f * dt;
                ft.Timer -= dt;
                if (ft.Timer <= 0f) _game.FloatingTexts.RemoveAt(i);
            }

            _game.UpdateWeatherParticles(dt);

            if (_game.CurrentScene == GameScene.TitleScreen)
            {
                UpdateTitleScreen();
                return;
            }

            if (_game.CurrentScene == GameScene.ElectionCutscene)
            {
                UpdateElectionScene(dt);
                return;
            }

            if (_game.ActiveDialogue != null)
            {
                UpdateDialogue(dt);
                return;
            }

            if (_game.Input.PhoneTogglePressed)
            {
                _game.PhoneExpanded = !_game.PhoneExpanded;
                _game.Audio.Play("click");
            }

            if (_game.Input.WeatherCyclePressed && _game.ActiveOverlay == OverlayMode.None)
            {
                int nextW = ((int)_game.CurrentWeather + 1) % 5;
                _game.SetWeather((WeatherType)nextW);
            }

            if (_game.Input.DayTypeCyclePressed && _game.ActiveOverlay == OverlayMode.None)
            {
                int nextD = ((int)_game.CurrentDayType + 1) % 5;
                _game.SetDayType((DayType)nextD);
            }

            if (_game.Input.ComputerQuickPressed)
            {
                if (_game.ActiveOverlay == OverlayMode.MayorComputer)
                    _game.ActiveOverlay = OverlayMode.None;
                else
                {
                    _game.ActiveOverlay = OverlayMode.MayorComputer;
                    _game.CompleteScheduleTask(0);
                }
                _game.Audio.Play("click");
                return;
            }

            if (_game.Input.BuildMenuPressed)
            {
                if (_game.ActiveOverlay == OverlayMode.BuildMenu)
                    _game.ActiveOverlay = OverlayMode.None;
                else
                    _game.ActiveOverlay = OverlayMode.BuildMenu;
                _game.Audio.Play("click");
                return;
            }

            if (_game.ActiveOverlay != OverlayMode.None)
            {
                UpdateOverlayInput();
                return;
            }

            if (_game.Input.PausePressed)
            {
                _game.ActiveOverlay = OverlayMode.PauseSettings;
                _game.Audio.Play("click");
                return;
            }

            UpdatePlayerMovement(dt);
            UpdateNPCs(dt);

            if (_game.Input.ConfirmPressed)
            {
                TryInteractInWorld();
            }
        }

        private void UpdateTitleScreen()
        {
            if (_game.Input.UpPressed)
            {
                _game.TitleMenuIndex = (_game.TitleMenuIndex + 2) % 3;
                _game.Audio.Play("click");
            }
            if (_game.Input.DownPressed)
            {
                _game.TitleMenuIndex = (_game.TitleMenuIndex + 1) % 3;
                _game.Audio.Play("click");
            }
            if (_game.Input.ConfirmPressed)
            {
                _game.Audio.Play("coin");
                if (_game.TitleMenuIndex == 0)
                {
                    _game.StartNewGameWithElection();
                }
                else if (_game.TitleMenuIndex == 1)
                {
                    if (_game.LoadGame())
                    {
                        _game.CurrentScene = GameScene.CityHallInterior;
                        _game.PlayerX = 320f;
                        _game.PlayerY = 140f;
                        _game.ActiveOverlay = OverlayMode.MayorComputer;
                    }
                    else
                    {
                        _game.StartNewGameWithElection();
                    }
                }
                else if (_game.TitleMenuIndex == 2)
                {
                    Environment.Exit(0);
                }
            }
        }

        private void UpdateElectionScene(float dt)
        {
            if (_game.ActiveDialogue != null)
            {
                UpdateDialogue(dt);
                return;
            }

            _game.Election.Update(dt, _game.Audio, _game.Input, () =>
            {
                _game.EnqueueDialogue(new DialogueMessage
                {
                    Speaker = "Election Commissioner Penny",
                    Title = "Official Ballot Result: 12 to 0!",
                    Text = "It's official! Every single citizen in Maplewood Valley voted for YOU! Even Cornelius's campaign manager voted for you!",
                    PortraitColor = Color.FromArgb(180, 75, 95)
                });
                _game.EnqueueDialogue(new DialogueMessage
                {
                    Speaker = "Cornelius Von Monocle",
                    Title = "Stunned Rival (0 Votes)",
                    Text = "Zero votes?! Even Fitz voted against me?! B-bah! Well, we'll see how you handle the Property Taxes and Police Incidents at City Hall!",
                    PortraitColor = Color.FromArgb(95, 55, 125)
                });
                _game.EnqueueDialogue(new DialogueMessage
                {
                    Speaker = "Chief Barnaby & Penny",
                    Title = "Welcome to City Hall, Mayor!",
                    Text = "Right this way into City Hall, Mayor! We've booted up your Desk Computer where you can check your Employees, Police Incidents, Property Tax Bills, Weather, and Day Type!",
                    PortraitColor = Color.FromArgb(45, 80, 145),
                    OnFinish = () => _game.TransitionFromElectionToCityHallDesk()
                });
            });
        }

        private void UpdateDialogue(float dt)
        {
            var dlg = _game.ActiveDialogue;
            if (dlg == null) return;

            _game.DialogueCharIndex += dt * 75f;
            bool fullTextShown = _game.DialogueCharIndex >= dlg.Text.Length;

            if (dlg.Choices.Count > 0 && fullTextShown)
            {
                if (_game.Input.UpPressed)
                {
                    _game.SelectedChoiceIndex = (_game.SelectedChoiceIndex + dlg.Choices.Count - 1) % dlg.Choices.Count;
                    _game.Audio.Play("click");
                }
                if (_game.Input.DownPressed)
                {
                    _game.SelectedChoiceIndex = (_game.SelectedChoiceIndex + 1) % dlg.Choices.Count;
                    _game.Audio.Play("click");
                }
                if (_game.Input.ConfirmPressed)
                {
                    var choice = dlg.Choices[_game.SelectedChoiceIndex];
                    _game.AdvanceDialogue();
                    if (choice.OnSelect != null) choice.OnSelect();
                }
            }
            else
            {
                if (_game.Input.ConfirmPressed || _game.Input.CancelPressed)
                {
                    if (!fullTextShown)
                    {
                        _game.DialogueCharIndex = dlg.Text.Length;
                    }
                    else
                    {
                        _game.AdvanceDialogue();
                    }
                }
            }
        }

        private void UpdateOverlayInput()
        {
            if (_game.Input.CancelPressed)
            {
                _game.ActiveOverlay = OverlayMode.None;
                _game.Audio.Play("click");
                return;
            }

            switch (_game.ActiveOverlay)
            {
                case OverlayMode.MayorComputer:
                    UpdateMayorComputerInput();
                    break;
                case OverlayMode.LibraryReader:
                    UpdateLibraryReaderInput();
                    break;
                case OverlayMode.BuildMenu:
                    UpdateBuildMenuInput();
                    break;
                case OverlayMode.PauseSettings:
                    UpdatePauseMenuInput();
                    break;
            }
        }

        private void UpdateMayorComputerInput()
        {
            if (_game.Input.TabPrevPressed || _game.Input.LeftPressed)
            {
                _game.ComputerTabIndex = (_game.ComputerTabIndex + 5) % 6;
                _game.ComputerSelectedRow = 0;
                _game.Audio.Play("click");
            }
            if (_game.Input.TabNextPressed || _game.Input.RightPressed)
            {
                _game.ComputerTabIndex = (_game.ComputerTabIndex + 1) % 6;
                _game.ComputerSelectedRow = 0;
                _game.Audio.Play("click");
            }

            int maxRows = GetComputerTabRowCount(_game.ComputerTabIndex);
            if (_game.Input.UpPressed && maxRows > 0)
            {
                _game.ComputerSelectedRow = (_game.ComputerSelectedRow + maxRows - 1) % maxRows;
                _game.Audio.Play("click");
            }
            if (_game.Input.DownPressed && maxRows > 0)
            {
                _game.ComputerSelectedRow = (_game.ComputerSelectedRow + 1) % maxRows;
                _game.Audio.Play("click");
            }

            if (_game.Input.ConfirmPressed)
            {
                ExecuteComputerAction(_game.ComputerTabIndex, _game.ComputerSelectedRow);
            }
        }

        private int GetComputerTabRowCount(int tab)
        {
            switch (tab)
            {
                case 0: return 4;
                case 1: return _game.Employees.Count;
                case 2: return _game.Incidents.Count;
                case 3: return 10;
                case 4: return _game.BuildingPlots.Count;
                case 5: return 4;
                default: return 1;
            }
        }

        public void ExecuteComputerAction(int tab, int row)
        {
            switch (tab)
            {
                case 0:
                    if (row == 0)
                    {
                        _game.PropertyTaxRate = _game.PropertyTaxRate >= 15 ? 5 : _game.PropertyTaxRate + 1;
                        _game.GenerateDailyTaxBills();
                        _game.Audio.Play("click");
                        _game.PhoneNotification = "Property Tax Rate set to " + _game.PropertyTaxRate + "%";
                    }
                    else if (row == 1)
                    {
                        _game.GenerateDailyTaxBills();
                        _game.Audio.Play("coin");
                        _game.PhoneNotification = "Fresh Property Tax Scrolls printed to your Mayor Bag!";
                    }
                    else if (row == 2)
                    {
                        _game.AdvanceToNextDay();
                    }
                    else
                    {
                        _game.ActiveOverlay = OverlayMode.None;
                    }
                    break;

                case 1:
                    if (row >= 0 && row < _game.Employees.Count)
                    {
                        var emp = _game.Employees[row];
                        if (_game.CityTreasury >= 75)
                        {
                            _game.CityTreasury -= 75;
                            emp.Morale = Math.Min(100, emp.Morale + 8);
                            emp.Efficiency = Math.Min(100, emp.Efficiency + 5);
                            emp.AIInsight = _game.AI.GenerateEmployeeEvaluation(emp, _game.CurrentDayType, _game.CurrentWeather);
                            _game.Happiness = Math.Min(100, _game.Happiness + 2);
                            _game.Audio.Play("coin");
                            _game.PhoneNotification = "Awarded $75 Cozy Bonus to " + emp.Name + "! Morale: " + emp.Morale + "%";
                        }
                        else
                        {
                            _game.Audio.Play("blip");
                            _game.PhoneNotification = "Need $75 in Treasury for Employee Bonus!";
                        }
                    }
                    break;

                case 2:
                    if (row >= 0 && row < _game.Incidents.Count)
                    {
                        var inc = _game.Incidents[row];
                        if (!inc.Resolved)
                        {
                            OpenIncidentResolutionDialogue(inc);
                        }
                        else
                        {
                            string raw = _game.AI.GenerateDynamicIncident(_game.CurrentDay, _game.CurrentWeather);
                            string[] pts = raw.Split('|');
                            if (pts.Length >= 4)
                            {
                                _game.Incidents[row] = new PoliceIncident
                                {
                                    Id = "inc_new_" + Environment.TickCount,
                                    Title = pts[0],
                                    Location = pts[1],
                                    Description = pts[2],
                                    Severity = pts[3],
                                    Resolved = false,
                                    AssignedOfficer = "Officer Maple",
                                    RewardBudget = 100,
                                    SafetyImpact = 5,
                                    HappinessImpact = 5
                                };
                                _game.Audio.Play("police");
                            }
                        }
                    }
                    break;

                case 3:
                    if (row < 5)
                    {
                        _game.SetDayType((DayType)row);
                    }
                    else
                    {
                        _game.SetWeather((WeatherType)(row - 5));
                    }
                    break;

                case 4:
                    if (row >= 0 && row < _game.BuildingPlots.Count)
                    {
                        _game.SelectedPlotIndex = row;
                        _game.ActiveOverlay = OverlayMode.BuildMenu;
                        _game.Audio.Play("click");
                    }
                    break;

                case 5:
                    string[] topics = new[] { "Tax & Budget Forecast", "Police & Safety Scan", "Employee Morale Review", "Weather & Schedule Advice" };
                    string q = topics[row % topics.Length];
                    _game.AI.AskAdvisor(q, _game.CurrentDay, _game.CityTreasury, _game.Happiness, _game.SafetyRating, _game.PropertyTaxRate, _game.CurrentWeather, _game.CurrentDayType, _game.Employees, _game.Incidents, _game.TaxBills);
                    _game.Audio.Play("blip");
                    break;
            }
        }

        private void OpenIncidentResolutionDialogue(PoliceIncident inc)
        {
            var msg = new DialogueMessage
            {
                Speaker = "Dispatch: " + inc.AssignedOfficer,
                Title = "Police Incident — " + inc.Title + " (" + inc.Location + ")",
                Text = inc.Description + "\n\nHow should the Maplewood Police Precinct handle this incident, Mayor?",
                PortraitColor = Color.FromArgb(45, 80, 145)
            };
            msg.Choices.Add(new DialogueChoice
            {
                Text = "1. Cozy Mediation & Warm Tea (+$" + inc.RewardBudget + ", +Safety, +Happiness)",
                OnSelect = () => _game.ResolvePoliceIncident(inc, 0)
            });
            msg.Choices.Add(new DialogueChoice
            {
                Text = "2. Full Detective Investigation (+$" + (inc.RewardBudget + 25) + ", +High Safety)",
                OnSelect = () => _game.ResolvePoliceIncident(inc, 1)
            });
            msg.Choices.Add(new DialogueChoice
            {
                Text = "3. Strict Municipal Ordinance Fine (+$" + (inc.RewardBudget + 75) + " Treasury, -2% Happy)",
                OnSelect = () => _game.ResolvePoliceIncident(inc, 2)
            });
            _game.EnqueueDialogue(msg);
        }

        private void UpdateLibraryReaderInput()
        {
            if (_game.Input.UpPressed)
            {
                _game.Library.SelectedBookIndex = (_game.Library.SelectedBookIndex + _game.Library.Books.Count - 1) % _game.Library.Books.Count;
                _game.Library.CurrentPageIndex = 0;
                _game.Audio.Play("page");
            }
            if (_game.Input.DownPressed)
            {
                _game.Library.SelectedBookIndex = (_game.Library.SelectedBookIndex + 1) % _game.Library.Books.Count;
                _game.Library.CurrentPageIndex = 0;
                _game.Audio.Play("page");
            }

            var book = _game.Library.Books[_game.Library.SelectedBookIndex];
            if (_game.Input.LeftPressed || _game.Input.TabPrevPressed)
            {
                if (_game.Library.CurrentPageIndex > 0)
                {
                    _game.Library.CurrentPageIndex--;
                    _game.Audio.Play("page");
                }
            }
            if (_game.Input.RightPressed || _game.Input.TabNextPressed || _game.Input.ConfirmPressed)
            {
                if (!book.HasBeenRead)
                {
                    book.HasBeenRead = true;
                    _game.BooksReadCount++;
                    int bonus = _game.CurrentDayType == DayType.LibraryLiteracyDay ? 60 : 30;
                    _game.CityTreasury += bonus;
                    _game.CozyScore += 10;
                    _game.CompleteScheduleTask(4);
                    _game.AddFloatingText("+$" + bonus + " Literacy Grant!", _game.PlayerX, _game.PlayerY - 20, Color.FromArgb(255, 220, 100));
                }

                if (_game.Library.CurrentPageIndex < book.Pages.Count - 1)
                {
                    _game.Library.CurrentPageIndex++;
                    _game.Audio.Play("page");
                }
                else
                {
                    _game.Library.CurrentPageIndex = 0;
                    _game.Audio.Play("page");
                }
            }
        }

        private void UpdateBuildMenuInput()
        {
            if (_game.Input.LeftPressed || _game.Input.TabPrevPressed)
            {
                _game.SelectedPlotIndex = (_game.SelectedPlotIndex + _game.BuildingPlots.Count - 1) % _game.BuildingPlots.Count;
                _game.Audio.Play("click");
            }
            if (_game.Input.RightPressed || _game.Input.TabNextPressed)
            {
                _game.SelectedPlotIndex = (_game.SelectedPlotIndex + 1) % _game.BuildingPlots.Count;
                _game.Audio.Play("click");
            }
            if (_game.Input.UpPressed)
            {
                _game.SelectedBuildOption = (_game.SelectedBuildOption + 7) % 8;
                _game.Audio.Play("click");
            }
            if (_game.Input.DownPressed)
            {
                _game.SelectedBuildOption = (_game.SelectedBuildOption + 1) % 8;
                _game.Audio.Play("click");
            }
            if (_game.Input.ConfirmPressed)
            {
                BuildingType chosenType = (BuildingType)(_game.SelectedBuildOption + 1);
                var plot = _game.BuildingPlots[_game.SelectedPlotIndex];
                _game.ConstructOrUpgradePlot(plot, chosenType);
            }
        }

        private void UpdatePauseMenuInput()
        {
            if (_game.Input.ConfirmPressed)
            {
                _game.SaveGame();
                _game.ActiveOverlay = OverlayMode.None;
            }
        }

        private void UpdatePlayerMovement(float dt)
        {
            float speed = 108f;
            float dx = _game.Input.MoveX * speed * dt;
            float dy = _game.Input.MoveY * speed * dt;

            if (Math.Abs(dx) > 0.01f || Math.Abs(dy) > 0.01f)
            {
                _game.WalkAnimTime += dt * 8f;
                if (Math.Abs(dx) > Math.Abs(dy))
                    _game.PlayerFacing = dx < 0 ? 2 : 3;
                else
                    _game.PlayerFacing = dy < 0 ? 1 : 0;

                float nextX = Math.Max(24f, Math.Min(616f, _game.PlayerX + dx));
                float nextY = Math.Max(46f, Math.Min(336f, _game.PlayerY + dy));

                _game.PlayerX = nextX;
                _game.PlayerY = nextY;

                CheckDoorwayTransitions();
            }
        }

        private void CheckDoorwayTransitions()
        {
            if (_game.CurrentScene == GameScene.TownExterior)
            {
                if (_game.PlayerX >= 290 && _game.PlayerX <= 350 && _game.PlayerY <= 66)
                {
                    _game.CurrentScene = GameScene.CityHallInterior;
                    _game.PlayerX = 320f;
                    _game.PlayerY = 305f;
                    _game.PlayerFacing = 1;
                    _game.Audio.Play("click");
                    _game.PhoneNotification = "Entered City Hall. Walk up to your Desk Computer!";
                }
                else if (_game.PlayerX >= 250 && _game.PlayerX <= 305 && _game.PlayerY >= 316)
                {
                    _game.CurrentScene = GameScene.PoliceStationInterior;
                    _game.PlayerX = 320f;
                    _game.PlayerY = 305f;
                    _game.PlayerFacing = 1;
                    _game.Audio.Play("click");
                    _game.PhoneNotification = "Entered Maplewood Police Precinct!";
                }
                else if (_game.PlayerX >= 345 && _game.PlayerX <= 405 && _game.PlayerY >= 316)
                {
                    _game.CurrentScene = GameScene.LibraryInterior;
                    _game.PlayerX = 320f;
                    _game.PlayerY = 305f;
                    _game.PlayerFacing = 1;
                    _game.Audio.Play("click");
                    _game.PhoneNotification = "Entered Public Library! Press [E] near bookshelves to read.";
                }
            }
            else
            {
                if (_game.PlayerX >= 280 && _game.PlayerX <= 360 && _game.PlayerY >= 322)
                {
                    GameScene prev = _game.CurrentScene;
                    _game.CurrentScene = GameScene.TownExterior;
                    if (prev == GameScene.CityHallInterior)
                    {
                        _game.PlayerX = 320f;
                        _game.PlayerY = 78f;
                        _game.PlayerFacing = 0;
                    }
                    else if (prev == GameScene.PoliceStationInterior)
                    {
                        _game.PlayerX = 278f;
                        _game.PlayerY = 300f;
                        _game.PlayerFacing = 1;
                    }
                    else
                    {
                        _game.PlayerX = 372f;
                        _game.PlayerY = 300f;
                        _game.PlayerFacing = 1;
                    }
                    _game.Audio.Play("click");
                    _game.PhoneNotification = "Stepped out into Maplewood Town Square.";
                }
            }
        }

        private void UpdateNPCs(float dt)
        {
            foreach (var npc in _game.Citizens)
            {
                if (npc.Scene != _game.CurrentScene) continue;
                npc.WanderTimer -= dt;
                if (npc.WanderTimer <= 0f)
                {
                    npc.WanderTimer = 1.8f + (float)(new Random(npc.Id.GetHashCode() + Environment.TickCount).NextDouble() * 2.0);
                    float distFromHome = (npc.X - npc.HomeX) * (npc.X - npc.HomeX) + (npc.Y - npc.HomeY) * (npc.Y - npc.HomeY);
                    if (distFromHome > 320f)
                    {
                        npc.MoveDX = Math.Sign(npc.HomeX - npc.X) * 14f;
                        npc.MoveDY = Math.Sign(npc.HomeY - npc.Y) * 14f;
                    }
                    else
                    {
                        npc.MoveDX = 0f;
                        npc.MoveDY = 0f;
                    }
                }
                npc.X += npc.MoveDX * dt;
                npc.Y += npc.MoveDY * dt;
            }
        }

        public void TryInteractInWorld()
        {
            CitizenNPC closestNpc = null;
            float bestDistSq = 42f * 42f;
            foreach (var npc in _game.Citizens)
            {
                if (npc.Scene != _game.CurrentScene) continue;
                float dx = npc.X - _game.PlayerX;
                float dy = npc.Y - _game.PlayerY;
                float dSq = dx * dx + dy * dy;
                if (dSq < bestDistSq)
                {
                    bestDistSq = dSq;
                    closestNpc = npc;
                }
            }

            if (closestNpc != null)
            {
                InteractWithNPC(closestNpc);
                return;
            }

            if (_game.CurrentScene == GameScene.CityHallInterior)
            {
                if (Math.Abs(_game.PlayerX - 320f) < 60f && _game.PlayerY < 155f)
                {
                    _game.ActiveOverlay = OverlayMode.MayorComputer;
                    _game.CompleteScheduleTask(0);
                    _game.Audio.Play("click");
                    return;
                }
            }
            else if (_game.CurrentScene == GameScene.LibraryInterior)
            {
                if (_game.PlayerY < 220f)
                {
                    _game.Library.RefreshAIChronicle(_game.MayorName, _game.CurrentDay, _game.CityTreasury, _game.Happiness, _game.SafetyRating, _game.CozyScore, _game.CurrentWeather, _game.CurrentDayType, _game.TaxBillsDeliveredTotal, _game.IncidentsResolvedTotal, _game.BuildingPlots);
                    _game.ActiveOverlay = OverlayMode.LibraryReader;
                    _game.Audio.Play("page");
                    return;
                }
            }
            else if (_game.CurrentScene == GameScene.PoliceStationInterior)
            {
                if (_game.PlayerY < 165f)
                {
                    _game.ActiveOverlay = OverlayMode.MayorComputer;
                    _game.ComputerTabIndex = 2;
                    _game.Audio.Play("police");
                    return;
                }
            }
            else if (_game.CurrentScene == GameScene.TownExterior)
            {
                for (int i = 0; i < _game.BuildingPlots.Count; i++)
                {
                    var p = _game.BuildingPlots[i];
                    Rectangle inflated = Rectangle.Inflate(p.WorldRect, 22, 22);
                    if (inflated.Contains((int)_game.PlayerX, (int)_game.PlayerY))
                    {
                        _game.SelectedPlotIndex = i;
                        _game.ActiveOverlay = OverlayMode.BuildMenu;
                        _game.Audio.Play("click");
                        return;
                    }
                }
            }
        }

        private void InteractWithNPC(CitizenNPC npc)
        {
            PropertyTaxBill pendingBill = null;
            if (npc.HasTaxBillPending)
            {
                foreach (var b in _game.TaxBills)
                {
                    if (b.Id == npc.TaxBillId && !b.Delivered)
                    {
                        pendingBill = b;
                        break;
                    }
                }
            }

            if (pendingBill != null)
            {
                string dayBonusNote = _game.CurrentDayType == DayType.TaxDay
                    ? " (TAX DAY ACTIVE: +25% Collection Bonus!)"
                    : " (Tip: Declare 'Tax Day' for +25% tax bonus!)";

                var msg = new DialogueMessage
                {
                    Speaker = npc.Name,
                    Title = npc.Title + " — " + pendingBill.PropertyName,
                    Text = "Hello Mayor! Oh, is that my Property Tax Bill for " + pendingBill.PropertyName + "? The assessed amount is $" + pendingBill.AmountDue + dayBonusNote + ". How shall we settle it today?",
                    PortraitColor = npc.PrimaryColor
                };

                msg.Choices.Add(new DialogueChoice
                {
                    Text = "1. Deliver Standard Property Tax Bill (Collect Full $" + pendingBill.AmountDue + ")",
                    OnSelect = () => _game.DeliverTaxBillWithOption(pendingBill, npc, 0)
                });
                msg.Choices.Add(new DialogueChoice
                {
                    Text = "2. Offer Cozy Payment Plan (85% Tax, +4% Happiness, +8 Cozy)",
                    OnSelect = () => _game.DeliverTaxBillWithOption(pendingBill, npc, 1)
                });
                msg.Choices.Add(new DialogueChoice
                {
                    Text = "3. Grant Neighborly Tax Relief (65% Tax, +7% Happiness, +15 Cozy)",
                    OnSelect = () => _game.DeliverTaxBillWithOption(pendingBill, npc, 2)
                });

                _game.EnqueueDialogue(msg);
                return;
            }

            if (npc.Id == "npc_clara")
            {
                var msg = new DialogueMessage
                {
                    Speaker = npc.Name,
                    Title = npc.Title,
                    Text = "Welcome to the Maplewood Public Library, " + _game.MayorName + "! We have 8 cozy volumes on the shelves, including the live AI Chronicle of your term. Would you like to open the Book Reader?",
                    PortraitColor = npc.PrimaryColor
                };
                msg.Choices.Add(new DialogueChoice
                {
                    Text = "1. Open the Cozy Library Book Reader",
                    OnSelect = () =>
                    {
                        _game.Library.RefreshAIChronicle(_game.MayorName, _game.CurrentDay, _game.CityTreasury, _game.Happiness, _game.SafetyRating, _game.CozyScore, _game.CurrentWeather, _game.CurrentDayType, _game.TaxBillsDeliveredTotal, _game.IncidentsResolvedTotal, _game.BuildingPlots);
                        _game.ActiveOverlay = OverlayMode.LibraryReader;
                    }
                });
                msg.Choices.Add(new DialogueChoice
                {
                    Text = "2. Just browsing the shelves, thank you!",
                    OnSelect = null
                });
                _game.EnqueueDialogue(msg);
            }
            else if (npc.Id == "npc_barnaby" || npc.Id == "npc_maple")
            {
                var msg = new DialogueMessage
                {
                    Speaker = npc.Name,
                    Title = npc.Title,
                    Text = "Reporting for duty, " + _game.MayorName + "! City safety is at " + _game.SafetyRating + "% and we have " + _game.GetOpenIncidentCount() + " active incident(s) on the blotter. Open the Police Dispatch terminal?",
                    PortraitColor = npc.PrimaryColor
                };
                msg.Choices.Add(new DialogueChoice
                {
                    Text = "1. Open Police Incidents & Dispatch Terminal",
                    OnSelect = () =>
                    {
                        _game.ActiveOverlay = OverlayMode.MayorComputer;
                        _game.ComputerTabIndex = 2;
                    }
                });
                msg.Choices.Add(new DialogueChoice
                {
                    Text = "2. Carry on the good patrol work!",
                    OnSelect = null
                });
                _game.EnqueueDialogue(msg);
            }
            else if (npc.Id == "npc_penny")
            {
                var msg = new DialogueMessage
                {
                    Speaker = npc.Name,
                    Title = npc.Title,
                    Text = "Good day, " + _game.MayorName + "! Your Desk Computer is right behind you, and " + _game.GetPendingTaxBillCount() + " Property Tax Bill(s) remain to be delivered around town today.",
                    PortraitColor = npc.PrimaryColor
                };
                msg.Choices.Add(new DialogueChoice
                {
                    Text = "1. Access Mayor's Desk Computer (CozyOS)",
                    OnSelect = () =>
                    {
                        _game.ActiveOverlay = OverlayMode.MayorComputer;
                        _game.CompleteScheduleTask(0);
                    }
                });
                msg.Choices.Add(new DialogueChoice
                {
                    Text = "2. Thanks Penny!",
                    OnSelect = null
                });
                _game.EnqueueDialogue(msg);
            }
            else if (npc.Id == "npc_cornelius")
            {
                _game.EnqueueDialogue(new DialogueMessage
                {
                    Speaker = npc.Name,
                    Title = npc.Title,
                    Text = "Hmph! I may have lost the election 12 to 0, " + _game.MayorName + ", and I already paid my property tax bill... but I must admit, this " + _game.CurrentWeather + " weather is surprisingly pleasant!",
                    PortraitColor = npc.PrimaryColor
                });
            }
            else
            {
                _game.EnqueueDialogue(new DialogueMessage
                {
                    Speaker = npc.Name,
                    Title = npc.Title,
                    Text = "What a wonderful " + _game.CurrentDayType + " in Maplewood Valley! Everyone is so glad we voted for you 12-to-0! Having " + _game.CurrentWeather + " skies makes the town feel extra cozy.",
                    PortraitColor = npc.PrimaryColor
                });
            }
        }
    }
}
