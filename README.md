# ![Logo](http://riii.me/nlog-targets-pushover-logo) NLog.Targets.Pushover
NLog.Targets.Pushover is a custom target for NLog enabling you to send logging messages to the Pushover service

## Configuration

To use the Pushover target, simply add it as extension in the NLog.config file and place the NLog.Targets.Pushover.dll in the same location as the NLog.dll & NLog.config files.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<nlog xmlns="http://www.nlog-project.org/schemas/NLog.xsd"
      xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">

  <extensions>
    <add assembly="NLog.Targets.Pushover" />
  </extensions>

  <targets>
    <target name="pushover" xsi:type="Pushover" layout="${logger}::${message}"
            applicationKey="azGDORePK8gMaC0QOYAMyEEuzJnyUi"
            userOrGroupName="uQiRzpo4DXghDmr9QzzfQu27cmVRsG" />
  </targets>

  <rules>
    <logger name="*" minlevel="Error" writeTo="pushover" />
  </rules>
</nlog>
```
The package is also available through NuGet as [`NLog.Targets.Pushover`](https://www.nuget.org/packages/NLog.Targets.Pushover/1.0.0).
