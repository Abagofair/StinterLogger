using System;
using iRacingSdkWrapper;
using System.Globalization;
using RaceLogging.Timing;
using RaceLogging.General.SimEventArgs;
using RaceLogging.General.Entities;
using RaceLogging.General.Enums;

namespace RaceLogging.Simulations.Iracing
{
    public class iRacingLogger : ISimLogger
    {
        private float Min_Lap_Complete_Meters { get => (this.CurrentTrack.Length * 1000.0f) - 10.0f; }

        private readonly SdkWrapper _sdkWrapper;

        private bool _inActiveSession;

        private int _incidents;

        private Driver _currentDriver;

        private Track _currentTrack;

        private Car _currentCar;

        #region lap tracking
        private bool _isLapComplete;

        private readonly TimeEstimation _timeEstimation;

        private Lap _currentLap;
        #endregion

        public iRacingLogger(float telemetryUpdateHz)
        {
            this._sdkWrapper = new SdkWrapper
            {
                TelemetryUpdateFrequency = telemetryUpdateHz
            };

            this._inActiveSession = false;

            this._currentLap = null;

            this._timeEstimation = new TimeEstimation();

            this._isLapComplete = true;

            this._currentDriver = new Driver();

            this._currentTrack = new Track();

            this._currentCar = new Car();
        }

        public Driver CurrentDriver { get => this._currentDriver; }

        public Track CurrentTrack { get => this._currentTrack; }

        public Car CurrentCar { get => this._currentCar; }

        public bool IsLive { get => this._sdkWrapper.IsConnected && this._sdkWrapper.IsRunning; }

        public event EventHandler<TelemetryEventArgs> TelemetryRecieved;
        private void OnTelemetryRecieved(TelemetryEventArgs e)
        {
            //this._debugLogger.CreateEventLog("Race state changed");
            this.TelemetryRecieved?.Invoke(this, e);
        }

        public event EventHandler<LapCompletedEventArgs> LapCompleted;
        private void OnLapCompleted(LapCompletedEventArgs e)
        {
            //this._debugLogger.CreateEventLog("Race state changed");
            this.LapCompleted?.Invoke(this, e);
        }

        public event EventHandler<DriverConnectionEventArgs> DriverConnected;
        private void OnDriverConnected(DriverConnectionEventArgs e)
        {
            //this._debugLogger.CreateEventLog("Race state changed");
            this.DriverConnected?.Invoke(this, e);
        }

        public event EventHandler<DriverConnectionEventArgs> DriverDisconnected;
        private void OnDriverDisconnected(DriverConnectionEventArgs e)
        {
            //this._debugLogger.CreateEventLog("Race state changed");
            this.DriverDisconnected?.Invoke(this, e);
        }

        public event EventHandler<TrackLocationEventArgs> TrackLocationChange;
        private void OnTrackLocationChange(TrackLocationEventArgs e)
        {
            this.TrackLocationChange?.Invoke(this, e);
        }

        public event EventHandler<TireEventArgs> TireWearUpdated;
        private void OnTireWearUpdated(TireEventArgs e)
        {
            this.TireWearUpdated?.Invoke(this, e);
        }

        public event EventHandler<PitRoadEventArgs> PitRoad;
        private void OnPitRoad(PitRoadEventArgs e)
        {
            this.PitRoad?.Invoke(this, e);
        }

        public void AddFuelOnPitStop(int fuelToAdd)
        {
            if (fuelToAdd > 0)
            {
                this._sdkWrapper.PitCommands.AddFuel(fuelToAdd);
            }
        }

        public void SetTelemetryUpdateHz(double hz)
        {
            this._sdkWrapper.TelemetryUpdateFrequency = hz;
        }

        public void StartListening()
        {
            this._sdkWrapper.Start();

            this._sdkWrapper.TelemetryUpdated += this.OnTelemetryUpdate;
            this._sdkWrapper.SessionInfoUpdated += this.OnSessionUpdate;
            this._sdkWrapper.Disconnected += this.InvokeDriverDisconnect;
        }

        public void StopListening()
        {
            this._sdkWrapper.TelemetryUpdated -= this.OnTelemetryUpdate;
            this._sdkWrapper.SessionInfoUpdated -= this.OnSessionUpdate;
            this._sdkWrapper.Disconnected -= this.InvokeDriverDisconnect;

            this._sdkWrapper.Stop();
        }

