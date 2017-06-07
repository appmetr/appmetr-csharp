# appmetr-csharp
Client-to-server appmetr event analytics library implemented on csharp

## How to use it?

Create appMetr instance:
```csharp
var appMetr = new AppMetr(Url, Deploy, mobUuid, filePath);
```

Start it:
```csharp
appMetr.Start();
```

Track some actions:
```csharp
appMetr.Track(new TrackEvent("platform"));
```

Stop it:
```csharp
appMetr.Stop();
```

To enable logging you should set it up:
```csharp
LogUtils.CustomLog = myLog;
```

