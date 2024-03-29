﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RoverInTheMars.Models;

namespace RoverInTheMars.Services.Rovers
{
    public class Rover : IRover
    {
        private readonly IRoverDriver _roverDriver;
        private readonly ILogger<Rover> _logger;

        public RoverStatus RoverStatus { get; internal set; }

        public string Name => _roverDriver.Name;
        public Position CurrentPosition => _roverDriver.CurrentPosition;

        public Rover()
        {
            RoverStatus = RoverStatus.Deployed;
        }

        public Rover(IRoverDriver roverDriver, ILogger<Rover> logger)
        {
            _roverDriver = roverDriver;
            _logger = logger;
            RoverStatus = RoverStatus.Deployed;
        }

        public async Task Go(RoverCommand roverCommand, Dimension plateauDimension, string roverName, CancellationToken cancellationToken = default)
        {
            RoverStatus = RoverStatus.OnAction;

            try
            {
                await _roverDriver.Drive(roverCommand, plateauDimension, roverName, cancellationToken);

                RoverStatus = RoverStatus.MissionCompleted;

                _logger.LogInformation($"{roverName} : {RoverStatus} Last Position{CurrentPosition} ############## OK ##############");
            }
            catch (DriveOffException)
            {
                RoverStatus = RoverStatus.AwaitingRescue;

                _logger.LogError($"{roverName} : avoided drive-off! {RoverStatus} Last Position{CurrentPosition} ############## DRIVE-OFF PLT. ##############");

            }
            catch (InvalidCommandException ex)
            {
                RoverStatus = RoverStatus.AwaitingRescue;

                _logger.LogError(ex, $"{roverName} : invalid command! {RoverStatus} Last Position{CurrentPosition}############## INVALID COMMAND ##############");
            }
            catch (Exception ex)
            {
                RoverStatus = RoverStatus.AwaitingRescue;

                _logger.LogError(ex, $"{roverName} : unknown Exception! {RoverStatus} Last Position{CurrentPosition}");

                throw;
            }
        }
    }
}