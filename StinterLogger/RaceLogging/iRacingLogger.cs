using System;
using iRacingSdkWrapper;
using StinterLogger.RaceLogging.Iracing.SimEventArgs;
using StinterLogger.RaceLogging.Iracing.Models;
using StinterLogger.RaceLogging.Iracing.Fuel;
using System.Globalization;
using System.Collections.Generic;

namespace StinterLogger.RaceLogging
{
    public class iRacingLogger : ISimLogger
    {
        private float Min_Lap_Complete_Meters { get => (this.TrackInfo.Length * 1000.0f) - 10.0f; }

        private readonly SdkWrapper _sdkWrapper;

        private bool _inActiveSession;

        #region lap tracking
        private int _lap;

        private float[] _sectorTimes;

        private bool _isLapComplete;

        private TimeEstimation _timeEstimation;

        private Lap _theCompletedLap;
        #endregion

        private List<Lap> List { get; set; }

        public iRacingLogger(float defaultTelemetryUpdateHz)
        {
            this._sdkWrapper = new SdkWrapper
            {
                TelemetryUpdateFrequency = defaultTelemetryUpdateHz
            };

            this._inActiveSession = false;

            this._theCompletedLap = null;

            this._timeEstimation = new TimeEstimation();

            this._isLapComplete = true;

            this.List = new List<Lap>();
        }

        public Driver ActiveDriverInfo { get; set; }

        public Track TrackInfo { get; set; }

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

        private float GetCurrentLapTime()
        {
            return this._sdkWrapper.GetTelemetryValue<float>("LapCurrentLapTime").Value;
        }

        private float GetLastLapTime()
        {
            return this._sdkWrapper.GetTelemetryValue<float>("LapLastLapTime").Value;
        }

        #region iracing event listeners
        private void OnTelemetryUpdate(object sender, SdkWrapper.TelemetryUpdatedEventArgs telemetryUpdatedEventArgs)
        {
            this.InvokeTelemetryRecieved(telemetryUpdatedEventArgs);

            if (this.List.Count >= 2)
            {
                Console.WriteLine();
            }

            if (this._inActiveSession)
            {               
                if (this._isLapComplete)
                {
                    //it appears iracing doesnt start logging lap times until first out lap after a reset
                    this._theCompletedLap = new Lap
                    {
                        LapNumber = telemetryUpdatedEventArgs.TelemetryInfo.Lap.Value
                    };
                    this._timeEstimation.ResetData();
                    this._isLapComplete = false;
                }

                float currentLapDistM = telemetryUpdatedEventArgs.TelemetryInfo.LapDist.Value;
                this._theCompletedLap.InPit = this.IsDriverOnPitRoad(telemetryUpdatedEventArgs);

                float currentLapTime = this.GetCurrentLapTime();

                //Did we pass the finish line?
                //real lap completed
                if (this._theCompletedLap.LapNumber < telemetryUpdatedEventArgs.TelemetryInfo.Lap.Value)
                {
                    this._theCompletedLap.FuelInTank = telemetryUpdatedEventArgs.TelemetryInfo.FuelLevel.Value;
                    currentLapTime = this.GetLastLapTime();
                    if (currentLapTime <= -1.0f)
                    {
                        currentLapTime = this._timeEstimation.TravelTimeToPoint(Min_Lap_Complete_Meters + 10.0f);
                    }

                    var sectorSum = 0.0f;
                    for (int currentSector = 0; currentSector < this.TrackInfo.Sectors.Count; ++currentSector)
                    {
                        var sector = this.TrackInfo.Sectors[currentSector];
                        var sectorTimeEstimate = this._timeEstimation.GetSectorTime(
                            sector.MetersUntilSectorEnd);
                        sectorTimeEstimate -= sectorSum;
                        sectorSum += sectorTimeEstimate;
                        this._theCompletedLap.SectorTimes.Add(sectorTimeEstimate);
                    }

                    this._theCompletedLap.LapTime = currentLapTime;
                    this._theCompletedLap.LapNumber = this._lap;

                    this.List.Add(this._theCompletedLap);

                    this.OnLapCompleted(new LapCompletedEventArgs
                    {
                        Lap = this._theCompletedLap
                    });

                    this._isLapComplete = true;
                }
                else
                {
                    this._timeEstimation.AddDistanceTimeValue(currentLapDistM, currentLapTime);
                }
            }
        }

