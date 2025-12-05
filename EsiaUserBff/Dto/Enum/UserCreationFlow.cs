namespace EsiaUserGenerator.Dto.Enum;

public enum UserCreationFlow
{
    Queued,
    Started,
    PostData,
    WaitingSms,
    ConfirmSms,
    PasswordSet,
    Authorization,
    PersonDataUpdate,
    PostmailWaiting,
    PostmailConfirmation,
    Completed
}