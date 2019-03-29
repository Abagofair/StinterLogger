# RaceLogging

-- [Simple Console Application that showcases the library](https://github.com/Abagofair/StinterLogger/releases/tag/v1.0)

Data gathering library for racing games.

Currently supported games:

- [iRacing](https://www.iracing.com/)
-- iRacing logging based on: https://github.com/NickThissen/iRacingSdkWrapper


## ProgramConfig

* Every property is required and needs to have a value.
* The telemetry property can be an empty array.
* There is not a syntax case requirement. 

*ProgramConfig format:*
```
  {	
    "Name": {type: String},	
    "TelemetryUpdateFrequency": {type: Float},
    "StartCondition": {type: String},
    "EndCondition": [{type: string}, {type: Integer}],
    "LogPitDelta": {type: Boolean},
    "LogTireWear": {type: Boolean},
    "Telemetry": [{type: String}]
  }
```
```
"Name": Name of the program to be run, e.g. 'RaceProgram' or 'FreeRoam'

"TelemetryUpdateFrequency": Updates per second. A higher value helps with precision (sector time estimation, lap time 
estimation..) because of the increased amount of data, but comes at a performance and memory cost. 
Values between 4-20 Hz seems to cause no noticable issues.

"StartCondition": Activates the program and emits ProgramActivation event when this condition is met.
  "PitExit": When you exit the pit lane or if you start the program on track.
  "AfterOutLap": When the first lap has been completed, the program activates.
  "GreenFlag": When the green flag is met in a race session, the program activates.
  "None": The program will immediately activate.

"EndCondition": End the program, stop data logging and emit ProgramEnd event when this condition is met.
  "FreeRoam". Does not require a count. Logs until you explicitly stop it.  
  "Minutes". Requires a count. The time to run the program in minutes. e.g. 60 minutes.
  "Laps". Requires a count. The amount of laps to run. e.g. 10 laps.
  "InPitStall". Requires a count. The amount of times the pit stall is entered. e.g. 3 times.
  "Checkered". When the checkered flag is met in a race session, the program will stop.

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
  "StartCondition": "AfterOutLap",
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

[Example output - pastebin](https://pastebin.com/1n1W3jjd)

# StinterLogger
Old application based on RaceLogging for racing games.

-- [v0.1.0.0](https://github.com/Abagofair/StinterLogger/releases/tag/0.1.0.0)

## Current functionality
Fuel calculator with automatic fueling upon pit entry, debug logging.

## In progress
Starting and creating programs

Options
