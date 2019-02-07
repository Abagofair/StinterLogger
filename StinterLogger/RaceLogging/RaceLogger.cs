using iRacingSdkWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StinterLogger.RaceLogging
{
    public class RaceLogger : IRaceLogger
    {
        private SdkWrapper _sdkWrapper;

        private List<TelemetryLapData> _telemetryLapData = null;
        private TelemetryLapData _currentLap = null;

        private bool _isLapCompleted = true;

        public RaceLogger(int telemetryUpdateFrequency)
        {
            this._sdkWrapper = new SdkWrapper();
            this._sdkWrapper.TelemetryUpdateFrequency = telemetryUpdateFrequency;
            this._telemetryLapData = new List<TelemetryLapData>();
        }

        public event EventHandler<LapCompletedEventArgs> LapCompleted;

        private void OnLapCompleted(LapCompletedEventArgs e)
        {
            LapCompleted?.Invoke(this, e);
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

        private void OnTelemetryUpdate(object sender, SdkWrapper.TelemetryUpdatedEventArgs telemetryUpdatedEventArgs)
        {
            if (this._isLapCompleted)
            {
                StartNewLap(telemetryUpdatedEventArgs.TelemetryInfo.Lap.Value);
                this._isLapCompleted = false;
            }

            bool onTrack = telemetryUpdatedEventArgs.TelemetryInfo.IsOnTrack.Value;
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
                    ClassPosition = telemetryUpdatedEventArgs.TelemetryInfo.CarIdxClassPosition.Value[this._sdkWrapper.DriverId],
                    OverallPosition = telemetryUpdatedEventArgs.TelemetryInfo.CarIdxPosition.Value[this._sdkWrapper.DriverId],
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

        private void RegisterTelemetryListener()
        {
            this._sdkWrapper.TelemetryUpdated += this.OnTelemetryUpdate;
        }

        private void UnRegisterTelemetryListener()
        {
            this._sdkWrapper.TelemetryUpdated -= this.OnTelemetryUpdate;
        }

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
    }
}
