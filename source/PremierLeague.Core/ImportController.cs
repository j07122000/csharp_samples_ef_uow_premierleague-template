using System;
using System.Collections.Generic;
using System.Linq;
using PremierLeague.Core.Entities;
using Utils;

namespace PremierLeague.Core
{
    public static class ImportController
    {
        public static IEnumerable<Game> ReadFromCsv()
        {
            var matrix = MyFile.ReadStringMatrixFromCsv("PremierLeague.csv", false);

            var teams = matrix
                .GroupBy(t => t[1])
                .Select(team => new Team
                {
                    Name = team.Key
                })
                .ToArray();

            var games = matrix
                .Select(g => new Game
                {
                    Round = Convert.ToInt32(g[0]),
                    HomeTeam = teams.Single(s => s.Name == g[1]),
                    GuestTeam = teams.Single(s => s.Name == g[2]),
                    GuestGoals = Convert.ToInt32(g[4]),
                    HomeGoals = Convert.ToInt32(g[3])
                   
                }).ToArray();
            return games;

        }

  
    }
}
