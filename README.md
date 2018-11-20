# Log4Net.Http.Appender

Installation:
```
dotnet add package Log4Net.Http.Appender
```
OR
```
Install-Package Log4Net.Http.Appender
```

To use HttpAppender just add the appender to your log4net config file:

```
<appender name="CustomHttpAppender" type="Log4net.Http.Appender.HttpAppender, Log4net.Http.Appender">
    <HttpEndpoint>http://localhost:30000</HttpEndpoint>
    <ErrorMaxRetries>10</ErrorMaxRetries>
    <ErrorSleepTime>00:00:00.200</ErrorSleepTime>
</appender>
```

To use HttpBufferedAppender just add the appender to your log4net config file:

```
<appender name="CustomHttpAppender" type="Log4net.Http.Appender.HttpBufferedAppender, Log4net.Http.Appender">
    <HttpEndpoint>http://localhost:30000</HttpEndpoint>
    <ErrorMaxRetries>10</ErrorMaxRetries>
    <ErrorSleepTime>00:00:00.200</ErrorSleepTime>
    <MaxItemsInMemory>10240</MaxItemsInMemory>
    <BatchMaxSize>10</BatchMaxSize>
    <BatchSleepTime>00:00:00.200</BatchSleepTime>
</appender>
```

Include appender to your logger:
```
<root>
    <level value="ALL" />
    <appender-ref ref="CustomHttpAppender" />
</root>
```