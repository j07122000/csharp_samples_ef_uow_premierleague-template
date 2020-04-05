using Microsoft.EntityFrameworkCore;
using PremierLeague.Core.Contracts;
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


    }
}