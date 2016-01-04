using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;
using DemoInfo;

namespace DemoGo.Parser
{
    public class Demo
    {
        public static bool Team1Ct(int half)
        {
            if (half == 1)
                return true;
            else if (half == 2)
                return false;
            else if (half > 2)
            {
                int currentOvertime = (half == 3 ? 0 : (int)Math.Floor(((half - 3) / 2f))) + 1;
                if (currentOvertime % 2 == 0)
                    return half % 2 != 0;
                else
                    return half % 2 == 0;
            }

            return false;
        }

        public Demo(Guid id)
        {
            Id = id;
        }

        public Guid Id { get; }
        public float ParsingProgress { get; set; }
        public string Map { get; set; }
        public string Host { get; set; }
        public byte Tickrate { get; set; }
        public byte ServerTickrate { get; set; }
        public string Team1Tag { get; set; }
        public string Team2Tag { get; set; }
        public List<long> Team1Players { get; set; } = new List<long>();
        public List<long> Team2Players { get; set; } = new List<long>();
        public int Team1FinalScore
        {
            get
            {
                return RoundLogs.Where(e => (Team1Ct(e.Half) && e.WinningSide == Team.CounterTerrorist) || (!Team1Ct(e.Half) && e.WinningSide == Team.Terrorist)).Count();
            }
        }
        public IEnumerable<int> Team1ScoreByHalf
        {
            get
            {
                return RoundLogs.Where(e => (Team1Ct(e.Half) && e.WinningSide == Team.CounterTerrorist) || (!Team1Ct(e.Half) && e.WinningSide == Team.Terrorist)).OrderBy(e => e.Half).GroupBy(e => e.Half).Select(e => e.Count());
            }
        }

        public int Team2FinalScore
        {
            get
            {
                return RoundLogs.Where(e => (Team1Ct(e.Half) && e.WinningSide == Team.Terrorist) || (!Team1Ct(e.Half) && e.WinningSide == Team.CounterTerrorist)).Count();
            }
        }
        public IEnumerable<int> Team2ScoreByHalf
        {
            get
            {
                return RoundLogs.Where(e => (Team1Ct(e.Half) && e.WinningSide == Team.Terrorist) || (!Team1Ct(e.Half) && e.WinningSide == Team.CounterTerrorist)).OrderBy(e => e.Half).GroupBy(e => e.Half).Select(e => e.Count());
            }
        }
        public float PlayTime { get; set; }
        public List<NotableEvent> NotableEvents { get; set; } = new List<NotableEvent>();
        [JsonIgnore]
        public List<EventLog> EventLogs { get; set; } = new List<EventLog>();
        public List<RoundLog> RoundLogs { get; set; } = new List<RoundLog>();
        public List<GamePlayer> Players { get; set; } = new List<GamePlayer>();

        public IEnumerable<EventPlayerKill> GetPlayerKillEvents()
        {
            return EventLogs.Where(e => e.Type == EventType.PlayerKill).Cast<EventPlayerKill>();
        }

        public IEnumerable<EventPlayerDamage> GetPlayerDamageEvents()
        {
            return EventLogs.Where(e => e.Type == EventType.PlayerDamage).Cast<EventPlayerDamage>();
        }

        public IEnumerable<EventPlayerMVP> GetPlayerMPVEvents()
        {
            return EventLogs.Where(e => e.Type == EventType.PlayerMVP).Cast<EventPlayerMVP>();
        }

        public IEnumerable<EventBombPlant> GetBombPlantEvents()
        {
            return EventLogs.Where(e => e.Type == EventType.BombPlanted).Cast<EventBombPlant>();
        }

        public IEnumerable<EventBombDefuse> GetBombDefuseEvents()
        {
            return EventLogs.Where(e => e.Type == EventType.BombDefused).Cast<EventBombDefuse>();
        }

        public class RoundLog
        {
            [JsonIgnore]
            public Demo Demo { get; set; }

            public long StartTick { get; set; }
            public long EndTick { get; set; }
            public long FinalTick { get; set; }
            public int Number { get; set; }
            public string WinState { get; set; }
            public Team WinningSide { get; set; }
            public int TimeLimit { get; set; }
            public int Half { get; set; }

            public int Team1StartMoney
            {
                get
                {
                    return Players.Where(e => Demo.Team1Players.Contains(e.SteamId)).Sum(e => e.StartMoney);
                }
            }

            public int Team1EquipmentValue
            {
                get
                {
                    return Players.Where(e => Demo.Team1Players.Contains(e.SteamId)).Sum(e => e.EquipmentValue);
                }
            }

            public int Team2StartMoney
            {
                get
                {
                    return Players.Where(e => Demo.Team2Players.Contains(e.SteamId)).Sum(e => e.StartMoney);
                }
            }

