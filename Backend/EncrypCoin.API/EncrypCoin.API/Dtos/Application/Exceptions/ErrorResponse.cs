using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EncrypCoin.API.Dtos
{
    public class ErrorResponse
    {
        [JsonPropertyName("traceId")]
        public string TraceId { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }

        [JsonPropertyName("errors")]
        public Dictionary<string, string[]>? Errors { get; set; }

        public ErrorResponse(string traceId, int status, string message)
        {
            TraceId = traceId;
            Status = status;
            Message = message;
        }
    }
}
