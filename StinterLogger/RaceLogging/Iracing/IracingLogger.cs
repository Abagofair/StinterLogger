using iRacingSdkWrapper;
using StinterLogger.RaceLogging.Iracing.IracingEventArgs;
using StinterLogger.RaceLogging.Iracing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging.Iracing
{
    public class IracingLogger : IRaceLogger
    {
        #region fields
        //The class i'm wrapping
        private readonly SdkWrapper _sdkWrapper;

#pragma warning disable IDE0044 // Add readonly modifier
        private List<TelemetryLapData> _telemetryLapData;
#pragma warning restore IDE0044 // Add readonly modifier

        private TelemetryLapData _currentLap;

        private bool _isLapCompleted;

        private SessionStates _sessionState;

        private DriverInfo _activeDriverInfo;

        private bool _inActiveSession;
        #endregion

        public IracingLogger(int telemetryUpdateFrequency)
        {
            this._sdkWrapper = new SdkWrapper
            {
                TelemetryUpdateFrequency = telemetryUpdateFrequency
            };

            this._telemetryLapData = new List<TelemetryLapData>();

            this._currentLap = null;

            this._isLapCompleted = true;

            this._sessionState = SessionStates.Invalid;

            this._sdkWrapper.Disconnected += OnDisconnected;

            this._activeDriverInfo = new DriverInfo();

            this._inActiveSession = false;
        }

        #region properties
        public DriverInfo ActiveDriverInfo
        {
            get
            {
                return this._activeDriverInfo;
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
            var state = telemetryUpdatedEventArgs.TelemetryInfo.SessionState;
            if (state.Value != _sessionState)
            {
                _sessionState = SessionStates.Racing;
                //the race state has changed
                var raceState = new RaceStateEventArgs
                {
                    RaceState = RaceState.GREEN
                };

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
            bool onPitRoad = telemetryUpdatedEventArgs.TelemetryInfo.CarIdxOnPitRoad.Value[this._sdkWrapper.DriverId];
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
            if (!this._inActiveSession)
            {
                //even though you're connected earlier, i define the connection to be active when the first sessionInfo has been recieved
                var globalIdQuery = sessionInfoUpdatedEventArgs.SessionInfo["DriverInfo"]["DriverUserID"];
                string globalId = globalIdQuery.GetValue();
                this.ActiveDriverInfo.GlobalUserId = Int32.Parse(globalId != null ? globalId : "-1");

                var localIdQuery = sessionInfoUpdatedEventArgs.SessionInfo["DriverInfo"]["DriverCarIdx"];
                string localId = localIdQuery.GetValue();
                this.ActiveDriverInfo.LocalId = Int32.Parse(localId != null ? localId : "-1");

                var usernameQuery = sessionInfoUpdatedEventArgs.SessionInfo["DriverInfo"]["Drivers"]["CarIdx", this.ActiveDriverInfo.LocalId]["UserName"];
                string userName = usernameQuery.GetValue();
                this.ActiveDriverInfo.DriverName = userName;

                this.Connected?.Invoke(this, new DriverConnectionEventArgs
                {
                    ActiveDriverInfo = this.ActiveDriverInfo
                });

                this._inActiveSession = true;
            }
        }

        private void OnDisconnected(object sender, EventArgs eventArgs)
        {
            this.OnDisconnection(eventArgs);
            this._inActiveSession = false;
        }

        private float GetLastLapTime()
        {
            return this._sdkWrapper.GetTelemetryValue<float>("LapLastLapTime").Value;
        }

        private void StartNewLap(int lapNumber)
        {
            this._currentLap = new TelemetryLapData
            {
                LapNumber = lapNumber,
                IsCompleted = false
            };
        }

        private void RegisterTelemetryListener()
        {
            this._sdkWrapper.TelemetryUpdated += this.OnTelemetryUpdate;
            this._sdkWrapper.SessionInfoUpdated += this.OnSessionUpdate;
        }

        private void UnRegisterTelemetryListener()
        {
            this._sdkWrapper.TelemetryUpdated -= this.OnTelemetryUpdate;
            this._sdkWrapper.SessionInfoUpdated -= this.OnSessionUpdate;
        }
        #endregion
    }
}
