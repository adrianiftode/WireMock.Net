#if PROTOBUF
using System;
using WireMock.Models;

namespace WireMock.Matchers.Request;

/// <summary>
/// The request body Grpc ProtoBuf matcher.
/// </summary>
public class RequestMessageProtoBufMatcher : IRequestMessageProtoBufMatcher
{
    /// <summary>
    /// The ProtoBufMatcher.
    /// </summary>
    public IProtoBufMatcher? Matcher { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestMessageProtoBufMatcher"/> class.
    /// </summary>
    /// <param name="matchBehaviour">The match behaviour. (default = "AcceptOnMatch")</param>
    /// <param name="protoDefinition">The Func to define The proto definition as text.</param>
    /// <param name="messageType">The full type of the protobuf (request/response) message object. Format is "{package-name}.{type-name}".</param>
    /// <param name="matcher">The optional matcher to use to match the ProtoBuf as (json) object.</param>
    public RequestMessageProtoBufMatcher(MatchBehaviour matchBehaviour, Func<IdOrText> protoDefinition, string messageType, IObjectMatcher? matcher = null)
    {
        Matcher = new ProtoBufMatcher(protoDefinition, messageType, matchBehaviour, matcher);
    }

    /// <inheritdoc />
    public double GetMatchingScore(IRequestMessage requestMessage, IRequestMatchResult requestMatchResult)
    {
        var (score, exception) = GetMatchResult(requestMessage).Expand();
        return requestMatchResult.AddScore(GetType(), score, exception);
    }

    private MatchResult GetMatchResult(IRequestMessage requestMessage)
    {
        return Matcher?.IsMatchAsync(requestMessage.BodyAsBytes).GetAwaiter().GetResult() ?? default;
    }
}
#endif