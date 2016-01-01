using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DemoGo.Parser
{
    public class Parser : IDisposable
    {
        public Guid DemoId { get; }
        public Demo Demo { get; }

        private DemoInfo.DemoParser DemoParser { get; }
        private bool MatchStarted { get; set; }
        private int CurrentRound { get; set; }
        private bool CurrentRoundInProgress { get; set; }
        private long? PossibleClutcher { get; set; }
        private int PossibleClutchVs { get; set; }
        private List<long> Team1LivingPlayers { get; } = new List<long>();
        private List<long> Team2LivingPlayers { get; } = new List<long>();
        public bool LastRoundOfHalf { get; set; }
        private int CurrentHalf { get; set; } = 1;
        private bool RoundMoneyFlag { get; set; }
        private bool RoundEqupmentFlag { get; set; }
        private bool MatchStartFlag { get; set; }

        private Parser(Guid demoId)
        {
            DemoId = demoId;
            Demo = new Demo(demoId);
        }

        public Parser(Services.DemoService.DemoQueue demoQueue) : this(demoQueue.DemoId)
        {
            using (var httpClient = new HttpClient())
            {
                var task = httpClient.GetStreamAsync(demoQueue.DemoUrl);
                task.Wait();
                var demoStream = task.Result;
                DemoParser = new DemoInfo.DemoParser(demoStream);
            }

            SetupEvents();
        }

        public Parser(Guid demoId, string demoUrl) : this(demoId)
        {
            using (var httpClient = new HttpClient())
            {
                var task = httpClient.GetStreamAsync(demoUrl);
                task.Wait();
                var demoStream = task.Result;
                DemoParser = new DemoInfo.DemoParser(demoStream);
            }

            SetupEvents();
        }

        public Parser(Guid demoId, DemoInfo.DemoParser demoParser) : this(demoId)
        {
            DemoParser = demoParser;
            SetupEvents();
        }

        private void SetupEvents()
        {
            DemoParser.PlayerBind += DemoParser_PlayerBind;
            DemoParser.PlayerTeam += DemoParser_PlayerTeam;
            DemoParser.RoundStart += DemoParser_RoundStart;
            DemoParser.FreezetimeEnded += DemoParser_FreezetimeEnded;
            DemoParser.LastRoundHalf += DemoParser_LastRoundHalf;
            DemoParser.RoundEnd += DemoParser_RoundEnd;
            DemoParser.PlayerHurt += DemoParser_PlayerHurt;
            DemoParser.PlayerKilled += DemoParser_PlayerKilled;
            #if SLOW_PROTOBUF
            DemoParser.ServerRankUpdate += DemoParser_ServerRankUpdate;
            #endif
            DemoParser.BombPlanted += DemoParser_BombPlanted;
            DemoParser.BombDefused += DemoParser_BombDefused;
            DemoParser.TickDone += DemoParser_TickDone;
            DemoParser.MatchStarted += DemoParser_MatchStarted;
            DemoParser.PlayerDisconnect += DemoParser_PlayerDisconnect;
        }

        #region Event Handlers
        private void DemoParser_PlayerBind(object sender, DemoInfo.PlayerBindEventArgs e)
        {
            if ((e.Player?.Team == DemoInfo.Team.CounterTerrorist && Demo.Team1Ct(CurrentHalf)) || (e.Player?.Team == DemoInfo.Team.Terrorist && !Demo.Team1Ct(CurrentHalf)))
            {
                if (!Demo.Team1Players.Contains(e.Player.SteamID))
                    Demo.Team1Players.Add(e.Player.SteamID);
            }
            else if ((e.Player?.Team == DemoInfo.Team.Terrorist && Demo.Team1Ct(CurrentHalf)) || (e.Player?.Team == DemoInfo.Team.CounterTerrorist && !Demo.Team1Ct(CurrentHalf)))
            {
                if (!Demo.Team2Players.Contains(e.Player.SteamID))
                    Demo.Team2Players.Add(e.Player.SteamID);
            }

            if(!Demo.Players.Where(p => p.SteamId == e.Player.SteamID).Any())
                Demo.Players.Add(new Demo.GamePlayer
                {
                    Demo = Demo,
                    SteamId = e.Player.SteamID,
                    Name = e.Player.Name
                });
        }

        private void DemoParser_PlayerTeam(object sender, DemoInfo.PlayerTeamEventArgs e)
        {
            if (MatchStarted || e.Swapped == null || e.IsBot) return;

            if (e.NewTeam == DemoInfo.Team.CounterTerrorist && !Demo.Team1Players.Contains(e.Swapped.SteamID))
                Demo.Team1Players.Add(e.Swapped.SteamID);
            else if (e.NewTeam == DemoInfo.Team.Terrorist && !Demo.Team2Players.Contains(e.Swapped.SteamID))
                Demo.Team2Players.Add(e.Swapped.SteamID);
        }

        private void DemoParser_MatchStarted(object sender, DemoInfo.MatchStartedEventArgs e)
        {
            MatchStarted = true;
            MatchStartFlag = true;
        }

        private void DemoParser_RoundStart(object sender, DemoInfo.RoundStartedEventArgs e)
        {
            var prevRound = Demo.RoundLogs.Where(round => round.Number == CurrentRound).LastOrDefault();
            if (prevRound != null)
                prevRound.FinalTick = DemoParser.IngameTick;

            if(CurrentRoundInProgress)
                Demo.RoundLogs.Remove(prevRound);
            else
                CurrentRound++;

            Team1LivingPlayers.Clear();
            Team2LivingPlayers.Clear();
            Team1LivingPlayers.AddRange(Demo.Team1Players);
            Team2LivingPlayers.AddRange(Demo.Team2Players);
            PossibleClutcher = null;

            CurrentRoundInProgress = true;
            Demo.RoundLogs.Add(new Demo.RoundLog
            {
                Demo = Demo,
                Number = CurrentRound,
                StartTick = DemoParser.IngameTick,
                TimeLimit = e.TimeLimit,
                Half = CurrentHalf
            });

            RoundMoneyFlag = true;
        }

        private void DemoParser_FreezetimeEnded(object sender, DemoInfo.FreezetimeEndedEventArgs e)
        {
            RoundEqupmentFlag = true;
        }

        private void DemoParser_LastRoundHalf(object sender, DemoInfo.LastRoundHalfEventArgs e)
        {
            LastRoundOfHalf = true;
        }

        private void DemoParser_RoundEnd(object sender, DemoInfo.RoundEndedEventArgs e)
        {
            CurrentRoundInProgress = false;

            if (PossibleClutcher != null)
            {
                var clutcherSide = DemoInfo.Team.Spectate;
                if(Demo.Team1Players.Contains(PossibleClutcher.Value))
                    clutcherSide = Demo.Team1Ct(CurrentHalf) ? DemoInfo.Team.CounterTerrorist : DemoInfo.Team.Terrorist;
                else if(Demo.Team2Players.Contains(PossibleClutcher.Value))
                    clutcherSide = Demo.Team1Ct(CurrentHalf) ? DemoInfo.Team.Terrorist : DemoInfo.Team.CounterTerrorist;

                if (e.Winner == clutcherSide)
                    Demo.NotableEvents.Add(new Demo.NotableEvent
                    {
                        Type = Demo.NotableEventType.Clutch,
                        SteamId = PossibleClutcher.Value,
                        RoundNumber = CurrentRound,
                        AdditionalData = new { Vs = PossibleClutchVs }
                    });
            }

            if (LastRoundOfHalf)
            {
                LastRoundOfHalf = false;
                CurrentHalf++;
            }

            var prevRound = Demo.RoundLogs.Where(round => round.Number == CurrentRound).LastOrDefault();
            if (prevRound != null)
            {
                prevRound.EndTick = DemoParser.IngameTick;
                prevRound.WinningSide = e.Winner;
                prevRound.WinState = e.Reason.ToString();
            }
        }

        private void DemoParser_PlayerHurt(object sender, DemoInfo.PlayerHurtEventArgs e)
        {
            if (!MatchStarted) return;

            Demo.EventLogs.Add(new Demo.EventPlayerDamage
            {
                Type = Demo.EventType.PlayerDamage,
                AttackerSteamId = e.Attacker?.SteamID,
                VictimSteamId = e.Player?.SteamID,
                RoundNumber = CurrentRound,
                GameTick = DemoParser.IngameTick,
                HealthDamage = e.HealthDamage,
                ArmorDamage = e.ArmorDamage,
                Hitgroup = e.Hitgroup
            });
        }

        private void DemoParser_PlayerKilled(object sender, DemoInfo.PlayerKilledEventArgs e)
        {
            if (!MatchStarted) return;

            Team1LivingPlayers.Remove(e.Victim.SteamID);
            Team2LivingPlayers.Remove(e.Victim.SteamID);

            // Clutch shit
            if (PossibleClutcher == null)
            {
                if (Team1LivingPlayers.Count == 1 && Team2LivingPlayers.Count >= 3)
                {
                    PossibleClutcher = Team1LivingPlayers.FirstOrDefault();
                    PossibleClutchVs = Team2LivingPlayers.Count;
                }
                if (Team2LivingPlayers.Count == 1 && Team1LivingPlayers.Count >= 3)
                {
                    PossibleClutcher = Team2LivingPlayers.FirstOrDefault();
                    PossibleClutchVs = Team1LivingPlayers.Count;
                }
            }
            else if (e.Victim.SteamID == PossibleClutcher)
                PossibleClutcher = null;

            Demo.EventLogs.Add(new Demo.EventPlayerKill
            {
                Type = Demo.EventType.PlayerKill,
                KillerSteamId = e.Killer?.SteamID,
                VictimSteamId = e.Victim?.SteamID,
                AssisterSteamId = e.Assister?.SteamID,
                RoundNumber = CurrentRound,
                GameTick = DemoParser.IngameTick,
                IsTeamKill = e.Killer?.Team == e.Victim?.Team,
                IsHeadshot = e.Headshot
            });
        }

        #if SLOW_PROTOBUF
        private void DemoParser_ServerRankUpdate(object sender, DemoInfo.ServerRankUpdateEventArgs e)
        {
            foreach (var rankInfo in e.RankStructList)
            {
                var _player = Demo.Players.Where(p => p.SteamId == rankInfo.SteamId).FirstOrDefault();

                if (_player != null)
                    _player.Rank = (byte)rankInfo.New;
            }
        }
        #endif

        private void DemoParser_BombPlanted(object sender, DemoInfo.BombEventArgs e)
        {
            Demo.EventLogs.Add(new Demo.EventBombPlant
            {
                Type = Demo.EventType.BombPlanted,
                RoundNumber = CurrentRound,
                GameTick = DemoParser.IngameTick,
                PlanterSteamId = e.Player?.SteamID,
                Site = e.Site
            });
        }

        private void DemoParser_BombDefused(object sender, DemoInfo.BombEventArgs e)
        {
            Demo.EventLogs.Add(new Demo.EventBombDefuse
            {
                Type = Demo.EventType.BombDefused,
                RoundNumber = CurrentRound,
                GameTick = DemoParser.IngameTick,
                DefuserSteamId = e.Player?.SteamID,
                Site = e.Site
            });
        }

        private void DemoParser_TickDone(object sender, DemoInfo.TickDoneEventArgs e)
        {
            Demo.ParsingProgress = DemoParser.ParsingProgess > 1 ? 1f : DemoParser.ParsingProgess;

            if(MatchStartFlag)
            {
                MatchStartFlag = false;

                Demo.Team1Tag = DemoParser.CTClanName ?? "Counter-Terrorist";
                Demo.Team2Tag = DemoParser.TClanName ?? "Terroritst";
            }

            if (RoundMoneyFlag)
            {
                RoundMoneyFlag = false;

                foreach (var p1 in DemoParser.PlayingParticipants)
                {
                    var player = Demo.Players.Where(p => p.SteamId == p1.SteamID).FirstOrDefault();
                    if (player != null)
                        player.StartMoneyByRound[CurrentRound] = p1.Money;
                }
            }

            if (RoundEqupmentFlag)
            {
                RoundEqupmentFlag = false;

                foreach (var p1 in DemoParser.PlayingParticipants)
                {
                    var player = Demo.Players.Where(p => p.SteamId == p1.SteamID).FirstOrDefault();
                    if (player != null)
                        player.EquipmentValuesByRound[CurrentRound] = p1.FreezetimeEndEquipmentValue;
                }
            }
        }

        private void DemoParser_PlayerDisconnect(object sender, DemoInfo.PlayerDisconnectEventArgs e)
        {
            if (e.Player != null)
            {
                Team1LivingPlayers.Remove(e.Player.SteamID);
                Team2LivingPlayers.Remove(e.Player.SteamID);
            }
        }
        #endregion

        public void Parse()
        {
            var header = DemoParser.ParseHeader();

            Demo.Map = header.MapName;
            Demo.Host = header.ServerName;
            Demo.Tickrate = (byte)Math.Round(DemoParser.TickRate);
            Demo.ServerTickrate = (byte)(header.PlaybackTicks / header.PlaybackTime);

            DemoParser.ParseToEnd();
            Demo.PlayTime = DemoParser.CurrentTime;
        }

        public void Dispose()
        {
            DemoParser.Dispose();
        }
    }
}