        #region iracing event listeners
        private void OnTelemetryUpdate(object sender, SdkWrapper.TelemetryUpdatedEventArgs telemetryUpdatedEventArgs)
        {
            if (this._inActiveSession)
            {
                this.InvokeTelemetryRecieved(telemetryUpdatedEventArgs);
                this.InvokeLapCompleted(telemetryUpdatedEventArgs);
                this.InvokePitRoad(telemetryUpdatedEventArgs);
                this.InvokeTrackLocationChange(telemetryUpdatedEventArgs);
                this.InvokeTireWearUpdated(telemetryUpdatedEventArgs);
            }
        }

        private void OnSessionUpdate(object sender, SdkWrapper.SessionInfoUpdatedEventArgs sessionInfoUpdatedEventArgs)
        {
            this.InvokeDriverConnection(sessionInfoUpdatedEventArgs);
        }
        #endregion

        private bool IsDriverOnPitRoad(SdkWrapper.TelemetryUpdatedEventArgs eventArgs)
        {
            bool carOnPitRoad = eventArgs.TelemetryInfo.CarIdxOnPitRoad.Value[this.CurrentDriver.LocalId];
            bool carOnTrack = eventArgs.TelemetryInfo.IsOnTrack.Value;
            return carOnPitRoad && carOnTrack;
        }

        private float GetCurrentLapTime()
        {
            return this._sdkWrapper.GetTelemetryValue<float>("LapCurrentLapTime").Value;
        }

        private float GetLastLapTime()
        {
            return this._sdkWrapper.GetTelemetryValue<float>("LapLastLapTime").Value;
        }

        private void InvokePitRoad(SdkWrapper.TelemetryUpdatedEventArgs eventArgs)
        {
            var isOnPitRoad = this.IsDriverOnPitRoad(eventArgs);
            this.OnPitRoad(new PitRoadEventArgs
            {
                IsOnPitRoad = isOnPitRoad
            });
        }

        private void InvokeTireWearUpdated(SdkWrapper.TelemetryUpdatedEventArgs telemetryUpdatedEventArgs)
        {
            var trackSurface = telemetryUpdatedEventArgs.TelemetryInfo.CarIdxTrackSurface.Value[this.CurrentDriver.LocalId];
            if (trackSurface == TrackSurfaces.InPitStall)
            {
                var tire = new Tire
                {
                    LFwearL = this._sdkWrapper.GetTelemetryValue<float>("LFwearL").Value,
                    LFwearM = this._sdkWrapper.GetTelemetryValue<float>("LFwearM").Value,
                    LFwearR = this._sdkWrapper.GetTelemetryValue<float>("LFwearR").Value,
                    RFwearL = this._sdkWrapper.GetTelemetryValue<float>("RFwearL").Value,
                    RFwearM = this._sdkWrapper.GetTelemetryValue<float>("RFwearM").Value,
                    RFwearR = this._sdkWrapper.GetTelemetryValue<float>("RFwearR").Value,
                    RRwearL = this._sdkWrapper.GetTelemetryValue<float>("RRwearL").Value,
                    RRwearM = this._sdkWrapper.GetTelemetryValue<float>("RRwearM").Value,
                    RRwearR = this._sdkWrapper.GetTelemetryValue<float>("RRwearR").Value,
                    LRwearL = this._sdkWrapper.GetTelemetryValue<float>("LRwearL").Value,
                    LRwearM = this._sdkWrapper.GetTelemetryValue<float>("LRwearM").Value,
                    LRwearR = this._sdkWrapper.GetTelemetryValue<float>("LRwearR").Value
                };
                this.OnTireWearUpdated(new TireEventArgs
                {
                    Tires = tire
                });
            }
        }

        private void InvokeTrackLocationChange(SdkWrapper.TelemetryUpdatedEventArgs telemetryUpdatedEventArgs)
        {
            var trackSurface = telemetryUpdatedEventArgs.TelemetryInfo.CarIdxTrackSurface.Value[this.CurrentDriver.LocalId];
            TrackLocation trackLocation;
            switch (trackSurface)
            {
                case TrackSurfaces.AproachingPits:
                    trackLocation = TrackLocation.ApproachingPits;
                    break;
                case TrackSurfaces.InPitStall:
                    trackLocation = TrackLocation.InPitStall;
                    break;
                case TrackSurfaces.OffTrack:
                    trackLocation = TrackLocation.OffTrack;
                    break;
                case TrackSurfaces.OnTrack:
                    trackLocation = TrackLocation.OnTrack;
                    break;
                default:
                    trackLocation = TrackLocation.NotInWorld;
                    break;
            }

            this.OnTrackLocationChange(new TrackLocationEventArgs
            {
                TrackLocation = trackLocation
            });
        }

