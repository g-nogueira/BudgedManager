namespace MonthlyBudget.IdentityHousehold.Domain.Exceptions;
public abstract class IdentityDomainException : Exception { protected IdentityDomainException(string m) : base(m) {} }
public class HouseholdFullException : IdentityDomainException { public HouseholdFullException() : base("Household already has maximum 2 members.") {} }
public class DuplicateOwnerException : IdentityDomainException { public DuplicateOwnerException() : base("Household already has an OWNER.") {} }
public class DuplicateEmailException : IdentityDomainException { public DuplicateEmailException(string email) : base($"Email '{email}' is already registered.") {} }
public class InvitationExpiredException : IdentityDomainException { public InvitationExpiredException() : base("This invitation has expired.") {} }
public class InvalidCredentialsException : IdentityDomainException { public InvalidCredentialsException() : base("Invalid email or password.") {} }
public class HouseholdNotFoundException : IdentityDomainException { public HouseholdNotFoundException(Guid id) : base($"Household '{id}' was not found.") {} }
