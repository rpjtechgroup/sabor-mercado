using Microsoft.Extensions.Localization;
using SaborMercado.Web.Domain.Status;
using SaborMercado.Web.Resources;

namespace SaborMercado.Web.Shared;

public sealed class StatusMessageLocalizer(IStringLocalizer<StatusMessages> localizer)
{
    public string Localize(StatusMessage message)
    {
        var key = ResolveResourceKey(message);
        string text = localizer[key];

        foreach (var (argKey, argValue) in message.Args)
        {
            text = text.Replace("{" + argKey + "}", argValue, StringComparison.Ordinal);
        }

        return text;
    }

    private static string ResolveResourceKey(StatusMessage message)
    {
        if (message.Code == StatusCodes.SessionFinished &&
            message.Args.TryGetValue("variant", out var variant))
        {
            return variant switch
            {
                "under" => "SESSION_FINISHED_UNDER",
                "over" => "SESSION_FINISHED_OVER",
                _ => "SESSION_FINISHED",
            };
        }

        return message.Code;
    }
}