        private void OnSessionUpdate(object sender, SdkWrapper.SessionInfoUpdatedEventArgs sessionInfoUpdatedEventArgs)
        {
            this.InvokeDriverConnection(sessionInfoUpdatedEventArgs);
        }
        #endregion

        private float GetSumOfSectorTimes(int n)
        {
            float sectorSum = 0;
            for (int i = 0; i < n; ++i)
            {
                sectorSum += this._sectorTimes[i];
            }
            return sectorSum;
        }

        private void ResetSectorTimes()
        {
            for (int i = 0; i < this._sectorTimes.Length; ++i)
            {
                this._sectorTimes[i] = 0.0f;
            }
        }

        private bool IsDriverOnPitRoad(SdkWrapper.TelemetryUpdatedEventArgs eventArgs)
        {
            bool carOnPitRoad = eventArgs.TelemetryInfo.CarIdxOnPitRoad.Value[this.ActiveDriverInfo.LocalId];
            bool carOnTrack = eventArgs.TelemetryInfo.IsOnTrack.Value;
            return carOnPitRoad && carOnTrack;
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
                var globalIdQuery = sessionInfoUpdatedEventArgs.SessionInfo["DriverInfo"]["DriverUserID"];
                string globalId = globalIdQuery.GetValue();
                this.ActiveDriverInfo.GlobalUserId = int.Parse(globalId != null ? globalId : "-1");

                var localIdQuery = sessionInfoUpdatedEventArgs.SessionInfo["DriverInfo"]["DriverCarIdx"];
                string localId = localIdQuery.GetValue();
                this.ActiveDriverInfo.LocalId = int.Parse(localId != null ? localId : "-1");

                var usernameQuery = sessionInfoUpdatedEventArgs.SessionInfo["DriverInfo"]["Drivers"]["CarIdx", this.ActiveDriverInfo.LocalId]["UserName"];
                string userName = usernameQuery.GetValue();
                this.ActiveDriverInfo.DriverName = userName;

                this.ActiveDriverInfo.Unit = this._sdkWrapper.GetTelemetryValue<int>("DisplayUnits").Value > 0 ? FuelUnit.Liters : FuelUnit.Gallons;

                this.OnDriverConnected(new DriverConnectionEventArgs
                {
                    ActiveDriverInfo = this.ActiveDriverInfo
                });

                var trackLengthQuery = sessionInfoUpdatedEventArgs.SessionInfo["WeekendInfo"]["TrackLength"];
                var trackLengthString = trackLengthQuery.GetValue();
                var str = trackLengthString != null ? trackLengthString.Split(' ')[0] : "-1.0";
                float length = float.Parse(str, CultureInfo.InvariantCulture);
                this.TrackInfo.Length = length;

                var trackNameQuery = sessionInfoUpdatedEventArgs.SessionInfo["WeekendInfo"]["TrackName"];
                var trackName = trackNameQuery.GetValue();
                this.TrackInfo.Name = trackName;

                int sector = 1;
                var sectorStartPctQuery = sessionInfoUpdatedEventArgs.SessionInfo["SplitTimeInfo"]["Sectors"]["SectorNum", sector]["SectorStartPct"];
                var sectorStartPct = sectorStartPctQuery.GetValue();
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

                    sectorStartPctQuery = sessionInfoUpdatedEventArgs.SessionInfo["SplitTimeInfo"]["Sectors"]["SectorNum", sector]["SectorStartPct"];
                    sectorStartPct = sectorStartPctQuery.GetValue();
                }

                this.TrackInfo.Sectors.Add(new Sector
                {
                    Number = sector,
                    PctOfTrack = 100,
                    MetersUntilSectorEnd = (length * 1000.0f)
                });

                this._sectorTimes = new float[sector];

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
