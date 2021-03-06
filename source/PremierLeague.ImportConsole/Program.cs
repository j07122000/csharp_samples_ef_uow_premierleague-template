﻿using ConsoleTables;
using PremierLeague.Core;
using PremierLeague.Core.Contracts;
using PremierLeague.Core.Entities;
using PremierLeague.Persistence;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PremierLeague.ImportConsole
{
    class Program
    {
        static void Main()
        {
            PrintHeader();
            InitData();
            AnalyzeData();

            Console.Write("Beenden mit Eingabetaste ...");
            Console.ReadLine();
        }

        private static void PrintHeader()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(new String('-', 60));

            Console.WriteLine(
                  @"
            _,...,_
          .'@/~~~\@'.          
         //~~\___/~~\\        P R E M I E R  L E A G U E 
        |@\__/@@@\__/@|             
        |@/  \@@@/  \@|            (inkl. Statistik)
         \\__/~~~\__//
          '.@\___/@.'
            `""""""
                ");

            Console.WriteLine(new String('-', 60));
            Console.WriteLine();
            Console.ResetColor();
        }

        /// <summary>
        /// Importiert die Ergebnisse (csv-Datei >> Datenbank).
        /// </summary>
        private static void InitData()
        {
            using (IUnitOfWork unitOfWork = new UnitOfWork())
            {
                Log.Information("Import der Spiele und Teams in die Datenbank");

                Log.Information("Datenbank löschen");
                unitOfWork.DeleteDatabase();
                // TODO: Datenbank löschen

                Log.Information("Datenbank migrieren");
                unitOfWork.MigrateDatabase();
                // TODO: Datenbank migrieren

                Log.Information("Spiele werden von premierleague.csv eingelesen");
                var games = ImportController.ReadFromCsv().ToArray();
                if (games.Length == 0)
                {
                    Log.Warning("!!! Es wurden keine Spiele eingelesen");
                }
                else
                {
                    Log.Debug($"  Es wurden {games.Count()} Spiele eingelesen!");

                    // TODO: Teams aus den Games ermitteln
                    var teams = games
                       .Select(t => new
                       {
                           Team = t.HomeTeam
                       })
                    .Concat(games.Select(t => new
                    {
                        Team = t.GuestTeam

                    })).Distinct().ToArray();

                    Log.Debug($"  Es wurden {teams.Count()} Teams eingelesen!");

                    Log.Information("Daten werden in Datenbank gespeichert (in Context übertragen)");

                 //   var t = ImportController.teamteam();
                 //   unitOfWork.Teams.AddRange(t);
                    unitOfWork.SaveChanges();
                    unitOfWork.Games.AddRange(games);
                    unitOfWork.SaveChanges();
                    Log.Information("Daten wurden in DB gespeichert!");
                }
            }
        }

        private static void AnalyzeData()
        {
             using (IUnitOfWork unitOfWork = new UnitOfWork())
            {
                var teamWithMostGoals = unitOfWork.Teams.GetTeamWithMostGoals();
                PrintResult(
                $"Team mit den meisten geschossenen Toren:",
                $"{teamWithMostGoals.Team.Name}: {teamWithMostGoals.Goals} Tore");

                var teamWithMostAwayGoals = unitOfWork.Teams.GetTeamWithMostAwayGoals();
                PrintResult(
                $"Team mit den meisten geschossenen Auswärtstoren:",
                $"{teamWithMostAwayGoals.Team.Name}: {teamWithMostAwayGoals.Goals} Auswärtstore");

                var teamWithMostHomeGoals = unitOfWork.Teams.GetTeamWithMostHomeGoals();
                PrintResult(
                $"Team mit den meisten geschossenen Heimtoren:",
                $"{teamWithMostHomeGoals.Team.Name}: {teamWithMostHomeGoals.Goals} Heimtore");

                var teamWithBestGoalDifference = unitOfWork.Teams.GetTeamWithBestGoalDifference();
                PrintResult(
                $"Team mit dem besten Torverhältnis:",
                $"{teamWithBestGoalDifference.Team.Name}: {teamWithBestGoalDifference.Difference} Torverhältnis");

                var teamAverage = unitOfWork.Teams.GetTeamAverage();
                PrintResult(
                    $"Team Leistung im Durchschnitt (sortiert nach durchschn. geschossene Tore pro Spiel [absteig.]):",
                ConsoleTable
                  .From(teamAverage)
                  .Configure(o => o.NumberAlignment = Alignment.Right)
                  .ToStringAlternative());

                var teamTable = unitOfWork.Teams.GetTeamTable();
                PrintResult(
                    $"Team Tabelle (sortiert nach Rang):",
                ConsoleTable
                  .From(teamTable)
                  .Configure(o => o.NumberAlignment = Alignment.Right)
                  .ToStringAlternative());

            }

        }

        /// <summary>
        /// Erstellt eine Konsolenausgabe
        /// </summary>
        /// <param name="caption">Enthält die Überschrift</param>
        /// <param name="result">Enthält das ermittelte Ergebnise</param>
        private static void PrintResult(string caption, string result)
        {
            Console.WriteLine();

            if (!string.IsNullOrEmpty(caption))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(new String('=', caption.Length));
                Console.WriteLine(caption);
                Console.WriteLine(new String('=', caption.Length));
                Console.ResetColor();
                Console.WriteLine();
            }

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(result);
            Console.ResetColor();
            Console.WriteLine();
        }


    }
}