        private void InvokeLapCompleted(SdkWrapper.TelemetryUpdatedEventArgs telemetryUpdatedEventArgs)
        {
            if (this._isLapComplete)
            {
                //it appears iracing doesnt start logging lap times until first out lap after a reset
                this._currentLap = new Lap
                {
                    LapNumber = telemetryUpdatedEventArgs.TelemetryInfo.Lap.Value,
                    FuelInTankAtStart = telemetryUpdatedEventArgs.TelemetryInfo.FuelLevel.Value,
                    Car = this._currentCar,
                    Driver = this._currentDriver,
                    Track = this._currentTrack,
                    TrackTemp = telemetryUpdatedEventArgs.TelemetryInfo.TrackTemp.Value,
                    AirTemp = telemetryUpdatedEventArgs.TelemetryInfo.AirTemp.Value
                };
                this._timeEstimation.ResetData();
                this._isLapComplete = false;
                this._incidents = telemetryUpdatedEventArgs.TelemetryInfo.PlayerCarDriverIncidentCount.Value;
            }

            float currentLapDistM = telemetryUpdatedEventArgs.TelemetryInfo.LapDist.Value;
            float currentLapTime = this.GetCurrentLapTime();

            //Did we pass the finish line?
            //finish line has been crossed
            if (this._currentLap.LapNumber < telemetryUpdatedEventArgs.TelemetryInfo.Lap.Value
                && currentLapDistM >= 40.0f)
            {
                currentLapTime = this.GetLastLapTime();
                //if iracing cant provide a lap time, see if we cant figure it out on based on the logged (distance, time) data
                if (currentLapTime <= 0.0f)
                {
                    currentLapTime = this._timeEstimation.GetClosestTimeToDistance(Min_Lap_Complete_Meters + 10.0f);
                }
                if (currentLapTime <= 0.0f)
                {
                    currentLapTime = this._timeEstimation.TravelTimeToPoint(Min_Lap_Complete_Meters + 10.0f);
                }

                var sectorSum = 0.0f;
                for (int currentSector = 0; currentSector < this.CurrentTrack.Sectors.Count; ++currentSector)
                {
                    var sector = this.CurrentTrack.Sectors[currentSector];
                    var sectorTimeEstimate = this._timeEstimation.GetClosestTimeToDistance(
                        sector.MetersUntilSectorStarts);

                    if (sectorTimeEstimate > 0.0f)
                    {
                        sectorTimeEstimate -= sectorSum;
                        sectorSum += sectorTimeEstimate;
                    }

                    this._currentLap.Time.SectorTimes.Add(sectorTimeEstimate);
                }

                this._currentLap.Time.LapTime = currentLapTime;

                int actualIncCount = telemetryUpdatedEventArgs.TelemetryInfo.PlayerCarDriverIncidentCount.Value;
                if (actualIncCount > this._incidents)
                {
                    this._currentLap.Incidents = actualIncCount - this._incidents;
                }
                else
                {
                    this._currentLap.Incidents = 0;
                }

                if (this.IsDriverOnPitRoad(telemetryUpdatedEventArgs))
                {
                    this._currentLap.Pit = new Pit();
                }

                this._currentLap.FuelInTankAtFinish = telemetryUpdatedEventArgs.TelemetryInfo.FuelLevel.Value;
                this._currentLap.RemainingSessionTime = (float)telemetryUpdatedEventArgs.TelemetryInfo.SessionTimeRemain.Value;

                this.OnLapCompleted(new LapCompletedEventArgs
                {
                    Lap = this._currentLap
                });

                this._isLapComplete = true;
            }
            else if (this._currentLap.LapNumber == telemetryUpdatedEventArgs.TelemetryInfo.Lap.Value)
            {
                this._timeEstimation.AddDistanceTimeValue(currentLapDistM, currentLapTime);
            }
        }

