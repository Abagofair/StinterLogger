using System;
using iRacingSdkWrapper;
using StinterLogger.RaceLogging.Iracing.SimEventArgs;
using StinterLogger.RaceLogging.Iracing.Models;
using StinterLogger.RaceLogging.Iracing.Fuel;
using System.Globalization;

namespace StinterLogger.RaceLogging
{
    public class iRacingLogger : ISimLogger
    {
        private const float MIN_LAP_COMPLETE_PCT = 0.985f;

        private float MIN_LAP_COMPLETE_METERS { get => (this.TrackInfo.Length * 1000.0f) - 10.0f; }

        private readonly SdkWrapper _sdkWrapper;

        private bool _inActiveSession;

#region lap tracking
        private float _currentLapDistPct;

        private float _currentLapDistM;

        private float _currentLapTime;

        private bool _isLapCompleted;

        private int _lapsCompletedSincePit;

        private int _currentSector;

        private float[] _sectorTimes;

        private float _sectorPctTarget;

        private float _sectorTarget;

        private float _sectorZoneBorder = 20.0f;

        private float _sectorDelta;

        private int _logAmount;

        private Lap _theCompletedLap;
#endregion

        private LapType LapType { get; set; }

        public iRacingLogger(float defaultTelemetryUpdateHz)
        {
            this._sdkWrapper = new SdkWrapper
            {
                TelemetryUpdateFrequency = defaultTelemetryUpdateHz
            };

            this._inActiveSession = false;

            this._theCompletedLap = new Lap();
        }

        public DriverInfo ActiveDriverInfo { get; set; }

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

        #region iracing event listeners
        private void OnTelemetryUpdate(object sender, SdkWrapper.TelemetryUpdatedEventArgs telemetryUpdatedEventArgs)
        {
            this.InvokeTelemetryRecieved(telemetryUpdatedEventArgs);

            this._currentLapDistPct = telemetryUpdatedEventArgs.TelemetryInfo.LapDistPct.Value;
            this._currentLapDistM = telemetryUpdatedEventArgs.TelemetryInfo.LapDist.Value;

            if (this._inActiveSession)
            {
                if (this.LapType == LapType.REAL)
                {
                    this._currentLapTime = this._sdkWrapper.GetTelemetryValue<float>("LapCurrentLapTime").Value;
                    bool insideSectorTimeZone = this._currentLapDistM >= (this._sectorTarget - this._sectorZoneBorder) && this._currentLapDistM <= (this._sectorTarget + this._sectorZoneBorder);

                    if (insideSectorTimeZone)
                    {
                        this._sectorTimes[this._currentSector] += this._currentLapTime - this._sectorDelta;
                        this._logAmount++;
                    }

                    if (this._logAmount > 0 && !insideSectorTimeZone)
                    {
                        //real lap completed
                        if (this._sectorTarget == MIN_LAP_COMPLETE_METERS && this._currentLapDistM > this._sectorZoneBorder)
                        {
                            this._theCompletedLap.FuelInTank = telemetryUpdatedEventArgs.TelemetryInfo.FuelLevel.Value;
                            this._currentLapTime = this._sdkWrapper.GetTelemetryValue<float>("LapLastLapTime").Value;
                            float sectorSum = 0;
                            for (int i = 0; i < this._currentSector; ++i)
                            {
                                sectorSum += this._sectorTimes[i];
                            }
                            this._sectorTimes[this._sectorTimes.Length - 1] = this._currentLapTime - sectorSum;

                            this._theCompletedLap.SectorTimes.AddRange(this._sectorTimes);
                            
                            this._theCompletedLap.LapTime = this._currentLapTime;
                            ++this._lapsCompletedSincePit;
                            this._isLapCompleted = true;
                            this._sectorDelta = 0;
                            this._currentSector = 0;
                            this._sectorPctTarget = this.TrackInfo.Sectors[1].PctOfTrack;
                        }
                        //just another sector
                        else if (this._currentLapDistM > this._sectorZoneBorder)
                        {
                            this._sectorTimes[this._currentSector] /= this._logAmount;
                            ++this._currentSector;
                            float sectorSum = 0;
                            for (int i = 0; i < this._currentSector; ++i)
                            {
                                sectorSum += this._sectorTimes[i];
                            }
                            this._sectorDelta = sectorSum;
                            this._logAmount = 0;
                            this._sectorTarget = (this._currentSector + 1) >= this._sectorTimes.Length ?
                                MIN_LAP_COMPLETE_METERS : this.TrackInfo.Sectors[this._currentSector + 1].MetersUntilSector;
                        }
                    }                  
                }

                if (this._currentLapDistM >= MIN_LAP_COMPLETE_METERS)
                {
                    if (this._lapsCompletedSincePit == 0)
                    {
                        //start new laps
                        this.LapType = LapType.REAL;
                        this._theCompletedLap = new Lap();
                        ++this._lapsCompletedSincePit;
                        
                        this._currentSector = 0;
                        this._sectorTarget = this.TrackInfo.Sectors[1].MetersUntilSector;
                    }
                }
            }
        }

        private void OnSessionUpdate(object sender, SdkWrapper.SessionInfoUpdatedEventArgs sessionInfoUpdatedEventArgs)
        {
            this.InvokeDriverConnection(sessionInfoUpdatedEventArgs);

            if (this._isLapCompleted)
            {

                this._isLapCompleted = false;
            }
        }
        #endregion

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
                this.ActiveDriverInfo = new DriverInfo();
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

                int sector = 0;
                var sectorStartPctQuery = sessionInfoUpdatedEventArgs.SessionInfo["SplitTimeInfo"]["Sectors"]["SectorNum", sector]["SectorStartPct"];
                var sectorStartPct = sectorStartPctQuery.GetValue();
                while (sectorStartPct != null)
                {
                    var sectorPct = float.Parse(sectorStartPct, CultureInfo.InvariantCulture);
                    this.TrackInfo.Sectors.Add(new Sector
                    {
                        Number = sector,
                        PctOfTrack = sectorPct,
                        MetersUntilSector = sectorPct * (length * 1000.0f)
                    });

                    ++sector;

                    sectorStartPctQuery = sessionInfoUpdatedEventArgs.SessionInfo["SplitTimeInfo"]["Sectors"]["SectorNum", sector]["SectorStartPct"];
                    sectorStartPct = sectorStartPctQuery.GetValue();
                }

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
