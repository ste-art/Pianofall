using Newtonsoft.Json;

public class Note
{
    [JsonProperty("n")]
    public string Value;
    [JsonProperty("v", NullValueHandling = NullValueHandling.Ignore)]
    public double Velocity;
    [JsonProperty("d", NullValueHandling = NullValueHandling.Ignore)]
    public long Delay;
    [JsonProperty("s", NullValueHandling = NullValueHandling.Ignore)]
    public int Stop;

    public Note()
    {
        Value = "a0";
        Velocity = 0.5;
        Delay = 0;
        Stop = 0;
    }
}