        private void InvokeTelemetryRecieved(SdkWrapper.TelemetryUpdatedEventArgs telemetryUpdatedEventArgs)
        {
            var telemetry = new Telemetry
            {
                FuelInTank = telemetryUpdatedEventArgs.TelemetryInfo.FuelLevel.Value,
                Speed = telemetryUpdatedEventArgs.TelemetryInfo.Speed.Value,
                BrakePressurePct = telemetryUpdatedEventArgs.TelemetryInfo.Brake.Value,
                ThrottlePressurePct = telemetryUpdatedEventArgs.TelemetryInfo.Throttle.Value,
                Rpm = telemetryUpdatedEventArgs.TelemetryInfo.RPM.Value,
                Gear = telemetryUpdatedEventArgs.TelemetryInfo.Gear.Value
            };

            var telemetryEventArgs = new TelemetryEventArgs
            {
                Telemetry = telemetry
            };

            this.OnTelemetryRecieved(telemetryEventArgs);
        }

        private void InvokeDriverConnection(SdkWrapper.SessionInfoUpdatedEventArgs sessionInfoUpdatedEventArgs)
        {
            if (!this._inActiveSession)
            {
                //even though you're connected earlier, i define the connection to be active when the first sessionInfo has been recieved
                //that way there is info about the connected driver i can post
                var globalId = sessionInfoUpdatedEventArgs.SessionInfo["DriverInfo"]["DriverUserID"].GetValue();
                this._currentDriver.GlobalUserId = int.Parse(globalId ?? "-1");

                var localId = sessionInfoUpdatedEventArgs.SessionInfo["DriverInfo"]["DriverCarIdx"].GetValue();
                this._currentDriver.LocalId = int.Parse(localId ?? "-1");

                var usernameQuery = sessionInfoUpdatedEventArgs.SessionInfo["DriverInfo"]["Drivers"]["CarIdx", this.CurrentDriver.LocalId]["UserName"];
                string userName = usernameQuery.GetValue();
                this._currentDriver.DriverName = userName;

                var carName = sessionInfoUpdatedEventArgs.SessionInfo["DriverInfo"]["Drivers"]["CarIdx", this.CurrentDriver.LocalId]["CarScreenName"].GetValue();
                this._currentCar.Name = carName;

                var trackLengthString = sessionInfoUpdatedEventArgs.SessionInfo["WeekendInfo"]["TrackLength"].GetValue();
                var str = trackLengthString != null ? trackLengthString.Split(' ')[0] : "-1.0";
                float length = float.Parse(str, CultureInfo.InvariantCulture);
                this._currentTrack.Length = length;

                var trackName = sessionInfoUpdatedEventArgs.SessionInfo["WeekendInfo"]["TrackName"].GetValue();
                this._currentTrack.Name = trackName;

                var trackDisplayName = sessionInfoUpdatedEventArgs.SessionInfo["WeekendInfo"]["TrackDisplayName"].GetValue();
                this._currentTrack.DisplayName = trackName;

                var trackCountry = sessionInfoUpdatedEventArgs.SessionInfo["WeekendInfo"]["TrackCountry"].GetValue();
                this._currentTrack.Country = trackCountry;

                int sector = 1;
                var sectorStartPct = sessionInfoUpdatedEventArgs.SessionInfo["SplitTimeInfo"]["Sectors"]["SectorNum", sector]["SectorStartPct"].GetValue();
                while (sectorStartPct != null)
                {
                    var sectorPct = float.Parse(sectorStartPct, CultureInfo.InvariantCulture);
                    this._currentTrack.Sectors.Add(new Sector
                    {
                        Number = sector,
                        PctOfTrack = sectorPct,
                        MetersUntilSectorStarts = sectorPct * (length * 1000.0f)
                    });

                    ++sector;

                    sectorStartPct = sessionInfoUpdatedEventArgs.SessionInfo["SplitTimeInfo"]["Sectors"]["SectorNum", sector]["SectorStartPct"].GetValue();
                }

                //the first sector is always zero length
                //Add the finish as the final sector end
                this._currentTrack.Sectors.Add(new Sector
                {
                    Number = sector,
                    PctOfTrack = 1.0f,
                    MetersUntilSectorStarts = (length * 1000.0f)
                });

                this._inActiveSession = true;

                this.OnDriverConnected(new DriverConnectionEventArgs
                {
                    CurrentDriver = this._currentDriver
                });
            }
        }

        private void InvokeDriverDisconnect(object sender, EventArgs eventArgs)
        {
            this.OnDriverDisconnected(new DriverConnectionEventArgs
            {
                CurrentDriver = this._currentDriver
            });
        }
    }
}
