namespace FlightControl.Domain.Rules;

public interface IPermitRule
{
    RuleResult Check(PermitCheckContext context);
}