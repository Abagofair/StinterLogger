using iRacingSdkWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging
{
    public class IracingLogger : IRaceLogger
    {
        #region fields
        private SdkWrapper _sdkWrapper;

        private List<TelemetryLapData> _telemetryLapData;

        private TelemetryLapData _currentLap;

        private bool _isLapCompleted;

        private SessionStates _sessionState;

        private int _driverId;
        #endregion

        public IracingLogger(int telemetryUpdateFrequency)
        {
            this._sdkWrapper = new SdkWrapper();

            this._sdkWrapper.TelemetryUpdateFrequency = telemetryUpdateFrequency;

            this._telemetryLapData = new List<TelemetryLapData>();

            this._currentLap = null;

            this._isLapCompleted = true;

            this._sessionState = SessionStates.Invalid;

            this._sdkWrapper.Connected += OnConnected;

            this._sdkWrapper.Disconnected += OnDisconnected;

            this._driverId = -1;
        }

        #region properties
        public int DriverId
        {
            get
            {
                return this._driverId;
            }

        }
        #endregion

        #region events
        public event EventHandler<LapCompletedEventArgs> LapCompleted;

        public event EventHandler<RaceStateEventArgs> RaceStateChanged;

        public event EventHandler PitRoad;

        public event EventHandler<DriverConnectionEventArgs> Connected;

        public event EventHandler Disconnected;
        #endregion

        #region event invocations
        private void OnRaceStateChange(RaceStateEventArgs e)
        {
            this.RaceStateChanged?.Invoke(this, e);
        }

        private void OnLapCompleted(LapCompletedEventArgs e)
        {
            this.LapCompleted?.Invoke(this, e);
        }

        public void OnPitRoad(EventArgs e)
        {
            this.PitRoad?.Invoke(this, e);
        }

        public void OnConnection(DriverConnectionEventArgs e)
        {
            this.Connected?.Invoke(this, e);
        }

        public void OnDisconnection(EventArgs e)
        {
            this.Disconnected?.Invoke(this, e);
        }
        #endregion

        #region public methods
        public void Start()
        {
            this._sdkWrapper.Start();
            this.RegisterTelemetryListener();
        }

        public void Stop()
        {
            this._sdkWrapper.Stop();
            this.UnRegisterTelemetryListener();
        }

        public void SetFuelLevelOnPitStop(int fuelLevel)
        {
            this._sdkWrapper.PitCommands.AddFuel(fuelLevel);
        }
        #endregion

        #region iracing updates
        private void OnTelemetryUpdate(object sender, SdkWrapper.TelemetryUpdatedEventArgs telemetryUpdatedEventArgs)
        {
            this._driverId = this._sdkWrapper.DriverId;
            var state = telemetryUpdatedEventArgs.TelemetryInfo.SessionState;
            if (state.Value != _sessionState)
            {
                _sessionState = SessionStates.Racing;
                //the race state has changed
                var raceState = new RaceStateEventArgs();
                raceState.RaceState = RaceState.GREEN;
                if (state.Value == SessionStates.Racing)
                {
                    raceState.RaceState = RaceState.GREEN;
                }
                else if (state.Value == SessionStates.Checkered)
                {
                    raceState.RaceState = RaceState.CHECKERED;
                }
                this.OnRaceStateChange(raceState);
            }

            if (this._isLapCompleted)
            {
                StartNewLap(telemetryUpdatedEventArgs.TelemetryInfo.Lap.Value);
                this._isLapCompleted = false;
            }

            bool onTrack = telemetryUpdatedEventArgs.TelemetryInfo.IsOnTrack.Value;
            bool onPitRoad = telemetryUpdatedEventArgs.TelemetryInfo.CarIdxOnPitRoad.Value[this.DriverId];
            //If the car is on track
            if (onTrack)
            {
                var onTrackReading = new OnTrackReading
                {
                    FuelLevel = telemetryUpdatedEventArgs.TelemetryInfo.FuelLevel.Value,
                    Incident = telemetryUpdatedEventArgs.TelemetryInfo.PlayerCarDriverIncidentCount.Value,
                    Speed = telemetryUpdatedEventArgs.TelemetryInfo.Speed.Value,
                    Brake = telemetryUpdatedEventArgs.TelemetryInfo.Brake.Value,
                    Throttle = telemetryUpdatedEventArgs.TelemetryInfo.Throttle.Value,
                    Rpm = telemetryUpdatedEventArgs.TelemetryInfo.RPM.Value,
                    LapDistancePct = telemetryUpdatedEventArgs.TelemetryInfo.LapDistPct.Value,
                    Gear = telemetryUpdatedEventArgs.TelemetryInfo.Gear.Value,
                    OilTemperature = telemetryUpdatedEventArgs.TelemetryInfo.OilTemp.Value,
                    TrackTempCrew = telemetryUpdatedEventArgs.TelemetryInfo.TrackTempCrew.Value,
                    UpdateTime = telemetryUpdatedEventArgs.UpdateTime
                };

                this._currentLap.Readings.Add(onTrackReading);

                var currentLapNumber = telemetryUpdatedEventArgs.TelemetryInfo.Lap.Value;
                //Has a lap been completed?
                if (currentLapNumber > this._currentLap.LapNumber)
                {
                    this._currentLap.IsCompleted = true;
                    this._currentLap.LapTime = this.GetLastLapTime();
                    this._currentLap.RemainingSessionTime = telemetryUpdatedEventArgs.TelemetryInfo.SessionTimeRemain.Value;
                    this._telemetryLapData.Add(this._currentLap);

                    //raise lap completed event
                    //should probably only be raised when the sector times has been added
                    this.OnLapCompleted(
                        new LapCompletedEventArgs
                        {
                            TelemetryLapData = this._currentLap
                        });

                    //reset current lap
                    this._isLapCompleted = true;

                    //Request session info update to fetch sector times
                    this._sdkWrapper.RequestSessionInfoUpdate();
                }
            }

            if (onPitRoad)
            {
                this.OnPitRoad(new EventArgs());
            }
        }

        private void OnSessionUpdate(object sender, SdkWrapper.SessionInfoUpdatedEventArgs sessionInfoUpdatedEventArgs)
        {
            //var query = sessionInfoUpdatedEventArgs.SessionInfo["SessionInfo"]["Sessions", 0]["CarIdx", 1]["UserName"];
            //if (query.TryGetValue)
            /*var lapCompletedEventArg = new LapCompletedEventArgs
                {
                    FuelInTank = telemetryUpdatedEventArgs.TelemetryInfo.FuelLevel.Value,
                    LapNumber = telemetryUpdatedEventArgs.TelemetryInfo.Lap.Value,
                    LapsCompleted = this._lapCount,
                    RemainingSessionTime = telemetryUpdatedEventArgs.TelemetryInfo.SessionTimeRemain.Value,
                    LapTime = this._sdkWrapper.GetTelemetryValue<float>("LapLastLapTime").Value,
                    IncidentCount = telemetryUpdatedEventArgs.TelemetryInfo.PlayerCarDriverIncidentCount.Value,
                    AverageSpeed = telemetryUpdatedEventArgs.TelemetryInfo.*/
        }

        private void OnConnected(object sender, EventArgs eventArgs)
        {
            this.OnConnection(new DriverConnectionEventArgs
            {
                DriverId = this.DriverId
            });
        }

        private void OnDisconnected(object sender, EventArgs eventArgs)
        {
            this.OnDisconnection(new EventArgs());
        }

        private float GetLastLapTime()
        {
            return this._sdkWrapper.GetTelemetryValue<float>("LapLastLapTime").Value;
        }

        private void StartNewLap(int lapNumber)
        {
            this._currentLap = new TelemetryLapData();
            this._currentLap.LapNumber = lapNumber;
            this._currentLap.IsCompleted = false;
        }

        private void RegisterTelemetryListener()
        {
            this._sdkWrapper.TelemetryUpdated += this.OnTelemetryUpdate;
        }

        private void UnRegisterTelemetryListener()
        {
            this._sdkWrapper.TelemetryUpdated -= this.OnTelemetryUpdate;
        }
        #endregion
    }
}
