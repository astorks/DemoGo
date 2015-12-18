using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;

namespace DemoInfo.Models
{
    public class Demo
    {
        public string Map { get; set; }
        public string Host { get; set; }
        public byte Tickrate { get; set; }
        public byte ServerTickrate { get; set; }
        public TimeSpan PlayTime { get; set; }
        [JsonIgnore]
        public List<KillLog> KillLogs { get; set; } = new List<KillLog>();
        [JsonIgnore]
        public List<DamageLog> DamageLogs { get; set; } = new List<DamageLog>();
        public List<RoundLog> RoundLogs { get; set; } = new List<RoundLog>();
        public List<Player> Players { get; set; } = new List<Player>();

        public class RoundLog
        {
            [JsonIgnore]
            public Demo Demo { get; set; }

            public long StartTick { get; set; }
            public long EndTick { get; set; }
            public long FinalTick { get; set; }
            public int Number { get; set; }
            public BombState BombState { get; set; }
            public TeamSide WinningSide { get; set; }
        }

        public enum BombState
        {
            NotPlanted,
            BlewUp,
            Defused
        }

        public enum TeamSide
        {
            CounterTerrorists,
            Terrorists
        }

        public class KillLog
        {
            public long? KillerSteamId { get; set; }
            public long? VictimSteamId { get; set; }
            public short RoundNumber { get; set; }
            public long Tick { get; set; }
            public bool TeamKill { get; set; }
            public bool Headshot { get; set; }
            public bool AfterRound { get; set; }
        }

        public class DamageLog
        {
            public long? AttackerSteamId { get; set; }
            public long? VictimSteamId { get; set; }
            public short RoundNumber { get; set; }
            public long Tick { get; set; }
            public int HealthDamage { get; set; }
            public int ArmorDamage { get; set; }
            public Hitgroup Hitgroup { get; set; }
            public bool AfterRound { get; set; }
        }

        public class Player
        {
            [JsonIgnore]
            public Demo Demo { get; set; }

            public long SteamId { get; set; }
            public string Name { get; set; }
            public byte Rank { get; set; }
            public int Kills
            {
                get
                {
                    return Demo.KillLogs.Where(e => e.KillerSteamId == SteamId && e.RoundNumber != 0).Select(e => e.TeamKill ? -1 : 1).Sum();
                }
            }
            public int HeadshotKills
            {
                get
                {
                    return Demo.KillLogs.Where(e => e.KillerSteamId == SteamId && e.RoundNumber != 0 && e.Headshot && !e.TeamKill).Count();
                }
            }
            public int Deaths
            {
                get
                {
                    return Demo.KillLogs.Where(e => e.VictimSteamId == SteamId && e.RoundNumber != 0).Count();
                }
            }
            public int TotalDamageHealth
            {
                get
                {
                    return Demo.DamageLogs.Where(e => e.AttackerSteamId == SteamId).Sum(e => e.HealthDamage);
                }
            }
            public int TotalDamageArmor
            {
                get
                {
                    return Demo.DamageLogs.Where(e => e.AttackerSteamId == SteamId).Sum(e => e.ArmorDamage);
                }
            }
            public float KDR
            {
                get
                {
                    return Kills / Deaths;
                }
            }
            public double ADR
            {
                get
                {
                    return (double)(TotalDamageHealth + TotalDamageArmor) / 30f;
                }
            }
            public double HSP
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
