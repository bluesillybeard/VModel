using System;

namespace vmodel;

public enum VErrorType{
    Exception, vmfSyntax, unknown
}
public struct VError{
    public readonly Exception? exception;
    public readonly string message;
    public readonly VErrorType type;

    public VError(string parseError){
        type = VErrorType.vmfSyntax;
        message = parseError;
        exception = null;
    }

    public VError(Exception exception){
        this.type = VErrorType.Exception;
        message = exception.Message;
        this.exception = exception;
    }

    public VError(Exception? ex, string me, VErrorType t)
    {
        this.exception = ex;
        this.message = me;
        this.type = t;
    }

    public override string ToString()
    {
        if(type == VErrorType.Exception){
            #pragma warning disable //disable the null error since if the type is Exception then the Exception member will be set.
            return "Exception: \"" + message + "\" \n Stacktrace: " + exception.StackTrace;
            #pragma warning enable
        }
        return type + ":\"" + message + "\"";
    }
}