namespace SaborMercado.Shared.Community;

public static class CommunityErrorCodes
{
    public const string SelfVoteNotAllowed = "SELF_VOTE_NOT_ALLOWED";
    public const string SelfReportNotAllowed = "SELF_REPORT_NOT_ALLOWED";
    public const string InvalidVoteValue = "INVALID_VOTE_VALUE";
    public const string ObservationNotFound = "OBSERVATION_NOT_FOUND";
    public const string ObservationNotVotable = "OBSERVATION_NOT_VOTABLE";
    public const string ReportAlreadySubmitted = "REPORT_ALREADY_SUBMITTED";
    public const string InvalidReportReason = "INVALID_REPORT_REASON";
    public const string ReportDetailsRequired = "REPORT_DETAILS_REQUIRED";
    public const string ContributorRestricted = "CONTRIBUTOR_RESTRICTED";
}
