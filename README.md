# StinterLogger
Data gathering library and application for racing games

# RaceLogging

iRacing logging based on: https://github.com/NickThissen/iRacingSdkWrapper

## ProgramConfig

*ProgramConfig format:*
```
  {	
    "Name": {type: String},	
    "TelemetryUpdateFrequency": {type: Float},
    "EndCondition": [{type: string}, {type: Integer}],
    "LogPitDelta": {type: Boolean},
    "LogTireWear": {type: Boolean},
    "Telemetry": [{type: String}]
  }
```
```
"Name": Name of the program to be run, e.g. 'RaceProgram' or 'FreeRoam'

"TelemetryUpdateFrequency": Updates per second. A higher value helps with precision and gathers more data, but comes at a performance and memory cost. 
Values between 4-20 Hz seems to cause no noticable issues.

"EndCondition": End the program and data logging when this condition is met.
  There are currently 4 end conditions:  
  "FreeRoam". Does not require a count. Logs until you explicitly stop it.  
  "Minutes". Requires a count. The time to run the program in minutes. e.g. 60 minutes.
  "Laps". Requires a count. The amount of laps to run. e.g. 10 laps.
  "InPitStall". Requires a count. The amount of times the pit stall is entered. e.g. 3 times.

"LogPitDelta": Whether to try and log a pit delta. Requires you to go through the pit lane at least once.

"LogTireWear": Whether to try and log tire degradation data. For it to work with iracing, you must enter the pit stall.

"Telemetry": What telemetry data to log. The amount of telemetry that is added in a lap depends on TelemetryUpdateFrequency.

  Currently you can log this:  
  "FuelInTank",
  "Speed",
  "BrakePressurePct",
  "ThrottlePressurePct",
  "Rpm",
  "LapDistancePct",
   "Gear"
```

*Example:*
```
{	
  "Name": "Example",	
  "TelemetryUpdateFrequency": 4.0,
  "EndCondition": ["laps", 5],
  "LogPitDelta": true,
  "LogTireWear": true,
  "Telemetry": [
      "FuelInTank",
      "Speed",
      "BrakePressurePct",
      "ThrottlePressurePct",
      "Rpm",
      "LapDistancePct",
      "Gear"
   ]
}
```
