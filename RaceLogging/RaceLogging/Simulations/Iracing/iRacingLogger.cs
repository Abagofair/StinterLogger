using System;
using iRacingSdkWrapper;
using System.Globalization;
using StinterLogger.RaceLogging.Timing;
using StinterLogger.RaceLogging.General.Models;
using StinterLogger.RaceLogging.General.SimEventArgs;
using StinterLogger.RaceLogging.General.Fuel;

namespace StinterLogger.RaceLogging.Simulations.Iracing
{
    public class iRacingLogger : ISimLogger
    {
        private float Min_Lap_Complete_Meters { get => (this.TrackInfo.Length * 1000.0f) - 10.0f; }

        private readonly SdkWrapper _sdkWrapper;

        private bool _inActiveSession;

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
        }

        public Driver ActiveDriverInfo { get; set; }

        public Track TrackInfo { get; set; }

        public bool IsLive { get; set; }

        public event EventHandler<TelemetryEventArgs> TelemetryRecieved;
        private void OnTelemetryRecieved(TelemetryEventArgs e)
        {
            //this._debugLogger.CreateEventLog("Race state changed");
            this.TelemetryRecieved?.Invoke(this, e);
        }

        public event EventHandler<DriverStateEventArgs> DriverStateChanged;
        private void OnDriverStateChanged(DriverStateEventArgs e)
        {
            //this._debugLogger.CreateEventLog("Race state changed");
            this.DriverStateChanged?.Invoke(this, e);
        }

        public event EventHandler RaceStateChanged;
        private void OnRaceStateChanged(EventArgs e)
        {
            //this._debugLogger.CreateEventLog("Race state changed");
            this.RaceStateChanged?.Invoke(this, null);
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

        public void SetFuelLevelOnPitStop(int fuelToAdd)
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

            this.IsLive = true;
        }