            public int Team2EquipmentValue
            {
                get
                {
                    return Players.Where(e => Demo.Team2Players.Contains(e.SteamId)).Sum(e => e.EquipmentValue);
                }
            }

            public IEnumerable<RoundPlayer> Players
            {
                get
                {
                    return Demo.Players.Select(e => e.AsRoundPlayer(Number));
                }
            }
        }

        public class EventLog
        {
            public EventType Type { get; set; }
            public int RoundNumber { get; set; }
            public int GameTick { get; set; }
        }

        public class EventPlayerDamage : EventLog
        {
            public long? AttackerSteamId { get; set; }
            public long? VictimSteamId { get; set; }
            public int HealthDamage { get; set; }
            public int ArmorDamage { get; set; }
            public Hitgroup Hitgroup { get; set; }
        }

        public class EventPlayerKill : EventLog
        {
            public long? KillerSteamId { get; set; }
            public long? AssisterSteamId { get; set; }
            public long? VictimSteamId { get; set; }
            public bool IsTeamKill { get; set; }
            public bool IsHeadshot { get; set; }
        }

        public class EventPlayerMVP : EventLog
        {
            public long? SteamID { get; set; }
            public RoundMVPReason Reason { get; set; }
        }

        public class EventBombPlant : EventLog
        {
            public long? PlanterSteamId { get; set; }
            public char Site { get; set; }
        }

        public class EventBombDefuse : EventLog
        {
            public long? DefuserSteamId { get; set; }
            public char Site { get; set; }
        }

        public enum EventType
        {
            PlayerDamage,
            PlayerKill,
            BombPlanted,
            BombDefused,
            PlayerMVP,
        }

        public enum NotableEventType
        {
            Clutch
        }

        public class NotableEvent
        {
            public NotableEventType Type { get; set; }
            public int RoundNumber { get; set; }
            public long SteamId { get; set; }
            public object AdditionalData { get; set; }
        }

        public class RoundPlayer
        {
            [JsonIgnore]
            public Demo Demo { get; set; }

            public long SteamId { get; set; }
            [JsonIgnore]
            public int RoundNumber { get; set; }
            public bool Clutched
            {
                get
                {
                    return Demo.NotableEvents.Where(e => e.Type == NotableEventType.Clutch && e.SteamId == SteamId && e.RoundNumber == RoundNumber).Any();
                }
            } 
            public int EquipmentValue
            {
                get
                {
                    var demoPlayer = Demo.Players.Where(e => e.SteamId == SteamId).FirstOrDefault();
                    return demoPlayer.EquipmentValuesByRound.ContainsKey(RoundNumber) ? demoPlayer.EquipmentValuesByRound[RoundNumber] : 0;
                }
            }
            public int StartMoney
            {
                get
                {
                    var demoPlayer = Demo.Players.Where(e => e.SteamId == SteamId).FirstOrDefault();
                    return demoPlayer.StartMoneyByRound.ContainsKey(RoundNumber) ? demoPlayer.StartMoneyByRound[RoundNumber] : 0;
                }
            }
            public bool PlantedBomb
            {
                get
                {
                    return Demo.GetBombPlantEvents().Where(e => e.RoundNumber == RoundNumber && e.PlanterSteamId == SteamId).Any();
                }
            }
            public bool DefusedBomb
            {
                get
                {
                    return Demo.GetBombDefuseEvents().Where(e => e.RoundNumber == RoundNumber && e.DefuserSteamId == SteamId).Any();
                }
            }
            public int Kills
            {
                get
                {
                    return Demo.GetPlayerKillEvents().Where(e => e.RoundNumber == RoundNumber && e.KillerSteamId == SteamId).Select(e => e.IsTeamKill ? -1 : 1).Sum();
                }
            }
            public int Assists
            {
                get
                {
                    return Demo.GetPlayerKillEvents().Where(e => e.RoundNumber == RoundNumber && e.AssisterSteamId == SteamId).Count();
                }
            }
            public int HeadshotKills
            {
                get
                {
                    return Demo.GetPlayerKillEvents().Where(e => e.RoundNumber == RoundNumber && e.KillerSteamId == SteamId && e.RoundNumber != 0 && e.IsHeadshot && !e.IsTeamKill).Count();
                }
            }
            public bool Died
            {
                get
                {
                    return Demo.GetPlayerKillEvents().Where(e => e.RoundNumber == RoundNumber && e.VictimSteamId == SteamId).Any();
                }
            }
            public int TotalDamageHealth
            {
                get
                {
                    return Demo.GetPlayerDamageEvents().Where(e => e.RoundNumber == RoundNumber && e.AttackerSteamId == SteamId).Sum(e => e.HealthDamage);
                }
            }
            public int TotalDamageArmor
            {
                get
                {
                    return Demo.GetPlayerDamageEvents().Where(e => e.RoundNumber == RoundNumber && e.AttackerSteamId == SteamId).Sum(e => e.ArmorDamage);
                }
            }
            public double HeadShotRatio
            {
                get
                {
                    if (HeadshotKills == 0 || Kills == 0)
                        return 0;

                    return (double)HeadshotKills / (double)Kills;
                }
            }
        }

