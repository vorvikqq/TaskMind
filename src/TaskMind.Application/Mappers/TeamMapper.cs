using TaskMind.Application.DTOs.Team;
using TaskMind.Domain.Models;

namespace TaskMind.Application.Mappers
{
    public static class TeamMapper
    {
        public static Team ToTeamFromCreate(this CreateTeamRequest teamDto)
        {
            return new Team
            {
                Name = teamDto.Name,
            };
        }

        public static Team ToTeamFromUpdate(this UpdateTeamRequest teamDto)
        {
            return new Team
            {
                Id = teamDto.Id,
                Name = teamDto.Name,
            };
        }
    }
}
