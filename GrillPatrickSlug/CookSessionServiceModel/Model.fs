

namespace CookSessionServiceModel

module Model =
    open System

    type BeginSessionRequest = {
        SessionName:string;
        TargetValue:double;
    }
    type BeginSessionResponse = {
        SessionName:string;
        SessionId:Guid;
    }

    type EndSessionRequest = {
        SessionId:Guid;
    }

    type GetActiveSessionResponse = {
        SessionName:string;
        SessionId:Guid;
        TargetValue:double;
        CurrentValue:double;
        StartDate:DateTime;
    }
