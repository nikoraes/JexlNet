using System.Text.Json.Nodes;

namespace JexlNet.Test;

public class ExtensionsUnitTest
{
    [Fact]
    public void RemovesDuplicateKeys()
    {
        string json =
            @"{
    ""key1"": ""value1"",
    ""key2"": ""value2"",
    ""key1"": ""value3"",
    ""key3"": {
        ""nestedKey1"": ""nestedValue1"",
        ""nestedKey1"": ""nestedValue2""
    }
}";
        var node = JsonNode.Parse(json);
        var obj = node as JsonObject;
        Assert.NotNull(obj);
        obj.RemoveDuplicateKeys();

        // Only the last value for key1 and nestedKey1 should remain
        Assert.Equal(3, obj.Count); // key1, key2, key3
        Assert.True(obj.ContainsKey("key1"));
        Assert.True(obj.ContainsKey("key2"));
        Assert.True(obj.ContainsKey("key3"));

        var nestedObj = obj["key3"] as JsonObject;
        Assert.NotNull(nestedObj);
        Assert.Single(nestedObj);
        Assert.True(nestedObj.ContainsKey("nestedKey1"));
        Assert.Equal("nestedValue2", nestedObj["nestedKey1"].ToString());
    }
}