        public class GamePlayer
        {
            public RoundPlayer AsRoundPlayer(int roundNumber)
            {
                return new RoundPlayer
                {
                    Demo = Demo,
                    SteamId = SteamId,
                    RoundNumber = roundNumber
                };
            }

            [JsonIgnore]
            public Demo Demo { get; set; }

            [JsonIgnore]
            public Dictionary<int, int> EquipmentValuesByRound { get; } = new Dictionary<int, int>();
            [JsonIgnore]
            public Dictionary<int, int> StartMoneyByRound { get; } = new Dictionary<int, int>();

            public long SteamId { get; set; }
            public string Name { get; set; }
            public byte Rank { get; set; }
            public int Clutches
            {
                get
                {
                    return Demo.NotableEvents.Where(e => e.Type == NotableEventType.Clutch && e.SteamId == SteamId).Count();
                }
            }
            public int Mvps
            {
                get
                {
                    return Demo.GetPlayerMPVEvents().Where(e => e.SteamID == SteamId).Count();
                }
            }
            public int BombPlants
            {
                get
                {
                    return Demo.GetBombPlantEvents().Where(e => e.PlanterSteamId == SteamId).Count();
                }
            }
            public int BombDefuses
            {
                get
                {
                    return Demo.GetBombDefuseEvents().Where(e => e.DefuserSteamId == SteamId).Count();
                }
            }
            public int Kills
            {
                get
                {
                    return Demo.GetPlayerKillEvents().Where(e => e.Type == EventType.PlayerKill && e.KillerSteamId == SteamId).Select(e => e.IsTeamKill ? -1 : 1).Sum();
                }
            }
            public IEnumerable<int> KillsByNumber
            {
                get
                {
                    yield return -1;
                    yield return Demo.GetPlayerKillEvents().Where(e => e.KillerSteamId == SteamId).GroupBy(e => e.RoundNumber).Where(e => e.Count() == 1).Count();
                    yield return Demo.GetPlayerKillEvents().Where(e => e.KillerSteamId == SteamId).GroupBy(e => e.RoundNumber).Where(e => e.Count() == 2).Count();
                    yield return Demo.GetPlayerKillEvents().Where(e => e.KillerSteamId == SteamId).GroupBy(e => e.RoundNumber).Where(e => e.Count() == 3).Count();
                    yield return Demo.GetPlayerKillEvents().Where(e => e.KillerSteamId == SteamId).GroupBy(e => e.RoundNumber).Where(e => e.Count() == 4).Count();
                    yield return Demo.GetPlayerKillEvents().Where(e => e.KillerSteamId == SteamId).GroupBy(e => e.RoundNumber).Where(e => e.Count() >= 5).Count();
                }
            }
            public int Assists
            {
                get
                {
                    return Demo.GetPlayerKillEvents().Where(e => e.Type == EventType.PlayerKill && e.AssisterSteamId == SteamId).Count();
                }
            }
            public int HeadshotKills
            {
                get
                {
                    return Demo.GetPlayerKillEvents().Where(e => e.Type == EventType.PlayerKill && e.KillerSteamId == SteamId && e.RoundNumber != 0 && e.IsHeadshot && !e.IsTeamKill).Count();
                }
            }
            public int Deaths
            {
                get
                {
                    return Demo.GetPlayerKillEvents().Where(e => e.Type == EventType.PlayerKill && e.VictimSteamId == SteamId).Count();
                }
            }
            public int TotalDamageHealth
            {
                get
                {
                    return Demo.GetPlayerDamageEvents().Where(e => e.AttackerSteamId == SteamId).Sum(e => e.HealthDamage);
                }
            }
            public int TotalDamageArmor
            {
                get
                {
                    return Demo.GetPlayerDamageEvents().Where(e => e.AttackerSteamId == SteamId).Sum(e => e.ArmorDamage);
                }
            }
            public float KillDeathRatio
            {
                get
                {
                    if (Kills == 0 || Deaths == 0)
                        return 0f;

                    return Kills / Deaths;
                }
            }
            public double AverageDamagePerRound
            {
                get
                {
                    if (TotalDamageHealth + TotalDamageArmor == 0)
                        return 0d;

                    return (double)(TotalDamageHealth + TotalDamageArmor) / 30f;
                }
            }
            public double HeadShotRatio
            {
                get
                {
                    if (HeadshotKills == 0 || Kills == 0)
                        return 0;

                    return (double)HeadshotKills / (double)Kills;
                }
            }
        }
    }
}
