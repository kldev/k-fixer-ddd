using K.Fixer.Domain.IAM.Aggregates.User;
using K.Fixer.Domain.Shared;

namespace K.Fixer.Web.Api.Tests.Domain;

public class UserTests
{
    [Fact]
    public void Email_Invalid_Will_Fail()
    {
        Result<Email> email = Email.Create("bad-email");

        Assert.False(email.IsSuccess);
        Assert.True(email.IsFailure);
    }
    
    [Fact]
    public void Email_Valid_Will_Succeed()
    {
        Result<Email> email = Email.Create("emma@acme.com");

        Assert.True(email.IsSuccess);
        Assert.False(email.IsFailure);
    }

    [Fact]
    public void Can_Create_Valid_User_And_Validate_Password()
    {
        var user = User.Create("dave@acme.com",
            "fixer777",
            "Dave Smith"
            , UserRole.Technician.Value);

        var dave = user.Value!;
        Assert.True(user.IsSuccess);

        var verityResult = dave.VerifyPassword("fixer777");
        Assert.True(verityResult.IsSuccess);
        
        user.Value!.ChangePassword("fixer777", "demo123");
        
        verityResult = dave.VerifyPassword("demo123");
        Assert.True(verityResult.IsSuccess);
        
        verityResult = dave.VerifyPassword("fixer777");
        Assert.True(verityResult.IsFailure);
    }


}