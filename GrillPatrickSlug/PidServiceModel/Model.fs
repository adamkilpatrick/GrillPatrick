namespace PidServiceModel

module Models =
    open System

    type PidServiceRequest = {
        PidSession:Guid;
        ConstantP:double;
        ConstantI:double;
        ConstantD:double;
        InputValue:double;
        TargetValue:double;
    }

    type PidServiceResponse = {
        OutputValue:double;
    }


