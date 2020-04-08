using Microsoft.EntityFrameworkCore;
using PremierLeague.Core.Contracts;
using PremierLeague.Core.DataTransferObjects;
using PremierLeague.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PremierLeague.Persistence
{
    public class TeamRepository : ITeamRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public TeamRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public IEnumerable<Team> GetAllWithGames()
        {
            return _dbContext.Teams.Include(t => t.HomeGames).Include(t => t.AwayGames).ToList();
        }

        public IEnumerable<Team> GetAll()
        {
            return _dbContext.Teams.OrderBy(t => t.Name).ToList();
        }

        public void AddRange(IEnumerable<Team> teams)
        {
            _dbContext.Teams.AddRange(teams);
        }

        public Team Get(int teamId)
        {
            return _dbContext.Teams.Find(teamId);
        }

        public void Add(Team team)
        {
            _dbContext.Teams.Add(team);
        }
        public (Team Team, int Goals) GetTeamWithMostGoals()
        {
             return _dbContext.Teams
                 .Select(t => new Tuple<Team,int>
                 (
                     t,
                     t.AwayGames.Select(s => s.GuestGoals).Sum() + t.HomeGames.Select(s => s.HomeGoals).Sum() 

                 ).ToValueTuple())
                 .AsEnumerable()
                 .OrderByDescending(o => o.Item2)
                 .First();
           

        }
        public (Team Team, int Goals) GetTeamWithMostAwayGoals()
        {
            return _dbContext.Teams
                .Select(t => new Tuple<Team, int>
                (
                    t,
                    t.AwayGames.Select(s => s.GuestGoals).Sum()
                ).ToValueTuple())
                .AsEnumerable()
                 .OrderByDescending(o => o.Item2)
                 .First();
        }
        public (Team Team, int Goals) GetTeamWithMostHomeGoals()
        {
             return _dbContext.Teams
                .Select(t => new Tuple<Team, int>
                (
                    t,
                    t.AwayGames.Select(s => s.GuestGoals).Sum()
                ).ToValueTuple())
                .AsEnumerable()
                 .OrderByDescending(o => o.Item2)
                 .First();
        }
        public (Team Team, int Difference) GetTeamWithBestGoalDifference()
        {
            return _dbContext.Teams
               .Select(t => new Tuple<Team, int>
               (
                   t,
                   (t.HomeGames.Sum(g => g.HomeGoals) + t.AwayGames.Sum(g => g.GuestGoals)) - (t.HomeGames.Sum(g => g.GuestGoals) + t.AwayGames.Sum(g => g.HomeGoals))
               ).ToValueTuple())
               .AsEnumerable()
                .OrderByDescending(o => o.Item2)
                .First();
        }

        public TeamStatisticDto[] GetTeamAverage()
        {
            return _dbContext.Teams
                .Select(s => new TeamStatisticDto()
                {
                    Name = s.Name,
                    AvgGoalsShotAtHome = s.HomeGames.Average(g => g.HomeGoals),
                    AvgGoalsShotOutwards = s.AwayGames.Average(g => g.GuestGoals),
                    AvgGoalsGotInTotal = (s.AwayGames.Average(g => g.GuestGoals) + s.HomeGames.Average(g => g.HomeGoals))/2,
                    AvgGoalsGotAtHome = s.HomeGames.Average(g => g.GuestGoals),
                    AvgGoalsGotOutwards = s.AwayGames.Average(g => g.HomeGoals),
                    AvgGoalsShotInTotal = (s.HomeGames.Average(g => g.GuestGoals) + s.AwayGames.Average(g => g.HomeGoals))/2
                })
                .OrderByDescending(o => o.AvgGoalsShotInTotal)
                .ToArray();
        }

        public  TeamTableRowDto[] GetTeamTable()
        {
            var teams = _dbContext.Teams
                 .Select(team => new
                 {
                     Id = team.Id,
                     Name = team.Name,
                     Matches = team.AwayGames.Count() + team.HomeGames.Count(),
                     Won = team.AwayGames.Where(game => game.GuestGoals > game.HomeGoals).Count()
                             + team.HomeGames.Where(game => game.HomeGoals > game.GuestGoals).Count(),
                     Lost = team.AwayGames.Where(game => game.GuestGoals < game.HomeGoals).Count()
                             + team.HomeGames.Where(game => game.HomeGoals < game.GuestGoals).Count(),
                     GoalsFor = team.AwayGames.Select(game => game.GuestGoals).Sum() +
                             team.HomeGames.Select(game => game.HomeGoals).Sum(),
                     GoalsAgainst = team.AwayGames.Select(game => game.HomeGoals).Sum() +
                             team.HomeGames.Select(game => game.GuestGoals).Sum(),

                 })

                 .ToArray();
            return teams.Select(team => new TeamTableRowDto
            {
                Id = team.Id,
                Name = team.Name,
                Matches = team.Matches,
                Won = team.Won,
                Lost = team.Lost,
                GoalsFor = team.GoalsFor,
                GoalsAgainst = team.GoalsAgainst,

            })
            .Where((team, index) => { team.Rank = index + 1; return true; })
            .OrderByDescending(t => t.Points)
            .ThenByDescending(t => t.GoalDifference)
            .ToArray();

        }





    }
}