        public void StopListening()
        {
            this._sdkWrapper.TelemetryUpdated -= this.OnTelemetryUpdate;
            this._sdkWrapper.SessionInfoUpdated -= this.OnSessionUpdate;
            this._sdkWrapper.Disconnected -= this.InvokeDriverDisconnect;

            this._sdkWrapper.Stop();

            this.IsLive = false;
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
            bool carOnPitRoad = eventArgs.TelemetryInfo.CarIdxOnPitRoad.Value[this.ActiveDriverInfo.LocalId];
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
            var trackSurface = telemetryUpdatedEventArgs.TelemetryInfo.CarIdxTrackSurface.Value[this.ActiveDriverInfo.LocalId];
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
                    TireWear = tire
                });
            }
        }

        private void InvokeTrackLocationChange(SdkWrapper.TelemetryUpdatedEventArgs telemetryUpdatedEventArgs)
        {
            var trackSurface = telemetryUpdatedEventArgs.TelemetryInfo.CarIdxTrackSurface.Value[this.ActiveDriverInfo.LocalId];
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
                    FuelInTankAtStart = telemetryUpdatedEventArgs.TelemetryInfo.FuelLevel.Value
                };
                this._timeEstimation.ResetData();
                this._isLapComplete = false;
            }

            float currentLapDistM = telemetryUpdatedEventArgs.TelemetryInfo.LapDist.Value;
            float currentLapTime = this.GetCurrentLapTime();

            //Did we pass the finish line?
            //finish line has been crossed
            if (this._currentLap.LapNumber < telemetryUpdatedEventArgs.TelemetryInfo.Lap.Value
                && currentLapDistM >= 20.0f)
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
                for (int currentSector = 0; currentSector < this.TrackInfo.Sectors.Count; ++currentSector)
                {
                    var sector = this.TrackInfo.Sectors[currentSector];
                    var sectorTimeEstimate = this._timeEstimation.GetClosestTimeToDistance(
                        sector.MetersUntilSectorEnd);
                    sectorTimeEstimate -= sectorSum;
                    sectorSum += sectorTimeEstimate;
                    this._currentLap.SectorTimes.Add(sectorTimeEstimate);
                }

                this._currentLap.LapTime = currentLapTime;
                this._currentLap.Incidents = telemetryUpdatedEventArgs.TelemetryInfo.PlayerCarMyIncidentCount.Value;
                this._currentLap.RaceLaps = telemetryUpdatedEventArgs.TelemetryInfo.RaceLaps.Value;
                this._currentLap.PlayerCarPosition = telemetryUpdatedEventArgs.TelemetryInfo.CarIdxPosition.Value[this.ActiveDriverInfo.LocalId];
                this._currentLap.PlayerCarClassPosition = telemetryUpdatedEventArgs.TelemetryInfo.CarIdxClassPosition.Value[this.ActiveDriverInfo.LocalId];
                this._currentLap.InPit = this.IsDriverOnPitRoad(telemetryUpdatedEventArgs);
                this._currentLap.FuelInTankAtFinish = telemetryUpdatedEventArgs.TelemetryInfo.FuelLevel.Value;

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
                this.ActiveDriverInfo = new Driver();
                this.TrackInfo = new Track();

                //even though you're connected earlier, i define the connection to be active when the first sessionInfo has been recieved
                //that way there is info about the connected driver i can post
                var globalId = sessionInfoUpdatedEventArgs.SessionInfo["DriverInfo"]["DriverUserID"].GetValue();
                this.ActiveDriverInfo.GlobalUserId = int.Parse(globalId ?? "-1");

                var localId = sessionInfoUpdatedEventArgs.SessionInfo["DriverInfo"]["DriverCarIdx"].GetValue();
                this.ActiveDriverInfo.LocalId = int.Parse(localId ?? "-1");

                var usernameQuery = sessionInfoUpdatedEventArgs.SessionInfo["DriverInfo"]["Drivers"]["CarIdx", this.ActiveDriverInfo.LocalId]["UserName"];
                string userName = usernameQuery.GetValue();
                this.ActiveDriverInfo.DriverName = userName;

                var carName = sessionInfoUpdatedEventArgs.SessionInfo["DriverInfo"]["Drivers"]["CarIdx", this.ActiveDriverInfo.LocalId]["CarScreenName"].GetValue();
                this.ActiveDriverInfo.CarNameLong = carName;

                this.ActiveDriverInfo.Unit = this._sdkWrapper.GetTelemetryValue<int>("DisplayUnits").Value > 0 ? FuelUnit.Liters : FuelUnit.Gallons;

                this.OnDriverConnected(new DriverConnectionEventArgs
                {
                    ActiveDriverInfo = this.ActiveDriverInfo
                });

                var trackLengthString = sessionInfoUpdatedEventArgs.SessionInfo["WeekendInfo"]["TrackLength"].GetValue();
                var str = trackLengthString != null ? trackLengthString.Split(' ')[0] : "-1.0";
                float length = float.Parse(str, CultureInfo.InvariantCulture);
                this.TrackInfo.Length = length;

                var trackName = sessionInfoUpdatedEventArgs.SessionInfo["WeekendInfo"]["TrackName"].GetValue();
                this.TrackInfo.Name = trackName;

                var trackDisplayName = sessionInfoUpdatedEventArgs.SessionInfo["WeekendInfo"]["TrackDisplayName"].GetValue();
                this.TrackInfo.DisplayName = trackName;

                var trackCountry = sessionInfoUpdatedEventArgs.SessionInfo["WeekendInfo"]["TrackCountry"].GetValue();
                this.TrackInfo.Country = trackCountry;

                var trackSurfaceTemp = sessionInfoUpdatedEventArgs.SessionInfo["WeekendInfo"]["TrackSurfaceTemp"].GetValue().Split(' ')[0];
                this.TrackInfo.SurfaceTemp = float.Parse(trackSurfaceTemp, CultureInfo.InvariantCulture);

                var trackAirTemp = sessionInfoUpdatedEventArgs.SessionInfo["WeekendInfo"]["TrackAirTemp"].GetValue().Split(' ')[0];
                this.TrackInfo.AirTemp = float.Parse(trackAirTemp, CultureInfo.InvariantCulture);

                int sector = 1;
                var sectorStartPct = sessionInfoUpdatedEventArgs.SessionInfo["SplitTimeInfo"]["Sectors"]["SectorNum", sector]["SectorStartPct"].GetValue();
                while (sectorStartPct != null)
                {
                    var sectorPct = float.Parse(sectorStartPct, CultureInfo.InvariantCulture);
                    this.TrackInfo.Sectors.Add(new Sector
                    {
                        Number = sector,
                        PctOfTrack = sectorPct,
                        MetersUntilSectorEnd = sectorPct * (length * 1000.0f)
                    });

                    ++sector;

                    sectorStartPct = sessionInfoUpdatedEventArgs.SessionInfo["SplitTimeInfo"]["Sectors"]["SectorNum", sector]["SectorStartPct"].GetValue();
                }

                //the first sector is always zero length
                //i add the finish as the final sector end
                this.TrackInfo.Sectors.Add(new Sector
                {
                    Number = sector,
                    PctOfTrack = 1.0f,
                    MetersUntilSectorEnd = (length * 1000.0f)
                });

                this._inActiveSession = true;
            }
        }

        private void InvokeDriverDisconnect(object sender, EventArgs eventArgs)
        {
            this.OnDriverDisconnected(new DriverConnectionEventArgs
            {
                ActiveDriverInfo = this.ActiveDriverInfo
            });
        }
    }
}
